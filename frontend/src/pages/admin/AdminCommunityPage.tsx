import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Star, MapPin, Check, Trash2 } from 'lucide-react';
import { doctorReviewsApi } from '../../api/doctorReviewsApi';
import { childFriendlyPlacesApi } from '../../api/childFriendlyPlacesApi';
import type { DoctorReviewDto } from '../../types/doctorReview';
import type { ChildFriendlyPlaceDto } from '../../types/childFriendlyPlace';
import LoadingSpinner from '../../components/common/LoadingSpinner';

function StarRow({ rating }: { rating: number }) {
  return (
    <span className="flex gap-0.5">
      {Array.from({ length: 5 }, (_, i) => (
        <Star
          key={i}
          size={12}
          className={i < rating ? 'fill-amber-400 text-amber-400' : 'text-gray-300 dark:text-gray-600'}
        />
      ))}
    </span>
  );
}

export default function AdminCommunityPage() {
  const { t } = useTranslation();
  const [reviews, setReviews] = useState<DoctorReviewDto[]>([]);
  const [places, setPlaces] = useState<ChildFriendlyPlaceDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState<string | null>(null);

  useEffect(() => {
    Promise.all([
      doctorReviewsApi.getPending(),
      childFriendlyPlacesApi.getPending(),
    ]).then(([r, p]) => {
      setReviews(r);
      setPlaces(p);
      setLoading(false);
    }).catch(() => setLoading(false));
  }, []);

  const handleApproveReview = async (id: string) => {
    setActionLoading(id);
    try {
      await doctorReviewsApi.approve(id);
      setReviews((prev) => prev.filter((r) => r.id !== id));
    } finally {
      setActionLoading(null);
    }
  };

  const handleDeleteReview = async (id: string) => {
    if (!confirm('Delete this review?')) return;
    setActionLoading(id);
    try {
      await doctorReviewsApi.adminDelete(id);
      setReviews((prev) => prev.filter((r) => r.id !== id));
    } finally {
      setActionLoading(null);
    }
  };

  const handleApprovePlace = async (id: string) => {
    setActionLoading(id);
    try {
      await childFriendlyPlacesApi.approve(id);
      setPlaces((prev) => prev.filter((p) => p.id !== id));
    } finally {
      setActionLoading(null);
    }
  };

  const handleDeletePlace = async (id: string) => {
    if (!confirm('Delete this place?')) return;
    setActionLoading(id);
    try {
      await childFriendlyPlacesApi.adminDelete(id);
      setPlaces((prev) => prev.filter((p) => p.id !== id));
    } finally {
      setActionLoading(null);
    }
  };

  if (loading) return <LoadingSpinner size="lg" className="py-20" />;

  return (
    <div>
      <h1 className="text-3xl font-bold text-[#364153] dark:text-[#bdb9bc] mb-6">
        {t('admin.community')}
      </h1>

      {/* Pending Doctor Reviews */}
      <section className="mb-10">
        <h2 className="text-xl font-semibold text-[#364153] dark:text-[#bdb9bc] mb-4 flex items-center gap-2">
          {t('admin.pending_doctor_reviews')}
          {reviews.length > 0 && (
            <span className="bg-amber-500 text-white text-xs font-bold px-2 py-0.5 rounded-full">
              {reviews.length}
            </span>
          )}
        </h2>

        {reviews.length === 0 ? (
          <p className="text-gray-500 dark:text-gray-400 bg-white dark:bg-[#2d2a42] rounded-xl p-4 border border-lavender/30 dark:border-white/10">
            {t('admin.no_pending')}
          </p>
        ) : (
          <div className="bg-white dark:bg-[#2d2a42] rounded-xl border border-amber-200 dark:border-amber-900/40 divide-y divide-amber-100 dark:divide-amber-900/30">
            {reviews.map((review) => (
              <div key={review.id} className="p-4 flex items-start gap-4">
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 flex-wrap">
                    <span className="font-medium text-gray-900 dark:text-white text-sm">
                      Dr. {review.doctorName}
                    </span>
                    <span className="text-xs px-2 py-0.5 rounded-full bg-primary/10 text-primary">
                      {review.specialization}
                    </span>
                    <span className="text-xs text-gray-400">{review.city}</span>
                  </div>
                  <div className="flex items-center gap-2 mt-1">
                    <StarRow rating={review.rating} />
                    <span className="text-xs text-gray-400">
                      by {review.isAnonymous ? 'Anonymous' : (review.authorDisplayName ?? 'User')}
                    </span>
                  </div>
                  <p className="mt-2 text-sm text-gray-600 dark:text-gray-300 line-clamp-3">
                    {review.content}
                  </p>
                  <span className="text-xs text-gray-400 mt-1 block">
                    {new Date(review.createdAt).toLocaleDateString()}
                  </span>
                </div>
                <div className="flex gap-2 flex-shrink-0">
                  <button
                    onClick={() => handleApproveReview(review.id)}
                    disabled={actionLoading === review.id}
                    className="flex items-center gap-1 px-3 py-1.5 bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400 rounded-lg text-xs font-medium hover:bg-green-200 dark:hover:bg-green-900/50 transition-colors disabled:opacity-50"
                  >
                    <Check size={13} />
                    {t('admin.approve_item')}
                  </button>
                  <button
                    onClick={() => handleDeleteReview(review.id)}
                    disabled={actionLoading === review.id}
                    className="flex items-center gap-1 px-3 py-1.5 bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400 rounded-lg text-xs font-medium hover:bg-red-200 dark:hover:bg-red-900/50 transition-colors disabled:opacity-50"
                  >
                    <Trash2 size={13} />
                    {t('admin.delete_item')}
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      {/* Pending Child-Friendly Places */}
      <section>
        <h2 className="text-xl font-semibold text-[#364153] dark:text-[#bdb9bc] mb-4 flex items-center gap-2">
          {t('admin.pending_places')}
          {places.length > 0 && (
            <span className="bg-amber-500 text-white text-xs font-bold px-2 py-0.5 rounded-full">
              {places.length}
            </span>
          )}
        </h2>

        {places.length === 0 ? (
          <p className="text-gray-500 dark:text-gray-400 bg-white dark:bg-[#2d2a42] rounded-xl p-4 border border-lavender/30 dark:border-white/10">
            {t('admin.no_pending')}
          </p>
        ) : (
          <div className="bg-white dark:bg-[#2d2a42] rounded-xl border border-amber-200 dark:border-amber-900/40 divide-y divide-amber-100 dark:divide-amber-900/30">
            {places.map((place) => (
              <div key={place.id} className="p-4 flex items-start gap-4">
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 flex-wrap">
                    <span className="font-medium text-gray-900 dark:text-white text-sm">
                      {place.name}
                    </span>
                  </div>
                  <div className="flex items-center gap-1.5 mt-1 text-xs text-gray-400">
                    <MapPin size={11} />
                    <span>{place.city}</span>
                    {place.address && <span>· {place.address}</span>}
                  </div>
                  <p className="mt-2 text-sm text-gray-600 dark:text-gray-300 line-clamp-3">
                    {place.description}
                  </p>
                  <div className="flex items-center gap-2 mt-1">
                    <span className="text-xs text-gray-400">
                      by {place.authorDisplayName ?? 'User'} · {new Date(place.createdAt).toLocaleDateString()}
                    </span>
                    {place.website && (
                      <a
                        href={place.website}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-xs text-blue-500 hover:underline"
                      >
                        Website
                      </a>
                    )}
                  </div>
                </div>
                <div className="flex gap-2 flex-shrink-0">
                  <button
                    onClick={() => handleApprovePlace(place.id)}
                    disabled={actionLoading === place.id}
                    className="flex items-center gap-1 px-3 py-1.5 bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400 rounded-lg text-xs font-medium hover:bg-green-200 dark:hover:bg-green-900/50 transition-colors disabled:opacity-50"
                  >
                    <Check size={13} />
                    {t('admin.approve_item')}
                  </button>
                  <button
                    onClick={() => handleDeletePlace(place.id)}
                    disabled={actionLoading === place.id}
                    className="flex items-center gap-1 px-3 py-1.5 bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400 rounded-lg text-xs font-medium hover:bg-red-200 dark:hover:bg-red-900/50 transition-colors disabled:opacity-50"
                  >
                    <Trash2 size={13} />
                    {t('admin.delete_item')}
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </section>
    </div>
  );
}
