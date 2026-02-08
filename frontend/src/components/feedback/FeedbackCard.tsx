import { useTranslation } from 'react-i18next';
import { FeedbackCategory } from '../../types/feedback';
import type { Feedback } from '../../types/feedback';
import StarRating from './StarRating';

interface FeedbackCardProps {
  feedback: Feedback;
  onDelete?: (id: string) => void;
  canDelete?: boolean;
}

const categoryConfig: Record<number, { labelKey: string; color: string }> = {
  [FeedbackCategory.Praise]: { labelKey: 'feedback.cat_praise', color: 'bg-green-100 text-green-700' },
  [FeedbackCategory.Improvement]: { labelKey: 'feedback.cat_improvement', color: 'bg-amber-100 text-amber-700' },
  [FeedbackCategory.FeatureRequest]: { labelKey: 'feedback.cat_feature', color: 'bg-blue-100 text-blue-700' },
  [FeedbackCategory.BugReport]: { labelKey: 'feedback.cat_bug', color: 'bg-red-100 text-red-700' },
};

export default function FeedbackCard({ feedback, onDelete, canDelete }: FeedbackCardProps) {
  const { t } = useTranslation();
  const cat = categoryConfig[feedback.category] ?? categoryConfig[0];
  const date = new Date(feedback.createdAt).toLocaleDateString();

  return (
    <div className="bg-white rounded-xl border border-lavender/30 p-5 space-y-3 hover:shadow-md transition-shadow duration-300 animate-fade-in">
      <div className="flex items-start justify-between">
        <div className="flex items-center gap-3">
          {feedback.userAvatarUrl ? (
            <img src={feedback.userAvatarUrl} alt="" className="w-10 h-10 rounded-full object-cover" />
          ) : (
            <div className="w-10 h-10 rounded-full bg-lavender flex items-center justify-center text-white font-bold text-sm">
              {feedback.userDisplayName?.charAt(0)?.toUpperCase() ?? '?'}
            </div>
          )}
          <div>
            <p className="font-medium text-primary-dark">{feedback.userDisplayName ?? t('feedback.anonymous')}</p>
            <p className="text-xs text-gray-400">{date}</p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <span className={`text-xs font-medium px-2.5 py-1 rounded-full ${cat.color}`}>
            {t(cat.labelKey)}
          </span>
          {canDelete && onDelete && (
            <button
              onClick={() => onDelete(feedback.id)}
              className="text-gray-400 hover:text-red-500 transition-colors text-sm"
            >
              {t('feedback.delete')}
            </button>
          )}
        </div>
      </div>

      <StarRating value={feedback.rating} readonly size="sm" />

      <p className="text-gray-700 text-sm leading-relaxed whitespace-pre-wrap">{feedback.content}</p>
    </div>
  );
}
