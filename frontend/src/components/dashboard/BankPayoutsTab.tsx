import { useEffect, useState, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { Loader2, ExternalLink, ShieldCheck, Hourglass, AlertTriangle, Wallet } from "lucide-react";
import axios from "axios";
import toast from "@/utils/toast";
import { connectApi } from "@/api/connectApi";
import { StripeConnectStatus, type ConnectStatusDto } from "@/types/connect";

const STATUS_BADGE: Record<StripeConnectStatus, { labelKey: string; cls: string; icon: React.ElementType }> = {
  [StripeConnectStatus.None]: {
    labelKey: "connect.status.none",
    cls: "bg-gray-100 text-gray-600 dark:bg-gray-700/60 dark:text-gray-300",
    icon: Wallet,
  },
  [StripeConnectStatus.Pending]: {
    labelKey: "connect.status.pending",
    cls: "bg-amber-100 text-amber-700 dark:bg-amber-500/20 dark:text-amber-300",
    icon: Hourglass,
  },
  [StripeConnectStatus.Verified]: {
    labelKey: "connect.status.verified",
    cls: "bg-emerald-100 text-emerald-700 dark:bg-emerald-500/20 dark:text-emerald-300",
    icon: ShieldCheck,
  },
  [StripeConnectStatus.Restricted]: {
    labelKey: "connect.status.restricted",
    cls: "bg-red-100 text-red-700 dark:bg-red-500/20 dark:text-red-300",
    icon: AlertTriangle,
  },
};

/**
 * Dashboard tab that lets sellers configure Stripe Connect Express — required
 * before they can list paid items. Reads status on mount, refreshes on window
 * focus (catches the user returning from Stripe's hosted onboarding flow), and
 * shows a context-appropriate CTA (Start / Continue / Open Express dashboard).
 */
export default function BankPayoutsTab() {
  const { t } = useTranslation();
  const [statusDto, setStatusDto] = useState<ConnectStatusDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);

  const loadStatus = useCallback(async (refresh = false) => {
    try {
      const { data } = await connectApi.getStatus(refresh);
      setStatusDto(data);
    } catch {
      toast.error(t("connect.loadError"));
    } finally {
      setLoading(false);
    }
  }, [t]);

  useEffect(() => {
    // First load: refresh from Stripe if we were just redirected back from onboarding.
    const params = new URLSearchParams(window.location.search);
    const justReturned = params.get("connect") === "return" || params.get("connect") === "refresh";
    void loadStatus(justReturned);

    // Refresh-on-focus catches the case where the user finishes onboarding in a
    // separate tab — they switch back to ours and we re-fetch the status.
    const onFocus = () => { void loadStatus(true); };
    window.addEventListener("focus", onFocus);
    return () => window.removeEventListener("focus", onFocus);
  }, [loadStatus]);

  const handleStartOnboarding = async () => {
    setBusy(true);
    try {
      const { data } = await connectApi.startOnboarding();
      // Same-tab redirect — Stripe will bounce back to /dashboard?connect=return.
      window.location.href = data.onboardingUrl;
    } catch (err) {
      const message = axios.isAxiosError(err)
        ? (err.response?.data as { error?: string } | undefined)?.error || t("connect.onboardingError")
        : t("connect.onboardingError");
      toast.error(message);
      setBusy(false);
    }
  };

  const handleOpenDashboard = async () => {
    setBusy(true);
    try {
      const { data } = await connectApi.getDashboardLink();
      window.open(data.dashboardUrl, "_blank", "noopener,noreferrer");
    } catch {
      toast.error(t("connect.dashboardLinkError"));
    } finally {
      setBusy(false);
    }
  };

  if (loading) {
    return (
      <div className="bg-white dark:bg-[#2d2a42] rounded-2xl border border-lavender/30 dark:border-[#3a3655] p-8 text-center">
        <Loader2 className="h-5 w-5 animate-spin mx-auto text-primary" />
      </div>
    );
  }

  if (!statusDto) return null;

  const badge = STATUS_BADGE[statusDto.status];
  const BadgeIcon = badge.icon;
  const isVerified = statusDto.status === StripeConnectStatus.Verified;
  const isInProgress = statusDto.hasAccount && !isVerified;

  return (
    <div id="panel-payouts" role="tabpanel" aria-labelledby="tab-payouts">
      <div className="bg-white dark:bg-[#2d2a42] rounded-2xl border border-lavender/30 dark:border-[#3a3655] p-6 md:p-8">
        <div className="flex items-start justify-between gap-4 flex-wrap mb-5">
          <div>
            <h2 className="text-xl md:text-2xl font-bold text-primary dark:text-white mb-1">
              {t("connect.heading")}
            </h2>
            <p className="text-sm text-gray-500 dark:text-[#bdb9bc]">
              {t("connect.subtitle")}
            </p>
          </div>
          <span className={`inline-flex items-center gap-1.5 text-xs font-semibold px-3 py-1.5 rounded-full ${badge.cls}`}>
            <BadgeIcon className="h-3.5 w-3.5" />
            {t(badge.labelKey)}
          </span>
        </div>

        {/* Body — context-specific copy + CTA */}
        {statusDto.status === StripeConnectStatus.None && (
          <BodyBlock
            title={t("connect.none.title")}
            body={t("connect.none.body")}
            cta={t("connect.none.cta")}
            ctaIcon={ExternalLink}
            onClick={handleStartOnboarding}
            busy={busy}
          />
        )}

        {statusDto.status === StripeConnectStatus.Pending && (
          <BodyBlock
            title={t("connect.pending.title")}
            body={t("connect.pending.body")}
            cta={t("connect.pending.cta")}
            ctaIcon={ExternalLink}
            onClick={handleStartOnboarding}
            busy={busy}
          />
        )}

        {statusDto.status === StripeConnectStatus.Restricted && (
          <BodyBlock
            title={t("connect.restricted.title")}
            body={t("connect.restricted.body")}
            cta={t("connect.restricted.cta")}
            ctaIcon={ExternalLink}
            onClick={handleStartOnboarding}
            busy={busy}
            tone="danger"
          />
        )}

        {isVerified && (
          <BodyBlock
            title={t("connect.verified.title")}
            body={t("connect.verified.body")}
            cta={t("connect.verified.cta")}
            ctaIcon={ExternalLink}
            onClick={handleOpenDashboard}
            busy={busy}
            tone="success"
          />
        )}

        {/* Footer line — staleness display when verified, never for transient states */}
        {isVerified && statusDto.statusUpdatedAt && (
          <p className="mt-4 text-[11px] text-gray-400 dark:text-[#bdb9bc]/60">
            {t("connect.verifiedSince", {
              date: new Date(statusDto.statusUpdatedAt).toLocaleDateString(),
            })}
          </p>
        )}
        {isInProgress && (
          <p className="mt-4 text-[11px] text-gray-400 dark:text-[#bdb9bc]/60">
            {t("connect.refreshHint")}
          </p>
        )}
      </div>
    </div>
  );
}

interface BodyBlockProps {
  title: string;
  body: string;
  cta: string;
  ctaIcon: React.ElementType;
  onClick: () => void;
  busy: boolean;
  tone?: "default" | "success" | "danger";
}

function BodyBlock({ title, body, cta, ctaIcon: CtaIcon, onClick, busy, tone = "default" }: BodyBlockProps) {
  const btnCls =
    tone === "success"
      ? "bg-emerald-600 hover:bg-emerald-700"
      : tone === "danger"
      ? "bg-red-600 hover:bg-red-700"
      : "bg-primary hover:bg-primary/90";

  return (
    <div className="space-y-3">
      <p className="text-sm font-semibold text-gray-800 dark:text-white">{title}</p>
      <p className="text-sm text-gray-600 dark:text-[#bdb9bc] leading-relaxed">{body}</p>
      <button
        type="button"
        onClick={onClick}
        disabled={busy}
        className={`inline-flex items-center gap-2 px-4 py-2.5 rounded-xl ${btnCls} text-white text-sm font-semibold disabled:opacity-60 transition-colors mt-1`}
      >
        {busy ? <Loader2 className="h-4 w-4 animate-spin" /> : <CtaIcon className="h-4 w-4" />}
        {cta}
      </button>
    </div>
  );
}
