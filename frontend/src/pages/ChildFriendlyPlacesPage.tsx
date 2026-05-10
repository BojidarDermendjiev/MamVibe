import { useState, useEffect, useCallback, useMemo } from "react";
import { useTranslation } from "react-i18next";
import { MapPin, ExternalLink, Plus, X, Clock } from "lucide-react";
import { usePageSEO } from "@/hooks/useSEO";
import { childFriendlyPlacesApi } from "../api/childFriendlyPlacesApi";
import { PlaceType } from "../types/childFriendlyPlace";
import type { ChildFriendlyPlaceDto, CreateChildFriendlyPlaceDto } from "../types/childFriendlyPlace";
import { useAuthStore } from "../store/authStore";

const PLACE_TYPE_COLORS: Record<PlaceType, string> = {
  [PlaceType.Walk]: "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400",
  [PlaceType.Playground]: "bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400",
  [PlaceType.Restaurant]: "bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400",
  [PlaceType.Cafe]: "bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400",
  [PlaceType.Museum]: "bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400",
  [PlaceType.Zoo]: "bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400",
  [PlaceType.Beach]: "bg-cyan-100 text-cyan-700 dark:bg-cyan-900/30 dark:text-cyan-400",
  [PlaceType.Park]: "bg-lime-100 text-lime-700 dark:bg-lime-900/30 dark:text-lime-400",
  [PlaceType.ThemeAttraction]: "bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400",
  [PlaceType.SportsActivity]: "bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400",
  [PlaceType.Other]: "bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-400",
};

function buildMapsUrl(place: ChildFriendlyPlaceDto): string {
  const destination = [place.name, place.address, place.city].filter(Boolean).join(", ");
  return `https://www.google.com/maps/dir/?api=1&destination=${encodeURIComponent(destination)}`;
}

const EMPTY_FORM: CreateChildFriendlyPlaceDto = {
  name: "",
  description: "",
  address: "",
  city: "",
  placeType: PlaceType.Playground,
  ageFromMonths: undefined,
  ageToMonths: undefined,
  photoUrl: "",
  website: "",
};

