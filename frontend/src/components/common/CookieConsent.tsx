import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import Button from './Button';

export default function CookieConsent() {
  const { t } = useTranslation();
  const [isVisible, setIsVisible] = useState(() => !localStorage.getItem('cookieConsent'));

  const accept = () => {
    localStorage.setItem('cookieConsent', 'accepted');
    setIsVisible(false);
  };

  if (!isVisible) return null;

  return (
    <div className="fixed bottom-0 left-0 right-0 z-50 bg-white border-t-2 border-primary/20 shadow-2xl">
      <div className="max-w-5xl mx-auto px-6 py-6 flex flex-col sm:flex-row items-center gap-5">
        <div className="flex-shrink-0 text-5xl" role="img" aria-label="cookie">
          🍪
        </div>
        <div className="flex-1 text-center sm:text-left">
          <p className="text-base font-semibold text-primary-dark mb-1">
            {t('common.cookie_settings')}
          </p>
          <p className="text-sm text-gray-600 leading-relaxed">
            {t('common.cookie_message')}
          </p>
        </div>
        <Button onClick={accept} className="px-8 py-3 text-base font-semibold">
          {t('common.cookie_accept')}
        </Button>
      </div>
    </div>
  );
}
