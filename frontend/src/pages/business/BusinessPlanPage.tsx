import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import axios from "axios";
import { motion } from "framer-motion";
import {
  Sparkles,
  Star,
  Check,
  Loader2,
  ArrowRight,
  ShieldCheck,
} from "lucide-react";
import { usePageSEO } from "@/hooks/useSEO";
import { businessApi } from "@/api/businessApi";
import type { SubscriptionPlanDto, BusinessSubscriptionDto } from "@/types/business";
import toast from "@/utils/toast";

const TIER_THEME: Record<string, { badge?: { icon: typeof Sparkles; label: string }; gradient: string }> = {
  Trial: { gradient: "from-gray-50 to-white dark:from-white/5 dark:to-transparent" },
  Basic: { gradient: "from-cream-dark/50 to-white dark:from-white/5 dark:to-transparent" },
  Featured: {
    badge: { icon: Sparkles, label: "popular" },
    gradient: "from-primary/10 to-mauve/10 dark:from-primary/15 dark:to-mauve/10",
  },
  Premium: {
    badge: { icon: Star, label: "best" },
    gradient: "from-mauve/20 to-primary/20 dark:from-mauve/15 dark:to-primary/20",
  },
};

interface PlanFeatures {
  badge?: string;
  photoLimit?: number;
  analytics?: boolean;
  prioritySupport?: boolean;
}

function parseFeatures(json: string | null): PlanFeatures {
  if (!json) return {};
  try {
    return JSON.parse(json) as PlanFeatures;
  } catch {
    return {};
  }
}