export default function ChildFriendlyPlacesPage() {
  const { t } = useTranslation();
  const { isAuthenticated, user } = useAuthStore();

  const placeTypeLabels = useMemo<Record<PlaceType, string>>(() => ({
    [PlaceType.Walk]: t("childFriendlyPlaces.type_walk"),
    [PlaceType.Playground]: t("childFriendlyPlaces.type_playground"),
    [PlaceType.Restaurant]: t("childFriendlyPlaces.type_restaurant"),
    [PlaceType.Cafe]: t("childFriendlyPlaces.type_cafe"),
    [PlaceType.Museum]: t("childFriendlyPlaces.type_museum"),
    [PlaceType.Zoo]: t("childFriendlyPlaces.type_zoo"),
    [PlaceType.Beach]: t("childFriendlyPlaces.type_beach"),
    [PlaceType.Park]: t("childFriendlyPlaces.type_park"),
    [PlaceType.ThemeAttraction]: t("childFriendlyPlaces.type_theme"),
    [PlaceType.SportsActivity]: t("childFriendlyPlaces.type_sports"),
    [PlaceType.Other]: t("childFriendlyPlaces.type_other"),
  }), [t]);

  const formatAge = useCallback((from: number | null, to: number | null): string => {
    if (!from && !to) return "";
    const y = t("childFriendlyPlaces.year_short");
    const m = t("childFriendlyPlaces.month_short");
    const upTo = t("childFriendlyPlaces.age_up_to");
    const fmt = (months: number) =>
      months >= 12
        ? `${Math.floor(months / 12)}${y}${months % 12 ? ` ${months % 12}${m}` : ""}`
        : `${months}${m}`;
    if (from && to) return `${fmt(from)} – ${fmt(to)}`;
    if (from) return `${fmt(from)}+`;
    if (to) return `${upTo} ${fmt(to)}`;
    return "";
  }, [t]);

  usePageSEO({
    title: "Child-Friendly Places in Bulgaria",
    description:
      "Discover playgrounds, parks, restaurants, museums, and family-friendly attractions across Bulgaria. Community-curated places loved by parents on MamVibe.",
    canonical: "https://mamvibe.com/child-friendly-places",
    structuredData: {
      "@context": "https://schema.org",
      "@type": "ItemList",
      name: "Child-Friendly Places in Bulgaria",
      description:
        "Community-curated child-friendly places across Bulgaria, shared by parents on MamVibe.",
      url: "https://mamvibe.com/child-friendly-places",
    },
  });

  const isAdmin = user?.roles?.includes("Admin") ?? false;

  const [places, setPlaces] = useState<ChildFriendlyPlaceDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [cityFilter, setCityFilter] = useState("");
  const [placeTypeFilter, setPlaceTypeFilter] = useState<PlaceType | "">("");
  const [ageMonthsFilter, setAgeMonthsFilter] = useState<string>("");
  const [page, setPage] = useState(1);
  const [showModal, setShowModal] = useState(false);
  const [form, setForm] = useState<CreateChildFriendlyPlaceDto>(EMPTY_FORM);
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [submitSuccess, setSubmitSuccess] = useState(false);

  const fetchPlaces = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await childFriendlyPlacesApi.getAll({
        city: cityFilter || undefined,
        placeType: placeTypeFilter !== "" ? (placeTypeFilter as PlaceType) : undefined,
        childAgeMonths: ageMonthsFilter ? Number(ageMonthsFilter) : undefined,
        page,
        pageSize: 20,
      });
      setPlaces(data);
    } catch {
      setError(t("childFriendlyPlaces.loadError"));
    } finally {
      setLoading(false);
    }
  }, [cityFilter, placeTypeFilter, ageMonthsFilter, page, t]);

  useEffect(() => {
    fetchPlaces();
  }, [fetchPlaces]);

  const handleFilterSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    fetchPlaces();
  };

  const handleSubmitPlace = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setSubmitError(null);
    try {
      await childFriendlyPlacesApi.create({
        ...form,
        address: form.address || undefined,
        photoUrl: form.photoUrl || undefined,
        website: form.website || undefined,
      });
      setSubmitSuccess(true);
      setForm(EMPTY_FORM);
      setTimeout(() => {
        setShowModal(false);
        setSubmitSuccess(false);
      }, 2000);
    } catch {
      setSubmitError(t("childFriendlyPlaces.submitError"));
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm(t("childFriendlyPlaces.deleteConfirm"))) return;
    try {
      await childFriendlyPlacesApi.delete(id);
      setPlaces((prev) => prev.filter((p) => p.id !== id));
    } catch {
      // ignore
    }
  };

  return (
    <div className="max-w-5xl mx-auto px-4 py-8">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">
            {t("childFriendlyPlaces.title")}
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
            {t("childFriendlyPlaces.subtitle")}
          </p>
        </div>
        {isAuthenticated && (
          <button
            onClick={() => setShowModal(true)}
            className="flex items-center gap-2 px-4 py-2 bg-primary text-white rounded-xl text-sm font-semibold hover:bg-primary/90 transition-colors"
          >
            <Plus size={16} />
            {t("childFriendlyPlaces.addPlace")}
          </button>
        )}
      </div>

      {/* Filters */}
      <form onSubmit={handleFilterSubmit} className="flex flex-wrap gap-3 mb-6">
        <input
          type="text"
          placeholder={t("childFriendlyPlaces.cityPlaceholder")}
          value={cityFilter}
          onChange={(e) => setCityFilter(e.target.value)}
          className="flex-1 min-w-[140px] px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-white dark:bg-[#2d2a42] text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
        />
        <select
          value={placeTypeFilter}
          onChange={(e) => setPlaceTypeFilter(e.target.value === "" ? "" : Number(e.target.value) as PlaceType)}
          className="flex-1 min-w-[160px] px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-white dark:bg-[#2d2a42] text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
        >
          <option value="">{t("childFriendlyPlaces.allTypes")}</option>
          {Object.entries(placeTypeLabels).map(([val, label]) => (
            <option key={val} value={val}>{label}</option>
          ))}
        </select>
        <input
          type="number"
          min={0}
          max={216}
          placeholder={t("childFriendlyPlaces.agePlaceholder")}
          value={ageMonthsFilter}
          onChange={(e) => setAgeMonthsFilter(e.target.value)}
          className="flex-1 min-w-[160px] px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-white dark:bg-[#2d2a42] text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
        />
        <button
          type="submit"
          className="px-4 py-2 bg-primary/10 text-primary rounded-lg text-sm font-medium hover:bg-primary/20 transition-colors"
        >
          {t("childFriendlyPlaces.searchBtn")}
        </button>
      </form>

      {/* Content */}
      {loading && <div className="text-center py-12 text-gray-400">{t("common.loading")}</div>}
      {error && <div className="text-center py-8 text-red-500">{error}</div>}
      {!loading && !error && places.length === 0 && (
        <div className="text-center py-12 text-gray-400 dark:text-gray-500">
          {t("childFriendlyPlaces.noPlaces")}
        </div>
      )}

      <h2 className="sr-only">{t("childFriendlyPlaces.title")}</h2>
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        {places.map((place) => (
          <div
            key={place.id}
            className="bg-white dark:bg-[#2d2a42] rounded-xl border border-gray-100 dark:border-white/5 shadow-sm overflow-hidden"
          >
            {place.photoUrl && (
              <img
                src={place.photoUrl}
                alt={place.name}
                className="w-full h-40 object-cover"
                onError={(e) => { (e.currentTarget as HTMLImageElement).style.display = "none"; }}
              />
            )}
            <div className="p-4">
              <div className="flex items-start justify-between gap-2">
                <h3 className="font-semibold text-gray-900 dark:text-white leading-tight">{place.name}</h3>
                <span className={`text-xs px-2 py-0.5 rounded-full font-medium flex-shrink-0 ${PLACE_TYPE_COLORS[place.placeType]}`}>
                  {placeTypeLabels[place.placeType]}
                </span>
              </div>

              <div className="flex items-center gap-1.5 mt-1 text-xs text-gray-400">
                <MapPin size={11} />
                <span>{place.city}</span>
                {place.address && <span>· {place.address}</span>}
              </div>

              {(place.ageFromMonths || place.ageToMonths) && (
                <div className="flex items-center gap-1.5 mt-1 text-xs text-gray-400">
                  <Clock size={11} />
                  <span>{t("childFriendlyPlaces.ageLabel")} {formatAge(place.ageFromMonths, place.ageToMonths)}</span>
                </div>
              )}

              <p className="mt-2 text-sm text-gray-600 dark:text-gray-300 line-clamp-2 leading-relaxed">
                {place.description}
              </p>

              <div className="mt-3 flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <a
                    href={buildMapsUrl(place)}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="btn-navigate"
                    aria-label={`${t("childFriendlyPlaces.navigate")} ${place.name}`}
                  >
                    <span>{t("childFriendlyPlaces.navigate")}</span>
                    <svg width="13px" height="10px" viewBox="0 0 13 10">
                      <path d="M1,5 L11,5" />
                      <polyline points="8 1 12 5 8 9" />
                    </svg>
                  </a>
                  {place.website && (
                    <a
                      href={place.website}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="flex items-center gap-1 text-xs text-blue-500 hover:underline"
                    >
                      {t("childFriendlyPlaces.website")} <ExternalLink size={10} />
                    </a>
                  )}
                  {(isAdmin || (isAuthenticated && place.userId === user?.id)) && (
                    <button
                      onClick={() => handleDelete(place.id)}
                      className="text-xs text-red-400 hover:text-red-500 transition-colors"
                    >
                      {t("childFriendlyPlaces.delete")}
                    </button>
                  )}
                </div>
                <span className="text-xs text-gray-400">
                  {new Date(place.createdAt).toLocaleDateString()}
                </span>
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Pagination */}
      {(page > 1 || places.length === 20) && (
        <div className="flex items-center justify-center gap-3 mt-8">
          <button
            onClick={() => { setPage((p) => Math.max(1, p - 1)); window.scrollTo({ top: 0, behavior: "smooth" }); }}
            disabled={page === 1}
            className="px-4 py-2 rounded-lg bg-gray-100 dark:bg-white/10 text-sm font-medium disabled:opacity-40 hover:bg-gray-200 dark:hover:bg-white/15 transition-colors"
          >
            {t("childFriendlyPlaces.prevPage")}
          </button>
          <span className="px-4 py-2 text-sm font-semibold text-gray-700 dark:text-gray-300 bg-primary/10 rounded-lg">
            {t("childFriendlyPlaces.page", { n: page })}
          </span>
          <button
            onClick={() => { setPage((p) => p + 1); window.scrollTo({ top: 0, behavior: "smooth" }); }}
            disabled={places.length < 20}
            className="px-4 py-2 rounded-lg bg-gray-100 dark:bg-white/10 text-sm font-medium disabled:opacity-40 hover:bg-gray-200 dark:hover:bg-white/15 transition-colors"
          >
            {t("childFriendlyPlaces.nextPage")}
          </button>
        </div>
      )}

      {/* Add Place Modal */}
      {showModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
          <div className="bg-white dark:bg-[#2d2a42] rounded-2xl w-full max-w-lg shadow-2xl max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between p-5 border-b border-gray-100 dark:border-white/10">
              <h2 className="font-bold text-gray-900 dark:text-white">
                {t("childFriendlyPlaces.addPlace")}
              </h2>
              <button
                onClick={() => { setShowModal(false); setForm(EMPTY_FORM); setSubmitError(null); setSubmitSuccess(false); }}
                className="p-1.5 rounded-lg hover:bg-gray-100 dark:hover:bg-white/10 transition-colors"
              >
                <X size={18} className="text-gray-500" />
              </button>
            </div>

            {submitSuccess ? (
              <div className="p-8 text-center">
                <div className="text-4xl mb-3">✓</div>
                <p className="font-semibold text-gray-900 dark:text-white">{t("childFriendlyPlaces.successTitle")}</p>
                <p className="text-sm text-gray-500 mt-1">{t("childFriendlyPlaces.successDesc")}</p>
              </div>
            ) : (
              <form onSubmit={handleSubmitPlace} className="p-5 space-y-4">
                <div>
                  <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
                    {t("childFriendlyPlaces.nameLabel")}
                  </label>
                  <input
                    required
                    maxLength={150}
                    value={form.name}
                    onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
                    className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
                    placeholder={t("childFriendlyPlaces.namePlaceholder")}
                  />
                </div>

                <div>
                  <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
                    {t("childFriendlyPlaces.descLabel")}
                  </label>
                  <textarea
                    required
                    maxLength={2000}
                    rows={3}
                    value={form.description}
                    onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
                    className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40 resize-none"
                    placeholder={t("childFriendlyPlaces.descPlaceholder")}
                  />
                </div>

                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
                      {t("childFriendlyPlaces.cityLabel")}
                    </label>
                    <input
                      required
                      maxLength={100}
                      value={form.city}
                      onChange={(e) => setForm((f) => ({ ...f, city: e.target.value }))}
                      className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
                      placeholder={t("childFriendlyPlaces.cityInputPlaceholder")}
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
                      {t("childFriendlyPlaces.placeTypeLabel")}
                    </label>
                    <select
                      value={form.placeType}
                      onChange={(e) => setForm((f) => ({ ...f, placeType: Number(e.target.value) as PlaceType }))}
                      className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
                    >
                      {Object.entries(placeTypeLabels).map(([val, label]) => (
                        <option key={val} value={val}>{label}</option>
                      ))}
                    </select>
                  </div>
                </div>

                <div>
                  <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
                    {t("childFriendlyPlaces.addressLabel")}
                  </label>
                  <input
                    maxLength={300}
                    value={form.address}
                    onChange={(e) => setForm((f) => ({ ...f, address: e.target.value }))}
                    className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
                    placeholder={t("childFriendlyPlaces.cityInputPlaceholder")}
                  />
                </div>

                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
                      {t("childFriendlyPlaces.ageFromLabel")}
                    </label>
                    <input
                      type="number"
                      min={0}
                      max={216}
                      value={form.ageFromMonths ?? ""}
                      onChange={(e) => setForm((f) => ({ ...f, ageFromMonths: e.target.value ? Number(e.target.value) : undefined }))}
                      className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
                      placeholder={t("childFriendlyPlaces.ageFromPlaceholder")}
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
                      {t("childFriendlyPlaces.ageToLabel")}
                    </label>
                    <input
                      type="number"
                      min={0}
                      max={216}
                      value={form.ageToMonths ?? ""}
                      onChange={(e) => setForm((f) => ({ ...f, ageToMonths: e.target.value ? Number(e.target.value) : undefined }))}
                      className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
                      placeholder={t("childFriendlyPlaces.ageToPlaceholder")}
                    />
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
                      {t("childFriendlyPlaces.photoLabel")}
                    </label>
                    <input
                      maxLength={2048}
                      value={form.photoUrl}
                      onChange={(e) => setForm((f) => ({ ...f, photoUrl: e.target.value }))}
                      className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
                      placeholder="https://..."
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
                      {t("childFriendlyPlaces.websiteLabel")}
                    </label>
                    <input
                      maxLength={2048}
                      value={form.website}
                      onChange={(e) => setForm((f) => ({ ...f, website: e.target.value }))}
                      className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
                      placeholder="https://..."
                    />
                  </div>
                </div>

                <p className="text-xs text-amber-600 dark:text-amber-400 bg-amber-50 dark:bg-amber-900/20 rounded-lg px-3 py-2">
                  {t("childFriendlyPlaces.reviewNote")}
                </p>

                {submitError && (
                  <p className="text-sm text-red-500">{submitError}</p>
                )}

                <div className="flex gap-3 pt-1">
                  <button
                    type="button"
                    onClick={() => { setShowModal(false); setForm(EMPTY_FORM); setSubmitError(null); }}
                    className="flex-1 px-4 py-2 rounded-xl border border-gray-200 dark:border-white/10 text-sm font-medium text-gray-600 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-white/5 transition-colors"
                  >
                    {t("common.cancel")}
                  </button>
                  <button
                    type="submit"
                    disabled={submitting}
                    className="flex-1 px-4 py-2 rounded-xl bg-primary text-white text-sm font-semibold hover:bg-primary/90 transition-colors disabled:opacity-60"
                  >
                    {submitting ? t("childFriendlyPlaces.submitting") : t("childFriendlyPlaces.submitBtn")}
                  </button>
                </div>
              </form>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
