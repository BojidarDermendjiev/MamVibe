import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { motion } from "framer-motion";
import { Sparkles, MapPin, Search, Filter, ArrowRight } from "lucide-react";
import { usePageSEO } from "@/hooks/useSEO";
import ListingCard from "@/components/business/ListingCard";
import { businessApi } from "@/api/businessApi";
import { useAuthStore } from "@/store/authStore";
import { ActivityType, BusinessCategory, type BrowseListingsResult } from "@/types/business";

const ACTIVITY_FILTER_OPTIONS: { value: ActivityType; key: string }[] = [
  { value: ActivityType.Swimming, key: "coaches.activityType.swimming" },
  { value: ActivityType.MartialArts, key: "coaches.activityType.martialArts" },
  { value: ActivityType.Music, key: "coaches.activityType.music" },
  { value: ActivityType.Dance, key: "coaches.activityType.dance" },
  { value: ActivityType.Gymnastics, key: "coaches.activityType.gymnastics" },
  { value: ActivityType.ArtAndCrafts, key: "coaches.activityType.artAndCrafts" },
  { value: ActivityType.EarlyDevelopment, key: "coaches.activityType.earlyDevelopment" },
  { value: ActivityType.LanguageClasses, key: "coaches.activityType.languageClasses" },
  { value: ActivityType.SportsTeam, key: "coaches.activityType.sportsTeam" },
  { value: ActivityType.Other, key: "coaches.activityType.other" },
];

const PAGE_SIZE = 20;

