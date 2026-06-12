import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { motion } from "framer-motion";
import { CheckCircle2, ArrowRight, Loader2 } from "lucide-react";
import { usePageSEO } from "@/hooks/useSEO";
import { businessApi } from "@/api/businessApi";
import type { BusinessSubscriptionDto } from "@/types/business";

const POLL_INTERVAL_MS = 1500;
const POLL_MAX_ATTEMPTS = 12; // 18 seconds total — long enough for the webhook to land

/**
 * Lands after Stripe Checkout success-redirects the user back. Polls
 * `/business/subscription/me` until the webhook has reconciled the subscription,
 * then surfaces the next step (Dashboard / Create listing).
 */
export default function SubscriptionSuccessPage() {
  const { t } = useTranslation();
  const [subscription, setSubscription] = useState<BusinessSubscriptionDto | null>(null);
  const [pollAttempts, setPollAttempts] = useState(0);
  const [done, setDone] = useState(false);

  usePageSEO({
    title: t("business.subscription.successTitle") || "Subscription active",
    description: t("business.subscription.successBody") || "Your business subscription is now active.",
    index: false,
  });

  useEffect(() => {
    let cancelled = false;
    let attempts = 0;
    const tick = async () => {
      if (cancelled || attempts >= POLL_MAX_ATTEMPTS) {
        if (!cancelled) setDone(true);
        return;
      }
      attempts += 1;
      setPollAttempts(attempts);
      try {
        const sub = await businessApi.getMySubscription();
        if (cancelled) return;
        setSubscription(sub);
        if (sub.status === 1 || sub.status === 2) {
          setDone(true);
          return;
        }
      } catch {
        // 404 or transient — keep polling
      }
      setTimeout(tick, POLL_INTERVAL_MS);
    };
    void tick();
    return () => {
      cancelled = true;
    };
  }, []);

  return (
    <div className="max-w-xl mx-auto px-4 py-16 text-center">
      <motion.div
        initial={{ scale: 0.85, opacity: 0 }}
        animate={{ scale: 1, opacity: 1 }}
        transition={{ duration: 0.35 }}
        className="inline-flex w-20 h-20 rounded-2xl bg-gradient-to-br from-green-100 to-emerald-100 dark:from-green-500/20 dark:to-emerald-500/20 items-center justify-center mb-5"
      >
        <CheckCircle2 className="h-10 w-10 text-emerald-600" />
      </motion.div>

      <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
        {t("business.subscription.successHeading")}
      </h1>

      {!done ? (
        <>
          <p className="text-gray-500 dark:text-gray-400 mb-4">
            {t("business.subscription.pollingBody")}
          </p>
          <div className="inline-flex items-center gap-2 text-xs text-gray-400">
            <Loader2 size={12} className="animate-spin" />
            {t("business.subscription.pollingAttempt", {
              attempt: pollAttempts,
              max: POLL_MAX_ATTEMPTS,
            })}
          </div>
        </>
      ) : subscription ? (
        <>
          <p className="text-gray-500 dark:text-gray-400 mb-5">
            {subscription.status === 1
              ? t("business.subscription.trialActiveBody", {
                  date: subscription.trialEndsAt
                    ? new Date(subscription.trialEndsAt).toLocaleDateString()
                    : "—",
                })
              : t("business.subscription.activeBody", {
                  plan: subscription.planDisplayName,
                })}
          </p>
          <div className="flex flex-wrap items-center justify-center gap-3">
            <Link
              to="/business/listing/new"
              className="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl bg-primary text-white text-sm font-semibold hover:bg-primary/90 transition-colors"
            >
              {t("business.subscription.createListingCta")} <ArrowRight size={14} />
            </Link>
            <Link
              to="/business/dashboard"
              className="text-sm text-gray-500 hover:text-primary"
            >
              {t("business.subscription.dashboardLink")}
            </Link>
          </div>
        </>
      ) : (
        <>
          <p className="text-gray-500 dark:text-gray-400 mb-5">
            {t("business.subscription.pendingBody")}
          </p>
          <Link
            to="/business/dashboard"
            className="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl bg-primary text-white text-sm font-semibold hover:bg-primary/90 transition-colors"
          >
            {t("business.subscription.dashboardLink")} <ArrowRight size={14} />
          </Link>
        </>
      )}
    </div>
  );
}
