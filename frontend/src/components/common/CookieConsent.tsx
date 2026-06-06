import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { X, Plus, Minus } from 'lucide-react';
import { Link } from 'react-router-dom';

// ─── Cookie character SVG (same kawaii cookie as before) ─────────────────────
function CookieCharacter() {
  return (
    <svg
      viewBox="0 0 100 100"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      className="w-full h-full"
      aria-hidden="true"
    >
      <circle cx="50" cy="50" r="46" fill="#D4956B" />
      <ellipse cx="38" cy="36" rx="20" ry="15" fill="#E8B07A" opacity="0.55" />
      <circle cx="50" cy="50" r="46" stroke="#A86030" strokeWidth="2.5" />
      <ellipse cx="29" cy="33" rx="7.5" ry="6"   fill="#3B1500" transform="rotate(-20 29 33)" />
      <ellipse cx="57" cy="27" rx="7"   ry="5.5" fill="#3B1500" transform="rotate(14 57 27)"  />
      <ellipse cx="74" cy="46" rx="6.5" ry="5.5" fill="#3B1500" transform="rotate(22 74 46)"  />
      <ellipse cx="21" cy="54" rx="6.5" ry="5.5" fill="#3B1500" transform="rotate(-12 21 54)" />
      <ellipse cx="66" cy="65" rx="7"   ry="6"   fill="#3B1500" transform="rotate(10 66 65)"  />
      <ellipse cx="34" cy="70" rx="7"   ry="5.5" fill="#3B1500" transform="rotate(-16 34 70)" />
      <ellipse cx="50" cy="36" rx="5.5" ry="5"   fill="#3B1500" transform="rotate(5 50 36)"   />
      <circle cx="37" cy="55" r="9" fill="#1A0800" />
      <circle cx="62" cy="55" r="9" fill="#1A0800" />
      <circle cx="41"  cy="51" r="3.8" fill="white" />
      <circle cx="66"  cy="51" r="3.8" fill="white" />
      <circle cx="35.5" cy="59" r="1.5" fill="white" opacity="0.6" />
      <circle cx="60.5" cy="59" r="1.5" fill="white" opacity="0.6" />
      <ellipse cx="24" cy="67" rx="11" ry="7.5" fill="#FF9090" opacity="0.5" />
      <ellipse cx="75" cy="67" rx="11" ry="7.5" fill="#FF9090" opacity="0.5" />
      <path d="M 37 69 Q 50 83 63 69" fill="#3B1500" />
      <path d="M 39 70 Q 50 80 61 70 Q 55 75 45 75 Z" fill="#D4694A" />
    </svg>
  );
}

// ─── Toggle switch ────────────────────────────────────────────────────────────
function Toggle({ checked, onChange }: { checked: boolean; onChange: () => void }) {
  return (
    <button
      role="switch"
      aria-checked={checked}
      onClick={onChange}
      className={[
        'relative inline-flex h-6 w-11 shrink-0 cursor-pointer rounded-full border-2 border-transparent',
        'transition-colors duration-200 focus:outline-none focus-visible:ring-2 focus-visible:ring-[#945c67]/50',
        checked ? 'bg-[#945c67]' : 'bg-gray-200 dark:bg-gray-600',
      ].join(' ')}
    >
      <span
        className={[
          'pointer-events-none inline-block h-5 w-5 rounded-full bg-white shadow-sm',
          'transform transition-transform duration-200',
          checked ? 'translate-x-5' : 'translate-x-0',
        ].join(' ')}
      />
      {checked && (
        <svg
          className="absolute right-1 top-1/2 -translate-y-1/2 w-3 h-3 text-white"
          viewBox="0 0 12 12"
          fill="none"
        >
          <path d="M2 6l3 3 5-5" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
      )}
    </button>
  );
}

// ─── Accordion row ────────────────────────────────────────────────────────────
interface AccordionRowProps {
  label: string;
  description?: string;
  control: React.ReactNode;
  expanded: boolean;
  onToggleExpand: () => void;
}

function AccordionRow({ label, description, control, expanded, onToggleExpand }: AccordionRowProps) {
  return (
    <div className="border border-[rgba(148,92,103,0.18)] dark:border-white/10 rounded-xl overflow-hidden">
      <div
        className="flex items-center justify-between px-4 py-3.5 bg-[#fdf7f2] dark:bg-[#2a2740] cursor-pointer select-none"
        onClick={onToggleExpand}
      >
        <div className="flex items-center gap-2">
          <span className="text-[#945c67] dark:text-[#c1c4e3]">
            {expanded ? <Minus className="w-4 h-4" /> : <Plus className="w-4 h-4" />}
          </span>
          <span className="text-sm font-semibold text-gray-800 dark:text-gray-100">{label}</span>
        </div>
        <div onClick={(e) => e.stopPropagation()}>{control}</div>
      </div>
      {expanded && description && (
        <div className="px-4 py-3 bg-white dark:bg-[#201d30] text-xs text-gray-500 dark:text-gray-400 leading-relaxed border-t border-[rgba(148,92,103,0.1)] dark:border-white/5">
          {description}
        </div>
      )}
    </div>
  );
}

// ─── Saved preferences type ───────────────────────────────────────────────────
interface CookiePrefs {
  functional: boolean;
  analytics: boolean;
  thirdParty: boolean;
}

function loadPrefs(): CookiePrefs {
  try {
    const raw = localStorage.getItem('cookiePreferences');
    if (raw) return JSON.parse(raw);
  } catch {
    // ignore
  }
  return { functional: true, analytics: true, thirdParty: false };
}

