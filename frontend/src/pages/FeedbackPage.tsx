import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { Paperclip, Mic, CornerDownLeft } from 'lucide-react';
import { feedbackApi } from '../api/feedbackApi';
import { FeedbackCategory } from '../types/feedback';
import type { Feedback, FeedbackCategory as FeedbackCategoryType } from '../types/feedback';
import { useAuthStore } from '../store/authStore';
import StarRating from '../components/feedback/StarRating';

const categoryOptions: { value: FeedbackCategoryType; labelKey: string; icon: string }[] = [
  { value: FeedbackCategory.Praise, labelKey: 'feedback.cat_praise', icon: '❤️' },
  { value: FeedbackCategory.Improvement, labelKey: 'feedback.cat_improvement', icon: '💡' },
  { value: FeedbackCategory.FeatureRequest, labelKey: 'feedback.cat_feature', icon: '🚀' },
  { value: FeedbackCategory.BugReport, labelKey: 'feedback.cat_bug', icon: '🐛' },
];

const categoryColor: Record<number, string> = {
  [FeedbackCategory.Praise]: 'bg-green-100 text-green-700',
  [FeedbackCategory.Improvement]: 'bg-amber-100 text-amber-700',
  [FeedbackCategory.FeatureRequest]: 'bg-blue-100 text-blue-700',
  [FeedbackCategory.BugReport]: 'bg-red-100 text-red-700',
};

export default function FeedbackPage() {
  const { t } = useTranslation();
  const { user } = useAuthStore();
  const [loading, setLoading] = useState(false);
  const [feedbacks, setFeedbacks] = useState<Feedback[]>([]);
  const [listLoading, setListLoading] = useState(true);

  const [form, setForm] = useState<{
    rating: number;
    category: FeedbackCategoryType;
    content: string;
    isContactable: boolean;
  }>({
    rating: 5,
    category: FeedbackCategory.Praise,
    content: '',
    isContactable: false,
  });

  const loadFeedbacks = async () => {
    setListLoading(true);
    try {
      const { data } = await feedbackApi.getAll(1, 20);
      setFeedbacks(data.items);
    } catch {
      // silently fail
    } finally {
      setListLoading(false);
    }
  };

  useEffect(() => {
    loadFeedbacks();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.content.trim()) {
      toast.error(t('feedback.content_required'));
      return;
    }
    setLoading(true);
    try {
      await feedbackApi.create(form);
      toast.success(t('feedback.submitted'));
      setForm({ rating: 5, category: FeedbackCategory.Praise, content: '', isContactable: false });
      loadFeedbacks();
    } catch {
      toast.error(t('feedback.submit_error'));
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await feedbackApi.delete(id);
      toast.success(t('feedback.deleted'));
      loadFeedbacks();
    } catch {
      toast.error(t('common.error'));
    }
  };

  // Exactly 2 copies — the -50% CSS animation moves one full copy off-screen,
  // landing perfectly on the identical second copy. Loop is seamless forever.
  const marqueeItems = feedbacks.length > 0
    ? [...feedbacks, ...feedbacks]
    : [];

  return (
    <div className="animate-fade-in">
      {/* ── Header ── */}
      <section className="bg-white py-12 px-4 text-center">
        <h1 className="text-3xl font-bold text-primary-dark">{t('feedback.title')}</h1>
        <p className="text-gray-500 mt-2 max-w-lg mx-auto">{t('feedback.subtitle')}</p>
      </section>

      {/* ── Community Testimonials Marquee ── */}
      {!listLoading && feedbacks.length > 0 && (
        <section className="bg-peach py-12 px-0">
          <h2 className="text-xl font-semibold text-primary-dark text-center mb-8 px-4">
            {t('feedback.community')}
          </h2>

          {/* Single overflow container — group lets child pause on hover */}
          <div className="group relative w-full overflow-hidden [--duration:45s]">
            <div className="flex w-max animate-marquee gap-4 group-hover:[animation-play-state:paused]">
              {marqueeItems.map((fb, i) => (
                <FeedbackMarqueeCard
                  key={`${fb.id}-${i}`}
                  feedback={fb}
                  canDelete={fb.userId === user?.id}
                  onDelete={handleDelete}
                  t={t}
                />
              ))}
            </div>

            {/* Gradient fades */}
            <div className="pointer-events-none absolute inset-y-0 left-0 hidden w-32 bg-gradient-to-r from-peach dark:from-[#201d30] sm:block" />
            <div className="pointer-events-none absolute inset-y-0 right-0 hidden w-32 bg-gradient-to-l from-peach dark:from-[#201d30] sm:block" />
          </div>
        </section>
      )}

      {listLoading && (
        <section className="py-10 text-center text-gray-400 bg-peach">
          {t('common.loading')}
        </section>
      )}

      {!listLoading && feedbacks.length === 0 && (
        <section className="py-10 text-center bg-peach">
          <p className="text-gray-400">{t('feedback.no_feedback')}</p>
        </section>
      )}

      {/* ── Feedback Form ── */}
      <section className="bg-white py-16 px-4">
        <div className="max-w-2xl mx-auto">
          <form
            onSubmit={handleSubmit}
            className="bg-white rounded-2xl border border-lavender/30 shadow-sm p-6 md:p-8 space-y-6"
          >
            <h2 className="text-lg font-semibold text-primary-dark">{t('feedback.share')}</h2>

            {/* Star Rating */}
            <div>
              <label className="block text-sm font-medium text-primary-dark mb-2">
                {t('feedback.rating_label')}
              </label>
              <StarRating value={form.rating} onChange={(rating) => setForm({ ...form, rating })} size="lg" />
            </div>

            {/* Category Selector */}
            <div>
              <label className="block text-sm font-medium text-primary-dark mb-2">
                {t('feedback.category_label')}
              </label>
              <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
                {categoryOptions.map((opt) => (
                  <button
                    key={opt.value}
                    type="button"
                    onClick={() => setForm({ ...form, category: opt.value })}
                    className={`flex flex-col items-center gap-1.5 p-3 rounded-xl border-2 transition-all text-sm font-medium ${
                      form.category === opt.value
                        ? 'border-primary-dark bg-primary-dark/5 text-primary-dark'
                        : 'border-gray-200 text-gray-500 hover:border-lavender'
                    }`}
                  >
                    <span className="text-xl">{opt.icon}</span>
                    <span>{t(opt.labelKey)}</span>
                  </button>
                ))}
              </div>
            </div>

            {/* Content — chat-input style */}
            <div>
              <label className="block text-sm font-medium text-primary-dark mb-2">
                {t('feedback.content_label')}
              </label>

              <div className="relative rounded-lg border border-lavender/50 bg-white dark:bg-[#2a2740] focus-within:ring-1 focus-within:ring-primary-dark/30 focus-within:border-primary-dark transition-shadow p-1">
                {/* Textarea */}
                <textarea
                  value={form.content}
                  onChange={(e) => setForm({ ...form, content: e.target.value })}
                  placeholder={t('feedback.content_placeholder')}
                  maxLength={2000}
                  className="min-h-[80px] max-h-48 w-full resize-none rounded-lg bg-white dark:bg-[#2a2740] dark:text-gray-200 dark:placeholder:text-gray-500 border-0 px-3 py-3 text-sm text-gray-700 placeholder:text-gray-400 focus:outline-none focus:ring-0 shadow-none"
                />

                {/* Toolbar */}
                <div className="flex items-center px-2 pb-2 pt-0 gap-1">
                  {/* Paperclip */}
                  <button
                    type="button"
                    title="Attach file"
                    className="p-1.5 rounded-md text-gray-400 hover:text-gray-600 hover:bg-gray-100 transition-colors"
                  >
                    <Paperclip size={15} />
                    <span className="sr-only">Attach file</span>
                  </button>

                  {/* Mic */}
                  <button
                    type="button"
                    title="Use microphone"
                    className="p-1.5 rounded-md text-gray-400 hover:text-gray-600 hover:bg-gray-100 transition-colors"
                  >
                    <Mic size={15} />
                    <span className="sr-only">Use Microphone</span>
                  </button>

                  {/* Character count + Send */}
                  <div className="ml-auto flex items-center gap-2">
                    <span className="text-xs text-gray-400">{form.content.length}/2000</span>
                    <button
                      type="submit"
                      disabled={loading}
                      className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-md bg-primary text-white text-xs font-semibold hover:bg-primary-dark transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      {loading ? t('feedback.submit') : t('feedback.submit')}
                      <CornerDownLeft size={13} />
                    </button>
                  </div>
                </div>
              </div>
            </div>

            {/* Contactable */}
            <label className="flex items-center gap-3 cursor-pointer">
              <input
                type="checkbox"
                checked={form.isContactable}
                onChange={(e) => setForm({ ...form, isContactable: e.target.checked })}
                className="w-4 h-4 rounded border-gray-300 text-primary-dark focus:ring-primary-dark"
              />
              <span className="text-sm text-gray-600">{t('feedback.contactable')}</span>
            </label>

          </form>
        </div>
      </section>
    </div>
  );
}

