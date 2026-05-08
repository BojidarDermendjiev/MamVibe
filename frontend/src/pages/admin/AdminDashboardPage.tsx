import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { HiUsers, HiCollection, HiGift, HiCurrencyDollar, HiSparkles } from 'react-icons/hi';
import { adminApi, type DashboardStats } from '../../api/adminApi';
import LoadingSpinner from '../../components/common/LoadingSpinner';

const MODEL_LABELS: Record<string, { name: string; tier: string; color: string }> = {
  'claude-haiku-4-5-20251001': { name: 'Claude Haiku 4.5',  tier: 'Fastest & Cheapest', color: 'text-green-600 dark:text-green-400' },
  'claude-sonnet-4-6':         { name: 'Claude Sonnet 4.6', tier: 'Balanced',             color: 'text-blue-600 dark:text-blue-400' },
  'claude-opus-4-5':           { name: 'Claude Opus 4.5',   tier: 'Best Quality',         color: 'text-purple-600 dark:text-purple-400' },
};

const DEFAULT_MODELS = Object.keys(MODEL_LABELS);

export default function AdminDashboardPage() {
  const { t } = useTranslation();
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);

  const [aiModel, setAiModel] = useState('claude-haiku-4-5-20251001');
  const [availableModels, setAvailableModels] = useState<string[]>(DEFAULT_MODELS);
  const [aiSaving, setAiSaving] = useState(false);
  const [aiSaved, setAiSaved] = useState(false);

  useEffect(() => {
    adminApi.getDashboard().then((res) => {
      setStats(res.data);
      setLoading(false);
    }).catch(() => setLoading(false));

    adminApi.getAiSettings().then((res) => {
      setAiModel(res.data.model);
      setAvailableModels(res.data.availableModels);
    }).catch(() => {});
  }, []);

  const saveAiModel = async () => {
    setAiSaving(true);
    setAiSaved(false);
    try {
      await adminApi.updateAiSettings(aiModel);
      setAiSaved(true);
      setTimeout(() => setAiSaved(false), 3000);
    } finally {
      setAiSaving(false);
    }
  };

  if (loading) return <LoadingSpinner size="lg" className="py-20" />;
  if (!stats) return null;

  const cards = [
    { label: t('admin.total_users'),     value: stats.totalUsers,                    icon: HiUsers,         color: 'bg-lavender/20 text-primary' },
    { label: t('admin.total_items'),     value: stats.totalItems,                    icon: HiCollection,    color: 'bg-peach/20 text-mauve' },
    { label: t('admin.total_donations'), value: stats.totalDonations,                icon: HiGift,          color: 'bg-green-100 text-green-600' },
    { label: t('admin.total_sales'),     value: `$${stats.totalRevenue.toFixed(2)}`, icon: HiCurrencyDollar, color: 'bg-mauve/10 text-mauve' },
  ];

  return (
    <div>
      <h1 className="text-3xl font-bold text-[#364153] dark:text-[#bdb9bc] mb-8">
        {t('admin.dashboard')}
      </h1>
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        {cards.map((card, i) => (
          <div key={i} className="bg-white dark:bg-[#2d2a42] rounded-xl p-6 border border-lavender/30 dark:border-white/10">
            <div className={`w-12 h-12 rounded-xl ${card.color} flex items-center justify-center mb-4`}>
              <card.icon className="h-6 w-6" />
            </div>
            <p className="text-sm text-gray-500 dark:text-gray-400">{card.label}</p>
            <p className="text-2xl font-bold text-[#364153] dark:text-[#bdb9bc] mt-1">{card.value}</p>
          </div>
        ))}
      </div>

      {/* AI Model Selector */}
      <div className="mt-8 bg-white dark:bg-[#2d2a42] rounded-xl p-6 border border-lavender/30 dark:border-white/10">
        <div className="flex items-center gap-3 mb-5">
          <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-[#945c67] to-[#3f4b7f] flex items-center justify-center">
            <HiSparkles className="h-5 w-5 text-white" />
          </div>
          <div>
            <h2 className="font-semibold text-[#364153] dark:text-[#bdb9bc]">AI Model</h2>
            <p className="text-xs text-gray-500 dark:text-gray-400">Controls all Claude-powered features: listing suggestions, moderation, price estimates, chat</p>
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-3 gap-3 mb-5">
          {availableModels.map((m) => {
            const meta = MODEL_LABELS[m];
            const isSelected = aiModel === m;
            return (
              <button
                key={m}
                onClick={() => setAiModel(m)}
                className={`text-left p-4 rounded-xl border-2 transition-all ${
                  isSelected
                    ? 'border-[#945c67] bg-gradient-to-br from-[#945c67]/5 to-[#3f4b7f]/5'
                    : 'border-lavender/30 dark:border-white/10 hover:border-lavender/60 dark:hover:border-white/20'
                }`}
              >
                <p className={`font-semibold text-sm ${isSelected ? 'text-[#945c67] dark:text-[#c47b87]' : 'text-[#364153] dark:text-[#bdb9bc]'}`}>
                  {meta?.name ?? m}
                </p>
                <p className={`text-xs mt-0.5 ${meta?.color ?? 'text-gray-500'}`}>
                  {meta?.tier ?? ''}
                </p>
                <p className="text-[10px] text-gray-400 dark:text-gray-500 mt-1 font-mono truncate">{m}</p>
              </button>
            );
          })}
        </div>

        <div className="flex items-center gap-3">
          <button
            onClick={saveAiModel}
            disabled={aiSaving}
            className="px-5 py-2 rounded-lg bg-gradient-to-r from-[#945c67] to-[#3f4b7f] text-white text-sm font-semibold hover:opacity-90 disabled:opacity-60 transition-opacity"
          >
            {aiSaving ? 'Saving…' : 'Save Model'}
          </button>
          {aiSaved && (
            <span className="text-sm text-green-600 dark:text-green-400 font-medium">Saved!</span>
          )}
        </div>
      </div>
    </div>
  );
}
