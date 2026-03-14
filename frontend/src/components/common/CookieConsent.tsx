import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { HiX } from 'react-icons/hi';
import Button from './Button';

function CookieCharacter() {
  return (
    <svg
      viewBox="0 0 100 100"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      className="w-full h-full"
      aria-hidden="true"
    >
      {/* Drop shadow behind cookie */}
      <ellipse cx="50" cy="96" rx="38" ry="5" fill="#00000022" />

      {/* Cookie body */}
      <circle cx="50" cy="50" r="46" fill="#F0A848" />
      <circle cx="50" cy="50" r="46" stroke="#C87830" strokeWidth="2.5" />

      {/* Inner lighter ring — baked look */}
      <circle cx="50" cy="50" r="40" fill="none" stroke="#F8C070" strokeWidth="5" opacity="0.35" />

      {/* Chocolate chips */}
      <ellipse cx="32" cy="36" rx="7"   ry="6"   fill="#3B1E08" transform="rotate(-18 32 36)" />
      <ellipse cx="57" cy="30" rx="6.5" ry="5.5" fill="#3B1E08" transform="rotate(10 57 30)"  />
      <ellipse cx="72" cy="48" rx="6"   ry="5.5" fill="#3B1E08" transform="rotate(20 72 48)"  />
      <ellipse cx="26" cy="58" rx="6"   ry="5.5" fill="#3B1E08" transform="rotate(-10 26 58)" />
      <ellipse cx="62" cy="62" rx="6.5" ry="6"   fill="#3B1E08" transform="rotate(12 62 62)"  />
      <ellipse cx="40" cy="68" rx="6"   ry="5.5" fill="#3B1E08" transform="rotate(-8 40 68)"  />

      {/* Eyes */}
      <circle cx="38" cy="52" r="7" fill="#1A0800" />
      <circle cx="60" cy="52" r="7" fill="#1A0800" />
      {/* Eye highlights */}
      <circle cx="41"  cy="49" r="2.6" fill="white" />
      <circle cx="63"  cy="49" r="2.6" fill="white" />
      <circle cx="37"  cy="55" r="1.2" fill="white" opacity="0.5" />
      <circle cx="59"  cy="55" r="1.2" fill="white" opacity="0.5" />

      {/* Rosy cheeks */}
      <ellipse cx="27" cy="62" rx="9" ry="6" fill="#FF8070" opacity="0.35" />
      <ellipse cx="71" cy="62" rx="9" ry="6" fill="#FF8070" opacity="0.35" />

      {/* Smile */}
      <path d="M 36 68 Q 50 82 64 68" stroke="#1A0800" strokeWidth="3" fill="none" strokeLinecap="round" />
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
        <div className="absolute -top-14 right-4 w-28 h-28 pointer-events-none z-10">
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
