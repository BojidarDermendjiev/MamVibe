import { useTranslation } from 'react-i18next';

interface TabErrorStateProps {
  onRetry: () => void;
}

export default function TabErrorState({ onRetry }: TabErrorStateProps) {
  const { t } = useTranslation();
  return (
    <div className="text-center py-12">
      <p className="text-red-500 mb-3">{t('common.error_loading')}</p>
      <button onClick={onRetry} className="text-primary text-sm underline">
        {t('common.retry')}
      </button>
    </div>
  );
}
