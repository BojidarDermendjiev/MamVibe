import { useCallback, useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import axios from "axios";
import {
  ArrowLeft,
  Loader2,
  X,
  ImagePlus,
  Save,
  GripVertical,
} from "lucide-react";
import { usePageSEO } from "@/hooks/useSEO";
import { businessApi } from "@/api/businessApi";
import {
  ActivityType,
  type BusinessErrorEnvelope,
  type BusinessListingDto,
  type CreateBusinessListingRequest,
  type UpdateBusinessListingRequest,
} from "@/types/business";
import toast from "@/utils/toast";

const ACTIVITY_OPTIONS: { value: ActivityType; key: string }[] = [
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

const MAX_PHOTOS = 15;

interface FormState {
  title: string;
  description: string;
  activityType: ActivityType;
  city: string;
  addressLine: string;
  latitude: string;
  longitude: string;
  ageFromMonths: string;
  ageToMonths: string;
  priceFromEur: string;
  schedule: string;
  isActive: boolean;
}

const EMPTY_FORM: FormState = {
  title: "",
  description: "",
  activityType: ActivityType.Swimming,
  city: "",
  addressLine: "",
  latitude: "",
  longitude: "",
  ageFromMonths: "",
  ageToMonths: "",
  priceFromEur: "",
  schedule: "",
  isActive: true,
};

/**
 * Combined create + edit form. The URL determines mode:
 *   /business/listing/new — create
 *   /business/listing/edit — edit existing listing
 * Photos use the project's existing /photos/upload endpoint; each pick is uploaded
 * immediately and the returned URL is appended to the photo list (so the user sees
 * progress and we don't need to retry on submit).
 */
export default function BusinessListingFormPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { id: routeId } = useParams<{ id?: string }>();
  const isEdit = window.location.pathname.includes("/edit");

  const [form, setForm] = useState<FormState>(EMPTY_FORM);
  const [photoUrls, setPhotoUrls] = useState<string[]>([]);
  const [existingId, setExistingId] = useState<string | null>(null);
  const [loading, setLoading] = useState(isEdit);
  const [uploading, setUploading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  usePageSEO({
    title: isEdit
      ? t("business.listing.editSeoTitle") || "Edit listing"
      : t("business.listing.createSeoTitle") || "Create listing",
    description: t("business.listing.formIntro") || "Manage your coach listing on MamVibe.",
    index: false,
  });

  useEffect(() => {
    if (!isEdit) return;
    let cancelled = false;
    const load = async () => {
      if (cancelled) return;
      setLoading(true);
      setError(null);
      try {
        const listing = await businessApi.getMyListing();
        if (cancelled) return;
        setExistingId(listing.id);
        setForm({
          title: listing.title,
          description: listing.description,
          activityType: listing.activityType,
          city: listing.city,
          addressLine: listing.addressLine ?? "",
          latitude: listing.latitude != null ? String(listing.latitude) : "",
          longitude: listing.longitude != null ? String(listing.longitude) : "",
          ageFromMonths: listing.ageFromMonths != null ? String(listing.ageFromMonths) : "",
          ageToMonths: listing.ageToMonths != null ? String(listing.ageToMonths) : "",
          priceFromEur: listing.priceFromEur != null ? String(listing.priceFromEur) : "",
          schedule: listing.schedule ?? "",
          isActive: listing.isActive,
        });
        setPhotoUrls(listing.photos.map((p) => p.url));
      } catch (err) {
        if (cancelled) return;
        if (axios.isAxiosError(err) && err.response?.status === 404) {
          // No listing exists yet — switch to create mode.
          navigate("/business/listing/new", { replace: true });
        } else {
          setError(t("business.listing.loadError"));
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    void load();
    return () => {
      cancelled = true;
    };
  }, [isEdit, navigate, t, routeId]);

  const handleAddFiles = useCallback(
    async (files: FileList | null) => {
      if (!files || files.length === 0) return;
      const remaining = MAX_PHOTOS - photoUrls.length;
      const toUpload = Array.from(files)
        .filter((f) => f.type.startsWith("image/"))
        .slice(0, remaining);
      if (toUpload.length === 0) return;

      setUploading(true);
      try {
        const urls = await Promise.all(toUpload.map((f) => businessApi.uploadPhoto(f)));
        setPhotoUrls((prev) => [...prev, ...urls]);
      } catch {
        toast.error(t("business.listing.uploadError") || "Upload failed");
      } finally {
        setUploading(false);
      }
    },
    [photoUrls.length, t],
  );

  const movePhoto = (from: number, to: number) => {
    setPhotoUrls((prev) => {
      const next = [...prev];
      const [pulled] = next.splice(from, 1);
      next.splice(to, 0, pulled);
      return next;
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (photoUrls.length === 0) {
      setError(t("business.listing.noPhotosError"));
      return;
    }
    setError(null);
    setSubmitting(true);
    try {
      const basePayload: CreateBusinessListingRequest = {
        title: form.title.trim(),
        description: form.description.trim(),
        activityType: form.activityType,
        city: form.city.trim(),
        addressLine: form.addressLine.trim() || undefined,
        latitude: form.latitude ? Number(form.latitude) : undefined,
        longitude: form.longitude ? Number(form.longitude) : undefined,
        ageFromMonths: form.ageFromMonths ? Number(form.ageFromMonths) : undefined,
        ageToMonths: form.ageToMonths ? Number(form.ageToMonths) : undefined,
        priceFromEur: form.priceFromEur ? Number(form.priceFromEur) : undefined,
        schedule: form.schedule.trim() || undefined,
        photoUrls,
      };

      let saved: BusinessListingDto;
      if (isEdit && existingId) {
        const updatePayload: UpdateBusinessListingRequest = {
          ...basePayload,
          isActive: form.isActive,
        };
        saved = await businessApi.updateListing(existingId, updatePayload);
        toast.success(t("business.listing.updated") || "Updated!");
      } else {
        saved = await businessApi.createListing(basePayload);
        toast.success(t("business.listing.created") || "Created!");
      }
      navigate(`/coaches/${saved.id}`);
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data) {
        const envelope = err.response.data as BusinessErrorEnvelope;
        setError(envelope.error || t("business.listing.saveError"));
      } else {
        setError(t("business.listing.saveError"));
      }
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <div className="max-w-3xl mx-auto px-4 py-20 text-center">
        <Loader2 className="h-6 w-6 animate-spin mx-auto text-gray-400" />
      </div>
    );
  }

  return (
    <div className="max-w-3xl mx-auto px-4 py-8">
      <button
        type="button"
        onClick={() => navigate(-1)}
        className="inline-flex items-center gap-1.5 text-sm text-gray-500 dark:text-gray-400 hover:text-primary mb-4"
      >
        <ArrowLeft size={14} />
        {t("common.back")}
      </button>

      <h1 className="text-2xl font-bold text-primary-dark dark:text-white mb-1">
        {isEdit ? t("business.listing.editHeading") : t("business.listing.createHeading")}
      </h1>
      <p className="text-sm text-gray-500 dark:text-gray-400 mb-6">
        {t("business.listing.formIntro")}
      </p>

      <form
        onSubmit={handleSubmit}
        className="bg-white dark:bg-[#2d2a42] border border-lavender/30 dark:border-white/10 rounded-2xl shadow-sm p-6 space-y-5"
      >
        <Field
          label={t("business.listing.titleLabel")}
          value={form.title}
          onChange={(v) => setForm((f) => ({ ...f, title: v }))}
          maxLength={150}
          required
        />

        <div>
          <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
            {t("business.listing.descriptionLabel")}
            <span className="text-red-500 ml-0.5">*</span>
          </label>
          <textarea
            required
            rows={5}
            maxLength={4000}
            value={form.description}
            onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
            placeholder={t("business.listing.descriptionPlaceholder")}
            className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40 resize-none"
          />
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
              {t("business.listing.activityLabel")}
              <span className="text-red-500 ml-0.5">*</span>
            </label>
            <select
              value={form.activityType}
              onChange={(e) =>
                setForm((f) => ({ ...f, activityType: Number(e.target.value) as ActivityType }))
              }
              className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
            >
              {ACTIVITY_OPTIONS.map((o) => (
                <option key={o.value} value={o.value}>
                  {t(o.key)}
                </option>
              ))}
            </select>
          </div>
          <Field
            label={t("business.listing.cityLabel")}
            value={form.city}
            onChange={(v) => setForm((f) => ({ ...f, city: v }))}
            maxLength={100}
            required
          />
        </div>

        <Field
          label={t("business.listing.addressLabel")}
          value={form.addressLine}
          onChange={(v) => setForm((f) => ({ ...f, addressLine: v }))}
          maxLength={300}
        />

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <Field
            label={t("business.listing.latLabel")}
            value={form.latitude}
            type="number"
            onChange={(v) => setForm((f) => ({ ...f, latitude: v }))}
            placeholder="42.6975"
          />
          <Field
            label={t("business.listing.lngLabel")}
            value={form.longitude}
            type="number"
            onChange={(v) => setForm((f) => ({ ...f, longitude: v }))}
            placeholder="23.3242"
          />
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <Field
            label={t("business.listing.ageFromLabel")}
            value={form.ageFromMonths}
            type="number"
            onChange={(v) => setForm((f) => ({ ...f, ageFromMonths: v }))}
            placeholder="6"
          />
          <Field
            label={t("business.listing.ageToLabel")}
            value={form.ageToMonths}
            type="number"
            onChange={(v) => setForm((f) => ({ ...f, ageToMonths: v }))}
            placeholder="72"
          />
          <Field
            label={t("business.listing.priceLabel")}
            value={form.priceFromEur}
            type="number"
            onChange={(v) => setForm((f) => ({ ...f, priceFromEur: v }))}
            placeholder="20.00"
          />
        </div>

        <div>
          <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
            {t("business.listing.scheduleLabel")}
          </label>
          <textarea
            rows={2}
            maxLength={500}
            value={form.schedule}
            onChange={(e) => setForm((f) => ({ ...f, schedule: e.target.value }))}
            placeholder={t("business.listing.schedulePlaceholder")}
            className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40 resize-none"
          />
        </div>

        {/* Photos */}
        <div>
          <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
            {t("business.listing.photosLabel")} ({photoUrls.length}/{MAX_PHOTOS})
            <span className="text-red-500 ml-0.5">*</span>
          </label>
          <div className="border-2 border-dashed border-lavender/40 dark:border-white/10 rounded-xl p-4 text-center">
            <input
              type="file"
              accept="image/*"
              multiple
              id="photo-input"
              onChange={(e) => handleAddFiles(e.target.files)}
              className="hidden"
            />
            <label
              htmlFor="photo-input"
              className="inline-flex items-center gap-2 cursor-pointer text-sm text-primary font-medium hover:underline"
            >
              {uploading ? (
                <Loader2 size={14} className="animate-spin" />
              ) : (
                <ImagePlus size={14} />
              )}
              {uploading
                ? t("business.listing.uploading")
                : t("business.listing.addPhotos")}
            </label>
          </div>
          {photoUrls.length > 0 && (
            <div className="mt-3 grid grid-cols-3 sm:grid-cols-5 gap-2">
              {photoUrls.map((url, i) => (
                <div
                  key={url + i}
                  className="relative aspect-square rounded-lg overflow-hidden border border-gray-100 dark:border-white/10 group"
                >
                  <img src={url} alt="" className="w-full h-full object-cover" />
                  {i === 0 && (
                    <span className="absolute top-1 left-1 text-[9px] font-semibold uppercase px-1.5 py-0.5 rounded bg-primary text-white">
                      {t("business.listing.cover")}
                    </span>
                  )}
                  <button
                    type="button"
                    onClick={() => setPhotoUrls((prev) => prev.filter((_, j) => j !== i))}
                    className="absolute top-1 right-1 p-1 rounded-full bg-black/60 text-white opacity-0 group-hover:opacity-100 transition-opacity"
                    aria-label="Remove photo"
                  >
                    <X size={11} />
                  </button>
                  {i > 0 && (
                    <button
                      type="button"
                      onClick={() => movePhoto(i, i - 1)}
                      className="absolute bottom-1 left-1 p-1 rounded bg-black/60 text-white opacity-0 group-hover:opacity-100 transition-opacity"
                      aria-label="Move earlier"
                    >
                      <GripVertical size={11} />
                    </button>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>

        {isEdit && (
          <label className="flex items-center gap-2 text-sm text-gray-700 dark:text-gray-200">
            <input
              type="checkbox"
              checked={form.isActive}
              onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.checked }))}
              className="rounded text-primary focus:ring-primary/40"
            />
            {t("business.listing.activeLabel")}
          </label>
        )}

        {error && (
          <p className="text-sm text-red-600 bg-red-50 dark:bg-red-500/10 rounded-lg px-3 py-2">
            {error}
          </p>
        )}

        <div className="pt-1 flex flex-col-reverse sm:flex-row gap-3 justify-end">
          <button
            type="button"
            onClick={() => navigate(-1)}
            className="px-5 py-2.5 rounded-xl border border-gray-200 dark:border-white/10 text-sm font-medium text-gray-600 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-white/5 transition-colors"
          >
            {t("common.cancel")}
          </button>
          <button
            type="submit"
            disabled={submitting || uploading}
            className="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl bg-primary text-white text-sm font-semibold shadow-sm hover:bg-primary/90 disabled:opacity-60 transition-colors"
          >
            {submitting ? (
              <Loader2 className="h-4 w-4 animate-spin" />
            ) : (
              <>
                <Save size={15} />
                {isEdit ? t("business.listing.saveChanges") : t("business.listing.publish")}
              </>
            )}
          </button>
        </div>
      </form>
    </div>
  );
}

interface FieldProps {
  label: string;
  value: string;
  onChange: (next: string) => void;
  type?: string;
  maxLength?: number;
  required?: boolean;
  placeholder?: string;
}
function Field({
  label,
  value,
  onChange,
  type = "text",
  maxLength,
  required,
  placeholder,
}: FieldProps) {
  return (
    <div>
      <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
        {label}
        {required && <span className="text-red-500 ml-0.5">*</span>}
      </label>
      <input
        type={type}
        value={value}
        required={required}
        maxLength={maxLength}
        placeholder={placeholder}
        onChange={(e) => onChange(e.target.value)}
        className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
      />
    </div>
  );
}
