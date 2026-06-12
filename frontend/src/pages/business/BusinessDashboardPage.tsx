import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { motion } from "framer-motion";
import axios from "axios";
import {
  Sparkles,
  ListPlus,
  ExternalLink,
  Loader2,
  Pencil,
  Eye,
  Heart,
  MessageCircle,
  CreditCard,
  AlertTriangle,
  ShieldCheck,
} from "lucide-react";
import { usePageSEO } from "@/hooks/useSEO";
import { businessApi } from "@/api/businessApi";
import {
  BusinessSubscriptionStatus,
  type BusinessListingDto,
  type BusinessProfileDto,
  type BusinessSubscriptionDto,
} from "@/types/business";
import { useBusinessHub } from "@/contexts/BusinessHubContext";
import toast from "@/utils/toast";

/**
 * Business owner home — surfaces profile, subscription state, and listing status with
 * the next-step CTA (pick a plan, create a listing, manage billing).
 */
export default function BusinessDashboardPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const [profile, setProfile] = useState<BusinessProfileDto | null>(null);
  const [subscription, setSubscription] = useState<BusinessSubscriptionDto | null>(null);
  const [listing, setListing] = useState<BusinessListingDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [livePulse, setLivePulse] = useState(false);

  const hub = useBusinessHub();

  // Live counter updates from the BusinessHub — bumps the relevant counter and flashes
  // a brief pulse animation so the owner can see the change land in real time.
  useEffect(() => {
    if (!listing) return;
    const unsubscribeViewed = hub.onListingViewed((delta) => {
      if (delta.listingId !== listing.id) return;
      setListing((prev) => (prev ? { ...prev, viewCount: delta.newViewCount } : prev));
      setLivePulse(true);
      setTimeout(() => setLivePulse(false), 700);
    });
    const unsubscribeLiked = hub.onListingLiked((delta) => {
      if (delta.listingId !== listing.id) return;
      setListing((prev) => (prev ? { ...prev, likeCount: delta.newLikeCount } : prev));
      setLivePulse(true);
      setTimeout(() => setLivePulse(false), 700);
    });
    const unsubscribeCommented = hub.onListingCommented((broadcast) => {
      if (broadcast.listingId !== listing.id) return;
      setListing((prev) =>
        prev
          ? {
              ...prev,
              commentCount: Math.max(0, prev.commentCount + (broadcast.deleted ? -1 : 1)),
            }
          : prev,
      );
      setLivePulse(true);
      setTimeout(() => setLivePulse(false), 700);
    });
    const unsubscribeSub = hub.onSubscriptionStatusChanged((broadcast) => {
      setSubscription((prev) =>
        prev && prev.businessProfileId === broadcast.businessProfileId
          ? {
              ...prev,
              status: broadcast.status as BusinessSubscriptionStatus,
              planCode: broadcast.planCode,
            }
          : prev,
      );
    });
    return () => {
      unsubscribeViewed();
      unsubscribeLiked();
      unsubscribeCommented();
      unsubscribeSub();
    };
  }, [hub, listing]);

  usePageSEO({
    title: t("business.dashboard.seoTitle") || "Business dashboard",
    description: t("business.dashboard.seoDescription") || "Manage your MamVibe business profile, listing and subscription.",
    index: false,
  });

  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      if (cancelled) return;
      setLoading(true);
      try {
        const [profileRes, subRes, listingRes] = await Promise.all([
          businessApi.getMyProfile().catch((err) => {
            if (axios.isAxiosError(err) && err.response?.status === 404) return null;
            throw err;
          }),
          businessApi.getMySubscription().catch((err) => {
            if (axios.isAxiosError(err) && err.response?.status === 404) return null;
            throw err;
          }),
          businessApi.getMyListing().catch((err) => {
            if (axios.isAxiosError(err) && err.response?.status === 404) return null;
            throw err;
          }),
        ]);
        if (cancelled) return;
        if (!profileRes) {
          navigate("/business/register", { replace: true });
          return;
        }
        setProfile(profileRes);
        setSubscription(subRes);
        setListing(listingRes);
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    void load();
    return () => {
      cancelled = true;
    };
  }, [navigate]);

  const openBillingPortal = async () => {
    try {
      const url = await businessApi.createBillingPortalUrl(window.location.href);
      window.location.assign(url);
    } catch {
      toast.error(t("business.dashboard.portalError") || "Could not open billing portal.");
    }
  };

  if (loading) {
    return (
      <div className="max-w-5xl mx-auto px-4 py-20 text-center text-gray-400">
        <Loader2 className="h-6 w-6 animate-spin mx-auto" />
      </div>
    );
  }
  if (!profile) return null;

  return (
    <div className="max-w-5xl mx-auto px-4 py-8">
      <motion.div initial={{ opacity: 0, y: 8 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.3 }}>
        <div className="flex items-center gap-3 mb-1">
          <h1 className="text-2xl sm:text-3xl font-bold text-primary-dark dark:text-white">
            {t("business.dashboard.heading", { name: profile.displayName })}
          </h1>
          {hub.isConnected && (
            <span
              className={`inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[10px] font-semibold uppercase tracking-wide transition-colors ${
                livePulse
                  ? "bg-primary text-white"
                  : "bg-emerald-100 text-emerald-700 dark:bg-emerald-500/20 dark:text-emerald-300"
              }`}
              title={t("business.dashboard.liveTitle") || "Live updates connected"}
            >
              <span
                className={`w-1.5 h-1.5 rounded-full ${
                  livePulse ? "bg-white animate-ping" : "bg-emerald-500"
                }`}
              />
              {t("business.dashboard.liveBadge")}
            </span>
          )}
        </div>
        <p className="text-sm text-gray-500 dark:text-gray-400">
          {profile.category === 0
            ? t("business.dashboard.subtitleCoach")
            : t("business.dashboard.subtitleVenue")}
        </p>
      </motion.div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-5 mt-6">
        <SubscriptionCard
          subscription={subscription}
          onOpenPortal={openBillingPortal}
        />
        <ListingCard listing={listing} />
        <ProfileCard profile={profile} />
      </div>
    </div>
  );
}

