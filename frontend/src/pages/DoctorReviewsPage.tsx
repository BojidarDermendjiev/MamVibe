import { useState, useEffect, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { Star, ExternalLink, User, Plus, X, Stethoscope, MapPin, Search, CheckCircle } from "lucide-react";
import { motion } from "framer-motion";
import { usePageSEO } from "@/hooks/useSEO";
import { doctorReviewsApi } from "../api/doctorReviewsApi";
import type { DoctorReviewDto, CreateDoctorReviewDto } from "../types/doctorReview";
import { useAuthStore } from "../store/authStore";

const EMPTY_FORM: CreateDoctorReviewDto = {
  doctorName: "",
  specialization: "",
  clinicName: "",
  city: "",
  rating: 5,
  content: "",
  superdocUrl: "",
};

function StarRating({ rating, max = 5 }: { rating: number; max?: number }) {
  return (
    <span className="flex gap-0.5">
      {Array.from({ length: max }, (_, i) => (
        <Star
          key={i}
          size={14}
          className={i < rating ? "fill-amber-400 text-amber-400" : "text-gray-300 dark:text-gray-600"}
        />
      ))}
    </span>
  );
}

function StarPicker({ value, onChange }: { value: number; onChange: (v: number) => void }) {
  return (
    <div className="flex gap-1">
      {[1, 2, 3, 4, 5].map((star) => (
        <button
          key={star}
          type="button"
          onClick={() => onChange(star)}
          className="focus:outline-none"
          aria-label={`Rate ${star} out of 5`}
        >
          <Star
            size={24}
            className={star <= value ? "fill-amber-400 text-amber-400" : "text-gray-300 dark:text-gray-600 hover:text-amber-300"}
          />
        </button>
      ))}
    </div>
  );
}

function DoctorAvatar({ name }: { name: string }) {
  const initials = name
    .split(" ")
    .slice(0, 2)
    .map((w) => w[0]?.toUpperCase() ?? "")
    .join("");
  return (
    <div className="w-11 h-11 rounded-full bg-primary/15 flex items-center justify-center flex-shrink-0 ring-2 ring-primary/20">
      <span className="text-primary font-bold text-sm">{initials || "Dr"}</span>
    </div>
  );
}

function SkeletonCard() {
  return (
    <div className="bg-[#ffffff] dark:bg-[#2d2a42] rounded-2xl p-5 border border-gray-100 dark:border-white/5 shadow-sm animate-pulse">
      <div className="flex items-start gap-4">
        <div className="w-11 h-11 rounded-full bg-gray-200 dark:bg-white/10 flex-shrink-0" />
        <div className="flex-1 space-y-2">
          <div className="h-4 bg-gray-200 dark:bg-white/10 rounded w-2/5" />
          <div className="h-3 bg-gray-200 dark:bg-white/10 rounded w-1/4" />
        </div>
        <div className="h-5 bg-gray-200 dark:bg-white/10 rounded w-16" />
      </div>
      <div className="mt-4 space-y-2">
        <div className="h-3 bg-gray-200 dark:bg-white/10 rounded w-full" />
        <div className="h-3 bg-gray-200 dark:bg-white/10 rounded w-5/6" />
        <div className="h-3 bg-gray-200 dark:bg-white/10 rounded w-4/6" />
      </div>
    </div>
  );
}

export default function DoctorReviewsPage() {
  const { t } = useTranslation();
  const { isAuthenticated, user } = useAuthStore();

  usePageSEO({
    title: "Doctor Reviews for Parents in Bulgaria",
    description:
      "Read parent reviews of pediatricians, gynecologists, and other doctors across Bulgaria. Community-shared experiences to help you find the right doctor for your family.",
    canonical: "https://mamvibe.com/doctor-reviews",
    structuredData: {
      "@context": "https://schema.org",
      "@type": "ItemList",
      name: "Doctor Reviews for Parents",
      description:
        "Community reviews of doctors across Bulgaria, shared by parents on MamVibe.",
      url: "https://mamvibe.com/doctor-reviews",
    },
  });

  const isAdmin = user?.roles?.includes("Admin") ?? false;

  const [reviews, setReviews] = useState<DoctorReviewDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [cityFilter, setCityFilter] = useState("");
  const [specializationFilter, setSpecializationFilter] = useState("");
  const [page, setPage] = useState(1);
  const [showModal, setShowModal] = useState(false);
  const [form, setForm] = useState<CreateDoctorReviewDto>(EMPTY_FORM);
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [submitSuccess, setSubmitSuccess] = useState(false);

  const fetchReviews = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await doctorReviewsApi.getAll({
        city: cityFilter || undefined,
        specialization: specializationFilter || undefined,
        page,
        pageSize: 20,
      });
      setReviews(data);
    } catch {
      setError("Failed to load reviews. Please try again.");
    } finally {
      setLoading(false);
    }
  }, [cityFilter, specializationFilter, page]);

  useEffect(() => {
    fetchReviews();
  }, [fetchReviews]);

  const handleFilterSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    fetchReviews();
  };

  const handleSubmitReview = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setSubmitError(null);
    try {
      await doctorReviewsApi.create({
        ...form,
        clinicName: form.clinicName || undefined,
        superdocUrl: form.superdocUrl || undefined,
      });
      setSubmitSuccess(true);
      setForm(EMPTY_FORM);
      setTimeout(() => {
        setShowModal(false);
        setSubmitSuccess(false);
      }, 2500);
    } catch {
      setSubmitError("Failed to submit review. Please check your input and try again.");
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Delete this review?")) return;
    try {
      await doctorReviewsApi.delete(id);
      setReviews((prev) => prev.filter((r) => r.id !== id));
    } catch {
      alert("Failed to delete review.");
    }
  };

  return (
    <div>
      {/* Hero */}
      <div className="bg-page py-12 px-4 mb-8">
        <div className="max-w-4xl mx-auto flex flex-col sm:flex-row items-center justify-between gap-6">
          <motion.div
            initial={{ opacity: 0, y: 16 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.45 }}
          >
            <div className="flex items-center gap-3 mb-2">
              <div className="w-10 h-10 rounded-2xl flex items-center justify-center" style={{ backgroundColor: "rgba(148,92,103,0.12)" }}>
                <Stethoscope className="w-5 h-5 text-primary" />
              </div>
              <h1 className="text-3xl font-bold text-primary-dark">
                {t("doctorReviews.title") || "Doctor Reviews"}
              </h1>
            </div>
            <p className="text-gray-500 dark:text-gray-400 text-sm max-w-md">
              {t("doctorReviews.subtitle") || "Community reviews in collaboration with superdoc.bg"}
            </p>
          </motion.div>
          {isAuthenticated && (
            <motion.button
              whileHover={{ scale: 1.03 }}
              whileTap={{ scale: 0.97 }}
              onClick={() => setShowModal(true)}
              className="flex-shrink-0 inline-flex items-center gap-2 px-5 py-2.5 bg-primary text-white rounded-xl text-sm font-semibold shadow-md hover:bg-primary/90 transition-colors"
            >
              <Plus size={15} />
              {t("doctorReviews.writeReview") || "Write a Review"}
            </motion.button>
          )}
        </div>
      </div>

      {/* Content */}
      <div className="max-w-4xl mx-auto px-4 pb-8">
        {/* Filter bar */}
        <form
          onSubmit={handleFilterSubmit}
          className="bg-[#ffffff] dark:bg-[#2d2a42] rounded-2xl p-4 border border-gray-100 dark:border-white/5 shadow-sm mb-8 flex flex-wrap gap-3 items-center"
        >
          <div className="relative flex-1 min-w-[150px]">
            <MapPin size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input
              type="text"
              placeholder={t("doctorReviews.filterCity") || "City"}
              value={cityFilter}
              onChange={(e) => setCityFilter(e.target.value)}
              className="w-full pl-8 pr-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
            />
          </div>
          <div className="relative flex-1 min-w-[150px]">
            <Stethoscope size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input
              type="text"
              placeholder={t("doctorReviews.filterSpecialization") || "Specialization"}
              value={specializationFilter}
              onChange={(e) => setSpecializationFilter(e.target.value)}
              className="w-full pl-8 pr-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
            />
          </div>
          <button
            type="submit"
            className="flex items-center gap-2 px-4 py-2 bg-primary text-white rounded-lg text-sm font-semibold hover:bg-primary/90 transition-colors"
          >
            <Search size={14} />
            {t("common.search") || "Search"}
          </button>
        </form>

        {/* Skeleton */}
        {loading && (
          <div className="space-y-4">
            {[0, 1, 2, 3].map((i) => (
              <SkeletonCard key={i} />
            ))}
          </div>
        )}

        {/* Error */}
        {!loading && error && (
          <div className="text-center py-10 text-red-500 text-sm">{error}</div>
        )}

        {/* Empty state */}
        {!loading && !error && reviews.length === 0 && (
          <div className="flex flex-col items-center justify-center py-20 text-center">
            <div className="w-16 h-16 rounded-full bg-primary/10 flex items-center justify-center mb-4">
              <Stethoscope size={28} className="text-primary/60" />
            </div>
            <p className="text-gray-500 dark:text-gray-400 font-medium">
              {t("doctorReviews.noReviews") || "No reviews found yet."}
            </p>
            <p className="text-gray-400 dark:text-gray-500 text-sm mt-1 mb-5">
              Be the first to share your experience!
            </p>
            {isAuthenticated && (
              <button
                onClick={() => setShowModal(true)}
                className="inline-flex items-center gap-2 px-4 py-2 bg-primary text-white rounded-xl text-sm font-semibold hover:bg-primary/90 transition-colors"
              >
                <Plus size={14} />
                Write a Review
              </button>
            )}
          </div>
        )}

        {/* Reviews list */}
        <h2 className="sr-only">Reviews list</h2>
        {!loading && !error && reviews.length > 0 && (
          <div className="space-y-4">
            {reviews.map((review, i) => (
              <motion.div
                key={review.id}
                initial={{ opacity: 0, y: 16 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.3, delay: i * 0.05 }}
                className="bg-[#ffffff] dark:bg-[#2d2a42] rounded-2xl p-5 border border-gray-100 dark:border-white/5 shadow-sm hover:shadow-md hover:-translate-y-0.5 transition-all duration-200"
              >
                {/* Top row: avatar + doctor info */}
                <div className="flex items-start gap-4">
                  <DoctorAvatar name={review.doctorName} />
                  <div className="flex-1 min-w-0">
                    <div className="flex flex-wrap items-center gap-2">
                      <span className="font-semibold text-gray-900 dark:text-white">
                        Dr. {review.doctorName}
                      </span>
                      <span className="text-xs px-2 py-0.5 rounded-full bg-primary/10 text-primary font-medium">
                        {review.specialization}
                      </span>
                    </div>
                    <div className="flex items-center gap-2 mt-1 flex-wrap">
                      <StarRating rating={review.rating} />
                      {review.city && (
                        <span className="flex items-center gap-0.5 text-xs text-gray-400">
                          <MapPin size={11} />
                          {review.city}
                        </span>
                      )}
                      {review.clinicName && (
                        <span className="text-xs text-gray-400">· {review.clinicName}</span>
                      )}
                    </div>
                  </div>
                  {review.superdocUrl && (
                    <a
                      href={review.superdocUrl}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="flex-shrink-0 flex items-center gap-1 text-xs text-blue-500 hover:text-blue-600 hover:underline transition-colors"
                    >
                      superdoc.bg
                      <ExternalLink size={10} />
                    </a>
                  )}
                </div>

                {/* Review text */}
                <p className="mt-4 text-sm text-gray-700 dark:text-gray-300 leading-relaxed">
                  {review.content}
                </p>

                {/* Footer */}
                <div className="mt-4 pt-3 border-t border-gray-100 dark:border-white/5 flex items-center justify-between">
                  <div className="flex items-center gap-1.5 text-xs text-gray-400">
                    <User size={11} />
                    <span>{review.authorDisplayName || "User"}</span>
                    <span>·</span>
                    <span>{new Date(review.createdAt).toLocaleDateString()}</span>
                  </div>
                  {(isAdmin || (isAuthenticated && review.userId === user?.id)) && (
                    <button
                      onClick={() => handleDelete(review.id)}
                      className="text-xs text-red-400 hover:text-red-500 transition-colors"
                    >
                      Delete
                    </button>
                  )}
                </div>
              </motion.div>
            ))}
          </div>
        )}

        {/* Pagination */}
        {!loading && (page > 1 || reviews.length === 20) && (
          <div className="flex items-center justify-center gap-3 mt-10">
            <button
              onClick={() => { setPage((p) => Math.max(1, p - 1)); window.scrollTo({ top: 0, behavior: "smooth" }); }}
              disabled={page === 1}
              className="px-4 py-2 rounded-lg bg-gray-100 dark:bg-white/10 text-sm font-medium disabled:opacity-40 hover:bg-gray-200 dark:hover:bg-white/15 transition-colors"
            >
              ← Previous
            </button>
            <span className="px-4 py-2 text-sm font-semibold text-gray-700 dark:text-gray-300 bg-primary/10 rounded-lg">
              Page {page}
            </span>
            <button
              onClick={() => { setPage((p) => p + 1); window.scrollTo({ top: 0, behavior: "smooth" }); }}
              disabled={reviews.length < 20}
              className="px-4 py-2 rounded-lg bg-gray-100 dark:bg-white/10 text-sm font-medium disabled:opacity-40 hover:bg-gray-200 dark:hover:bg-white/15 transition-colors"
            >
              Next →
            </button>
          </div>
        )}
      </div>

      {/* Write Review Modal */}
      {showModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
          <div className="bg-[#ffffff] dark:bg-[#2d2a42] rounded-2xl w-full max-w-lg shadow-2xl max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between p-5 border-b border-gray-100 dark:border-white/10">
              <h2 className="font-bold text-gray-900 dark:text-white">
                {t("doctorReviews.writeReview") || "Write a Review"}
              </h2>
              <button
                onClick={() => { setShowModal(false); setForm(EMPTY_FORM); setSubmitError(null); setSubmitSuccess(false); }}
                className="p-1.5 rounded-lg hover:bg-gray-100 dark:hover:bg-white/10 transition-colors"
              >
                <X size={18} className="text-gray-500" />
              </button>
            </div>

            {submitSuccess ? (
              <div className="p-10 text-center">
                <div className="flex justify-center mb-4">
                  <CheckCircle size={44} className="text-green-500" />
                </div>
                <p className="font-semibold text-gray-900 dark:text-white text-lg">Review submitted!</p>
                <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                  Your review will appear after admin approval.
                </p>
              </div>
            ) : (
              <form onSubmit={handleSubmitReview} className="p-5 space-y-4">
                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
                      Doctor Name *
                    </label>
                    <input
                      required
                      maxLength={100}
                      value={form.doctorName}
                      onChange={(e) => setForm((f) => ({ ...f, doctorName: e.target.value }))}
                      className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
                      placeholder="e.g. Ivan Petrov"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
                      Specialization *
                    </label>
                    <input
                      required
                      maxLength={100}
                      value={form.specialization}
                      onChange={(e) => setForm((f) => ({ ...f, specialization: e.target.value }))}
                      className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
                      placeholder="e.g. Pediatrician"
                    />
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
                      Clinic (optional)
                    </label>
                    <input
                      maxLength={150}
                      value={form.clinicName}
                      onChange={(e) => setForm((f) => ({ ...f, clinicName: e.target.value }))}
                      className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
                      placeholder="Clinic name"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
                      City *
                    </label>
                    <input
                      required
                      maxLength={100}
                      value={form.city}
                      onChange={(e) => setForm((f) => ({ ...f, city: e.target.value }))}
                      className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
                      placeholder="e.g. Sofia"
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
                    Rating *
                  </label>
                  <StarPicker value={form.rating} onChange={(v) => setForm((f) => ({ ...f, rating: v }))} />
                </div>

                <div>
                  <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
                    Your Experience *
                  </label>
                  <textarea
                    required
                    maxLength={2000}
                    rows={4}
                    value={form.content}
                    onChange={(e) => setForm((f) => ({ ...f, content: e.target.value }))}
                    className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40 resize-none"
                    placeholder="Share your experience with this doctor..."
                  />
                  <div className="text-right text-xs text-gray-400 mt-0.5">{form.content.length}/2000</div>
                </div>

                <div>
                  <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
                    Superdoc.bg link (optional)
                  </label>
                  <input
                    maxLength={2048}
                    value={form.superdocUrl}
                    onChange={(e) => setForm((f) => ({ ...f, superdocUrl: e.target.value }))}
                    className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
                    placeholder="https://superdoc.bg/..."
                  />
                </div>

                {submitError && (
                  <p className="text-sm text-red-500">{submitError}</p>
                )}

                <div className="flex gap-3 pt-1">
                  <button
                    type="button"
                    onClick={() => { setShowModal(false); setForm(EMPTY_FORM); setSubmitError(null); }}
                    className="flex-1 px-4 py-2 rounded-xl border border-gray-200 dark:border-white/10 text-sm font-medium text-gray-600 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-white/5 transition-colors"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    disabled={submitting}
                    className="flex-1 px-4 py-2 rounded-xl bg-primary text-white text-sm font-semibold hover:bg-primary/90 transition-colors disabled:opacity-60"
                  >
                    {submitting ? "Submitting..." : "Submit Review"}
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
