namespace MomVibe.Infrastructure.Services;

using System.Data;

using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Stripe;

using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Application.Interfaces;
using Application.DTOs.Common;
using Application.DTOs.Wallet;
using Infrastructure.Configuration;

/// <summary>
/// Implements the full wallet lifecycle: top-up via Stripe, user-to-user transfers,
/// item payments from wallet balance, withdrawal requests, and admin monitoring/control.
///
/// Concurrency: transfers and withdrawals run inside a Serializable DB transaction.
/// If PostgreSQL raises a serialization conflict (SqlState 40001), the caller receives
/// a DomainException asking them to retry — no silent data corruption is possible.
///
/// Balance: never stored on Wallet — always derived from the latest non-terminal
/// WalletTransaction.BalanceAfter snapshot, which is O(1) thanks to the
/// IX_WalletTransactions_WalletId_CreatedAt composite index.
///
/// TakeANap: fiscal receipts are created automatically for every credit event
/// (top-up, transfer received, item purchase, refund) inside a try-catch so that
/// a TakeANap outage never blocks the financial operation.
/// </summary>
public class WalletService : IWalletService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly ITakeANapService _takeANapService;
    private readonly IN8nWebhookService _n8nWebhook;
    private readonly N8nSettings _n8nSettings;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEBillService _eBillService;
    private readonly ILogger<WalletService> _logger;

    public WalletService(
        IApplicationDbContext context,
        IMapper mapper,
        IConfiguration configuration,
        ITakeANapService takeANapService,
        IN8nWebhookService n8nWebhook,
        IOptions<N8nSettings> n8nSettings,
        UserManager<ApplicationUser> userManager,
        IEBillService eBillService,
        ILogger<WalletService> logger)
    {
        _context = context;
        _mapper = mapper;
        _configuration = configuration;
        _takeANapService = takeANapService;
        _n8nWebhook = n8nWebhook;
        _n8nSettings = n8nSettings.Value;
        _userManager = userManager;
        _eBillService = eBillService;
        _logger = logger;
        StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
    }

    // =========================================================================
    // User operations
    // =========================================================================

    public async Task<WalletDto> GetOrCreateWalletAsync(string userId)
    {
        var wallet = await GetOrCreateWalletEntityAsync(userId);
        return await BuildWalletDtoAsync(wallet);
    }

    public async Task<WalletTopUpResultDto> CreateTopUpIntentAsync(string userId, decimal amount)
    {
        var wallet = await GetOrCreateWalletEntityAsync(userId);
        if (wallet.Status != WalletStatus.Active)
            throw new WalletFrozenException();

        // Test mode: Stripe not configured — credit immediately and return fake secret.
        if (!IsStripeConfigured())
        {
            var currentBalance = await GetCurrentBalanceAsync(wallet.Id);
            var testTx = new WalletTransaction
            {
                WalletId = wallet.Id,
                Type = WalletTransactionType.Credit,
                Kind = WalletTransactionKind.TopUp,
                Amount = amount,
                BalanceAfter = currentBalance + amount,
                Status = WalletTransactionStatus.Completed,
                Reference = $"test_{Guid.NewGuid()}",
                Description = $"Wallet top-up — {amount:F2} EUR (test mode)"
            };
            _context.WalletTransactions.Add(testTx);
            await _context.SaveChangesAsync();

            return new WalletTopUpResultDto
            {
                ClientSecret = "test_simulated_client_secret",
                Amount = amount,
                Currency = WalletConstants.Defaults.Currency
            };
        }

        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100),
            Currency = "eur",
            Metadata = new Dictionary<string, string>
            {
                { "type", "wallet_topup" },
                { "userId", userId },
                { "walletId", wallet.Id.ToString() }
            }
        };

        var intentService = new PaymentIntentService();
        var intent = await intentService.CreateAsync(options);

        // Create a Pending transaction now; the webhook will mark it Completed.
        var pendingTx = new WalletTransaction
        {
            WalletId = wallet.Id,
            Type = WalletTransactionType.Credit,
            Kind = WalletTransactionKind.TopUp,
            Amount = amount,
            BalanceAfter = 0,   // updated by HandleTopUpWebhookAsync
            Status = WalletTransactionStatus.Pending,
            Reference = intent.Id,
            Description = $"Wallet top-up — {amount:F2} EUR"
        };
        _context.WalletTransactions.Add(pendingTx);
        await _context.SaveChangesAsync();

        return new WalletTopUpResultDto
        {
            ClientSecret = intent.ClientSecret,
            Amount = amount,
            Currency = WalletConstants.Defaults.Currency
        };
    }

    public async Task HandleTopUpWebhookAsync(string json, string stripeSignature)
    {
        var webhookSecret = _configuration["Stripe:WalletWebhookSecret"];
        if (string.IsNullOrWhiteSpace(webhookSecret) || webhookSecret.Contains("YOUR_"))
        {
            _logger.LogWarning("Stripe:WalletWebhookSecret is not configured — skipping wallet webhook.");
            return;
        }

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Wallet top-up webhook signature validation failed.");
            throw;
        }

        if (stripeEvent.Type != EventTypes.PaymentIntentSucceeded) return;

        var intent = stripeEvent.Data.Object as PaymentIntent;
        if (intent == null) return;
        if (intent.Metadata.GetValueOrDefault("type") != "wallet_topup") return;

        var transaction = await _context.WalletTransactions
            .FirstOrDefaultAsync(t => t.Reference == intent.Id
                                   && t.Kind == WalletTransactionKind.TopUp);

        if (transaction == null)
        {
            _logger.LogWarning("No pending top-up transaction found for PaymentIntent {IntentId}.", intent.Id);
            return;
        }

        // Idempotency guard — Stripe delivers at-least-once.
        if (transaction.Status == WalletTransactionStatus.Completed)
        {
            _logger.LogInformation("Duplicate top-up webhook for {IntentId} — skipping.", intent.Id);
            return;
        }

        var currentBalance = await GetCurrentBalanceAsync(transaction.WalletId);
        transaction.BalanceAfter = currentBalance + transaction.Amount;
        transaction.Status = WalletTransactionStatus.Completed;
        await _context.SaveChangesAsync();

        var userId = await _context.Wallets
            .Where(w => w.Id == transaction.WalletId)
            .Select(w => w.UserId)
            .FirstOrDefaultAsync();

        var user = userId != null ? await _userManager.FindByIdAsync(userId) : null;

        // TakeANap fiscal receipt (failure must not block the webhook response).
        if (user?.Email != null)
        {
            try
            {
                var receiptUrl = await _takeANapService.CreateWalletReceiptAsync(transaction, user.Email);
                if (receiptUrl != null)
                {
                    transaction.ReceiptUrl = receiptUrl;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TakeANap receipt failed for wallet top-up tx {TxId}.", transaction.Id);
            }
        }

        try
        {
            _n8nWebhook.Send(_n8nSettings.WalletTopUp, new
            {
                Event = "wallet.topup.completed",
                Timestamp = DateTime.UtcNow,
                UserId = userId,
                Amount = transaction.Amount,
                Currency = WalletConstants.Defaults.Currency
            });
        }
        catch { }
    }

    public async Task<WalletTransferDto> TransferAsync(
        string senderUserId,
        string receiverEmail,
        decimal amount,
        string? note,
        string? initiatedByIp)
    {
        var senderUser = await _userManager.FindByIdAsync(senderUserId)
            ?? throw new KeyNotFoundException("Sender not found.");

        var receiverUser = await _userManager.FindByEmailAsync(receiverEmail)
            ?? throw new KeyNotFoundException("No account found with that email address.");

        if (senderUser.Id == receiverUser.Id)
            throw new DomainException("You cannot transfer funds to yourself.");

        await using var dbTx = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var senderWallet = await GetOrCreateWalletEntityAsync(senderUser.Id);
            var receiverWallet = await GetOrCreateWalletEntityAsync(receiverUser.Id);

            if (senderWallet.Status != WalletStatus.Active)
                throw new WalletFrozenException();
            if (receiverWallet.Status != WalletStatus.Active)
                throw new DomainException("The recipient's wallet is currently unavailable.");

            var senderBalance = await GetCurrentBalanceAsync(senderWallet.Id);
            if (senderBalance < amount)
                throw new InsufficientFundsException();

            var receiverBalance = await GetCurrentBalanceAsync(receiverWallet.Id);

            // Create the transfer record first to obtain its Id for use as Reference.
            var transfer = new WalletTransfer
            {
                SenderWalletId = senderWallet.Id,
                ReceiverWalletId = receiverWallet.Id,
                Amount = amount,
                Currency = WalletConstants.Defaults.Currency,
                Status = WalletTransferStatus.Pending,
                Note = note,
                InitiatedByIp = initiatedByIp
            };
            _context.WalletTransfers.Add(transfer);
            await _context.SaveChangesAsync();

            var debitDescription = string.IsNullOrWhiteSpace(note)
                ? $"Transfer to {receiverUser.Email}"
                : $"Transfer to {receiverUser.Email} — {note}";

            var creditDescription = string.IsNullOrWhiteSpace(note)
                ? $"Transfer from {senderUser.Email}"
                : $"Transfer from {senderUser.Email} — {note}";

            var debitTx = new WalletTransaction
            {
                WalletId = senderWallet.Id,
                Type = WalletTransactionType.Debit,
                Kind = WalletTransactionKind.Transfer,
                Amount = amount,
                BalanceAfter = senderBalance - amount,
                Status = WalletTransactionStatus.Completed,
                Reference = transfer.Id.ToString(),
                Description = debitDescription
            };

            var creditTx = new WalletTransaction
            {
                WalletId = receiverWallet.Id,
                Type = WalletTransactionType.Credit,
                Kind = WalletTransactionKind.Transfer,
                Amount = amount,
                BalanceAfter = receiverBalance + amount,
                Status = WalletTransactionStatus.Completed,
                Reference = transfer.Id.ToString(),
                Description = creditDescription
            };

            _context.WalletTransactions.Add(debitTx);
            _context.WalletTransactions.Add(creditTx);
            await _context.SaveChangesAsync();

            // Cross-link both legs and finalise the transfer record.
            debitTx.RelatedTransactionId = creditTx.Id;
            creditTx.RelatedTransactionId = debitTx.Id;
            transfer.SenderTransactionId = debitTx.Id;
            transfer.ReceiverTransactionId = creditTx.Id;
            transfer.Status = WalletTransferStatus.Completed;
            await _context.SaveChangesAsync();

            await dbTx.CommitAsync();

            // TakeANap receipt for the receiver's credit entry.
            if (receiverUser.Email != null)
            {
                try
                {
                    var receiptUrl = await _takeANapService.CreateWalletReceiptAsync(creditTx, receiverUser.Email);
                    if (receiptUrl != null)
                    {
                        creditTx.ReceiptUrl = receiptUrl;
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "TakeANap receipt failed for transfer credit tx {TxId}.", creditTx.Id);
                }
            }

            try
            {
                _n8nWebhook.Send(_n8nSettings.WalletTransfer, new
                {
                    Event = "wallet.transfer.completed",
                    Timestamp = DateTime.UtcNow,
                    TransferId = transfer.Id,
                    SenderUserId = senderUser.Id,
                    ReceiverUserId = receiverUser.Id,
                    Amount = amount,
                    Currency = WalletConstants.Defaults.Currency
                });
            }
            catch { }

            return new WalletTransferDto
            {
                Id = transfer.Id,
                SenderWalletId = senderWallet.Id,
                ReceiverWalletId = receiverWallet.Id,
                Amount = amount,
                Currency = WalletConstants.Defaults.Currency,
                Status = WalletTransferStatus.Completed,
                Note = note,
                SenderDisplayName = senderUser.DisplayName,
                ReceiverDisplayName = receiverUser.DisplayName,
                CreatedAt = transfer.CreatedAt
            };
        }
        catch (DomainException)
        {
            await dbTx.RollbackAsync();
            throw;
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "40001")
        {
            await dbTx.RollbackAsync();
            throw new DomainException("Transfer could not be completed due to concurrent activity. Please try again.");
        }
        catch
        {
            await dbTx.RollbackAsync();
            throw;
        }
    }

    public async Task<WalletTransactionDto> PayForItemFromWalletAsync(string buyerUserId, Guid itemId)
    {
        var item = await _context.Items.FindAsync(itemId)
            ?? throw new KeyNotFoundException("Item not found.");

        if (item.ListingType != Domain.Enums.ListingType.Sell)
            throw new InvalidOperationException("This item is not for sale.");

        var amount = item.Price ?? 0m;

        await using var dbTx = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var wallet = await GetOrCreateWalletEntityAsync(buyerUserId);
            if (wallet.Status != WalletStatus.Active)
                throw new WalletFrozenException();

            var balance = await GetCurrentBalanceAsync(wallet.Id);
            if (balance < amount)
                throw new InsufficientFundsException();

            // Payment stays Pending — funds are held in escrow until the buyer
            // confirms delivery via ConfirmDeliveryAsync.
            var payment = new Domain.Entities.Payment
            {
                ItemId = itemId,
                BuyerId = buyerUserId,
                SellerId = item.UserId,
                Amount = amount,
                PaymentMethod = Domain.Enums.PaymentMethod.Wallet,
                Status = Domain.Enums.PaymentStatus.Pending
            };
            _context.Payments.Add(payment);

            var tx = new WalletTransaction
            {
                WalletId = wallet.Id,
                Type = WalletTransactionType.Debit,
                Kind = WalletTransactionKind.ItemPayment,
                Amount = amount,
                BalanceAfter = balance - amount,
                Status = WalletTransactionStatus.Completed,
                Description = $"Payment for: {item.Title} (awaiting delivery)"
            };
            _context.WalletTransactions.Add(tx);
            await _context.SaveChangesAsync();

            tx.PaymentId = payment.Id;
            await _context.SaveChangesAsync();
            await dbTx.CommitAsync();

            return _mapper.Map<WalletTransactionDto>(tx);
        }
        catch (DomainException)
        {
            await dbTx.RollbackAsync();
            throw;
        }
        catch (InvalidOperationException)
        {
            await dbTx.RollbackAsync();
            throw;
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "40001")
        {
            await dbTx.RollbackAsync();
            throw new DomainException("Payment could not be processed due to concurrent activity. Please try again.");
        }
        catch
        {
            await dbTx.RollbackAsync();
            throw;
        }
    }

    public async Task<WalletTransactionDto> ConfirmDeliveryAsync(Guid paymentId, string buyerUserId)
    {
        var payment = await _context.Payments
            .Include(p => p.Item)
            .FirstOrDefaultAsync(p => p.Id == paymentId)
            ?? throw new KeyNotFoundException("Payment not found.");

        if (payment.BuyerId != buyerUserId)
            throw new DomainException("Only the buyer can confirm delivery.");

        if (payment.PaymentMethod != Domain.Enums.PaymentMethod.Wallet)
            throw new DomainException("Only wallet payments use the escrow confirmation flow.");

        if (payment.Status != Domain.Enums.PaymentStatus.Pending)
            throw new DomainException("This payment is not awaiting delivery confirmation.");

        await using var dbTx = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var sellerWallet = await GetOrCreateWalletEntityAsync(payment.SellerId);
            if (sellerWallet.Status == WalletStatus.Frozen)
                throw new DomainException("Seller wallet is currently unavailable. Contact support.");

            var sellerBalance = await GetCurrentBalanceAsync(sellerWallet.Id);

            var creditTx = new WalletTransaction
            {
                WalletId = sellerWallet.Id,
                Type = WalletTransactionType.Credit,
                Kind = WalletTransactionKind.ItemPayment,
                Amount = payment.Amount,
                BalanceAfter = sellerBalance + payment.Amount,
                Status = WalletTransactionStatus.Completed,
                PaymentId = payment.Id,
                Description = $"Sale proceeds: {payment.Item?.Title}"
            };
            _context.WalletTransactions.Add(creditTx);

            payment.Status = Domain.Enums.PaymentStatus.Completed;
            await _context.SaveChangesAsync();
            await dbTx.CommitAsync();

            var seller = await _userManager.FindByIdAsync(payment.SellerId);
            if (seller?.Email != null)
            {
                try
                {
                    var receiptUrl = await _takeANapService.CreateWalletReceiptAsync(creditTx, seller.Email);
                    if (receiptUrl != null)
                    {
                        creditTx.ReceiptUrl = receiptUrl;
                        // Mirror the receipt URL on Payment so the e-bill endpoint can surface it.
                        payment.ReceiptUrl = receiptUrl;
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "TakeANap receipt failed for escrow release tx {TxId}.", creditTx.Id);
                }
            }

            try
            {
                _n8nWebhook.Send(_n8nSettings.WalletEscrowReleased, new
                {
                    Event = "wallet.escrow.released",
                    Timestamp = DateTime.UtcNow,
                    PaymentId = payment.Id,
                    BuyerUserId = buyerUserId,
                    SellerUserId = payment.SellerId,
                    Amount = payment.Amount,
                    Currency = WalletConstants.Defaults.Currency
                });
            }
            catch { }

            // Issue e-bill and send receipt email to buyer (non-blocking on failure)
            try
            {
                var buyer = await _userManager.FindByIdAsync(buyerUserId);
                if (buyer?.Email != null)
                    await _eBillService.IssueEBillAsync(payment.Id, buyer.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "E-bill issuance failed for wallet payment {PaymentId}.", payment.Id);
            }

            return _mapper.Map<WalletTransactionDto>(creditTx);
        }
        catch (DomainException)
        {
            await dbTx.RollbackAsync();
            throw;
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "40001")
        {
            await dbTx.RollbackAsync();
            throw new DomainException("Delivery confirmation could not be processed. Please try again.");
        }
        catch
        {
            await dbTx.RollbackAsync();
            throw;
        }
    }

    public async Task<WalletTransactionDto> WithdrawAsync(string userId, decimal amount)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (string.IsNullOrWhiteSpace(user.Iban))
            throw new DomainException("You must add an IBAN to your profile before requesting a withdrawal.");

        await using var dbTx = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var wallet = await GetOrCreateWalletEntityAsync(userId);
            if (wallet.Status != WalletStatus.Active)
                throw new WalletFrozenException();

            var balance = await GetCurrentBalanceAsync(wallet.Id);
            if (balance < amount)
                throw new InsufficientFundsException();

            var tx = new WalletTransaction
            {
                WalletId = wallet.Id,
                Type = WalletTransactionType.Debit,
                Kind = WalletTransactionKind.Withdrawal,
                Amount = amount,
                BalanceAfter = balance - amount,
                Status = WalletTransactionStatus.Pending,
                Description = $"Withdrawal request — {amount:F2} EUR to IBAN"
            };
            _context.WalletTransactions.Add(tx);
            await _context.SaveChangesAsync();
            await dbTx.CommitAsync();

            try
            {
                _n8nWebhook.Send(_n8nSettings.WalletWithdrawalRequested, new
                {
                    Event = "wallet.withdrawal.requested",
                    Timestamp = DateTime.UtcNow,
                    UserId = userId,
                    TransactionId = tx.Id,
                    Amount = amount,
                    Currency = WalletConstants.Defaults.Currency
                });
            }
            catch { }

            return _mapper.Map<WalletTransactionDto>(tx);
        }
        catch (DomainException)
        {
            await dbTx.RollbackAsync();
            throw;
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "40001")
        {
            await dbTx.RollbackAsync();
            throw new DomainException("Withdrawal request could not be processed due to concurrent activity. Please try again.");
        }
        catch
        {
            await dbTx.RollbackAsync();
            throw;
        }
    }

    public async Task<PagedResult<WalletTransactionDto>> GetTransactionsAsync(string userId, int page, int pageSize)
    {
        var walletId = await _context.Wallets
            .Where(w => w.UserId == userId)
            .Select(w => (Guid?)w.Id)
            .FirstOrDefaultAsync();

        if (walletId == null)
            return new PagedResult<WalletTransactionDto> { Page = page, PageSize = pageSize };

        var query = _context.WalletTransactions
            .Where(t => t.WalletId == walletId)
            .OrderByDescending(t => t.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<WalletTransactionDto>
        {
            Items = _mapper.Map<List<WalletTransactionDto>>(items),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    // =========================================================================
    // Admin operations
    // =========================================================================

    public async Task<PagedResult<AdminWalletDto>> GetAllWalletsAsync(int page, int pageSize, WalletStatus? status = null)
    {
        var query = _context.Wallets.AsQueryable();

        if (status.HasValue)
            query = query.Where(w => w.Status == status.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(w => w.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(w => new AdminWalletDto
            {
                Id = w.Id,
                UserId = w.UserId,
                UserEmail = w.User != null ? w.User.Email : null,
                UserDisplayName = w.User != null ? w.User.DisplayName : null,
                Currency = w.Currency,
                Status = w.Status,
                Balance = w.Transactions
                    .Where(t => t.Status != WalletTransactionStatus.Failed
                             && t.Status != WalletTransactionStatus.Reversed)
                    .OrderByDescending(t => t.CreatedAt)
                    .Select(t => (decimal?)t.BalanceAfter)
                    .FirstOrDefault() ?? 0m,
                TransactionCount = w.Transactions.Count,
                CreatedAt = w.CreatedAt,
                UpdatedAt = w.UpdatedAt
            })
            .ToListAsync();

        return new PagedResult<AdminWalletDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AdminWalletDto> GetWalletByIdAsync(Guid walletId)
    {
        var dto = await _context.Wallets
            .Where(w => w.Id == walletId)
            .Select(w => new AdminWalletDto
            {
                Id = w.Id,
                UserId = w.UserId,
                UserEmail = w.User != null ? w.User.Email : null,
                UserDisplayName = w.User != null ? w.User.DisplayName : null,
                Currency = w.Currency,
                Status = w.Status,
                Balance = w.Transactions
                    .Where(t => t.Status != WalletTransactionStatus.Failed
                             && t.Status != WalletTransactionStatus.Reversed)
                    .OrderByDescending(t => t.CreatedAt)
                    .Select(t => (decimal?)t.BalanceAfter)
                    .FirstOrDefault() ?? 0m,
                TransactionCount = w.Transactions.Count,
                CreatedAt = w.CreatedAt,
                UpdatedAt = w.UpdatedAt
            })
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Wallet {walletId} not found.");

        return dto;
    }

    public async Task FreezeWalletAsync(Guid walletId, string reason)
    {
        var wallet = await _context.Wallets.FindAsync(walletId)
            ?? throw new KeyNotFoundException($"Wallet {walletId} not found.");

        wallet.Status = WalletStatus.Frozen;
        await _context.SaveChangesAsync();

        try
        {
            _n8nWebhook.Send(_n8nSettings.WalletFrozen, new
            {
                Event = "wallet.frozen",
                Timestamp = DateTime.UtcNow,
                WalletId = walletId,
                UserId = wallet.UserId,
                Reason = reason
            });
        }
        catch { }
    }

    public async Task UnfreezeWalletAsync(Guid walletId)
    {
        var wallet = await _context.Wallets.FindAsync(walletId)
            ?? throw new KeyNotFoundException($"Wallet {walletId} not found.");

        wallet.Status = WalletStatus.Active;
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResult<WalletTransactionDto>> GetAllTransactionsAsync(TransactionFilterDto filter)
    {
        var query = _context.WalletTransactions.AsQueryable();

        if (filter.WalletId.HasValue)
            query = query.Where(t => t.WalletId == filter.WalletId.Value);

        if (!string.IsNullOrWhiteSpace(filter.UserId))
        {
            var walletId = await _context.Wallets
                .Where(w => w.UserId == filter.UserId)
                .Select(w => (Guid?)w.Id)
                .FirstOrDefaultAsync();
            if (walletId != null)
                query = query.Where(t => t.WalletId == walletId);
            else
                return new PagedResult<WalletTransactionDto> { Page = filter.Page, PageSize = filter.PageSize };
        }

        if (filter.Kind.HasValue)
            query = query.Where(t => t.Kind == filter.Kind.Value);

        if (filter.Status.HasValue)
            query = query.Where(t => t.Status == filter.Status.Value);

        if (filter.Type.HasValue)
            query = query.Where(t => t.Type == filter.Type.Value);

        if (filter.DateFrom.HasValue)
            query = query.Where(t => t.CreatedAt >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(t => t.CreatedAt <= filter.DateTo.Value);

        if (filter.MinAmount.HasValue)
            query = query.Where(t => t.Amount >= filter.MinAmount.Value);

        if (filter.MaxAmount.HasValue)
            query = query.Where(t => t.Amount <= filter.MaxAmount.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<WalletTransactionDto>
        {
            Items = _mapper.Map<List<WalletTransactionDto>>(items),
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<WalletTransactionDto> RefundTransactionAsync(Guid transactionId, string adminUserId, string reason)
    {
        var original = await _context.WalletTransactions.FindAsync(transactionId)
            ?? throw new KeyNotFoundException($"Transaction {transactionId} not found.");

        if (original.Status != WalletTransactionStatus.Completed)
            throw new DomainException("Only completed transactions can be refunded.");

        if (original.Type != WalletTransactionType.Debit)
            throw new DomainException("Only debit transactions can be refunded.");

        var currentBalance = await GetCurrentBalanceAsync(original.WalletId);

        var refundTx = new WalletTransaction
        {
            WalletId = original.WalletId,
            Type = WalletTransactionType.Credit,
            Kind = WalletTransactionKind.Refund,
            Amount = original.Amount,
            BalanceAfter = currentBalance + original.Amount,
            Status = WalletTransactionStatus.Completed,
            RelatedTransactionId = original.Id,
            Description = $"Refund — {reason}"
        };

        original.Status = WalletTransactionStatus.Reversed;
        _context.WalletTransactions.Add(refundTx);
        await _context.SaveChangesAsync();

        // TakeANap receipt for the refund credit.
        var userId = await _context.Wallets
            .Where(w => w.Id == original.WalletId)
            .Select(w => w.UserId)
            .FirstOrDefaultAsync();

        var user = userId != null ? await _userManager.FindByIdAsync(userId) : null;
        if (user?.Email != null)
        {
            try
            {
                var receiptUrl = await _takeANapService.CreateWalletReceiptAsync(refundTx, user.Email);
                if (receiptUrl != null)
                {
                    refundTx.ReceiptUrl = receiptUrl;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TakeANap receipt failed for refund tx {TxId}.", refundTx.Id);
            }
        }

        try
        {
            _n8nWebhook.Send(_n8nSettings.WalletRefund, new
            {
                Event = "wallet.refund.completed",
                Timestamp = DateTime.UtcNow,
                OriginalTransactionId = original.Id,
                RefundTransactionId = refundTx.Id,
                AdminUserId = adminUserId,
                Amount = refundTx.Amount,
                Currency = WalletConstants.Defaults.Currency,
                Reason = reason
            });
        }
        catch { }

        return _mapper.Map<WalletTransactionDto>(refundTx);
    }

    public async Task<PagedResult<WalletTransactionDto>> GetPendingWithdrawalsAsync(int page, int pageSize)
    {
        var query = _context.WalletTransactions
            .Where(t => t.Kind == WalletTransactionKind.Withdrawal
                     && t.Status == WalletTransactionStatus.Pending)
            .OrderBy(t => t.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<WalletTransactionDto>
        {
            Items = _mapper.Map<List<WalletTransactionDto>>(items),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task ApproveWithdrawalAsync(Guid transactionId, string adminUserId)
    {
        var tx = await _context.WalletTransactions.FindAsync(transactionId)
            ?? throw new KeyNotFoundException($"Transaction {transactionId} not found.");

        if (tx.Kind != WalletTransactionKind.Withdrawal || tx.Status != WalletTransactionStatus.Pending)
            throw new DomainException("Transaction is not a pending withdrawal.");

        tx.Status = WalletTransactionStatus.Completed;
        await _context.SaveChangesAsync();

        try
        {
            _n8nWebhook.Send(_n8nSettings.WalletWithdrawalApproved, new
            {
                Event = "wallet.withdrawal.approved",
                Timestamp = DateTime.UtcNow,
                TransactionId = transactionId,
                AdminUserId = adminUserId,
                Amount = tx.Amount,
                Currency = WalletConstants.Defaults.Currency
            });
        }
        catch { }
    }

    public async Task RejectWithdrawalAsync(Guid transactionId, string adminUserId, string reason)
    {
        var tx = await _context.WalletTransactions.FindAsync(transactionId)
            ?? throw new KeyNotFoundException($"Transaction {transactionId} not found.");

        if (tx.Kind != WalletTransactionKind.Withdrawal || tx.Status != WalletTransactionStatus.Pending)
            throw new DomainException("Transaction is not a pending withdrawal.");

        var currentBalance = await GetCurrentBalanceAsync(tx.WalletId);

        // Mark original as Reversed.
        tx.Status = WalletTransactionStatus.Reversed;

        // Return the reserved funds to the wallet.
        var returnTx = new WalletTransaction
        {
            WalletId = tx.WalletId,
            Type = WalletTransactionType.Credit,
            Kind = WalletTransactionKind.Refund,
            Amount = tx.Amount,
            BalanceAfter = currentBalance + tx.Amount,
            Status = WalletTransactionStatus.Completed,
            RelatedTransactionId = tx.Id,
            Description = $"Withdrawal rejected — {reason}"
        };
        _context.WalletTransactions.Add(returnTx);
        await _context.SaveChangesAsync();

        try
        {
            _n8nWebhook.Send(_n8nSettings.WalletWithdrawalRejected, new
            {
                Event = "wallet.withdrawal.rejected",
                Timestamp = DateTime.UtcNow,
                TransactionId = transactionId,
                AdminUserId = adminUserId,
                Amount = tx.Amount,
                Currency = WalletConstants.Defaults.Currency,
                Reason = reason
            });
        }
        catch { }
    }

    // =========================================================================
    // Private helpers
    // =========================================================================

    private async Task<Wallet> GetOrCreateWalletEntityAsync(string userId)
    {
        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet != null) return wallet;

        wallet = new Wallet { UserId = userId };
        _context.Wallets.Add(wallet);
        await _context.SaveChangesAsync();
        return wallet;
    }

    /// <summary>
    /// Returns the effective balance by reading the BalanceAfter snapshot from the
    /// most recent non-terminal (not Failed, not Reversed) transaction.
    /// Pending withdrawals are included so funds are correctly reserved.
    /// </summary>
    private async Task<decimal> GetCurrentBalanceAsync(Guid walletId)
    {
        return await _context.WalletTransactions
            .Where(t => t.WalletId == walletId
                     && t.Status != WalletTransactionStatus.Failed
                     && t.Status != WalletTransactionStatus.Reversed)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => (decimal?)t.BalanceAfter)
            .FirstOrDefaultAsync() ?? 0m;
    }

    private async Task<WalletDto> BuildWalletDtoAsync(Wallet wallet)
    {
        var dto = _mapper.Map<WalletDto>(wallet);
        dto.Balance = await GetCurrentBalanceAsync(wallet.Id);
        return dto;
    }

    private bool IsStripeConfigured()
    {
        var key = _configuration["Stripe:SecretKey"];
        return !string.IsNullOrWhiteSpace(key) && !key.Contains("YOUR_STRIPE");
    }
}
