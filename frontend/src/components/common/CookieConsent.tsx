import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import Button from './Button';

export default function CookieConsent() {
  const { t } = useTranslation();
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    const consent = localStorage.getItem('cookieConsent');
    if (!consent) {
      setIsVisible(true);
    }
  }, []);

  const accept = () => {
    localStorage.setItem('cookieConsent', 'accepted');
    setIsVisible(false);
  };

  if (!isVisible) return null;

  return (
    <div className="fixed bottom-0 left-0 right-0 z-50 bg-white border-t border-lavender shadow-lg p-4">
      <div className="max-w-5xl mx-auto flex flex-col sm:flex-row items-center gap-4">
        <p className="text-sm text-gray-600 flex-1">
          {t('common.cookie_message')}
        </p>
        <Button size="sm" onClick={accept}>
          {t('common.cookie_accept')}
        </Button>
      </div>
    </div>
  );
}
