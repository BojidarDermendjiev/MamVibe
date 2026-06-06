import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { usePageSEO } from '@/hooks/useSEO';
import { Users, LayoutGrid, Gift, CircleDollarSign, Sparkles } from 'lucide-react';
import { adminApi, type DashboardStats } from '../../api/adminApi';
import LoadingSpinner from '../../components/common/LoadingSpinner';

// ── Model metadata ────────────────────────────────────────────────────────────

const ANTHROPIC_MODEL_META: Record<string, { name: string; tier: string; color: string }> = {
  'claude-haiku-4-5-20251001': { name: 'Claude Haiku 4.5',  tier: 'Fastest & Cheapest', color: 'text-green-600 dark:text-green-400' },
  'claude-sonnet-4-6':         { name: 'Claude Sonnet 4.6', tier: 'Balanced',            color: 'text-blue-600 dark:text-blue-400'  },
  'claude-opus-4-5':           { name: 'Claude Opus 4.5',   tier: 'Best Quality',        color: 'text-purple-600 dark:text-purple-400' },
};

const GROQ_MODEL_META: Record<string, { name: string; tier: string; color: string }> = {
  'llama-3.3-70b-versatile': { name: 'Llama 3.3 70B',   tier: 'Best Quality (Free)', color: 'text-orange-600 dark:text-orange-400' },
  'llama-3.1-8b-instant':    { name: 'Llama 3.1 8B',    tier: 'Fastest (Free)',      color: 'text-green-600 dark:text-green-400'  },
  'mixtral-8x7b-32768':      { name: 'Mixtral 8x7B',    tier: 'Long Context (Free)', color: 'text-blue-600 dark:text-blue-400'    },
};

const DEFAULT_ANTHROPIC_MODELS = Object.keys(ANTHROPIC_MODEL_META);
const DEFAULT_GROQ_MODELS      = Object.keys(GROQ_MODEL_META);

// ── Component ─────────────────────────────────────────────────────────────────

