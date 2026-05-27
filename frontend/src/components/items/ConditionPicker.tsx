import { useTranslation } from 'react-i18next';
import { ItemCondition } from '../../types/item';

const OPTIONS: { value: ItemCondition; labelKey: string; descKey: string; color: string }[] = [
  {
    value: ItemCondition.NewWithTags,
    labelKey: 'items.condition_new_with_tags',
    descKey: 'items.condition_new_with_tags_desc',
    color: 'border-emerald-400 bg-emerald-50 dark:bg-emerald-900/20 text-emerald-800 dark:text-emerald-300',
  },
  {
    value: ItemCondition.LikeNew,
    labelKey: 'items.condition_like_new',
    descKey: 'items.condition_like_new_desc',
    color: 'border-teal-400 bg-teal-50 dark:bg-teal-900/20 text-teal-800 dark:text-teal-300',
  },
  {
    value: ItemCondition.Good,
    labelKey: 'items.condition_good',
    descKey: 'items.condition_good_desc',
    color: 'border-blue-400 bg-blue-50 dark:bg-blue-900/20 text-blue-800 dark:text-blue-300',
  },
  {
    value: ItemCondition.Fair,
    labelKey: 'items.condition_fair',
    descKey: 'items.condition_fair_desc',
    color: 'border-amber-400 bg-amber-50 dark:bg-amber-900/20 text-amber-800 dark:text-amber-300',
  },
];

interface ConditionPickerProps {
  value: ItemCondition;
  onChange: (value: ItemCondition) => void;
}

export default function ConditionPicker({ value, onChange }: ConditionPickerProps) {
  const { t } = useTranslation();

  return (
    <div>
      <label className="block text-sm font-medium text-primary mb-2">
        {t('items.condition_label')}
      </label>
      <div className="grid grid-cols-2 gap-2">
        {OPTIONS.map((opt) => (
          <button
            key={opt.value}
            type="button"
            onClick={() => onChange(opt.value)}
            className={`text-left px-3 py-2.5 rounded-xl border-2 transition-all ${
              value === opt.value
                ? opt.color
                : 'border-gray-200 dark:border-white/10 text-gray-600 dark:text-gray-400 hover:border-gray-300 dark:hover:border-white/20'
            }`}
          >
            <p className="text-sm font-semibold leading-tight">{t(opt.labelKey)}</p>
            <p className="text-xs opacity-70 mt-0.5 leading-tight">{t(opt.descKey)}</p>
          </button>
        ))}
      </div>
    </div>
  );
}
