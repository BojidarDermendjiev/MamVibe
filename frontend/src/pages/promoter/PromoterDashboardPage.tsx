import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { motion } from "framer-motion";
import axios from "axios";
import {
  Sparkles,
  Copy,
  CheckCheck,
  Share2,
  Users,
  TrendingUp,
  Megaphone,
  Loader2,
  ExternalLink,
} from "lucide-react";
import { usePageSEO } from "@/hooks/useSEO";
import { businessApi } from "@/api/businessApi";
import {
  ActivityType,
  CoachReferralStatus,
  type PromoterDashboardDto,
} from "@/types/business";
import toast from "@/utils/toast";

const ACTIVITY_LABEL_KEYS: Record<ActivityType, string> = {
  [ActivityType.Swimming]: "coaches.activityType.swimming",
  [ActivityType.MartialArts]: "coaches.activityType.martialArts",
  [ActivityType.Music]: "coaches.activityType.music",
  [ActivityType.Dance]: "coaches.activityType.dance",
  [ActivityType.Gymnastics]: "coaches.activityType.gymnastics",
  [ActivityType.ArtAndCrafts]: "coaches.activityType.artAndCrafts",
  [ActivityType.EarlyDevelopment]: "coaches.activityType.earlyDevelopment",
  [ActivityType.LanguageClasses]: "coaches.activityType.languageClasses",
  [ActivityType.SportsTeam]: "coaches.activityType.sportsTeam",
  [ActivityType.Other]: "coaches.activityType.other",
};

const STATUS_COLOR: Record<CoachReferralStatus, string> = {
  [CoachReferralStatus.Submitted]: "bg-gray-100 text-gray-700 dark:bg-white/10 dark:text-gray-300",
  [CoachReferralStatus.Contacted]: "bg-amber-100 text-amber-700 dark:bg-amber-500/20 dark:text-amber-300",
  [CoachReferralStatus.Onboarded]: "bg-emerald-100 text-emerald-700 dark:bg-emerald-500/20 dark:text-emerald-300",
  [CoachReferralStatus.Rejected]: "bg-red-100 text-red-700 dark:bg-red-500/20 dark:text-red-300",
};

const STATUS_KEY: Record<CoachReferralStatus, string> = {
  [CoachReferralStatus.Submitted]: "promoter.status.submitted",
  [CoachReferralStatus.Contacted]: "promoter.status.contacted",
  [CoachReferralStatus.Onboarded]: "promoter.status.onboarded",
  [CoachReferralStatus.Rejected]: "promoter.status.rejected",
};

/**
 * Promoter home page. Surfaces the referral code with a copy/share affordance plus
 * counters and the 10 most-recent referrals. When the user is not a promoter yet,
 * renders a one-click "Become a promoter" CTA that calls <c>POST /api/v1/promoter</c>.
 */
