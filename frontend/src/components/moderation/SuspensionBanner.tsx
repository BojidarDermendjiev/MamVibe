import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { AlertTriangle, Ban, Eye, Hourglass } from 'lucide-react';
import { useModerationStore } from '../../store/moderationStore';
import type { ModerationLevel } from '../../types/moderation';
import AppealModal from './AppealModal';

/**
 * Global banner shown to users whose account is in an active moderation state
 * (Warned / Restricted / Suspended / Banned). Reads from the moderation store so it
 * re-renders when the axios interceptor receives a fresh 403 envelope.
 *
 * Banned/Suspended users typically cannot log in, but the banner still renders if a
 * Restricted user happens to be in the app — handles the read-only case where browse
 * still works but writes return 403.
 */
export default function SuspensionBanner() {
  const { t } = useTranslation();
  const status = useModerationStore((s) => s.status);

  const [nowMs, setNowMs] = useState<number>(() => Date.now());
  const [appealOpen, setAppealOpen] = useState(false);

  useEffect(() => {
    if (!status?.expiresAt) return;
    const handle = setInterval(() => setNowMs(Date.now()), 30_000);
    return () => clearInterval(handle);
  }, [status?.expiresAt]);

  if (!status) return null;

  // Guard against numeric enum values arriving before the backend emits strings
  const LEVEL_NAMES = ['None', 'Warned', 'Restricted', 'Suspended', 'Banned'] as const;
  const level: ModerationLevel =
    typeof status.level === 'number'
      ? (LEVEL_NAMES[(status.level as number)] ?? 'None')
      : status.level;

  if (level === 'None') return null;

  const palette = paletteFor(level);
  let countdown: string | null = null;
  if (status.expiresAt) {
    const remaining = new Date(status.expiresAt).getTime() - nowMs;
    countdown = remaining <= 0
      ? t('moderation.expires_soon', 'Expiring shortly')
      : formatDuration(remaining);
  }

  const Icon = palette.icon;
  const titleKey = `moderation.banner_${level.toLowerCase()}`;

  return (
    <div
      role="status"
      aria-live="polite"
      className={`mx-3 mt-3 rounded-xl border ${palette.border} ${palette.bg} px-4 py-3 text-sm shadow-sm`}
    >
      <div className="flex items-start gap-3">
        <Icon className={`mt-0.5 h-5 w-5 shrink-0 ${palette.icoColor}`} aria-hidden="true" />
        <div className="min-w-0 flex-1">
          <p className={`font-semibold ${palette.title}`}>{t(titleKey, defaultTitle(level))}</p>
          {status.publicReason ? (
            <p className={`mt-0.5 ${palette.body}`}>{status.publicReason}</p>
          ) : null}
          {countdown ? (
            <p className={`mt-1 text-xs ${palette.meta}`}>
              {t('moderation.expires_in', 'Restored in')}: <span className="font-mono">{countdown}</span>
            </p>
          ) : null}
          {status.canAppeal ? (
            <button
              type="button"
              onClick={() => setAppealOpen(true)}
              className={`mt-2 inline-block text-xs font-medium underline ${palette.cta}`}
            >
              {t('moderation.appeal_cta', 'Submit an appeal')}
            </button>
          ) : null}
        </div>
      </div>
      <AppealModal isOpen={appealOpen} onClose={() => setAppealOpen(false)} />
    </div>
  );
}

function paletteFor(level: ModerationLevel) {
  switch (level) {
    case 'Warned':
      return {
        icon: AlertTriangle,
        border: 'border-amber-300',
        bg: 'bg-amber-50',
        icoColor: 'text-amber-600',
        title: 'text-amber-900',
        body: 'text-amber-900/90',
        meta: 'text-amber-800/80',
        cta: 'text-amber-900',
      };
    case 'Restricted':
      return {
        icon: Eye,
        border: 'border-orange-300',
        bg: 'bg-orange-50',
        icoColor: 'text-orange-600',
        title: 'text-orange-900',
        body: 'text-orange-900/90',
        meta: 'text-orange-800/80',
        cta: 'text-orange-900',
      };
    case 'Suspended':
      return {
        icon: Hourglass,
        border: 'border-red-300',
        bg: 'bg-red-50',
        icoColor: 'text-red-600',
        title: 'text-red-900',
        body: 'text-red-900/90',
        meta: 'text-red-800/80',
        cta: 'text-red-900',
      };
    case 'Banned':
      return {
        icon: Ban,
        border: 'border-red-400',
        bg: 'bg-red-100',
        icoColor: 'text-red-700',
        title: 'text-red-900',
        body: 'text-red-900/90',
        meta: 'text-red-800/80',
        cta: 'text-red-900',
      };
    default:
      return {
        icon: AlertTriangle,
        border: 'border-gray-300',
        bg: 'bg-gray-50',
        icoColor: 'text-gray-600',
        title: 'text-gray-900',
        body: 'text-gray-900/90',
        meta: 'text-gray-800/80',
        cta: 'text-gray-900',
      };
  }
}

function defaultTitle(level: ModerationLevel): string {
  switch (level) {
    case 'Warned': return 'A note from the MamVibe team';
    case 'Restricted': return 'Your account is read-only';
    case 'Suspended': return 'Your account is temporarily suspended';
    case 'Banned': return 'Your account has been closed';
    default: return '';
  }
}

function formatDuration(ms: number): string {
  const totalSeconds = Math.floor(ms / 1000);
  const days = Math.floor(totalSeconds / 86_400);
  const hours = Math.floor((totalSeconds % 86_400) / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  if (days > 0) return `${days}d ${hours}h`;
  if (hours > 0) return `${hours}h ${minutes}m`;
  return `${minutes}m`;
}
