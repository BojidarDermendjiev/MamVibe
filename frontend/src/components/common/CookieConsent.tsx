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
      {/* Cookie body */}
      <circle cx="50" cy="50" r="46" fill="#D4956B" />
      {/* Baked highlight — upper-left glow */}
      <ellipse cx="38" cy="36" rx="20" ry="15" fill="#E8B07A" opacity="0.55" />
      {/* Cookie outline */}
      <circle cx="50" cy="50" r="46" stroke="#A86030" strokeWidth="2.5" />

      {/* Chocolate chips — organic blob shapes */}
      <ellipse cx="29" cy="33" rx="7.5" ry="6"   fill="#3B1500" transform="rotate(-20 29 33)" />
      <ellipse cx="57" cy="27" rx="7"   ry="5.5" fill="#3B1500" transform="rotate(14 57 27)"  />
      <ellipse cx="74" cy="46" rx="6.5" ry="5.5" fill="#3B1500" transform="rotate(22 74 46)"  />
      <ellipse cx="21" cy="54" rx="6.5" ry="5.5" fill="#3B1500" transform="rotate(-12 21 54)" />
      <ellipse cx="66" cy="65" rx="7"   ry="6"   fill="#3B1500" transform="rotate(10 66 65)"  />
      <ellipse cx="34" cy="70" rx="7"   ry="5.5" fill="#3B1500" transform="rotate(-16 34 70)" />
      <ellipse cx="50" cy="36" rx="5.5" ry="5"   fill="#3B1500" transform="rotate(5 50 36)"   />

      {/* ── Face ── */}

      {/* Eyes — large kawaii circles */}
      <circle cx="37" cy="55" r="9" fill="#1A0800" />
      <circle cx="62" cy="55" r="9" fill="#1A0800" />
      {/* Eye main highlight */}
      <circle cx="41"  cy="51" r="3.8" fill="white" />
      <circle cx="66"  cy="51" r="3.8" fill="white" />
      {/* Eye tiny sparkle */}
      <circle cx="35.5" cy="59" r="1.5" fill="white" opacity="0.6" />
      <circle cx="60.5" cy="59" r="1.5" fill="white" opacity="0.6" />

      {/* Rosy cheeks */}
      <ellipse cx="24" cy="67" rx="11" ry="7.5" fill="#FF9090" opacity="0.5" />
      <ellipse cx="75" cy="67" rx="11" ry="7.5" fill="#FF9090" opacity="0.5" />

      {/* Open happy mouth */}
      <path d="M 37 69 Q 50 83 63 69" fill="#3B1500" />
      <path d="M 39 70 Q 50 80 61 70 Q 55 75 45 75 Z" fill="#D4694A" />
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
        <div className="absolute -top-16 right-2 w-32 h-32 pointer-events-none z-10">
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
