import { useState } from 'react';
import { Flag } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '../../store/authStore';
import ReportModal from './ReportModal';
import type { ReportTargetType } from '../../types/moderation';

interface Props {
  targetType: ReportTargetType;
  targetId: string;
  targetLabel: string;
  /** When true, hide the button if the viewer is the target. Defaults to true. */
  hideForSelf?: boolean;
  /** Self-id used by the hide-for-self check (for users) or target-owner id (for items/messages). */
  ownerUserId?: string;
  className?: string;
}

/**
 * Small icon-button that opens the abuse-report modal. Mount on user profile pages,
 * item detail pages, and message thread headers.
 */
export default function ReportButton({ targetType, targetId, targetLabel, hideForSelf = true, ownerUserId, className }: Props) {
  const { t } = useTranslation();
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const currentUserId = useAuthStore((s) => s.user?.id);
  const [open, setOpen] = useState(false);

  if (!isAuthenticated) return null;
  if (hideForSelf && ownerUserId && currentUserId && ownerUserId === currentUserId) return null;

  return (
    <>
      <button
        type="button"
        onClick={() => setOpen(true)}
        className={`inline-flex items-center gap-1 text-xs text-gray-500 hover:text-red-600 transition-colors ${className ?? ''}`}
        aria-label={t('moderation.report_button', 'Report')}
      >
        <Flag className="h-3.5 w-3.5" aria-hidden="true" />
        {t('moderation.report_button', 'Report')}
      </button>
      <ReportModal
        isOpen={open}
        onClose={() => setOpen(false)}
        targetType={targetType}
        targetId={targetId}
        targetLabel={targetLabel}
      />
    </>
  );
}