interface SubCardProps {
  subscription: BusinessSubscriptionDto | null;
  onOpenPortal: () => void;
}
function SubscriptionCard({ subscription, onOpenPortal }: SubCardProps) {
  const { t } = useTranslation();
  if (!subscription) {
    return (
      <div className="bg-white dark:bg-[#2d2a42] rounded-2xl border border-lavender/30 dark:border-white/10 p-5 lg:col-span-1">
        <h3 className="text-sm font-semibold text-gray-900 dark:text-white mb-2 flex items-center gap-2">
          <ShieldCheck size={16} className="text-primary" />
          {t("business.dashboard.subscriptionHeading")}
        </h3>
        <p className="text-xs text-gray-500 dark:text-gray-400 mb-4">
          {t("business.dashboard.subscriptionEmpty")}
        </p>
        <Link
          to="/business/plan"
          className="inline-flex items-center gap-2 px-3.5 py-2 rounded-xl bg-primary text-white text-xs font-semibold hover:bg-primary/90 transition-colors"
        >
          <Sparkles size={13} />
          {t("business.dashboard.choosePlanCta")}
        </Link>
      </div>
    );
  }

  const statusColor =
    subscription.status === BusinessSubscriptionStatus.Active
      ? "text-emerald-600 dark:text-emerald-400"
      : subscription.status === BusinessSubscriptionStatus.Trialing
        ? "text-primary"
        : subscription.status === BusinessSubscriptionStatus.PastDue
          ? "text-amber-600"
          : "text-gray-500";

  return (
    <div className="bg-white dark:bg-[#2d2a42] rounded-2xl border border-lavender/30 dark:border-white/10 p-5">
      <h3 className="text-sm font-semibold text-gray-900 dark:text-white mb-3 flex items-center gap-2">
        <ShieldCheck size={16} className="text-primary" />
        {t("business.dashboard.subscriptionHeading")}
      </h3>
      <p className="text-2xl font-bold text-primary-dark dark:text-white">
        {subscription.planDisplayName}
      </p>
      <p className={`text-xs font-semibold uppercase tracking-wide mt-0.5 ${statusColor}`}>
        {t(`business.dashboard.status.${subscription.status}`)}
      </p>
      {subscription.status === BusinessSubscriptionStatus.PastDue && (
        <p className="mt-3 px-3 py-2 rounded-lg bg-amber-50 dark:bg-amber-500/10 text-xs text-amber-700 dark:text-amber-300 flex items-start gap-1.5">
          <AlertTriangle size={13} className="mt-0.5 flex-shrink-0" />
          {t("business.dashboard.pastDueWarning", {
            date: subscription.gracePeriodEndsAt
              ? new Date(subscription.gracePeriodEndsAt).toLocaleDateString()
              : "—",
          })}
        </p>
      )}
      {subscription.status === BusinessSubscriptionStatus.Trialing && subscription.trialEndsAt && (
        <p className="text-xs text-gray-500 dark:text-gray-400 mt-3">
          {t("business.dashboard.trialEnds", {
            date: new Date(subscription.trialEndsAt).toLocaleDateString(),
          })}
        </p>
      )}

      <div className="mt-4 flex flex-col gap-2">
        {subscription.hasStripeSubscription && (
          <button
            type="button"
            onClick={onOpenPortal}
            className="inline-flex items-center justify-between px-3.5 py-2 rounded-xl border border-gray-200 dark:border-white/10 text-xs font-medium text-gray-700 dark:text-gray-200 hover:bg-gray-50 dark:hover:bg-white/5 transition-colors"
          >
            <span className="inline-flex items-center gap-2">
              <CreditCard size={13} />
              {t("business.dashboard.manageBilling")}
            </span>
            <ExternalLink size={11} />
          </button>
        )}
        <Link
          to="/business/plan"
          className="inline-flex items-center justify-center gap-2 px-3.5 py-2 rounded-xl bg-cream-dark dark:bg-white/5 text-xs font-medium text-gray-700 dark:text-gray-200 hover:bg-cream-dark/80 transition-colors"
        >
          {t("business.dashboard.changePlan")}
        </Link>
      </div>
    </div>
  );
}

