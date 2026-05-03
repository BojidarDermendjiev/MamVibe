import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Lock } from 'lucide-react';

interface PresetTier {
  value: number;
  emoji: string;
  labelKey: string;
  subtitle?: string;
  activeColor: string;
  activeBorder: string;
}

const PRESET_AMOUNTS: PresetTier[] = [
  { value: 5,  emoji: '☕', labelKey: 'donate.tier_coffee', activeColor: '#fef3c7', activeBorder: '#f59e0b' },
  { value: 10, emoji: '💗', labelKey: 'donate.tier_love',   activeColor: '#fce7f3', activeBorder: '#ec4899' },
  { value: 25, emoji: '⭐', labelKey: 'donate.tier_hero',   activeColor: '#ede9fe', activeBorder: '#8b5cf6' },
  { value: 0,  emoji: '🦄', labelKey: 'donate.tier_custom', subtitle: "I'm magical ✨", activeColor: '#f0d0c7', activeBorder: '#945c67' },
];

export default function DonationPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [selected, setSelected] = useState<number>(10);
  const [customAmount, setCustomAmount] = useState('');
  const [isCustom, setIsCustom] = useState(false);

  const effectiveAmount = isCustom ? parseFloat(customAmount) || 0 : selected;
  const canDonate = effectiveAmount >= 1;

  const handlePreset = (value: number, custom: boolean) => {
    setIsCustom(custom);
    if (!custom) setSelected(value);
  };

  const handleDonate = () => {
    if (!canDonate) return;
    navigate('/donate/card', { state: { amount: effectiveAmount } });
  };

  const donateLabel = canDonate
    ? t('donate.donate_btn').replace('{amount}', effectiveAmount.toFixed(2))
    : t('donate.donate_btn_default');

  return (
    <div className="min-h-[80vh] flex flex-col items-center justify-center px-4 py-16">
      <motion.div
        initial={{ opacity: 0, y: 24 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5 }}
        className="w-full max-w-lg"
      >
        {/* Header — "Small vibes, big impact!" */}
        <div className="text-center mb-10">
          <motion.div
            initial={{ opacity: 0, scale: 0.8 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ type: 'spring', stiffness: 260, damping: 18, delay: 0.1 }}
            className="mb-5"
          >
            <p className="text-4xl md:text-5xl font-black text-gray-800 dark:text-gray-100 leading-tight">
              Small vibes,
            </p>
            <div className="flex items-center justify-center gap-3 mt-1">
              <div className="relative inline-block">
                <span className="text-4xl md:text-5xl font-black text-primary leading-tight">
                  big impact!
                </span>
                <div className="absolute -bottom-1.5 left-0 right-0 h-1.5 bg-yellow-400 rounded-full" />
              </div>
              <span className="text-4xl select-none">💗</span>
            </div>
          </motion.div>

          <p className="text-gray-500 text-base max-w-sm mx-auto leading-relaxed mt-4">
            {t('donate.subtitle')}
          </p>
        </div>

        {/* Amount picker card */}
        <div className="bg-white rounded-2xl border shadow-sm p-6 mb-4" style={{ borderColor: 'rgba(148,92,103,0.18)' }}>
          {/* Section label */}
          <div className="flex justify-center mb-5">
            <span className="px-5 py-1.5 rounded-full bg-primary text-white text-xs font-bold uppercase tracking-widest shadow-sm">
              Choose your superpower
            </span>
          </div>

          <div className="grid grid-cols-2 gap-3 mb-5">
            {PRESET_AMOUNTS.map(({ value, emoji, labelKey, subtitle, activeColor, activeBorder }) => {
              const isThisCustom = value === 0;
              const isActive = isThisCustom ? isCustom : !isCustom && selected === value;
              return (
                <motion.button
                  key={labelKey}
                  whileHover={{ scale: 1.03 }}
                  whileTap={{ scale: 0.97 }}
                  onClick={() => handlePreset(value, isThisCustom)}
                  className="flex flex-col items-center gap-1.5 p-5 rounded-xl border-2 transition-all duration-200 cursor-pointer"
                  style={{
                    backgroundColor: isActive ? activeColor : '#f9fafb',
                    borderColor: isActive ? activeBorder : 'transparent',
                  }}
                >
                  <span className="text-4xl leading-none select-none">{emoji}</span>
                  {!isThisCustom && (
                    <span
                      className="text-xl font-bold"
                      style={{ color: isActive ? activeBorder : '#374151' }}
                    >
                      {value} лв.
                    </span>
                  )}
                  <span
                    className="text-xs font-semibold"
                    style={{ color: isActive ? activeBorder : '#6b7280' }}
                  >
                    {t(labelKey)}
                  </span>
                  {subtitle && (
                    <span
                      className="text-xs"
                      style={{ color: isActive ? activeBorder : '#9ca3af' }}
                    >
                      {subtitle}
                    </span>
                  )}
                </motion.button>
              );
            })}
          </div>

          {/* Custom amount input */}
          {isCustom && (
            <motion.div
              initial={{ opacity: 0, height: 0 }}
              animate={{ opacity: 1, height: 'auto' }}
              transition={{ duration: 0.2 }}
              className="mb-1"
            >
              <input
                type="number"
                min="1"
                max="500"
                step="0.50"
                value={customAmount}
                onChange={(e) => setCustomAmount(e.target.value)}
                placeholder={t('donate.custom_placeholder')}
                className="w-full px-4 py-3 rounded-xl border-2 focus:outline-none text-lg font-semibold text-gray-700 placeholder:text-gray-400 transition-colors bg-peach-light/40"
                style={{ borderColor: 'rgba(148,92,103,0.3)' }}
                autoFocus
              />
            </motion.div>
          )}

          {/* Donate button */}
          <motion.button
            whileHover={canDonate ? { scale: 1.02 } : {}}
            whileTap={canDonate ? { scale: 0.98 } : {}}
            onClick={handleDonate}
            disabled={!canDonate}
            className={`
              w-full mt-4 py-3.5 rounded-xl font-semibold text-base transition-all duration-200
              ${canDonate
                ? 'bg-primary text-white shadow-md hover:bg-primary-dark hover:shadow-lg cursor-pointer'
                : 'bg-gray-100 text-gray-400 cursor-not-allowed'
              }
            `}
          >
            {donateLabel} {canDonate && '→'}
          </motion.button>
        </div>

        {/* Footer notes */}
        <div className="text-center space-y-1.5">
          <p className="flex items-center justify-center gap-1.5 text-xs text-gray-400">
            <Lock className="w-3 h-3" />
            {t('donate.secure_note')}
          </p>
          <p className="text-xs text-gray-400 italic">
            {t('donate.fun_note')}
          </p>
        </div>

        {/* ── Fun sticky notes + piggy bank ── */}
        <motion.div
          initial={{ opacity: 0, y: 32 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6, delay: 0.4 }}
          className="relative mt-10 flex flex-col items-center"
        >
          {/* Scattered decorations */}
          <span className="absolute -left-4 top-6 text-xl select-none opacity-70">💜</span>
          <span className="absolute -right-2 top-0 text-sm select-none opacity-60">✨</span>
          <span className="absolute right-6 bottom-32 text-lg select-none opacity-50">💜</span>
          <span className="absolute -left-2 bottom-40 text-xs select-none opacity-60">✨</span>

          {/* Yellow sticky note */}
          <div className="relative -rotate-2 bg-yellow-200 rounded-md px-6 py-5 shadow-md w-72 z-10">
            {/* Tape strip */}
            <div className="absolute -top-3.5 left-1/2 -translate-x-1/2 w-14 h-6 bg-red-200/70 rounded-sm rotate-3 shadow-sm" />
            <p className="font-bold text-gray-700 text-sm mb-2.5">Together we can:</p>
            <ul className="space-y-1.5">
              {['Support families', 'Share baby items', 'Build a kinder community'].map((item) => (
                <li key={item} className="flex items-center gap-2 text-sm text-gray-600">
                  <span className="text-primary font-bold">✓</span>
                  {item}
                </li>
              ))}
            </ul>
          </div>

          {/* Purple sticky note */}
          <div className="relative rotate-3 bg-purple-200 rounded-md px-6 py-5 shadow-md w-60 -mt-3 -ml-8 z-0">
            <p className="font-semibold text-gray-700 text-sm italic leading-relaxed">
              Every little bit creates a big change! 🤍
            </p>
          </div>

          {/* Falling coins + piggy bank */}
          <div className="flex flex-col items-center mt-4">
            <div className="flex items-end gap-1 mb-1">
              <motion.span
                animate={{ y: [0, -6, 0] }}
                transition={{ duration: 1.2, repeat: Infinity, delay: 0 }}
                className="text-2xl select-none"
              >
                🪙
              </motion.span>
              <motion.span
                animate={{ y: [0, -10, 0] }}
                transition={{ duration: 1.2, repeat: Infinity, delay: 0.3 }}
                className="text-3xl select-none"
              >
                🪙
              </motion.span>
              <motion.span
                animate={{ y: [0, -5, 0] }}
                transition={{ duration: 1.2, repeat: Infinity, delay: 0.6 }}
                className="text-xl select-none"
              >
                🪙
              </motion.span>
            </div>
            <span className="text-8xl select-none leading-none">🐷</span>
          </div>
        </motion.div>
      </motion.div>
    </div>
  );
}
