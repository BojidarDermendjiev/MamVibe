import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import Modal from '../common/Modal';
import Button from '../common/Button';
import toast from '../../utils/toast';
import { reportsApi } from '../../api/reportsApi';
import { MODERATION_REASONS, type ModerationReason, type ReportTargetType } from '../../types/moderation';

interface Props {
  isOpen: boolean;
  onClose: () => void;
  targetType: ReportTargetType;
  targetId: string;
  targetLabel: string;
}

/**
 * Modal users open to submit an abuse report against another user, an item, or a message thread.
 * Backed by <c>POST /api/v1/reports</c> with per-user rate limiting (10/day).
 */
export default function ReportModal({ isOpen, onClose, targetType, targetId, targetLabel }: Props) {
  const { t } = useTranslation();
  const [reason, setReason] = useState<ModerationReason>('Spam');
  const [description, setDescription] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async () => {
    const trimmed = description.trim();
    if (trimmed.length < 10) {
      toast.error(t('moderation.report_description_min', 'Please describe the issue (at least 10 characters).'));
      return;
    }
    setSubmitting(true);
    try {
      await reportsApi.submit({ targetType, targetId, reason, description: trimmed });
      toast.success(t('moderation.report_submitted', 'Report submitted — thank you for helping us keep MamVibe safe.'));
      setDescription('');
      onClose();
    } catch (err) {
      const status = (err as { response?: { status?: number; data?: { message?: string } } })?.response?.status;
      const message = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      if (status === 409) {
        toast.error(message ?? t('moderation.report_duplicate', 'You already have an open report against this target.'));
      } else if (status === 429) {
        toast.error(t('moderation.report_rate_limited', 'You have submitted too many reports today. Please try again tomorrow.'));
      } else {
        toast.error(message ?? t('common.error'));
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={t('moderation.report_modal_title', 'Report')}>
      <p className="text-sm text-gray-600 mb-3">{targetLabel}</p>

      <label className="block mb-3">
        <span className="text-sm font-semibold text-gray-700">{t('moderation.report_reason_label', 'Reason')}</span>
        <select
          value={reason}
          onChange={(e) => setReason(e.target.value as ModerationReason)}
          className="mt-1 w-full rounded-lg border border-gray-200 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
        >
          {MODERATION_REASONS.map((r) => (
            <option key={r} value={r}>{r}</option>
          ))}
        </select>
      </label>

      <label className="block mb-4">
        <span className="text-sm font-semibold text-gray-700">{t('moderation.report_description_label', 'What happened?')}</span>
        <textarea
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          rows={4}
          maxLength={2000}
          placeholder={t('moderation.report_description_placeholder', 'Describe the issue in detail. The team will review.')}
          className="mt-1 w-full rounded-lg border border-gray-200 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
        />
        <div className="mt-1 text-xs text-gray-500">{description.length} / 2000</div>
      </label>

      <div className="flex gap-2 justify-end">
        <Button variant="secondary" onClick={onClose} disabled={submitting}>
          {t('common.cancel', 'Cancel')}
        </Button>
        <Button onClick={handleSubmit} disabled={submitting || description.trim().length < 10}>
          {submitting ? t('common.submitting', 'Submitting…') : t('moderation.report_submit', 'Submit report')}
        </Button>
      </div>
    </Modal>
  );
}
