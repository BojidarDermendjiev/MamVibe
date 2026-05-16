namespace MomVibe.Infrastructure.Services;

using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Domain.Entities;
using Domain.Enums;
using Application.Interfaces;
using Application.DTOs.Payments;

/// <summary>
/// Manages the buyer-facing e-bill lifecycle:
/// - <see cref="IssueEBillAsync"/>: assigns a human-readable receipt number and emails the buyer.
///   Idempotent — skips both steps when EBillNumber is already set.
/// - <see cref="GetMyEBillsAsync"/> / <see cref="GetEBillAsync"/>: read-only projections.
/// - <see cref="ResendEBillEmailAsync"/>: re-sends the receipt email on buyer request.
///
/// Only Sell-type payments with <see cref="PaymentStatus.Completed"/> produce an e-bill.
/// Donations (Booking) and on-spot payments are excluded at the query level.
/// </summary>
public class EBillService : IEBillService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<EBillService> _logger;

    public EBillService(
        IApplicationDbContext context,
        IMapper mapper,
        IEmailService emailService,
        UserManager<ApplicationUser> userManager,
        ILogger<EBillService> logger)
    {
        _context = context;
        _mapper = mapper;
        _emailService = emailService;
        _userManager = userManager;
        _logger = logger;
    }

    // =========================================================================
    // Issuance
    // =========================================================================

    public async Task IssueEBillAsync(Guid paymentId, string buyerEmail)
    {
        var payment = await _context.Payments
            .Include(p => p.Item)
            .Include(p => p.Seller)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment is null)
        {
            _logger.LogWarning("IssueEBillAsync: payment {PaymentId} not found.", paymentId);
            return;
        }

        // Idempotency guard — do not overwrite or re-send if already issued.
        if (payment.EBillNumber != null)
            return;

        payment.EBillNumber = GenerateEBillNumber(payment.Id);
        await _context.SaveChangesAsync();

        var bill = _mapper.Map<EBillDto>(payment);

        try
        {
            await _emailService.SendEmailAsync(
                buyerEmail,
                $"Вашата е-фактура / Your E-Bill {bill.EBillNumber}",
                BuildEBillEmailHtml(bill));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send e-bill email for payment {PaymentId}.", paymentId);
        }
    }

    // =========================================================================
    // Queries
    // =========================================================================

    public async Task<List<EBillDto>> GetMyEBillsAsync(string userId)
    {
        var payments = await _context.Payments
            .Include(p => p.Item)
            .Include(p => p.Seller)
            .Where(p => p.BuyerId == userId
                     && p.Status == PaymentStatus.Completed
                     && p.Item != null
                     && p.Item.ListingType == ListingType.Sell)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<EBillDto>>(payments);
    }

    public async Task<EBillDto> GetEBillAsync(Guid paymentId, string userId)
    {
        var payment = await _context.Payments
            .Include(p => p.Item)
            .Include(p => p.Seller)
            .FirstOrDefaultAsync(p => p.Id == paymentId
                                   && p.Status == PaymentStatus.Completed
                                   && p.Item != null
                                   && p.Item.ListingType == ListingType.Sell);

        if (payment is null)
            throw new KeyNotFoundException("E-bill not found.");

        if (payment.BuyerId != userId)
            throw new UnauthorizedAccessException("You do not have access to this e-bill.");

        return _mapper.Map<EBillDto>(payment);
    }

    public async Task ResendEBillEmailAsync(Guid paymentId, string userId)
    {
        var payment = await _context.Payments
            .Include(p => p.Item)
            .Include(p => p.Seller)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment is null)
            throw new KeyNotFoundException("E-bill not found.");

        if (payment.BuyerId != userId)
            throw new UnauthorizedAccessException("You do not have access to this e-bill.");

        var buyer = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("Buyer not found.");

        if (buyer.Email is null)
            return;

        var bill = _mapper.Map<EBillDto>(payment);

        try
        {
            await _emailService.SendEmailAsync(
                buyer.Email,
                $"Вашата е-фактура / Your E-Bill {bill.EBillNumber ?? payment.Id.ToString("N")[..8].ToUpper()}",
                BuildEBillEmailHtml(bill));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resend e-bill email for payment {PaymentId}.", paymentId);
        }
    }

    // =========================================================================
    // Helpers
    // =========================================================================

    /// <summary>
    /// Produces a short, human-readable receipt number that is unique per payment.
    /// Format: MV-{YEAR}-{first 8 hex chars of payment ID in uppercase}
    /// Example: MV-2026-A1B2C3D4
    /// </summary>
    private static string GenerateEBillNumber(Guid paymentId)
        => $"MV-{DateTime.UtcNow.Year}-{paymentId:N}"[..17].ToUpper();

    /// <summary>
    /// Builds the branded HTML email sent to the buyer.
    /// VAT is back-calculated at 20% (Bulgarian standard rate) from the gross amount.
    /// The "Download Receipt" button is omitted when <c>ReceiptUrl</c> is null (dev/test mode).
    /// </summary>
    private static string BuildEBillEmailHtml(EBillDto bill)
    {
        var gross   = bill.Amount;
        var net     = Math.Round(gross / 1.20m, 2);
        var vat     = Math.Round(gross - net, 2);
        var billNo  = bill.EBillNumber ?? "—";
        var date    = bill.IssuedAt.ToString("dd.MM.yyyy");
        var item    = System.Net.WebUtility.HtmlEncode(bill.ItemTitle ?? "Item");
        var seller  = System.Net.WebUtility.HtmlEncode(bill.SellerDisplayName ?? "—");
        var method  = bill.PaymentMethod switch
        {
            Domain.Enums.PaymentMethod.Card   => "Card / Карта",
            Domain.Enums.PaymentMethod.Wallet => "Wallet / Портфейл",
            _                                 => bill.PaymentMethod.ToString()
        };

        // Only embed the receipt link when the URL is a secure https:// URL.
        // An unsanitised URL from an external service could otherwise inject a javascript: URI.
        var safeReceiptUrl = bill.ReceiptUrl is not null
            && bill.ReceiptUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? bill.ReceiptUrl
            : null;

        var downloadBtn = safeReceiptUrl is not null
            ? $"""
              <tr>
                <td align="center" style="padding-top:28px;">
                  <a href="{System.Net.WebUtility.HtmlEncode(safeReceiptUrl)}"
                     style="background:#7c3aed;color:#fff;text-decoration:none;padding:12px 32px;
                            border-radius:8px;font-size:15px;font-weight:600;display:inline-block;">
                    Download Official Receipt / Изтегли фискален бон
                  </a>
                </td>
              </tr>
              """
            : "";

        return $"""
            <!DOCTYPE html>
            <html lang="bg">
            <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
            <body style="margin:0;padding:0;background:#f5f5f5;font-family:Arial,Helvetica,sans-serif;">
              <table width="100%" cellpadding="0" cellspacing="0" style="background:#f5f5f5;padding:32px 0;">
                <tr><td align="center">
                  <table width="600" cellpadding="0" cellspacing="0"
                         style="background:#ffffff;border-radius:12px;overflow:hidden;
                                box-shadow:0 2px 8px rgba(0,0,0,.08);max-width:600px;width:100%;">

                    <!-- Header -->
                    <tr>
                      <td style="background:#7c3aed;padding:28px 40px;">
                        <p style="margin:0;font-size:26px;font-weight:700;color:#fff;letter-spacing:-0.5px;">
                          MamVibe
                        </p>
                        <p style="margin:6px 0 0;font-size:13px;color:#ddd6fe;">
                          Електронна фактура / Electronic Bill
                        </p>
                      </td>
                    </tr>

                    <!-- Body -->
                    <tr>
                      <td style="padding:36px 40px;">
                        <table width="100%" cellpadding="0" cellspacing="0">

                          <!-- Bill meta -->
                          <tr>
                            <td style="padding-bottom:24px;border-bottom:1px solid #e5e7eb;">
                              <table width="100%" cellpadding="0" cellspacing="0">
                                <tr>
                                  <td>
                                    <p style="margin:0;font-size:12px;color:#9ca3af;text-transform:uppercase;letter-spacing:.5px;">
                                      Receipt No. / Фактура №
                                    </p>
                                    <p style="margin:4px 0 0;font-size:20px;font-weight:700;color:#111827;">
                                      {billNo}
                                    </p>
                                  </td>
                                  <td align="right">
                                    <p style="margin:0;font-size:12px;color:#9ca3af;text-transform:uppercase;letter-spacing:.5px;">
                                      Date / Дата
                                    </p>
                                    <p style="margin:4px 0 0;font-size:16px;font-weight:600;color:#374151;">
                                      {date}
                                    </p>
                                  </td>
                                </tr>
                              </table>
                            </td>
                          </tr>

                          <!-- Item row -->
                          <tr>
                            <td style="padding:24px 0 0;">
                              <p style="margin:0 0 16px;font-size:13px;color:#6b7280;text-transform:uppercase;letter-spacing:.5px;">
                                Purchase Details / Детайли за покупка
                              </p>
                              <table width="100%" cellpadding="0" cellspacing="0"
                                     style="background:#f9fafb;border-radius:8px;overflow:hidden;">
                                <tr style="background:#f3f4f6;">
                                  <td style="padding:10px 16px;font-size:12px;color:#6b7280;font-weight:600;">
                                    ITEM / АРТИКУЛ
                                  </td>
                                  <td align="right" style="padding:10px 16px;font-size:12px;color:#6b7280;font-weight:600;">
                                    AMOUNT / СУМА
                                  </td>
                                </tr>
                                <tr>
                                  <td style="padding:14px 16px;font-size:15px;color:#111827;font-weight:500;">
                                    {item}
                                    <br>
                                    <span style="font-size:12px;color:#9ca3af;font-weight:400;">
                                      Sold by / Продавач: {seller}
                                    </span>
                                  </td>
                                  <td align="right" style="padding:14px 16px;font-size:15px;color:#111827;font-weight:600;white-space:nowrap;">
                                    {net:F2} {bill.Currency}
                                  </td>
                                </tr>
                              </table>
                            </td>
                          </tr>

                          <!-- Totals -->
                          <tr>
                            <td style="padding-top:16px;">
                              <table width="100%" cellpadding="0" cellspacing="0">
                                <tr>
                                  <td style="padding:6px 0;font-size:14px;color:#6b7280;">
                                    Subtotal (excl. VAT) / Без ДДС
                                  </td>
                                  <td align="right" style="padding:6px 0;font-size:14px;color:#374151;">
                                    {net:F2} {bill.Currency}
                                  </td>
                                </tr>
                                <tr>
                                  <td style="padding:6px 0;font-size:14px;color:#6b7280;">
                                    VAT 20% / ДДС 20%
                                  </td>
                                  <td align="right" style="padding:6px 0;font-size:14px;color:#374151;">
                                    {vat:F2} {bill.Currency}
                                  </td>
                                </tr>
                                <tr style="border-top:2px solid #e5e7eb;">
                                  <td style="padding:12px 0 0;font-size:16px;font-weight:700;color:#111827;">
                                    Total / Общо
                                  </td>
                                  <td align="right" style="padding:12px 0 0;font-size:18px;font-weight:700;color:#7c3aed;">
                                    {gross:F2} {bill.Currency}
                                  </td>
                                </tr>
                                <tr>
                                  <td colspan="2" style="padding-top:6px;font-size:13px;color:#9ca3af;">
                                    Payment method / Начин на плащане: {method}
                                  </td>
                                </tr>
                              </table>
                            </td>
                          </tr>

                          {downloadBtn}

                        </table>
                      </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                      <td style="background:#f9fafb;padding:20px 40px;border-top:1px solid #e5e7eb;">
                        <p style="margin:0;font-size:12px;color:#9ca3af;text-align:center;">
                          MamVibe · Този документ е автоматично генериран / This document is auto-generated.
                          <br>За въпроси / For questions: <a href="mailto:support@momvibe.bg" style="color:#7c3aed;">support@momvibe.bg</a>
                        </p>
                      </td>
                    </tr>

                  </table>
                </td></tr>
              </table>
            </body>
            </html>
            """;
    }
}
