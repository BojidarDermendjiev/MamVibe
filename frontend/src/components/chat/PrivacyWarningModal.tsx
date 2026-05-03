import { ShieldAlert, Phone, Mail, CreditCard, Landmark, User } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import type { SensitiveMatch, SensitiveDataType } from '../../utils/sensitiveDataDetector';

const TYPE_ICONS: Record<SensitiveDataType, React.ElementType> = {
  phone: Phone,
  email: Mail,
  'national-id': User,
  iban: Landmark,
  card: CreditCard,
};

interface Props {
  matches: SensitiveMatch[];
  onEdit: () => void;
  onSendAnyway: () => void;
}

export default function PrivacyWarningModal({ matches, onEdit, onSendAnyway }: Props) {
  const { t } = useTranslation();

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm"
      onClick={onEdit}
    >
      <div
        className="bg-white dark:bg-[#2d2a42] rounded-2xl shadow-2xl max-w-md w-full mx-4 overflow-hidden"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="bg-amber-50 dark:bg-amber-900/20 px-6 pt-6 pb-5 flex items-start gap-4">
          <div className="flex-shrink-0 h-12 w-12 rounded-full bg-amber-100 dark:bg-amber-900/60 flex items-center justify-center">
            <ShieldAlert className="h-6 w-6 text-amber-600 dark:text-amber-400" />
          </div>
          <div>
            <h2 className="text-base font-bold text-gray-900 dark:text-gray-100">
              {t('privacy.warning_title')}
            </h2>
            <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
              {t('privacy.warning_subtitle')}
            </p>
          </div>
        </div>

        {/* Detected types */}
        <div className="px-6 py-4 border-b border-gray-100 dark:border-white/10">
          <p className="text-[11px] font-semibold text-gray-400 uppercase tracking-wider mb-3">
            {t('privacy.detected_label')}
          </p>
          <div className="flex flex-wrap gap-2">
            {matches.map((m) => {
              const Icon = TYPE_ICONS[m.type];
              return (
                <span
                  key={m.type}
                  className="flex items-center gap-1.5 px-3 py-1.5 bg-amber-100 dark:bg-amber-900/40 text-amber-800 dark:text-amber-200 rounded-full text-xs font-medium"
                >
                  <Icon className="h-3 w-3" />
                  {t(m.labelKey)}
                </span>
              );
            })}
          </div>
        </div>

        {/* Safety tips */}
        <div className="px-6 py-4 border-b border-gray-100 dark:border-white/10">
          <p className="text-[11px] font-semibold text-gray-400 uppercase tracking-wider mb-3">
            {t('privacy.tips_title')}
          </p>
          <ul className="space-y-2.5">
            {([1, 2, 3, 4] as const).map((i) => (
              <li key={i} className="flex items-start gap-2.5 text-sm text-gray-600 dark:text-gray-400">
                <span className="flex-shrink-0 mt-0.5 h-4 w-4 rounded-full bg-primary/10 flex items-center justify-center text-[10px] font-bold text-primary">
                  {i}
                </span>
                {t(`privacy.tip_${i}`)}
              </li>
            ))}
          </ul>
        </div>

        {/* Actions */}
        <div className="px-6 py-5 flex flex-col gap-2">
          <button
            onClick={onEdit}
            className="w-full py-3 rounded-xl bg-primary text-white text-sm font-semibold hover:bg-primary-dark transition-colors"
          >
            {t('privacy.btn_edit')}
          </button>
          <button
            onClick={onSendAnyway}
            className="w-full py-2 text-sm text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors"
          >
            {t('privacy.btn_send_anyway')}
          </button>
        </div>
      </div>
    </div>
  );
}
