import { lazy, Suspense, useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { motion } from "framer-motion";
import {
  MapPin,
  Mail,
  Phone,
  Globe,
  CalendarClock,
  ArrowLeft,
  Sparkles,
  Star,
  Heart,
  MessageCircle,
  Eye,
  Loader2,
  Flag,
  Send,
  Trash2,
  EyeOff,
} from "lucide-react";
import axios from "axios";
import { usePageSEO } from "@/hooks/useSEO";
import { businessApi } from "@/api/businessApi";
import { useAuthStore } from "@/store/authStore";
import ReportListingModal from "@/components/business/ReportListingModal";
import Avatar from "@/components/common/Avatar";
import {
  ActivityType,
  type BusinessListingCommentDto,
  type BusinessListingDto,
} from "@/types/business";
import toast from "@/utils/toast";

const ListingMap = lazy(() => import("@/components/business/ListingMap"));

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

export default function CoachDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { isAuthenticated, user } = useAuthStore();
  const isAdmin = user?.roles?.includes("Admin") ?? false;

  const [listing, setListing] = useState<BusinessListingDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [coverIndex, setCoverIndex] = useState(0);

  // Like state
  const [likeBusy, setLikeBusy] = useState(false);

  // Comments state
  const [comments, setComments] = useState<BusinessListingCommentDto[]>([]);
  const [commentsLoading, setCommentsLoading] = useState(false);
  const [commentBody, setCommentBody] = useState("");
  const [posting, setPosting] = useState(false);

  // Report state
  const [reportOpen, setReportOpen] = useState(false);

  usePageSEO({
    title: listing ? `${listing.title} — MamVibe` : "Coach detail",
    description: listing?.description.slice(0, 160) ?? "Coach and activity listing on MamVibe.",
    canonical: id ? `https://mamvibe.com/coaches/${id}` : undefined,
    index: !!listing?.isApproved,
  });

  useEffect(() => {
    if (!id) return;
    let cancelled = false;
    const load = async () => {
      if (cancelled) return;
      setLoading(true);
      setError(null);
      try {
        const result = await businessApi.getListingById(id);
        if (!cancelled) setListing(result);
        // Fire-and-forget view tracking — anonymous-safe, no error surfacing.
        businessApi.trackListingView(id).catch(() => {});
      } catch {
        if (!cancelled) setError(t("coaches.detail.loadError"));
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    void load();
    return () => {
      cancelled = true;
    };
  }, [id, t]);

  useEffect(() => {
    if (!id) return;
    let cancelled = false;
    const load = async () => {
      if (cancelled) return;
      setCommentsLoading(true);
      try {
        const result = await businessApi.getListingComments(id, 1, 20);
        if (!cancelled) setComments(result.items);
      } catch {
        // Comments are best-effort; leave the list empty.
      } finally {
        if (!cancelled) setCommentsLoading(false);
      }
    };
    void load();
    return () => {
      cancelled = true;
    };
  }, [id]);

  const handleToggleLike = async () => {
    if (!listing) return;
    if (!isAuthenticated) {
      navigate(`/login?next=/coaches/${listing.id}`);
      return;
    }
    setLikeBusy(true);
    try {
      const state = listing.isLikedByCurrentUser
        ? await businessApi.unlikeListing(listing.id)
        : await businessApi.likeListing(listing.id);
      setListing({
        ...listing,
        isLikedByCurrentUser: state.isLiked,
        likeCount: state.likeCount,
      });
    } catch {
      toast.error(t("coaches.detail.likeError") || "Could not update like.");
    } finally {
      setLikeBusy(false);
    }
  };

  const handlePostComment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!listing) return;
    if (!isAuthenticated) {
      navigate(`/login?next=/coaches/${listing.id}`);
      return;
    }
    const body = commentBody.trim();
    if (body.length === 0) return;
    setPosting(true);
    try {
      const created = await businessApi.postListingComment(listing.id, { body });
      setComments((prev) => [created, ...prev]);
      setListing({ ...listing, commentCount: listing.commentCount + 1 });
      setCommentBody("");
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data) {
        toast.error(
          (err.response.data as { error?: string }).error ??
            t("coaches.detail.commentError") ??
            "Could not post comment.",
        );
      } else {
        toast.error(t("coaches.detail.commentError") || "Could not post comment.");
      }
    } finally {
      setPosting(false);
    }
  };

  const handleDeleteComment = async (commentId: string) => {
    if (!listing) return;
    if (!window.confirm(t("coaches.detail.confirmDeleteComment"))) return;
    try {
      await businessApi.deleteListingComment(listing.id, commentId);
      setComments((prev) => prev.filter((c) => c.id !== commentId));
      setListing((prev) =>
        prev ? { ...prev, commentCount: Math.max(0, prev.commentCount - 1) } : prev,
      );
    } catch {
      toast.error(t("coaches.detail.commentDeleteError") || "Could not delete comment.");
    }
  };

  if (loading) {
    return (
      <div className="max-w-4xl mx-auto px-4 py-20 text-center text-gray-400">
        <Loader2 className="h-6 w-6 animate-spin mx-auto" />
      </div>
    );
  }

  if (error || !listing) {
    return (
      <div className="max-w-4xl mx-auto px-4 py-20 text-center">
        <p className="text-gray-500 dark:text-gray-400 mb-4">
          {error ?? t("coaches.detail.notFound")}
        </p>
        <Link to="/coaches" className="text-primary hover:underline text-sm">
          {t("coaches.detail.backToBrowse")}
        </Link>
      </div>
    );
  }

  const cover = listing.photos[coverIndex] ?? listing.photos[0];
  const tierBadge =
    listing.rankBoost >= 100
      ? { label: t("coaches.badge.premium"), icon: Star, className: "bg-gradient-to-r from-mauve to-primary text-white" }
      : listing.rankBoost > 0
        ? { label: t("coaches.badge.featured"), icon: Sparkles, className: "bg-gradient-to-r from-primary/15 to-mauve/15 text-primary" }
        : null;

  return (
    <div className="max-w-5xl mx-auto px-4 py-8">
      <ReportListingModal
        isOpen={reportOpen}
        listingId={listing.id}
        onClose={() => setReportOpen(false)}
        onSuccess={() => toast.success(t("business.report.successToast") || "Report submitted")}
      />

      <Link
        to="/coaches"
        className="inline-flex items-center gap-1.5 text-sm text-gray-500 dark:text-gray-400 hover:text-primary mb-5"
      >
        <ArrowLeft size={14} />
        {t("coaches.detail.backToBrowse")}
      </Link>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Main column */}
        <div className="lg:col-span-2 space-y-5">
          {/* Cover photo */}
          {cover ? (
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              transition={{ duration: 0.4 }}
              className="rounded-2xl overflow-hidden border border-lavender/30 dark:border-white/10 bg-gray-100 dark:bg-white/5"
            >
              <img src={cover.url} alt={listing.title} className="w-full h-72 object-cover" />
            </motion.div>
          ) : (
            <div className="rounded-2xl bg-gradient-to-br from-primary/10 to-mauve/10 h-56 flex items-center justify-center">
              <Sparkles className="w-12 h-12 text-primary/30" />
            </div>
          )}

          {listing.photos.length > 1 && (
            <div className="flex gap-2 overflow-x-auto pb-1">
              {listing.photos.map((photo, i) => (
                <button
                  key={photo.id}
                  type="button"
                  onClick={() => setCoverIndex(i)}
                  className={`flex-shrink-0 w-20 h-16 rounded-lg overflow-hidden border-2 ${
                    i === coverIndex ? "border-primary" : "border-transparent"
                  }`}
                >
                  <img src={photo.url} alt="" className="w-full h-full object-cover" />
                </button>
              ))}
            </div>
          )}

          {/* Heading + meta + like/report row */}
          <div>
            <div className="flex items-start justify-between gap-3 mb-2">
              <div>
                <h1 className="text-2xl sm:text-3xl font-bold text-primary-dark dark:text-white">
                  {listing.title}
                </h1>
                <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                  {listing.businessDisplayName}
                </p>
              </div>
              {tierBadge && (
                <span
                  className={`inline-flex items-center gap-1 text-xs px-2.5 py-1 rounded-full font-semibold ${tierBadge.className}`}
                >
                  <tierBadge.icon size={12} />
                  {tierBadge.label}
                </span>
              )}
            </div>
            <div className="flex flex-wrap items-center gap-x-3 gap-y-1 text-xs text-gray-500 dark:text-gray-400">
              <span className="inline-flex items-center gap-1">
                <MapPin size={12} />
                {listing.city}
              </span>
              <span className="inline-block w-1 h-1 rounded-full bg-gray-300" />
              <span>{t(ACTIVITY_LABEL_KEYS[listing.activityType])}</span>
              {(listing.ageFromMonths != null || listing.ageToMonths != null) && (
                <>
                  <span className="inline-block w-1 h-1 rounded-full bg-gray-300" />
                  <span>{formatAge(listing.ageFromMonths, listing.ageToMonths, t)}</span>
                </>
              )}
            </div>

            {/* Like + Report */}
            <div className="mt-4 flex flex-wrap items-center gap-2">
              <button
                type="button"
                onClick={handleToggleLike}
                disabled={likeBusy}
                className={`inline-flex items-center gap-1.5 px-3.5 py-1.5 rounded-full text-sm font-medium transition-colors disabled:opacity-50 ${
                  listing.isLikedByCurrentUser
                    ? "bg-red-50 text-red-600 dark:bg-red-500/20 dark:text-red-300"
                    : "bg-gray-100 text-gray-700 dark:bg-white/10 dark:text-gray-200 hover:bg-gray-200"
                }`}
              >
                <Heart
                  size={14}
                  className={listing.isLikedByCurrentUser ? "fill-current" : ""}
                />
                {listing.likeCount}
              </button>
              <button
                type="button"
                onClick={() => {
                  if (!isAuthenticated) {
                    navigate(`/login?next=/coaches/${listing.id}`);
                    return;
                  }
                  setReportOpen(true);
                }}
                className="inline-flex items-center gap-1.5 px-3.5 py-1.5 rounded-full text-sm font-medium bg-gray-100 text-gray-600 dark:bg-white/10 dark:text-gray-300 hover:bg-red-50 hover:text-red-600 dark:hover:bg-red-500/20 transition-colors"
              >
                <Flag size={14} />
                {t("coaches.detail.report")}
              </button>
            </div>
          </div>

          {/* Description */}
          <div className="bg-white dark:bg-[#2d2a42] rounded-2xl border border-gray-100 dark:border-white/10 p-5">
            <h2 className="text-base font-semibold text-gray-900 dark:text-white mb-2">
              {t("coaches.detail.aboutHeading")}
            </h2>
            <p className="text-sm text-gray-600 dark:text-gray-300 leading-relaxed whitespace-pre-line">
              {listing.description}
            </p>
          </div>

          {listing.schedule && (
            <div className="bg-white dark:bg-[#2d2a42] rounded-2xl border border-gray-100 dark:border-white/10 p-5">
              <h2 className="text-base font-semibold text-gray-900 dark:text-white mb-2 flex items-center gap-2">
                <CalendarClock size={16} className="text-primary" />
                {t("coaches.detail.scheduleHeading")}
              </h2>
              <p className="text-sm text-gray-600 dark:text-gray-300 whitespace-pre-line">
                {listing.schedule}
              </p>
            </div>
          )}

          {/* Map */}
          {listing.latitude != null && listing.longitude != null && (
            <div className="bg-white dark:bg-[#2d2a42] rounded-2xl border border-gray-100 dark:border-white/10 p-5">
              <h2 className="text-base font-semibold text-gray-900 dark:text-white mb-3 flex items-center gap-2">
                <MapPin size={16} className="text-primary" />
                {t("coaches.detail.locationHeading")}
              </h2>
              <Suspense
                fallback={
                  <div className="h-[280px] rounded-xl bg-gray-100 dark:bg-white/5 animate-pulse" />
                }
              >
                <ListingMap
                  latitude={Number(listing.latitude)}
                  longitude={Number(listing.longitude)}
                  label={listing.title}
                />
              </Suspense>
              {listing.addressLine && (
                <p className="text-xs text-gray-500 dark:text-gray-400 mt-3 flex items-center gap-1.5">
                  <MapPin size={12} />
                  {listing.addressLine}
                </p>
              )}
            </div>
          )}

          {/* Comments */}
          <div className="bg-white dark:bg-[#2d2a42] rounded-2xl border border-gray-100 dark:border-white/10 p-5">
            <h2 className="text-base font-semibold text-gray-900 dark:text-white mb-3 flex items-center gap-2">
              <MessageCircle size={16} className="text-primary" />
              {t("coaches.detail.commentsHeading")}
              <span className="text-xs text-gray-400 font-normal">
                ({listing.commentCount})
              </span>
            </h2>

            {/* Composer */}
            {isAuthenticated ? (
              <form onSubmit={handlePostComment} className="mb-5">
                <textarea
                  rows={2}
                  maxLength={1000}
                  value={commentBody}
                  onChange={(e) => setCommentBody(e.target.value)}
                  placeholder={t("coaches.detail.commentPlaceholder")}
                  className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40 resize-none"
                />
                <div className="flex justify-end mt-2">
                  <button
                    type="submit"
                    disabled={posting || commentBody.trim().length === 0}
                    className="inline-flex items-center gap-1.5 px-4 py-1.5 rounded-lg bg-primary text-white text-sm font-medium hover:bg-primary/90 disabled:opacity-50 transition-colors"
                  >
                    {posting ? (
                      <Loader2 size={13} className="animate-spin" />
                    ) : (
                      <Send size={13} />
                    )}
                    {t("coaches.detail.postCommentButton")}
                  </button>
                </div>
              </form>
            ) : (
              <p className="text-xs text-gray-500 dark:text-gray-400 mb-4">
                <Link
                  to={`/login?next=/coaches/${listing.id}`}
                  className="text-primary hover:underline"
                >
                  {t("coaches.detail.signInToComment")}
                </Link>
              </p>
            )}

            {commentsLoading ? (
              <div className="text-center py-6 text-gray-400">
                <Loader2 className="h-5 w-5 animate-spin mx-auto" />
              </div>
            ) : comments.length === 0 ? (
              <p className="text-sm text-gray-400 text-center py-4">
                {t("coaches.detail.noComments")}
              </p>
            ) : (
              <div className="space-y-3">
                {comments.map((comment) => {
                  const canDelete = isAdmin || comment.userId === user?.id;
                  return (
                    <div
                      key={comment.id}
                      className={`flex gap-3 p-3 rounded-xl ${
                        comment.isHidden
                          ? "bg-amber-50 dark:bg-amber-500/10 border border-amber-200/40"
                          : "bg-cream-dark/30 dark:bg-white/[0.02]"
                      }`}
                    >
                      <Avatar src={comment.authorAvatarUrl} size="sm" />
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2 mb-0.5">
                          <p className="text-xs font-semibold text-gray-700 dark:text-gray-200">
                            {comment.authorDisplayName}
                          </p>
                          <span className="text-[10px] text-gray-400">
                            {new Date(comment.createdAt).toLocaleDateString()}
                          </span>
                          {comment.isHidden && (
                            <span className="inline-flex items-center gap-1 text-[10px] text-amber-700 dark:text-amber-300 font-semibold">
                              <EyeOff size={10} />
                              {t("coaches.detail.commentHidden")}
                            </span>
                          )}
                        </div>
                        <p className="text-sm text-gray-700 dark:text-gray-200 whitespace-pre-line">
                          {comment.body}
                        </p>
                        {comment.hiddenReason && isAdmin && (
                          <p className="text-[11px] text-amber-700 dark:text-amber-300 mt-1">
                            Reason: {comment.hiddenReason}
                          </p>
                        )}
                      </div>
                      {canDelete && (
                        <button
                          type="button"
                          onClick={() => handleDeleteComment(comment.id)}
                          aria-label={t("common.delete") || "Delete"}
                          className="p-1 rounded-md text-gray-400 hover:bg-red-50 hover:text-red-500 transition-colors flex-shrink-0"
                        >
                          <Trash2 size={13} />
                        </button>
                      )}
                    </div>
                  );
                })}
              </div>
            )}
          </div>
        </div>

        {/* Sidebar */}
        <aside className="lg:col-span-1 space-y-4">
          <div className="bg-white dark:bg-[#2d2a42] rounded-2xl border border-gray-100 dark:border-white/10 p-5">
            {listing.priceFromEur != null && (
              <div className="mb-4">
                <p className="text-xs uppercase tracking-wide text-gray-400 font-semibold">
                  {t("coaches.detail.priceLabel")}
                </p>
                <p className="text-xl font-bold text-primary-dark dark:text-white">
                  {t("coaches.priceFrom", { value: listing.priceFromEur.toFixed(2) })}
                </p>
              </div>
            )}

            <div className="space-y-2 text-sm">
              <a
                href={`mailto:${listing.businessContactEmail}`}
                className="flex items-center gap-2 text-gray-700 dark:text-gray-200 hover:text-primary transition-colors"
              >
                <Mail size={15} className="text-primary" />
                {listing.businessContactEmail}
              </a>
              {listing.businessContactPhone && (
                <a
                  href={`tel:${listing.businessContactPhone}`}
                  className="flex items-center gap-2 text-gray-700 dark:text-gray-200 hover:text-primary transition-colors"
                >
                  <Phone size={15} className="text-primary" />
                  {listing.businessContactPhone}
                </a>
              )}
              {listing.businessWebsite && (
                <a
                  href={listing.businessWebsite}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="flex items-center gap-2 text-gray-700 dark:text-gray-200 hover:text-primary transition-colors"
                >
                  <Globe size={15} className="text-primary" />
                  {t("coaches.detail.websiteLink")}
                </a>
              )}
            </div>
          </div>

          <div className="bg-white dark:bg-[#2d2a42] rounded-2xl border border-gray-100 dark:border-white/10 p-5 grid grid-cols-3 gap-3 text-center">
            <Stat icon={Eye} value={listing.viewCount} label={t("coaches.detail.stats.views")} />
            <Stat icon={Heart} value={listing.likeCount} label={t("coaches.detail.stats.likes")} />
            <Stat icon={MessageCircle} value={listing.commentCount} label={t("coaches.detail.stats.comments")} />
          </div>

          {listing.businessBio && (
            <div className="bg-white dark:bg-[#2d2a42] rounded-2xl border border-gray-100 dark:border-white/10 p-5">
              <h3 className="text-sm font-semibold text-gray-900 dark:text-white mb-2">
                {t("coaches.detail.aboutBusinessHeading")}
              </h3>
              <p className="text-xs text-gray-500 dark:text-gray-400 leading-relaxed">
                {listing.businessBio}
              </p>
            </div>
          )}
        </aside>
      </div>
    </div>
  );
}

interface StatProps {
  icon: React.ComponentType<{ size?: number; className?: string }>;
  value: number;
  label: string;
}
function Stat({ icon: Icon, value, label }: StatProps) {
  return (
    <div>
      <Icon size={16} className="mx-auto mb-1 text-primary" />
      <p className="text-base font-semibold text-gray-900 dark:text-white">{value}</p>
      <p className="text-[10px] uppercase tracking-wide text-gray-400">{label}</p>
    </div>
  );
}

function formatAge(
  from: number | null,
  to: number | null,
  t: (key: string, opts?: Record<string, unknown>) => string,
): string {
  if (!from && !to) return "";
  const formatMonths = (m: number) => {
    if (m >= 12) {
      const years = Math.floor(m / 12);
      return years === 1
        ? t("coaches.ageYearsOne", { count: 1 })
        : t("coaches.ageYears", { count: years });
    }
    return t("coaches.ageMonths", { count: m });
  };
  if (from && to) return `${formatMonths(from)} – ${formatMonths(to)}`;
  if (from) return `${formatMonths(from)}+`;
  if (to) return `${t("coaches.ageUpTo")} ${formatMonths(to)}`;
  return "";
}
