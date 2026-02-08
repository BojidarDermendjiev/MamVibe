import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { feedbackApi } from '../api/feedbackApi';
import { FeedbackCategory } from '../types/feedback';
import type { Feedback, FeedbackCategory as FeedbackCategoryType } from '../types/feedback';
import { useAuthStore } from '../store/authStore';
import StarRating from '../components/feedback/StarRating';
import FeedbackCard from '../components/feedback/FeedbackCard';
import Button from '../components/common/Button';
import Pagination from '../components/common/Pagination';

const categoryOptions: { value: FeedbackCategoryType; labelKey: string; icon: string }[] = [
  { value: FeedbackCategory.Praise, labelKey: 'feedback.cat_praise', icon: '❤️' },
  { value: FeedbackCategory.Improvement, labelKey: 'feedback.cat_improvement', icon: '💡' },
  { value: FeedbackCategory.FeatureRequest, labelKey: 'feedback.cat_feature', icon: '🚀' },
  { value: FeedbackCategory.BugReport, labelKey: 'feedback.cat_bug', icon: '🐛' },
];

export default function FeedbackPage() {
  const { t } = useTranslation();
  const { user } = useAuthStore();
  const [loading, setLoading] = useState(false);
  const [feedbacks, setFeedbacks] = useState<Feedback[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
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

  const loadFeedbacks = async (p: number) => {
    setListLoading(true);
    try {
      const { data } = await feedbackApi.getAll(p, 8);
      setFeedbacks(data.items);
      setTotalPages(data.totalPages);
      setPage(data.page);
    } catch {
      // silently fail
    } finally {
      setListLoading(false);
    }
  };

  useEffect(() => {
    loadFeedbacks(1);
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
      loadFeedbacks(1);
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
      loadFeedbacks(page);
    } catch {
      toast.error(t('common.error'));
    }
  };

  return (
    <div className="max-w-3xl mx-auto px-4 py-8 space-y-10 animate-fade-in">
      {/* Header */}
      <div className="text-center">
        <h1 className="text-3xl font-bold text-primary-dark">{t('feedback.title')}</h1>
        <p className="text-gray-500 mt-2 max-w-lg mx-auto">{t('feedback.subtitle')}</p>
      </div>

      {/* Feedback Form */}
      <form onSubmit={handleSubmit} className="bg-white rounded-2xl border border-lavender/30 shadow-sm p-6 md:p-8 space-y-6">
        <h2 className="text-lg font-semibold text-primary-dark">{t('feedback.share')}</h2>

        {/* Star Rating */}
        <div>
          <label className="block text-sm font-medium text-primary-dark mb-2">{t('feedback.rating_label')}</label>
          <StarRating value={form.rating} onChange={(rating) => setForm({ ...form, rating })} size="lg" />
        </div>

        {/* Category Selector */}
        <div>
          <label className="block text-sm font-medium text-primary-dark mb-2">{t('feedback.category_label')}</label>
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

        {/* Content */}
        <div>
          <label className="block text-sm font-medium text-primary-dark mb-2">{t('feedback.content_label')}</label>
          <textarea
            value={form.content}
            onChange={(e) => setForm({ ...form, content: e.target.value })}
            placeholder={t('feedback.content_placeholder')}
            rows={5}
            maxLength={2000}
            className="w-full px-4 py-3 rounded-xl border border-lavender/50 bg-cream focus:outline-none focus:ring-2 focus:ring-primary-dark/30 focus:border-primary-dark resize-none text-sm placeholder:text-gray-400"
          />
          <p className="text-xs text-gray-400 text-right mt-1">{form.content.length}/2000</p>
        </div>

        {/* Contactable Checkbox */}
        <label className="flex items-center gap-3 cursor-pointer">
          <input
            type="checkbox"
            checked={form.isContactable}
            onChange={(e) => setForm({ ...form, isContactable: e.target.checked })}
            className="w-4 h-4 rounded border-gray-300 text-primary-dark focus:ring-primary-dark"
          />
          <span className="text-sm text-gray-600">{t('feedback.contactable')}</span>
        </label>

        {/* Submit */}
        <Button type="submit" fullWidth isLoading={loading}>
          {t('feedback.submit')}
        </Button>
      </form>

      {/* Community Feedback */}
      <div className="space-y-4">
        <h2 className="text-xl font-semibold text-primary-dark">{t('feedback.community')}</h2>

        {listLoading ? (
          <div className="text-center py-10 text-gray-400">{t('common.loading')}</div>
        ) : feedbacks.length === 0 ? (
          <div className="text-center py-10 bg-white rounded-xl border border-lavender/30">
            <p className="text-gray-400">{t('feedback.no_feedback')}</p>
          </div>
        ) : (
          <>
            <div className="space-y-4">
              {feedbacks.map((fb) => (
                <FeedbackCard
                  key={fb.id}
                  feedback={fb}
                  canDelete={fb.userId === user?.id}
                  onDelete={handleDelete}
                />
              ))}
            </div>
            {totalPages > 1 && (
              <Pagination
                currentPage={page}
                totalPages={totalPages}
                onPageChange={(p) => loadFeedbacks(p)}
              />
            )}
          </>
        )}
      </div>
    </div>
  );
}