export default function CoachesBrowsePage() {
  const { t } = useTranslation();
  // Unauthenticated visitors land on the partner signup (business-styled, public)
  // instead of /business/register which is auth-protected and would bounce them
  // to the consumer login page.
  const { isAuthenticated } = useAuthStore();
  const startFreePath = isAuthenticated ? "/business/register" : "/partner/register";

  usePageSEO({
    title: t("coaches.seoTitle") || "Coaches & activities for kids",
    description:
      t("coaches.seoDescription") ||
      "Discover swimming, martial arts, music, dance, and more — children's activities curated for MamVibe families.",
    canonical: "https://mamvibe.com/coaches",
  });

  const [data, setData] = useState<BrowseListingsResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [cityInput, setCityInput] = useState("");
  const [ageInput, setAgeInput] = useState("");
  const [activityInput, setActivityInput] = useState<ActivityType | "">("");
  const [applied, setApplied] = useState({
    city: "",
    age: "",
    activity: "" as ActivityType | "",
  });
  const [page, setPage] = useState(1);

  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      if (cancelled) return;
      setLoading(true);
      setError(null);
      try {
        const result = await businessApi.browseListings({
          category: BusinessCategory.Coach,
          city: applied.city || undefined,
          activityType: applied.activity === "" ? undefined : applied.activity,
          ageMonths: applied.age ? Number(applied.age) : undefined,
          page,
          pageSize: PAGE_SIZE,
        });
        if (!cancelled) setData(result);
      } catch {
        if (!cancelled) setError(t("coaches.browseError"));
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    void load();
    return () => {
      cancelled = true;
    };
  }, [applied, page, t]);

  const totalPages = useMemo(
    () => (data ? Math.max(1, Math.ceil(data.totalCount / PAGE_SIZE)) : 1),
    [data],
  );

  const handleFilterSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    setApplied({ city: cityInput.trim(), age: ageInput, activity: activityInput });
  };

  return (
    <div>
      {/* Hero */}
      <div className="bg-page py-12 px-4">
        <div className="max-w-5xl mx-auto flex flex-col sm:flex-row items-start sm:items-center justify-between gap-6">
          <motion.div
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.4 }}
          >
            <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-primary/10 text-primary text-xs font-semibold mb-3">
              <Sparkles size={13} />
              {t("coaches.tagline")}
            </div>
            <h1 className="text-3xl font-bold text-primary-dark dark:text-white">
              {t("coaches.title")}
            </h1>
            <p className="text-gray-500 dark:text-gray-400 text-sm max-w-md mt-1">
              {t("coaches.subtitle")}
            </p>
          </motion.div>
          <Link
            to={startFreePath}
            className="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl bg-primary text-white text-sm font-semibold shadow-md hover:bg-primary/90 transition-colors"
          >
            {t("coaches.ctaStartFree")} <ArrowRight size={16} />
          </Link>
        </div>
      </div>

      <div className="max-w-5xl mx-auto px-4 pb-12">
        {/* Filters */}
        <form
          onSubmit={handleFilterSubmit}
          className="bg-white dark:bg-[#2d2a42] rounded-2xl border border-gray-100 dark:border-white/10 shadow-sm p-4 flex flex-wrap gap-3 mb-8"
        >
          <div className="relative flex-1 min-w-[140px]">
            <MapPin
              size={14}
              className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none"
            />
            <input
              type="text"
              placeholder={t("coaches.cityPlaceholder")}
              value={cityInput}
              onChange={(e) => setCityInput(e.target.value)}
              className="w-full pl-8 pr-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
            />
          </div>
          <select
            value={activityInput}
            onChange={(e) =>
              setActivityInput(e.target.value === "" ? "" : (Number(e.target.value) as ActivityType))
            }
            className="flex-1 min-w-[170px] px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
          >
            <option value="">{t("coaches.allActivities")}</option>
            {ACTIVITY_FILTER_OPTIONS.map((o) => (
              <option key={o.value} value={o.value}>
                {t(o.key)}
              </option>
            ))}
          </select>
          <input
            type="number"
            min={0}
            max={216}
            placeholder={t("coaches.agePlaceholder")}
            value={ageInput}
            onChange={(e) => setAgeInput(e.target.value)}
            className="flex-1 min-w-[160px] px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
          />
          <button
            type="submit"
            className="flex items-center gap-2 px-4 py-2 bg-primary text-white rounded-lg text-sm font-medium hover:bg-primary/90 transition-colors"
          >
            <Search size={14} />
            {t("coaches.searchBtn")}
          </button>
        </form>

        {loading && (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {Array.from({ length: 6 }).map((_, i) => (
              <div
                key={i}
                className="bg-white dark:bg-[#2d2a42] rounded-2xl border border-gray-100 dark:border-white/5 overflow-hidden animate-pulse"
              >
                <div className="h-44 bg-gray-100 dark:bg-white/5" />
                <div className="p-4 space-y-2">
                  <div className="h-4 bg-gray-100 dark:bg-white/5 rounded w-2/3" />
                  <div className="h-3 bg-gray-100 dark:bg-white/5 rounded w-1/3" />
                  <div className="h-3 bg-gray-100 dark:bg-white/5 rounded w-full" />
                </div>
              </div>
            ))}
          </div>
        )}

        {error && (
          <div className="text-center py-8 text-red-500">{error}</div>
        )}

        {!loading && !error && data && (
          <>
            {data.featured.length > 0 && (
              <section className="mb-8">
                <div className="flex items-center gap-2 mb-3">
                  <Sparkles size={15} className="text-primary" />
                  <h2 className="text-sm font-semibold text-primary uppercase tracking-wide">
                    {t("coaches.featuredHeading")}
                  </h2>
                </div>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  {data.featured.map((listing, i) => (
                    <ListingCard key={listing.id} listing={listing} index={i} />
                  ))}
                </div>
              </section>
            )}

            {data.items.length === 0 && data.featured.length === 0 ? (
              <div className="text-center py-20">
                <div className="w-16 h-16 rounded-full bg-primary/10 flex items-center justify-center mx-auto mb-4">
                  <Filter className="w-7 h-7 text-primary/60" />
                </div>
                <p className="text-gray-500 dark:text-gray-400 font-medium mb-1">
                  {t("coaches.empty")}
                </p>
                <Link
                  to={startFreePath}
                  className="mt-3 inline-block text-sm text-primary hover:underline"
                >
                  {t("coaches.ctaStartFree")} →
                </Link>
              </div>
            ) : (
              <>
                <h2 className="sr-only">{t("coaches.allHeading")}</h2>
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                  {data.items.map((listing, i) => (
                    <ListingCard key={listing.id} listing={listing} index={i} />
                  ))}
                </div>

                {totalPages > 1 && (
                  <div className="flex items-center justify-center gap-3 mt-8">
                    <button
                      onClick={() => {
                        setPage((p) => Math.max(1, p - 1));
                        window.scrollTo({ top: 0, behavior: "smooth" });
                      }}
                      disabled={page === 1}
                      className="px-4 py-2 rounded-lg bg-gray-100 dark:bg-white/10 text-sm font-medium disabled:opacity-40 hover:bg-gray-200 dark:hover:bg-white/15 transition-colors"
                    >
                      {t("coaches.prev")}
                    </button>
                    <span className="px-4 py-2 text-sm font-semibold text-gray-700 dark:text-gray-300 bg-primary/10 rounded-lg">
                      {t("coaches.pageOf", { page, total: totalPages })}
                    </span>
                    <button
                      onClick={() => {
                        setPage((p) => Math.min(totalPages, p + 1));
                        window.scrollTo({ top: 0, behavior: "smooth" });
                      }}
                      disabled={page === totalPages}
                      className="px-4 py-2 rounded-lg bg-gray-100 dark:bg-white/10 text-sm font-medium disabled:opacity-40 hover:bg-gray-200 dark:hover:bg-white/15 transition-colors"
                    >
                      {t("coaches.next")}
                    </button>
                  </div>
                )}
              </>
            )}
          </>
        )}
      </div>
    </div>
  );
}
