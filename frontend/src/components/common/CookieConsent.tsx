import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { HiX } from 'react-icons/hi';
import Button from './Button';

function CookieCharacter() {
  return (
    <svg
      viewBox="0 0 130 130"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      className="w-full h-full"
      aria-hidden="true"
    >
      {/* Waving arm (rendered first so body overlaps it) */}
      <path d="M 96 44 Q 104 32 108 20" stroke="#C47A4A" strokeWidth="9" strokeLinecap="round" />
      <circle cx="110" cy="16" r="12" fill="#D4956B" />
      <circle cx="110" cy="16" r="12" stroke="#C47A4A" strokeWidth="1.5" />
      <ellipse cx="109" cy="14" rx="4" ry="3" fill="#5C3A1E" transform="rotate(-10 109 14)" />

      {/* Cookie body */}
      <circle cx="62" cy="74" r="52" fill="#D4956B" />
      <circle cx="62" cy="74" r="52" stroke="#C47A4A" strokeWidth="2" />

      {/* Chocolate chips */}
      <ellipse cx="46" cy="62" rx="8" ry="7"   fill="#5C3A1E" transform="rotate(-12 46 62)" />
      <ellipse cx="72" cy="56" rx="7" ry="6"   fill="#5C3A1E" transform="rotate(8 72 56)"  />
      <ellipse cx="50" cy="84" rx="8" ry="6.5" fill="#5C3A1E" transform="rotate(-6 50 84)" />
      <ellipse cx="76" cy="80" rx="6.5" ry="6" fill="#5C3A1E" transform="rotate(14 76 80)" />
      <ellipse cx="38" cy="84" rx="5.5" ry="5" fill="#5C3A1E" transform="rotate(-10 38 84)"/>

      {/* Eyes */}
      <circle cx="49" cy="68" r="6.5" fill="#2D1B0E" />
      <circle cx="70" cy="68" r="6.5" fill="#2D1B0E" />
      {/* Eye shine */}
      <circle cx="51.5" cy="65.5" r="2.2" fill="white" />
      <circle cx="72.5" cy="65.5" r="2.2" fill="white" />

      {/* Smile */}
      <path d="M 47 80 Q 60 91 74 80" stroke="#2D1B0E" strokeWidth="2.8" fill="none" strokeLinecap="round" />
    </svg>
  );
}

export default function CookieConsent() {
  const { t } = useTranslation();
  const [isVisible, setIsVisible] = useState(() => !localStorage.getItem('cookieConsent'));

  const accept = () => {
    localStorage.setItem('cookieConsent', 'accepted');
    setIsVisible(false);
  };

  const reject = () => {
    localStorage.setItem('cookieConsent', 'rejected');
    setIsVisible(false);
  };

  if (!isVisible) return null;

  return (
    <div
      className="fixed bottom-4 left-4 z-50 animate-fade-in-slow"
      role="dialog"
      aria-label="Cookie consent"
      aria-modal="false"
    >
      <div className="relative">
        {/* Cookie character — peeks out from top-right of card */}
        <div className="absolute -top-16 -right-10 w-32 h-32 pointer-events-none drop-shadow-lg z-10">
          <CookieCharacter />
        </div>

        {/* Card */}
        <div className="relative bg-white dark:bg-[#2d2a42] rounded-2xl shadow-2xl p-6 w-[300px] sm:w-[320px] border border-gray-100 dark:border-white/10">
          {/* Close */}
          <button
            onClick={reject}
            className="absolute top-3 right-3 p-1.5 rounded-full text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-white/10 transition-colors"
            aria-label="Close"
          >
            <HiX className="w-4 h-4" />
          </button>

          <h2 className="text-lg font-bold text-gray-800 dark:text-gray-100 mb-2 pr-6">
            {t('common.cookie_title')}
          </h2>
          <p className="text-sm text-gray-500 dark:text-gray-400 leading-relaxed mb-5">
            {t('common.cookie_message')}
          </p>

          <div className="flex gap-3">
            <Button onClick={accept} variant="primary" size="sm" className="flex-1 rounded-full font-semibold">
              {t('common.cookie_accept')}
            </Button>
            <Button onClick={reject} variant="ghost" size="sm" className="flex-1 rounded-full">
              {t('common.cookie_reject')}
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}
