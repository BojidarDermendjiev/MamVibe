import { useTranslation } from 'react-i18next';
import { ItemCondition } from '../../types/item';

const CONFIG: Record<number, { label: string; classes: string }> = {
  [ItemCondition.NewWithTags]: {
    label: 'items.condition_new_with_tags',
    classes: 'bg-emerald-100 text-emerald-800 dark:bg-emerald-900/30 dark:text-emerald-300',
  },
  [ItemCondition.LikeNew]: {
    label: 'items.condition_like_new',
    classes: 'bg-teal-100 text-teal-800 dark:bg-teal-900/30 dark:text-teal-300',
  },
  [ItemCondition.Good]: {
    label: 'items.condition_good',
    classes: 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300',
  },
  [ItemCondition.Fair]: {
    label: 'items.condition_fair',
    classes: 'bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-300',
  },
};

interface ConditionBadgeProps {
  condition: ItemCondition;
  size?: 'sm' | 'md';
}

export default function ConditionBadge({ condition, size = 'md' }: ConditionBadgeProps) {
  const { t } = useTranslation();
  const config = CONFIG[condition];
  if (!config) return null;

  return (
    <span className={`inline-flex items-center rounded-full font-medium ${config.classes} ${
      size === 'sm' ? 'px-2 py-0.5 text-xs' : 'px-2.5 py-1 text-xs'
    }`}>
      {t(config.label)}
    </span>
  );
}
