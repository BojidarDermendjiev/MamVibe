import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { motion } from "framer-motion";
import { MapPin, Sparkles, Star } from "lucide-react";
import type { BusinessListingSummaryDto } from "@/types/business";
import { ActivityType } from "@/types/business";

interface ListingCardProps {
  listing: BusinessListingSummaryDto;
  index?: number;
}

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

export default function ListingCard({ listing, index = 0 }: ListingCardProps) {
  const { t } = useTranslation();

  const ageLabel = formatAge(listing.ageFromMonths, listing.ageToMonths, t);
  const badge = listing.rankBoost >= 100
    ? { label: t("coaches.badge.premium"), icon: Star, className: "bg-gradient-to-r from-mauve to-primary text-white" }
    : listing.rankBoost > 0
      ? { label: t("coaches.badge.featured"), icon: Sparkles, className: "bg-gradient-to-r from-primary/15 to-mauve/15 text-primary" }
      : null;

  return (
    <motion.div
      initial={{ opacity: 0, y: 14 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.35, delay: index * 0.04 }}
    >
      <Link
        to={`/coaches/${listing.id}`}
        className="block bg-white dark:bg-[#2d2a42] border border-gray-100 dark:border-white/5 rounded-2xl overflow-hidden shadow-sm hover:shadow-md hover:-translate-y-0.5 transition-all"
      >
        {listing.coverPhotoUrl ? (
          <img
            src={listing.coverPhotoUrl}
            alt={listing.title}
            className="w-full h-44 object-cover"
            onError={(e) => {
              (e.currentTarget as HTMLImageElement).style.display = "none";
            }}
          />
        ) : (
          <div className="w-full h-44 bg-gradient-to-br from-primary/10 to-mauve/10 flex items-center justify-center">
            <Sparkles className="w-10 h-10 text-primary/30" />
          </div>
        )}

        <div className="p-4">
          <div className="flex items-start justify-between gap-2 mb-1.5">
            <h3 className="font-semibold text-gray-900 dark:text-white leading-tight line-clamp-2">
              {listing.title}
            </h3>
            {badge && (
              <span
                className={`flex-shrink-0 inline-flex items-center gap-1 text-[10px] px-2 py-0.5 rounded-full font-semibold ${badge.className}`}
              >
                <badge.icon size={10} />
                {badge.label}
              </span>
            )}
          </div>
          <p className="text-xs text-gray-400 mb-2 truncate">
            {listing.businessDisplayName}
          </p>

          <div className="flex items-center flex-wrap gap-x-3 gap-y-1 text-xs text-gray-500 dark:text-gray-400">
            <span className="inline-flex items-center gap-1">
              <MapPin size={11} />
              {listing.city}
            </span>
            <span className="inline-block w-1 h-1 rounded-full bg-gray-300" />
            <span>{t(ACTIVITY_LABEL_KEYS[listing.activityType])}</span>
            {ageLabel && (
              <>
                <span className="inline-block w-1 h-1 rounded-full bg-gray-300" />
                <span>{ageLabel}</span>
              </>
            )}
          </div>

          <div className="mt-3 flex items-center justify-between">
            <span className="text-sm font-semibold text-primary-dark dark:text-white">
              {listing.priceFromEur != null
                ? t("coaches.priceFrom", { value: listing.priceFromEur.toFixed(2) })
                : t("coaches.priceContact")}
            </span>
            <span className="text-xs text-gray-400">
              {listing.likeCount > 0 && `♡ ${listing.likeCount}`}
            </span>
          </div>
        </div>
      </Link>
    </motion.div>
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