export default function BusinessPlanPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const [plans, setPlans] = useState<SubscriptionPlanDto[]>([]);
  const [currentSubscription, setCurrentSubscription] = useState<BusinessSubscriptionDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [busyPlan, setBusyPlan] = useState<string | null>(null);

  usePageSEO({
    title: t("business.plan.seoTitle") || "Pick your plan",
    description: t("business.plan.seoDescription") || "Choose a tier and start your 7-day trial.",
    index: false,
  });

  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      if (cancelled) return;
      setLoading(true);
      setError(null);
      try {
        const [plansResult, subResult] = await Promise.all([
          businessApi.getSubscriptionPlans(),
          businessApi.getMySubscription().catch((err) => {
            if (axios.isAxiosError(err) && err.response?.status === 404) return null;
            throw err;
          }),
        ]);
        if (cancelled) return;
        setPlans(plansResult);
        setCurrentSubscription(subResult);
      } catch {
        if (!cancelled) setError(t("business.plan.loadError"));
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    void load();
    return () => {
      cancelled = true;
    };
  }, [t]);

  const handleSelectPlan = async (plan: SubscriptionPlanDto) => {
    if (!plan.isCheckoutEnabled) {
      toast.error(t("business.plan.notPurchasable"));
      return;
    }
    setBusyPlan(plan.code);
    try {
      const origin = window.location.origin;
      const url = await businessApi.createSubscriptionCheckout(
        plan.code,
        `${origin}/business/subscription/success`,
        `${origin}/business/plan`,
      );
      window.location.assign(url);
    } catch (err) {
      const message =
        axios.isAxiosError(err) && err.response?.data
          ? (err.response.data as { error?: string }).error
          : null;
      toast.error(message ?? t("business.plan.checkoutError"));
      setBusyPlan(null);
    }
  };

  if (loading) {
    return (
      <div className="max-w-5xl mx-auto px-4 py-20 text-center text-gray-400">
        <Loader2 className="h-6 w-6 animate-spin mx-auto" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="max-w-5xl mx-auto px-4 py-20 text-center text-red-500">{error}</div>
    );
  }

  return (
    <div className="max-w-6xl mx-auto px-4 py-10">
      <div className="text-center mb-10">
        <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-primary/10 text-primary text-xs font-semibold mb-3">
          <ShieldCheck size={13} />
          {t("business.plan.trialBadge")}
        </div>
        <h1 className="text-3xl font-bold text-primary-dark dark:text-white mb-2">
          {t("business.plan.heading")}
        </h1>
        <p className="text-gray-500 dark:text-gray-400 max-w-xl mx-auto text-sm">
          {t("business.plan.subtitle")}
        </p>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {plans.map((plan, i) => {
          const theme = TIER_THEME[plan.code] ?? { gradient: "from-white to-white" };
          const features = parseFeatures(plan.featuresJson);
          const isCurrent = currentSubscription?.planCode === plan.code;
          const busy = busyPlan === plan.code;

          return (
            <motion.div
              key={plan.code}
              initial={{ opacity: 0, y: 14 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.35, delay: i * 0.05 }}
              className={`relative rounded-2xl border bg-gradient-to-br ${theme.gradient} ${
                isCurrent
                  ? "border-primary dark:border-primary/60"
                  : "border-gray-100 dark:border-white/10"
              } p-5 shadow-sm flex flex-col`}
            >
              {theme.badge && (
                <span className="absolute -top-2.5 left-1/2 -translate-x-1/2 inline-flex items-center gap-1 px-3 py-0.5 rounded-full bg-primary text-white text-[10px] font-semibold uppercase tracking-wide shadow">
                  <theme.badge.icon size={10} />
                  {t(`business.plan.badges.${theme.badge.label}`)}
                </span>
              )}

              <h3 className="text-lg font-bold text-gray-900 dark:text-white">
                {plan.displayName}
              </h3>
              <div className="mt-2 mb-4">
                {plan.monthlyPriceEur > 0 ? (
                  <>
                    <span className="text-3xl font-bold text-primary-dark dark:text-white">
                      €{plan.monthlyPriceEur.toFixed(2)}
                    </span>
                    <span className="text-xs text-gray-400 ml-1">
                      {t("business.plan.perMonth")}
                    </span>
                  </>
                ) : (
                  <span className="text-3xl font-bold text-primary-dark dark:text-white">
                    {t("business.plan.free")}
                  </span>
                )}
              </div>

              <ul className="space-y-2 text-xs text-gray-600 dark:text-gray-300 mb-5 flex-1">
                {plan.trialDays > 0 && (
                  <FeatureRow text={t("business.plan.features.trialDays", { days: plan.trialDays })} />
                )}
                {features.photoLimit != null && (
                  <FeatureRow
                    text={t("business.plan.features.photoLimit", { count: features.photoLimit })}
                  />
                )}
                {plan.rankBoost > 0 && (
                  <FeatureRow text={t("business.plan.features.topPlacement")} />
                )}
                {features.analytics && (
                  <FeatureRow text={t("business.plan.features.analytics")} />
                )}
                {features.prioritySupport && (
                  <FeatureRow text={t("business.plan.features.prioritySupport")} />
                )}
              </ul>

              <button
                type="button"
                onClick={() => handleSelectPlan(plan)}
                disabled={busy || isCurrent}
                className={`w-full inline-flex items-center justify-center gap-2 px-4 py-2.5 rounded-xl text-sm font-semibold transition-colors disabled:opacity-60 ${
                  isCurrent
                    ? "bg-gray-200 dark:bg-white/10 text-gray-500"
                    : "bg-primary text-white hover:bg-primary/90"
                }`}
              >
                {busy ? (
                  <Loader2 size={14} className="animate-spin" />
                ) : isCurrent ? (
                  t("business.plan.currentPlan")
                ) : !plan.isCheckoutEnabled ? (
                  t("business.plan.unavailable")
                ) : (
                  <>
                    {t("business.plan.selectButton")}
                    <ArrowRight size={14} />
                  </>
                )}
              </button>
            </motion.div>
          );
        })}
      </div>

      <p className="text-[11px] text-gray-400 text-center mt-8">
        {t("business.plan.smallPrint")}
      </p>

      <div className="mt-6 text-center">
        <button
          type="button"
          onClick={() => navigate("/business/dashboard")}
          className="text-sm text-gray-500 hover:text-primary"
        >
          {t("business.plan.skipForNow")}
        </button>
      </div>
    </div>
  );
}

function FeatureRow({ text }: { text: string }) {
  return (
    <li className="flex items-start gap-2">
      <Check size={13} className="text-primary flex-shrink-0 mt-0.5" />
      <span>{text}</span>
    </li>
  );
}