export default function PromoterDashboardPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const [dashboard, setDashboard] = useState<PromoterDashboardDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [creating, setCreating] = useState(false);
  const [notRegistered, setNotRegistered] = useState(false);
  const [copied, setCopied] = useState(false);

  usePageSEO({
    title: t("promoter.seoTitle") || "Promoter dashboard",
    description: t("promoter.seoDescription") || "Track the coaches and venues you've recommended to MamVibe.",
    index: false,
  });

  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      if (cancelled) return;
      setLoading(true);
      try {
        const result = await businessApi.getPromoterDashboard();
        if (!cancelled) {
          setDashboard(result);
          setNotRegistered(false);
        }
      } catch (err) {
        if (cancelled) return;
        if (axios.isAxiosError(err)) {
          if (err.response?.status === 401) {
            navigate("/partner/login?next=/promoter/dashboard");
            return;
          }
          if (err.response?.status === 404) {
            setNotRegistered(true);
          }
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    void load();
    return () => {
      cancelled = true;
    };
  }, [navigate]);

  const handleBecomePromoter = async () => {
    setCreating(true);
    try {
      await businessApi.createPromoterProfile();
      const result = await businessApi.getPromoterDashboard();
      setDashboard(result);
      setNotRegistered(false);
      toast.success(t("promoter.welcomeToast"));
    } catch {
      toast.error(t("promoter.becomeError"));
    } finally {
      setCreating(false);
    }
  };

  const referralLink = dashboard
    ? `${window.location.origin}/coaches/recommend?ref=${dashboard.profile.referralCode}`
    : "";

  const handleCopy = async () => {
    if (!referralLink) return;
    try {
      await navigator.clipboard.writeText(referralLink);
      setCopied(true);
      setTimeout(() => setCopied(false), 1800);
    } catch {
      toast.error(t("promoter.copyError"));
    }
  };

  const handleShare = async () => {
    if (!referralLink) return;
    if (navigator.share) {
      try {
        await navigator.share({
          title: t("promoter.shareTitle") || "Recommend a coach on MamVibe",
          text: t("promoter.shareText") || "Help families find this coach on MamVibe",
          url: referralLink,
        });
      } catch {
        // user canceled — no-op
      }
    } else {
      await handleCopy();
    }
  };

  if (loading) {
    return (
      <div className="max-w-3xl mx-auto px-4 py-20 text-center text-gray-400">
        <Loader2 className="h-6 w-6 animate-spin mx-auto" />
      </div>
    );
  }

  if (notRegistered || !dashboard) {
    return (
      <div className="max-w-2xl mx-auto px-4 py-12">
        <motion.div
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.35 }}
          className="bg-white dark:bg-[#2d2a42] border border-lavender/30 dark:border-white/10 rounded-2xl shadow-sm p-8 text-center"
        >
          <div className="inline-flex w-14 h-14 rounded-2xl bg-gradient-to-br from-primary/20 to-mauve/20 items-center justify-center mb-4">
            <Megaphone className="h-7 w-7 text-primary" />
          </div>
          <h1 className="text-2xl font-bold text-primary-dark dark:text-white mb-2">
            {t("promoter.becomeHeading")}
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 max-w-md mx-auto mb-6">
            {t("promoter.becomeBody")}
          </p>
          <button
            type="button"
            onClick={handleBecomePromoter}
            disabled={creating}
            className="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl bg-primary text-white text-sm font-semibold hover:bg-primary/90 disabled:opacity-60 transition-colors"
          >
            {creating ? (
              <Loader2 size={15} className="animate-spin" />
            ) : (
              <>
                <Sparkles size={15} />
                {t("promoter.becomeCta")}
              </>
            )}
          </button>
        </motion.div>
      </div>
    );
  }

  return (
    <div className="max-w-5xl mx-auto px-4 py-8">
      <motion.div
        initial={{ opacity: 0, y: 8 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.3 }}
      >
        <h1 className="text-2xl sm:text-3xl font-bold text-primary-dark dark:text-white mb-1">
          {t("promoter.heading")}
        </h1>
        <p className="text-sm text-gray-500 dark:text-gray-400">
          {t("promoter.subtitle")}
        </p>
      </motion.div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-5 mt-6">
        {/* Referral code + share card */}
        <div className="lg:col-span-2 bg-white dark:bg-[#2d2a42] border border-lavender/30 dark:border-white/10 rounded-2xl shadow-sm p-5">
          <h3 className="text-sm font-semibold text-gray-900 dark:text-white mb-3 flex items-center gap-2">
            <Sparkles size={16} className="text-primary" />
            {t("promoter.codeHeading")}
          </h3>
          <div className="rounded-xl bg-gradient-to-r from-primary/10 to-mauve/10 dark:from-primary/15 dark:to-mauve/10 p-4 flex items-center justify-between gap-3 flex-wrap">
            <code className="text-xl font-bold tracking-wider text-primary-dark dark:text-white">
              {dashboard.profile.referralCode}
            </code>
            <div className="flex items-center gap-2">
              <button
                type="button"
                onClick={handleCopy}
                className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-white dark:bg-white/10 border border-gray-200 dark:border-white/10 text-xs font-medium text-gray-700 dark:text-gray-200 hover:bg-gray-50 dark:hover:bg-white/15 transition-colors"
              >
                {copied ? <CheckCheck size={13} /> : <Copy size={13} />}
                {copied ? t("promoter.copied") : t("promoter.copyLink")}
              </button>
              <button
                type="button"
                onClick={handleShare}
                className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-primary text-white text-xs font-medium hover:bg-primary/90 transition-colors"
              >
                <Share2 size={13} />
                {t("promoter.share")}
              </button>
            </div>
          </div>
          <p className="text-xs text-gray-500 dark:text-gray-400 mt-3">
            {t("promoter.codeIntro")}
          </p>
          <Link
            to={`/coaches/recommend?ref=${dashboard.profile.referralCode}`}
            target="_blank"
            rel="noopener noreferrer"
            className="mt-3 inline-flex items-center gap-1.5 text-xs text-primary hover:underline"
          >
            {t("promoter.previewForm")} <ExternalLink size={11} />
          </Link>
        </div>

        {/* Counters card */}
        <div className="bg-white dark:bg-[#2d2a42] border border-lavender/30 dark:border-white/10 rounded-2xl shadow-sm p-5">
          <h3 className="text-sm font-semibold text-gray-900 dark:text-white mb-3 flex items-center gap-2">
            <TrendingUp size={16} className="text-primary" />
            {t("promoter.statsHeading")}
          </h3>
          <div className="grid grid-cols-2 gap-3">
            <Mini value={dashboard.totalSubmitted} label={t("promoter.status.submitted")} />
            <Mini value={dashboard.totalContacted} label={t("promoter.status.contacted")} />
            <Mini value={dashboard.totalOnboarded} label={t("promoter.status.onboarded")} accent />
            <Mini value={dashboard.totalRejected} label={t("promoter.status.rejected")} />
          </div>
          <p className="text-[10px] text-gray-400 mt-3 text-center">
            {t("promoter.rewardsTeaser")}
          </p>
        </div>

        {/* Recent referrals — full width */}
        <div className="lg:col-span-3 bg-white dark:bg-[#2d2a42] border border-lavender/30 dark:border-white/10 rounded-2xl shadow-sm p-5">
          <h3 className="text-sm font-semibold text-gray-900 dark:text-white mb-3 flex items-center gap-2">
            <Users size={16} className="text-primary" />
            {t("promoter.recentHeading")}
          </h3>
          {dashboard.recent.length === 0 ? (
            <p className="text-sm text-gray-400 text-center py-6">
              {t("promoter.recentEmpty")}
            </p>
          ) : (
            <ul className="divide-y divide-gray-100 dark:divide-white/10">
              {dashboard.recent.map((r) => (
                <li key={r.id} className="py-2.5 flex items-center justify-between gap-3">
                  <div className="min-w-0 flex-1">
                    <p className="text-sm font-medium text-gray-900 dark:text-white truncate">
                      {r.businessName}
                    </p>
                    <p className="text-xs text-gray-400 truncate">
                      {r.city} · {t(ACTIVITY_LABEL_KEYS[r.activityType])} ·{" "}
                      {new Date(r.createdAt).toLocaleDateString()}
                    </p>
                  </div>
                  <span
                    className={`text-[10px] uppercase tracking-wide font-semibold px-2 py-0.5 rounded-full ${STATUS_COLOR[r.status]}`}
                  >
                    {t(STATUS_KEY[r.status])}
                  </span>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </div>
  );
}

function Mini({ value, label, accent }: { value: number; label: string; accent?: boolean }) {
  return (
    <div
      className={`rounded-lg py-3 text-center ${
        accent ? "bg-primary/10 text-primary" : "bg-cream-dark/40 dark:bg-white/[0.04]"
      }`}
    >
      <p className={`text-2xl font-bold ${accent ? "text-primary" : "text-gray-900 dark:text-white"}`}>
        {value}
      </p>
      <p className="text-[10px] uppercase tracking-wide text-gray-400">{label}</p>
    </div>
  );
}
