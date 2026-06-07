import { useTranslation } from 'react-i18next';
import { clsx } from 'clsx';
import { ProfileType } from '../../types/auth';

interface ProfileTypeSelectorProps {
  value: ProfileType;
  onChange: (type: ProfileType) => void;
}

const options = [
  { type: ProfileType.Female, icon: '👩', colorClass: 'border-peach bg-peach/10 text-mauve' },
  { type: ProfileType.Male, icon: '👨', colorClass: 'border-lavender bg-lavender/10 text-primary' },
  { type: ProfileType.Family, icon: '👨‍👩‍👧', colorClass: 'border-mauve/40 bg-mauve/10 text-mauve' },
];

export default function ProfileTypeSelector({ value, onChange }: ProfileTypeSelectorProps) {
  const { t } = useTranslation();

  const labels = {
    [ProfileType.Female]: t('common.female'),
    [ProfileType.Male]: t('common.male'),
    [ProfileType.Family]: t('common.family'),
  };

  return (
    <div>
      <label className="block text-sm font-medium text-primary mb-2">
        {t('auth.profile_type')}
      </label>
      <div className="grid grid-cols-3 gap-2">
        {options.map((opt) => (
          <button
            key={opt.type}
            type="button"
            onClick={() => onChange(opt.type)}
            className={clsx(
              'flex flex-col items-center gap-1 p-2 rounded-xl border-2 transition-all duration-200',
              value === opt.type
                ? `${opt.colorClass} shadow-sm scale-105`
                : 'border-gray-200 hover:border-lavender bg-white'
            )}
          >
            <span className="text-2xl">{opt.icon}</span>
            <span className="text-xs font-medium">{labels[opt.type]}</span>
          </button>
        ))}
      </div>
    </div>
  );
}
