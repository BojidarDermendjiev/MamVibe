import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import Modal from '../common/Modal';
import Button from '../common/Button';
import toast from '../../utils/toast';
import { appealsApi } from '../../api/appealsApi';
import { useModerationStore } from '../../store/moderationStore';

interface Props {
  isOpen: boolean;
  onClose: () => void;
}

/**
 * Modal users with an active Restricted moderation open to submit an appeal against the
 * triggering log entry. POSTs to <c>/api/v1/users/me/appeals</c>, which is on the
 * middleware's Restricted-allowlist so the submission is not blocked.
 */
export default function AppealModal({ isOpen, onClose }: Props) {
  const { t } = useTranslation();
  const status = useModerationStore((s) => s.status);
  const refresh = useModerationStore((s) => s.refresh);
  const [statement, setStatement] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const canSubmit = !!status?.activeModerationLogId && statement.trim().length >= 20;

  const handleSubmit = async () => {
    if (!status?.activeModerationLogId) return;
    setSubmitting(true);
    try {
      await appealsApi.submit({
        moderationLogId: status.activeModerationLogId,
        statement: statement.trim(),
      });
      toast.success(t('moderation.appeal_submitted', 'Appeal submitted — we will review it shortly.'));
      setStatement('');
      onClose();
      refresh();
    } catch (err) {
      const status = (err as { response?: { status?: number } })?.response?.status;
      const message = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      if (status === 409) toast.error(message ?? t('moderation.appeal_open', 'You already have an open appeal for this action.'));
      else if (status === 429) toast.error(t('moderation.appeal_rate_limited', 'Too many appeals submitted recently. Try again later.'));
      else toast.error(message ?? t('common.error'));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={t('moderation.appeal_modal_title', 'Submit appeal')}>
      <p className="text-sm text-gray-600 mb-3">
        {t('moderation.appeal_intro', 'Explain why you believe this moderation action should be reversed. Our team will review your statement and respond.')}
      </p>
      <label className="block mb-4">
        <span className="text-sm font-semibold text-gray-700">{t('moderation.appeal_statement_label', 'Your statement')}</span>
        <textarea
          value={statement}
          onChange={(e) => setStatement(e.target.value)}
          rows={6}
          maxLength={3000}
          placeholder={t('moderation.appeal_statement_placeholder', 'I disagree with this moderation because…')}
          className="mt-1 w-full rounded-lg border border-gray-200 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
        />
        <div className="mt-1 text-xs text-gray-500">{statement.length} / 3000 · {t('moderation.appeal_min_chars', 'min 20 characters')}</div>
      </label>
      <div className="flex gap-2 justify-end">
        <Button variant="secondary" onClick={onClose} disabled={submitting}>
          {t('common.cancel', 'Cancel')}
        </Button>
        <Button onClick={handleSubmit} disabled={!canSubmit || submitting}>
          {submitting ? t('common.submitting', 'Submitting…') : t('moderation.appeal_submit', 'Submit appeal')}
        </Button>
      </div>
    </Modal>
  );
}
