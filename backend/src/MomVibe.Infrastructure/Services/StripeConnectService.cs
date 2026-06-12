namespace MomVibe.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Stripe;

using Application.Interfaces;
using Application.DTOs.Payments;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

/// <summary>
/// EF Core + Stripe SDK-backed implementation of <see cref="IStripeConnectService"/>.
/// Manages the user's Stripe Express account lifecycle for peer-to-peer item payouts.
/// </summary>
/// <remarks>
/// Behaviour when Stripe is NOT configured (no SecretKey or placeholder value): all
/// network-bound methods short-circuit to a deterministic "test_*" path and write
/// local state as Verified so dev environments don't hard-block listing creation.
/// </remarks>
public class StripeConnectService : IStripeConnectService
{
    private const string PlaceholderToken = "YOUR_STRIPE";
    private const string DefaultCountry = "BG"; // Bulgaria — change if expanding markets

    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeConnectService> _logger;

    public StripeConnectService(
        IApplicationDbContext context,
        IConfiguration configuration,
        ILogger<StripeConnectService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    private bool IsStripeConfigured()
    {
        var key = _configuration["Stripe:SecretKey"];
        return !string.IsNullOrWhiteSpace(key) && !key.Contains(PlaceholderToken);
    }

    public async Task<StripeConnectStatusDto> GetStatusAsync(string userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.StripeConnectAccountId,
                u.StripeConnectStatus,
                u.StripeConnectStatusUpdatedAt,
            })
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("User not found.");

        return new StripeConnectStatusDto
        {
            Status = user.StripeConnectStatus,
            CanReceivePayouts = user.StripeConnectStatus == StripeConnectStatus.Verified,
            HasAccount = !string.IsNullOrEmpty(user.StripeConnectAccountId),
            StatusUpdatedAt = user.StripeConnectStatusUpdatedAt,
        };
    }

    public async Task<StripeConnectOnboardingLinkDto> CreateOnboardingLinkAsync(
        string userId, string returnUrl, string refreshUrl)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException("User not found.");

        // Dev / unconfigured path: skip Stripe entirely, mark verified so the rest of
        // the platform doesn't block sellers in local environments.
        if (!IsStripeConfigured())
        {
            if (string.IsNullOrEmpty(user.StripeConnectAccountId))
            {
                user.StripeConnectAccountId = $"acct_test_{Guid.NewGuid():N}";
                user.StripeConnectStatus = StripeConnectStatus.Verified;
                user.StripeConnectStatusUpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Stripe not configured — created stub Connect account {AccountId} for user {UserId}.",
                    user.StripeConnectAccountId, userId);
            }
            return new StripeConnectOnboardingLinkDto { OnboardingUrl = returnUrl };
        }

        // Idempotent account creation — only create when we don't already have one.
        if (string.IsNullOrEmpty(user.StripeConnectAccountId))
        {
            var accountOptions = new AccountCreateOptions
            {
                Type = "express",
                Country = DefaultCountry,
                Email = user.Email,
                Capabilities = new AccountCapabilitiesOptions
                {
                    Transfers = new AccountCapabilitiesTransfersOptions { Requested = true },
                    CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true },
                },
                BusinessType = "individual",
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId },
                    { "platform", "MomVibe" },
                },
            };

            var accountService = new AccountService();
            var account = await accountService.CreateAsync(accountOptions);
            user.StripeConnectAccountId = account.Id;
            user.StripeConnectStatus = StripeConnectStatus.Pending;
            user.StripeConnectStatusUpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created Stripe Connect account {AccountId} for user {UserId}.", account.Id, userId);
        }

        var linkOptions = new AccountLinkCreateOptions
        {
            Account = user.StripeConnectAccountId,
            ReturnUrl = returnUrl,
            RefreshUrl = refreshUrl,
            Type = "account_onboarding",
        };
        var linkService = new AccountLinkService();
        var link = await linkService.CreateAsync(linkOptions);

        return new StripeConnectOnboardingLinkDto { OnboardingUrl = link.Url };
    }

    public async Task<StripeConnectDashboardLinkDto> CreateDashboardLinkAsync(string userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (string.IsNullOrEmpty(user.StripeConnectAccountId))
            throw new BusinessConflictException("connect_no_account",
                "No Stripe Connect account exists for this user yet.");
        if (user.StripeConnectStatus != StripeConnectStatus.Verified)
            throw new BusinessConflictException("connect_not_verified",
                "Stripe onboarding must be completed before the dashboard is available.");

        if (!IsStripeConfigured())
            return new StripeConnectDashboardLinkDto { DashboardUrl = "/dashboard" };

        var loginLinkService = new AccountLoginLinkService();
        var link = await loginLinkService.CreateAsync(user.StripeConnectAccountId);
        return new StripeConnectDashboardLinkDto { DashboardUrl = link.Url };
    }

    public async Task<StripeConnectStatusDto> RefreshStatusFromStripeAsync(string userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (string.IsNullOrEmpty(user.StripeConnectAccountId) || !IsStripeConfigured())
            return await GetStatusAsync(userId);

        var accountService = new AccountService();
        var account = await accountService.GetAsync(user.StripeConnectAccountId);

        var hasDisabledReason = account.Requirements?.DisabledReason != null;
        var newStatus = DeriveStatus(account.ChargesEnabled, account.PayoutsEnabled, hasDisabledReason);
        if (newStatus != user.StripeConnectStatus)
        {
            user.StripeConnectStatus = newStatus;
            user.StripeConnectStatusUpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return new StripeConnectStatusDto
        {
            Status = user.StripeConnectStatus,
            CanReceivePayouts = user.StripeConnectStatus == StripeConnectStatus.Verified,
            HasAccount = true,
            StatusUpdatedAt = user.StripeConnectStatusUpdatedAt,
        };
    }

    public async Task ApplyAccountUpdateAsync(
        string stripeAccountId, bool chargesEnabled, bool payoutsEnabled, bool hasDisabledReason)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.StripeConnectAccountId == stripeAccountId);
        if (user == null)
        {
            _logger.LogWarning("account.updated webhook for unknown Connect account {AccountId} — ignoring.", stripeAccountId);
            return;
        }

        var newStatus = DeriveStatus(chargesEnabled, payoutsEnabled, hasDisabledReason);
        if (newStatus == user.StripeConnectStatus) return;

        user.StripeConnectStatus = newStatus;
        user.StripeConnectStatusUpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Connect account {AccountId} for user {UserId} transitioned to {Status}.",
            stripeAccountId, user.Id, newStatus);
    }

    private static StripeConnectStatus DeriveStatus(bool chargesEnabled, bool payoutsEnabled, bool hasDisabledReason)
    {
        if (hasDisabledReason) return StripeConnectStatus.Restricted;
        if (chargesEnabled && payoutsEnabled) return StripeConnectStatus.Verified;
        return StripeConnectStatus.Pending;
    }
}