// ─── Main component ───────────────────────────────────────────────────────────
export default function CookieConsent() {
  const { t } = useTranslation();

  const [showModal, setShowModal] = useState(() => !localStorage.getItem('cookieConsent'));
  const [prefs, setPrefs] = useState<CookiePrefs>(loadPrefs);
  const [expanded, setExpanded] = useState<string | null>(null);

  const toggle = (key: keyof CookiePrefs) =>
    setPrefs((p) => ({ ...p, [key]: !p[key] }));

  const toggleExpand = (key: string) =>
    setExpanded((prev) => (prev === key ? null : key));

  const saveAndExit = () => {
    localStorage.setItem('cookieConsent', 'customized');
    localStorage.setItem('cookiePreferences', JSON.stringify(prefs));
    setShowModal(false);
  };

  return (
    <>
      {/* ── Floating cookie FAB ─────────────────────────────────────────── */}
      <button
        onClick={() => setShowModal(true)}
        aria-label={t('common.cookie_settings')}
        className={[
          'fixed bottom-4 left-4 z-40 w-14 h-14 rounded-full',
          'bg-[#f0d0c7] dark:bg-[#2d2a42]',
          'shadow-lg hover:shadow-xl',
          'flex items-center justify-center',
          'border-2 border-[rgba(148,92,103,0.25)] dark:border-[rgba(193,196,227,0.2)]',
          'transition-transform duration-200 hover:scale-110 active:scale-95',
          'animate-fade-in-slow',
        ].join(' ')}
      >
        <span className="w-9 h-9">
          <CookieCharacter />
        </span>
      </button>

      {/* ── Modal backdrop + dialog ─────────────────────────────────────── */}
      {showModal && (
        <div
          className="fixed inset-0 z-50 flex items-end sm:items-center justify-start sm:justify-start p-4"
          role="dialog"
          aria-modal="true"
          aria-label={t('common.cookie_settings_title')}
        >
          {/* backdrop — semi-transparent, clicking it does nothing (user must Save) */}
          <div className="absolute inset-0 bg-black/30 dark:bg-black/50 backdrop-blur-sm" />

          {/* card */}
          <div className="relative z-10 w-full max-w-sm sm:ml-2 sm:mb-0 mb-16 animate-fade-in-slow">
            <div className="bg-white dark:bg-[#2d2a42] rounded-2xl shadow-2xl border border-[rgba(148,92,103,0.18)] dark:border-white/10 overflow-hidden">

              {/* header */}
              <div className="flex items-start justify-between px-5 pt-5 pb-3">
                <div className="flex items-center gap-3">
                  <span className="w-8 h-8 shrink-0">
                    <CookieCharacter />
                  </span>
                  <h2 className="text-base font-bold text-gray-800 dark:text-gray-100 leading-tight">
                    {t('common.cookie_settings_title')}
                  </h2>
                </div>
                <button
                  onClick={saveAndExit}
                  className="p-1.5 rounded-full text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-[#f0d0c7]/60 dark:hover:bg-white/10 transition-colors shrink-0"
                  aria-label="Close"
                >
                  <X className="w-4 h-4" />
                </button>
              </div>

              {/* description */}
              <p className="px-5 pb-4 text-xs text-gray-500 dark:text-gray-400 leading-relaxed">
                {t('common.cookie_settings_desc')}{' '}
                <Link
                  to="/cookies"
                  className="text-[#945c67] dark:text-[#c1c4e3] underline underline-offset-2 hover:opacity-75"
                  onClick={saveAndExit}
                >
                  {t('footer.legal_cookies')}
                </Link>
              </p>

              {/* accordion sections */}
              <div className="px-4 space-y-2 pb-4">
                {/* 1 — Strictly Necessary */}
                <AccordionRow
                  label={t('common.cookie_strictly_necessary')}
                  description={t('common.cookie_strictly_necessary_desc')}
                  expanded={expanded === 'necessary'}
                  onToggleExpand={() => toggleExpand('necessary')}
                  control={
                    <span className="text-xs font-semibold text-[#945c67] dark:text-[#c1c4e3]">
                      {t('common.cookie_always_active')}
                    </span>
                  }
                />

                {/* 2 — Functional */}
                <AccordionRow
                  label={t('common.cookie_functional')}
                  description={t('common.cookie_functional_desc')}
                  expanded={expanded === 'functional'}
                  onToggleExpand={() => toggleExpand('functional')}
                  control={
                    <Toggle
                      checked={prefs.functional}
                      onChange={() => toggle('functional')}
                    />
                  }
                />

                {/* 3 — Analytics */}
                <AccordionRow
                  label={t('common.cookie_analytics')}
                  description={t('common.cookie_analytics_desc')}
                  expanded={expanded === 'analytics'}
                  onToggleExpand={() => toggleExpand('analytics')}
                  control={
                    <Toggle
                      checked={prefs.analytics}
                      onChange={() => toggle('analytics')}
                    />
                  }
                />

                {/* 4 — Third-party */}
                <AccordionRow
                  label={t('common.cookie_third_party')}
                  description={t('common.cookie_third_party_desc')}
                  expanded={expanded === 'thirdParty'}
                  onToggleExpand={() => toggleExpand('thirdParty')}
                  control={
                    <Toggle
                      checked={prefs.thirdParty}
                      onChange={() => toggle('thirdParty')}
                    />
                  }
                />
              </div>

              {/* footer — Save & Exit */}
              <div className="px-4 pb-5">
                <button
                  onClick={saveAndExit}
                  className={[
                    'w-full py-2.5 px-4 rounded-xl',
                    'text-sm font-bold tracking-wide text-white',
                    'bg-gradient-to-r from-[#945c67] to-[#3f4b7f]',
                    'hover:opacity-90 active:scale-[0.98]',
                    'transition-all duration-200 shadow-md hover:shadow-lg',
                  ].join(' ')}
                >
                  {t('common.cookie_save_exit')}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