// ── Marquee card ──
function FeedbackMarqueeCard({
  feedback,
  canDelete,
  onDelete,
  t,
}: {
  feedback: Feedback;
  canDelete: boolean;
  onDelete: (id: string) => void;
  t: (key: string) => string;
}) {
  const cat = categoryColor[feedback.category] ?? 'bg-gray-100 text-gray-600';
  const date = new Date(feedback.createdAt).toLocaleDateString();

  return (
    <div className="flex flex-col rounded-xl border border-lavender/30 bg-white dark:bg-[#2d2a42] p-5 max-w-[300px] min-w-[280px] transition-shadow duration-300 hover:shadow-md">
      <div className="flex items-center gap-3 mb-3">
        {feedback.userAvatarUrl ? (
          <img
            src={feedback.userAvatarUrl}
            alt=""
            className="w-10 h-10 rounded-full object-cover shrink-0"
          />
        ) : (
          <div className="w-10 h-10 rounded-full bg-lavender flex items-center justify-center text-white font-bold text-sm shrink-0">
            {feedback.userDisplayName?.charAt(0)?.toUpperCase() ?? '?'}
          </div>
        )}
        <div className="min-w-0">
          <p className="font-medium text-primary-dark text-sm truncate">
            {feedback.userDisplayName ?? t('feedback.anonymous')}
          </p>
          <p className="text-xs text-gray-400">{date}</p>
        </div>
        <span className={`ml-auto text-xs font-medium px-2 py-0.5 rounded-full shrink-0 ${cat}`}>
          {'★'.repeat(feedback.rating)}
        </span>
      </div>

      <p className="text-gray-600 text-sm leading-relaxed line-clamp-3 flex-1">
        {feedback.content}
      </p>

      {canDelete && (
        <button
          onClick={() => onDelete(feedback.id)}
          className="mt-3 text-xs text-gray-400 hover:text-red-500 transition-colors self-end"
        >
          {t('feedback.delete')}
        </button>
      )}
    </div>
  );
}
