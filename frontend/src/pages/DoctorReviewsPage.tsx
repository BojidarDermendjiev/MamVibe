import { useState, useEffect, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { Star, ExternalLink, User, Plus, X, Clock } from "lucide-react";
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
  isAnonymous: false,
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

export default function DoctorReviewsPage() {
  const { t } = useTranslation();
  const { isAuthenticated, user } = useAuthStore();
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
    <div className="max-w-4xl mx-auto px-4 py-8">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">
            {t("doctorReviews.title") || "Doctor Reviews"}
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
            {t("doctorReviews.subtitle") || "Community reviews in collaboration with superdoc.bg"}
          </p>
        </div>
        {isAuthenticated && (
          <button
            onClick={() => setShowModal(true)}
            className="flex items-center gap-2 px-4 py-2 bg-primary text-white rounded-xl text-sm font-semibold hover:bg-primary/90 transition-colors"
          >
            <Plus size={16} />
            {t("doctorReviews.writeReview") || "Write a Review"}
          </button>
        )}
      </div>

      {/* Filters */}
      <form onSubmit={handleFilterSubmit} className="flex flex-wrap gap-3 mb-6">
        <input
          type="text"
          placeholder={t("doctorReviews.filterCity") || "City"}
          value={cityFilter}
          onChange={(e) => setCityFilter(e.target.value)}
          className="flex-1 min-w-[160px] px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-white dark:bg-[#2d2a42] text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
        />
        <input
          type="text"
          placeholder={t("doctorReviews.filterSpecialization") || "Specialization"}
          value={specializationFilter}
          onChange={(e) => setSpecializationFilter(e.target.value)}
          className="flex-1 min-w-[160px] px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-white dark:bg-[#2d2a42] text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
        />
        <button
          type="submit"
          className="px-4 py-2 bg-primary/10 text-primary rounded-lg text-sm font-medium hover:bg-primary/20 transition-colors"
        >
          {t("common.search") || "Search"}
        </button>
      </form>

      {/* Content */}
      {loading && (
        <div className="text-center py-12 text-gray-400">Loading...</div>
      )}
      {error && (
        <div className="text-center py-8 text-red-500">{error}</div>
      )}
      {!loading && !error && reviews.length === 0 && (
        <div className="text-center py-12 text-gray-400 dark:text-gray-500">
          {t("doctorReviews.noReviews") || "No reviews found. Be the first to share your experience!"}
        </div>
      )}

      <div className="space-y-4">
        {reviews.map((review) => (
          <div
            key={review.id}
            className="bg-white dark:bg-[#2d2a42] rounded-xl p-5 border border-gray-100 dark:border-white/5 shadow-sm"
          >
            <div className="flex items-start justify-between gap-3">
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2 flex-wrap">
                  <span className="font-semibold text-gray-900 dark:text-white">
                    Dr. {review.doctorName}
                  </span>
                  <span className="text-xs px-2 py-0.5 rounded-full bg-primary/10 text-primary font-medium">
                    {review.specialization}
                  </span>
                  {review.clinicName && (
                    <span className="text-xs text-gray-500 dark:text-gray-400">
                      {review.clinicName}
                    </span>
                  )}
                </div>
                <div className="flex items-center gap-3 mt-1.5">
                  <StarRating rating={review.rating} />
                  <span className="text-xs text-gray-400">{review.city}</span>
                </div>
              </div>
              <div className="flex items-center gap-2 flex-shrink-0">
                {review.superdocUrl && (
                  <a
                    href={review.superdocUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-xs flex items-center gap-1 text-blue-500 hover:underline"
                  >
                    superdoc.bg
                    <ExternalLink size={10} />
                  </a>
                )}
              </div>
            </div>

            <p className="mt-3 text-sm text-gray-700 dark:text-gray-300 leading-relaxed">
              {review.content}
            </p>

            <div className="mt-3 flex items-center justify-between">
              <div className="flex items-center gap-1.5 text-xs text-gray-400">
                {review.isAnonymous ? (
                  <span>Anonymous</span>
                ) : (
                  <>
                    <User size={12} />
                    <span>{review.authorDisplayName || "User"}</span>
                  </>
                )}
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
          </div>
        ))}
      </div>

      {/* Pagination */}
      {(page > 1 || reviews.length === 20) && (
        <div className="flex items-center justify-center gap-3 mt-8">
          <button
            onClick={() => { setPage((p) => Math.max(1, p - 1)); window.scrollTo({ top: 0, behavior: 'smooth' }); }}
            disabled={page === 1}
            className="px-4 py-2 rounded-lg bg-gray-100 dark:bg-white/10 text-sm font-medium disabled:opacity-40 hover:bg-gray-200 dark:hover:bg-white/15 transition-colors"
          >
            ← Previous
          </button>
          <span className="px-4 py-2 text-sm font-semibold text-gray-700 dark:text-gray-300 bg-primary/10 rounded-lg">
            Page {page}
          </span>
          <button
            onClick={() => { setPage((p) => p + 1); window.scrollTo({ top: 0, behavior: 'smooth' }); }}
            disabled={reviews.length < 20}
            className="px-4 py-2 rounded-lg bg-gray-100 dark:bg-white/10 text-sm font-medium disabled:opacity-40 hover:bg-gray-200 dark:hover:bg-white/15 transition-colors"
          >
            Next →
          </button>
        </div>
      )}

      {/* Write Review Modal */}
      {showModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
          <div className="bg-white dark:bg-[#2d2a42] rounded-2xl w-full max-w-lg shadow-2xl max-h-[90vh] overflow-y-auto">
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
              <div className="p-8 text-center">
                <div className="flex justify-center mb-3">
                  <Clock size={40} className="text-amber-500" />
                </div>
                <p className="font-semibold text-gray-900 dark:text-white">Review submitted!</p>
                <p className="text-sm text-gray-500 mt-1">Your review will appear after admin approval.</p>
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

              <label className="flex items-center gap-2 cursor-pointer select-none">
                <input
                  type="checkbox"
                  checked={form.isAnonymous}
                  onChange={(e) => setForm((f) => ({ ...f, isAnonymous: e.target.checked }))}
                  className="rounded"
                />
                <span className="text-sm text-gray-700 dark:text-gray-300">Post anonymously</span>
              </label>

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
