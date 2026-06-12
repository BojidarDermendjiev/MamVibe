import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { motion } from "framer-motion";
import { MapPin, Search, Filter, ArrowRight, Building2 } from "lucide-react";
import { usePageSEO } from "@/hooks/useSEO";
import ListingCard from "@/components/business/ListingCard";
import { businessApi } from "@/api/businessApi";
import { BusinessCategory, type BrowseListingsResult } from "@/types/business";

const PAGE_SIZE = 20;

/**
 * Parents-facing browse for venue advertisers (indoor playgrounds, family restaurants,
 * museums, attractions). Mirrors CoachesBrowsePage but filters by
 * <c>BusinessCategory.VenueAdvertiser</c>.
 */
export default function VenuesBrowsePage() {
  const { t } = useTranslation();

  usePageSEO({
    title: t("venues.seoTitle") || "Family-friendly venues & attractions",
    description:
      t("venues.seoDescription") ||
      "Discover indoor playgrounds, family-friendly restaurants, museums, and attractions for kids on MamVibe.",
    canonical: "https://mamvibe.com/venues",
  });

  const [data, setData] = useState<BrowseListingsResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [cityInput, setCityInput] = useState("");
  const [applied, setApplied] = useState({ city: "" });
  const [page, setPage] = useState(1);

  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      if (cancelled) return;
      setLoading(true);
      setError(null);
      try {
        const result = await businessApi.browseListings({
          category: BusinessCategory.VenueAdvertiser,
          city: applied.city || undefined,
          page,
          pageSize: PAGE_SIZE,
        });
        if (!cancelled) setData(result);
      } catch {
        if (!cancelled) setError(t("venues.browseError"));
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
    setApplied({ city: cityInput.trim() });
  };

  return (
    <div>
      <div className="bg-page py-12 px-4">
        <div className="max-w-5xl mx-auto flex flex-col sm:flex-row items-start sm:items-center justify-between gap-6">
          <motion.div
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.4 }}
          >
            <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-primary/10 text-primary text-xs font-semibold mb-3">
              <Building2 size={13} />
              {t("venues.tagline")}
            </div>
            <h1 className="text-3xl font-bold text-primary-dark dark:text-white">
              {t("venues.title")}
            </h1>
            <p className="text-gray-500 dark:text-gray-400 text-sm max-w-md mt-1">
              {t("venues.subtitle")}
            </p>
          </motion.div>
          <Link
            to="/business/register"
            className="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl bg-primary text-white text-sm font-semibold shadow-md hover:bg-primary/90 transition-colors"
          >
            {t("venues.ctaListVenue")} <ArrowRight size={16} />
          </Link>
        </div>
      </div>

      <div className="max-w-5xl mx-auto px-4 pb-12">
        <form
          onSubmit={handleFilterSubmit}
          className="bg-white dark:bg-[#2d2a42] rounded-2xl border border-gray-100 dark:border-white/10 shadow-sm p-4 flex flex-wrap gap-3 mb-8"
        >
          <div className="relative flex-1 min-w-[180px]">
            <MapPin
              size={14}
              className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none"
            />
            <input
              type="text"
              placeholder={t("venues.cityPlaceholder")}
              value={cityInput}
              onChange={(e) => setCityInput(e.target.value)}
              className="w-full pl-8 pr-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
            />
          </div>
          <button
            type="submit"
            className="flex items-center gap-2 px-4 py-2 bg-primary text-white rounded-lg text-sm font-medium hover:bg-primary/90 transition-colors"
          >
            <Search size={14} />
            {t("venues.searchBtn")}
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
                </div>
              </div>
            ))}
          </div>
        )}

        {error && <div className="text-center py-8 text-red-500">{error}</div>}

        {!loading && !error && data && (
          <>
            {data.featured.length > 0 && (
              <section className="mb-8">
                <h2 className="text-sm font-semibold text-primary uppercase tracking-wide mb-3">
                  {t("venues.featuredHeading")}
                </h2>
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
                <p className="text-gray-500 dark:text-gray-400 font-medium">{t("venues.empty")}</p>
              </div>
            ) : (
              <>
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                  {data.items.map((listing, i) => (
                    <ListingCard key={listing.id} listing={listing} index={i} />
                  ))}
                </div>
                {totalPages > 1 && (
                  <div className="flex items-center justify-center gap-3 mt-8">
                    <button
                      onClick={() => setPage((p) => Math.max(1, p - 1))}
                      disabled={page === 1}
                      className="px-4 py-2 rounded-lg bg-gray-100 dark:bg-white/10 text-sm font-medium disabled:opacity-40"
                    >
                      {t("coaches.prev")}
                    </button>
                    <span className="px-4 py-2 text-sm font-semibold bg-primary/10 rounded-lg">
                      {t("coaches.pageOf", { page, total: totalPages })}
                    </span>
                    <button
                      onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                      disabled={page === totalPages}
                      className="px-4 py-2 rounded-lg bg-gray-100 dark:bg-white/10 text-sm font-medium disabled:opacity-40"
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