export default function AdminDashboardPage() {
  usePageSEO({ title: "Admin Dashboard", description: "MamVibe admin control panel.", index: false });
  const { t } = useTranslation();

  const [stats,   setStats]   = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);

  // AI settings state
  const [anthropicModel,    setAnthropicModel]    = useState('claude-haiku-4-5-20251001');
  const [chatProvider,      setChatProvider]      = useState<'groq' | 'anthropic'>('groq');
  const [groqModel,         setGroqModel]         = useState('llama-3.3-70b-versatile');
  const [availableAnthropicModels, setAvailableAnthropicModels] = useState<string[]>(DEFAULT_ANTHROPIC_MODELS);
  const [availableGroqModels,      setAvailableGroqModels]      = useState<string[]>(DEFAULT_GROQ_MODELS);
  const [aiSaving, setAiSaving] = useState(false);
  const [aiSaved,  setAiSaved]  = useState(false);

  useEffect(() => {
    adminApi.getDashboard().then((res) => {
      setStats(res.data);
      setLoading(false);
    }).catch(() => setLoading(false));

    adminApi.getAiSettings().then((res) => {
      const d = res.data;
      setAnthropicModel(d.model);
      setChatProvider(d.chatProvider as 'groq' | 'anthropic');
      setGroqModel(d.groqModel);
      if (d.availableModels.length)      setAvailableAnthropicModels(d.availableModels);
      if (d.availableGroqModels.length)  setAvailableGroqModels(d.availableGroqModels);
    }).catch(() => {});
  }, []);

  const saveAiSettings = async () => {
    setAiSaving(true);
    setAiSaved(false);
    try {
      await adminApi.updateAiSettings(anthropicModel, chatProvider, groqModel);
      setAiSaved(true);
      setTimeout(() => setAiSaved(false), 3000);
    } finally {
      setAiSaving(false);
    }
  };

  if (loading) return <LoadingSpinner size="lg" className="py-20" />;
  if (!stats)  return null;

  const cards = [
    { label: t('admin.total_users'),     value: stats.totalUsers,                    icon: Users,              color: 'bg-lavender/20 text-primary' },
    { label: t('admin.total_items'),     value: stats.totalItems,                    icon: LayoutGrid,         color: 'bg-peach/20 text-mauve' },
    { label: t('admin.total_donations'), value: stats.totalDonations,                icon: Gift,               color: 'bg-green-100 text-green-600' },
    { label: t('admin.total_sales'),     value: `$${stats.totalRevenue.toFixed(2)}`, icon: CircleDollarSign,   color: 'bg-mauve/10 text-mauve' },
  ];

  const chatModelMeta  = chatProvider === 'groq' ? GROQ_MODEL_META      : ANTHROPIC_MODEL_META;
  const chatModelList  = chatProvider === 'groq' ? availableGroqModels   : availableAnthropicModels;
  const activeChatModel = chatProvider === 'groq' ? groqModel            : anthropicModel;
  const setActiveChatModel = chatProvider === 'groq' ? setGroqModel      : setAnthropicModel;

  return (
    <div>
      <h1 className="text-3xl font-bold text-[#364153] dark:text-[#bdb9bc] mb-8">
        {t('admin.dashboard')}
      </h1>

      {/* Stats cards */}
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

      {/* AI Settings panel */}
      <div className="mt-8 bg-white dark:bg-[#2d2a42] rounded-xl p-6 border border-lavender/30 dark:border-white/10 space-y-6">

        {/* Header */}
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-[#945c67] to-[#3f4b7f] flex items-center justify-center shrink-0">
            <Sparkles className="h-5 w-5 text-white" />
          </div>
          <div>
            <h2 className="font-semibold text-[#364153] dark:text-[#bdb9bc]">AI Settings</h2>
            <p className="text-xs text-gray-500 dark:text-gray-400">
              Changes take effect immediately — no redeploy needed.
            </p>
          </div>
        </div>

        {/* ── Section 1: Chat provider ── */}
        <div>
          <p className="text-xs font-semibold uppercase tracking-wider text-gray-400 dark:text-gray-500 mb-2">
            Chat Widget Provider
          </p>
          <div className="flex gap-2">
            {(['groq', 'anthropic'] as const).map((p) => (
              <button
                key={p}
                onClick={() => setChatProvider(p)}
                className={`px-4 py-2 rounded-lg text-sm font-semibold border-2 transition-all ${
                  chatProvider === p
                    ? 'border-[#945c67] bg-gradient-to-r from-[#945c67]/10 to-[#3f4b7f]/10 text-[#945c67] dark:text-[#c47b87]'
                    : 'border-lavender/30 dark:border-white/10 text-gray-600 dark:text-gray-300 hover:border-lavender/60'
                }`}
              >
                {p === 'groq' ? 'Groq (Free)' : 'Anthropic'}
              </button>
            ))}
          </div>
          {chatProvider === 'groq' && (
            <p className="mt-1.5 text-[11px] text-gray-400 dark:text-gray-500">
              Free tier: 30 req/min · 14 400 req/day — sufficient for normal traffic.
            </p>
          )}
          {chatProvider === 'anthropic' && (
            <p className="mt-1.5 text-[11px] text-gray-400 dark:text-gray-500">
              Uses the Anthropic model selected below — same API key as vision features.
            </p>
          )}
        </div>

        {/* ── Section 2: Chat model (changes with provider) ── */}
        <div>
          <p className="text-xs font-semibold uppercase tracking-wider text-gray-400 dark:text-gray-500 mb-2">
            Chat Model
          </p>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
            {chatModelList.map((m) => {
              const meta = chatModelMeta[m];
              const isSelected = activeChatModel === m;
              return (
                <button
                  key={m}
                  onClick={() => setActiveChatModel(m)}
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
        </div>

        {/* ── Section 3: Anthropic vision model (always shown) ── */}
        {chatProvider === 'groq' && (
          <div>
            <p className="text-xs font-semibold uppercase tracking-wider text-gray-400 dark:text-gray-500 mb-1">
              Anthropic Vision Model
            </p>
            <p className="text-[11px] text-gray-400 dark:text-gray-500 mb-2">
              Used for listing suggestions, content moderation, and price estimates.
            </p>
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
              {availableAnthropicModels.map((m) => {
                const meta = ANTHROPIC_MODEL_META[m];
                const isSelected = anthropicModel === m;
                return (
                  <button
                    key={m}
                    onClick={() => setAnthropicModel(m)}
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
          </div>
        )}

        {/* Save button */}
        <div className="flex items-center gap-3 pt-1">
          <button
            onClick={saveAiSettings}
            disabled={aiSaving}
            className="px-5 py-2 rounded-lg bg-gradient-to-r from-[#945c67] to-[#3f4b7f] text-white text-sm font-semibold hover:opacity-90 disabled:opacity-60 transition-opacity"
          >
            {aiSaving ? 'Saving…' : 'Save Settings'}
          </button>
          {aiSaved && (
            <span className="text-sm text-green-600 dark:text-green-400 font-medium">Saved!</span>
          )}
        </div>
      </div>
    </div>
  );
}