function ListingCard({ listing }: { listing: BusinessListingDto | null }) {
  const { t } = useTranslation();
  if (!listing) {
    return (
      <div className="bg-white dark:bg-[#2d2a42] rounded-2xl border border-lavender/30 dark:border-white/10 p-5">
        <h3 className="text-sm font-semibold text-gray-900 dark:text-white mb-2 flex items-center gap-2">
          <ListPlus size={16} className="text-primary" />
          {t("business.dashboard.listingHeading")}
        </h3>
        <p className="text-xs text-gray-500 dark:text-gray-400 mb-4">
          {t("business.dashboard.listingEmpty")}
        </p>
        <Link
          to="/business/listing/new"
          className="inline-flex items-center gap-2 px-3.5 py-2 rounded-xl bg-primary text-white text-xs font-semibold hover:bg-primary/90 transition-colors"
        >
          <ListPlus size={13} />
          {t("business.dashboard.createListingCta")}
        </Link>
      </div>
    );
  }

  return (
    <div className="bg-white dark:bg-[#2d2a42] rounded-2xl border border-lavender/30 dark:border-white/10 p-5">
      <h3 className="text-sm font-semibold text-gray-900 dark:text-white mb-3 flex items-center gap-2">
        <ListPlus size={16} className="text-primary" />
        {t("business.dashboard.listingHeading")}
      </h3>
      <p className="text-base font-semibold text-gray-900 dark:text-white mb-1 line-clamp-1">
        {listing.title}
      </p>
      <p className="text-xs text-gray-500 dark:text-gray-400 mb-3">
        {listing.isApproved
          ? t("business.dashboard.listingApproved")
          : t("business.dashboard.listingPending")}
      </p>
      <div className="grid grid-cols-3 gap-2 text-center">
        <Mini icon={Eye} value={listing.viewCount} />
        <Mini icon={Heart} value={listing.likeCount} />
        <Mini icon={MessageCircle} value={listing.commentCount} />
      </div>
      <div className="mt-4 flex flex-col gap-2">
        <Link
          to={`/coaches/${listing.id}`}
          className="inline-flex items-center justify-center gap-2 px-3.5 py-2 rounded-xl bg-cream-dark dark:bg-white/5 text-xs font-medium text-gray-700 dark:text-gray-200 hover:bg-cream-dark/80 transition-colors"
        >
          <Eye size={13} />
          {t("business.dashboard.viewPublic")}
        </Link>
        <Link
          to="/business/listing/edit"
          className="inline-flex items-center justify-center gap-2 px-3.5 py-2 rounded-xl bg-primary/10 text-primary text-xs font-semibold hover:bg-primary/15 transition-colors"
        >
          <Pencil size={13} />
          {t("business.dashboard.editListing")}
        </Link>
      </div>
    </div>
  );
}

function Mini({ icon: Icon, value }: { icon: React.ComponentType<{ size?: number; className?: string }>; value: number }) {
  return (
    <div className="rounded-lg bg-cream-dark/40 dark:bg-white/[0.04] py-2">
      <Icon size={13} className="mx-auto text-primary mb-0.5" />
      <p className="text-sm font-bold text-gray-900 dark:text-white">{value}</p>
    </div>
  );
}

function ProfileCard({ profile }: { profile: BusinessProfileDto }) {
  const { t } = useTranslation();
  return (
    <div className="bg-white dark:bg-[#2d2a42] rounded-2xl border border-lavender/30 dark:border-white/10 p-5">
      <h3 className="text-sm font-semibold text-gray-900 dark:text-white mb-3">
        {t("business.dashboard.profileHeading")}
      </h3>
      <dl className="space-y-2 text-xs text-gray-600 dark:text-gray-300">
        <Row label={t("business.dashboard.profileLegalName")} value={profile.legalName} />
        <Row label={t("business.dashboard.profileCity")} value={profile.city} />
        <Row label={t("business.dashboard.profileEmail")} value={profile.contactEmail} />
        {profile.contactPhone && (
          <Row label={t("business.dashboard.profilePhone")} value={profile.contactPhone} />
        )}
      </dl>
      {profile.policyReacceptanceRequired && (
        <p className="mt-3 px-3 py-2 rounded-lg bg-amber-50 dark:bg-amber-500/10 text-xs text-amber-700 dark:text-amber-300">
          {t("business.dashboard.policyUpdate")}
        </p>
      )}
    </div>
  );
}

function Row({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <dt className="text-[10px] uppercase tracking-wide text-gray-400 font-semibold">{label}</dt>
      <dd className="text-gray-700 dark:text-gray-200 truncate">{value}</dd>
    </div>
  );
}
