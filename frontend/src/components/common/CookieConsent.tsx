import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { HiX } from 'react-icons/hi';
import Button from './Button';

function CookieCharacter() {
  return (
    <svg
      viewBox="0 0 160 165"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      className="w-full h-full"
      aria-hidden="true"
    >
      {/* Waving arm — rendered first so body overlaps the base */}
      <path d="M 112 62 C 122 46 130 30 128 16" stroke="#BF6A30" strokeWidth="11" strokeLinecap="round" />
      {/* Hand */}
      <circle cx="126" cy="12" r="14" fill="#E8A55A" />
      <circle cx="126" cy="12" r="14" stroke="#BF6A30" strokeWidth="2" />
      <ellipse cx="125" cy="10" rx="5.5" ry="4.5" fill="#3D2010" transform="rotate(-15 125 10)" />

      {/* Cookie body */}
      <circle cx="70" cy="92" r="60" fill="#E8A55A" />
      <circle cx="70" cy="92" r="60" stroke="#BF6A30" strokeWidth="2.5" />
      {/* Subtle baked-texture ring */}
      <circle cx="70" cy="92" r="55" stroke="#D4884A" strokeWidth="1.2" opacity="0.25" strokeDasharray="4 9" />

      {/* Chocolate chips */}
      <ellipse cx="52" cy="80"  rx="9.5" ry="8.5" fill="#3D2010" transform="rotate(-14 52 80)"  />
      <ellipse cx="80" cy="74"  rx="8.5" ry="7.5" fill="#3D2010" transform="rotate(11 80 74)"   />
      <ellipse cx="56" cy="104" rx="9"   ry="8"   fill="#3D2010" transform="rotate(-7 56 104)"  />
      <ellipse cx="84" cy="100" rx="8"   ry="7.5" fill="#3D2010" transform="rotate(16 84 100)"  />
      <ellipse cx="38" cy="103" rx="7"   ry="6.5" fill="#3D2010" transform="rotate(-11 38 103)" />
      <ellipse cx="94" cy="80"  rx="7"   ry="6"   fill="#3D2010" transform="rotate(7 94 80)"    />

      {/* Eyes */}
      <circle cx="54" cy="87" r="8.5" fill="#1A0A00" />
      <circle cx="78" cy="87" r="8.5" fill="#1A0A00" />
      {/* Main highlight */}
      <circle cx="57.5" cy="83.5" r="3.2" fill="white" />
      <circle cx="81.5" cy="83.5" r="3.2" fill="white" />
      {/* Secondary sparkle */}
      <circle cx="52.5" cy="91" r="1.5" fill="white" opacity="0.55" />
      <circle cx="76.5" cy="91" r="1.5" fill="white" opacity="0.55" />

      {/* Rosy cheeks */}
      <ellipse cx="41"  cy="98" rx="10" ry="7" fill="#FF8888" opacity="0.32" />
      <ellipse cx="91"  cy="98" rx="10" ry="7" fill="#FF8888" opacity="0.32" />

      {/* Smile */}
      <path d="M 52 100 Q 68 117 86 100" stroke="#1A0A00" strokeWidth="3.5" fill="none" strokeLinecap="round" />
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
        <div className="absolute -top-20 -right-8 w-36 h-36 pointer-events-none drop-shadow-lg z-10">
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
