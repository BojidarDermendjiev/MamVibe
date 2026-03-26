import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Heart, Coffee, Sparkles, Star, Lock } from 'lucide-react';

interface PresetTier {
  value: number;
  icon: React.ElementType;
  labelKey: string;
  activeColor: string; // inline bg when selected
  activeBorder: string; // inline border when selected
}

const PRESET_AMOUNTS: PresetTier[] = [
  { value: 5,  icon: Coffee,   labelKey: 'donate.tier_coffee', activeColor: '#fef3c7', activeBorder: '#f59e0b' },
  { value: 10, icon: Heart,    labelKey: 'donate.tier_love',   activeColor: '#fce7f3', activeBorder: '#ec4899' },
  { value: 25, icon: Star,     labelKey: 'donate.tier_hero',   activeColor: '#ede9fe', activeBorder: '#8b5cf6' },
  { value: 0,  icon: Sparkles, labelKey: 'donate.tier_custom', activeColor: '#f0d0c7', activeBorder: '#945c67' },
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
        {/* Header */}
        <div className="text-center mb-10">
          <motion.div
            initial={{ scale: 0 }}
            animate={{ scale: 1 }}
            transition={{ type: 'spring', stiffness: 260, damping: 18, delay: 0.1 }}
            className="inline-flex items-center justify-center w-16 h-16 rounded-full bg-gradient-to-br from-peach to-peach-light mb-5 shadow-sm"
          >
            <Heart className="w-8 h-8 text-primary fill-primary/30" />
          </motion.div>

          <h1 className="text-3xl font-bold text-heading mb-3 leading-tight">
            {t('donate.title')} ✨
          </h1>
          <p className="text-gray-500 text-base max-w-sm mx-auto leading-relaxed">
            {t('donate.subtitle')}
          </p>
        </div>

        {/* Amount picker card */}
        <div className="bg-white rounded-2xl border shadow-sm p-6 mb-4" style={{ borderColor: 'rgba(148,92,103,0.18)' }}>
          <p className="text-xs font-semibold text-gray-400 uppercase tracking-widest mb-4">
            {t('donate.amounts_label')}
          </p>

          <div className="grid grid-cols-2 gap-3 mb-5">
            {PRESET_AMOUNTS.map(({ value, icon: Icon, labelKey, activeColor, activeBorder }) => {
              const isThisCustom = value === 0;
              const isActive = isThisCustom ? isCustom : !isCustom && selected === value;
              return (
                <motion.button
                  key={labelKey}
                  whileHover={{ scale: 1.03 }}
                  whileTap={{ scale: 0.97 }}
                  onClick={() => handlePreset(value, isThisCustom)}
                  className="flex flex-col items-center gap-1.5 p-4 rounded-xl border-2 transition-all duration-200 cursor-pointer"
                  style={{
                    backgroundColor: isActive ? activeColor : '#f9fafb',
                    borderColor: isActive ? activeBorder : 'transparent',
                  }}
                >
                  <Icon
                    className="w-5 h-5 transition-colors"
                    style={{ color: isActive ? activeBorder : '#9ca3af' }}
                  />
                  {!isThisCustom && (
                    <span
                      className="text-xl font-bold"
                      style={{ color: isActive ? activeBorder : '#374151' }}
                    >
                      {value} лв.
                    </span>
                  )}
                  <span
                    className="text-xs font-medium"
                    style={{ color: isActive ? activeBorder : '#6b7280' }}
                  >
                    {t(labelKey)}
                  </span>
                </motion.button>
              );
            })}
          </div>

          {/* Custom input */}
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
      </motion.div>
    </div>
  );
}
