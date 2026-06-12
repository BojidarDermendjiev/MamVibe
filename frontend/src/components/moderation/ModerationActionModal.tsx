import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import Modal from '../common/Modal';
import Button from '../common/Button';
import toast from '../../utils/toast';
import { adminModerationApi } from '../../api/adminModerationApi';
import {
  MODERATION_LEVELS,
  MODERATION_REASONS,
  type ModerationActionRequest,
  type ModerationLevel,
  type ModerationReason,
} from '../../types/moderation';

interface Props {
  isOpen: boolean;
  onClose: () => void;
  userId: string;
  userDisplayName: string;
  onActionApplied: () => void;
}

const DURATION_OPTIONS: { label: string; minutes: number | null }[] = [
  { label: '1 hour',  minutes: 60 },
  { label: '24 hours', minutes: 60 * 24 },
  { label: '7 days',   minutes: 60 * 24 * 7 },
  { label: '30 days',  minutes: 60 * 24 * 30 },
  { label: 'Custom',   minutes: -1 },
  { label: 'Permanent', minutes: null },
];

/**
 * Admin modal for applying a graded moderation action against a user. Replaces the
 * legacy binary block/unblock buttons.
 */
export default function ModerationActionModal({ isOpen, onClose, userId, userDisplayName, onActionApplied }: Props) {
  const { t } = useTranslation();
  const [level, setLevel] = useState<ModerationLevel>('Warned');
  const [reason, setReason] = useState<ModerationReason>('RuleViolation');
  const [publicReason, setPublicReason] = useState('');
  const [internalNote, setInternalNote] = useState('');
  const [durationMinutes, setDurationMinutes] = useState<number | null>(60 * 24);
  const [customMinutes, setCustomMinutes] = useState<number>(60);
  const [submitting, setSubmitting] = useState(false);

  const supportsDuration = level === 'Restricted' || level === 'Suspended';

  const handleSubmit = async () => {
    if (!publicReason.trim()) {
      toast.error('Public reason is required');
      return;
    }
    setSubmitting(true);
    try {
      const effectiveDuration =
        !supportsDuration ? null
          : (durationMinutes === -1 ? customMinutes : durationMinutes);
      const request: ModerationActionRequest = {
        newLevel: level,
        reason,
        publicReason: publicReason.trim(),
        internalNote: internalNote.trim() || null,
        durationMinutes: effectiveDuration,
      };
      await adminModerationApi.applyAction(userId, request);
      toast.success(`Moderation action applied to ${userDisplayName}`);
      onActionApplied();
      onClose();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to apply moderation';
      toast.error(message);
    } finally {
      setSubmitting(false);
    }
  };

  const handleClear = async () => {
    setSubmitting(true);
    try {
      await adminModerationApi.clearAction(userId, internalNote.trim() || 'Cleared by administrator');
      toast.success(`Moderation cleared for ${userDisplayName}`);
      onActionApplied();
      onClose();
    } catch {
      toast.error('Failed to clear moderation');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={t('admin.moderate_user', 'Moderate user')}>
      <p className="text-sm text-gray-600 mb-4">{userDisplayName}</p>

      <fieldset className="mb-3">
        <legend className="text-sm font-semibold text-gray-700 mb-1">Level</legend>
        <div className="grid grid-cols-2 gap-2">
          {MODERATION_LEVELS.map((l) => (
            <label key={l} className={`flex items-center gap-2 rounded-lg border px-3 py-2 text-sm cursor-pointer ${
              level === l ? 'border-primary bg-primary/5' : 'border-gray-200'
            }`}>
              <input
                type="radio"
                name="moderation-level"
                value={l}
                checked={level === l}
                onChange={() => setLevel(l)}
                className="accent-primary"
              />
              {l}
            </label>
          ))}
        </div>
      </fieldset>

      <label className="block mb-3">
        <span className="text-sm font-semibold text-gray-700">Reason</span>
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

      {supportsDuration && (
        <fieldset className="mb-3">
          <legend className="text-sm font-semibold text-gray-700 mb-1">Duration</legend>
          <div className="flex flex-wrap gap-2">
            {DURATION_OPTIONS.map((opt) => (
              <label key={opt.label} className={`rounded-full border px-3 py-1 text-xs cursor-pointer ${
                durationMinutes === opt.minutes ? 'border-primary bg-primary/5' : 'border-gray-200'
              }`}>
                <input
                  type="radio"
                  name="moderation-duration"
                  className="sr-only"
                  checked={durationMinutes === opt.minutes}
                  onChange={() => setDurationMinutes(opt.minutes)}
                />
                {opt.label}
              </label>
            ))}
          </div>
          {durationMinutes === -1 && (
            <input
              type="number"
              min={1}
              value={customMinutes}
              onChange={(e) => setCustomMinutes(parseInt(e.target.value, 10) || 1)}
              className="mt-2 w-32 rounded-lg border border-gray-200 px-3 py-2 text-sm"
              placeholder="minutes"
            />
          )}
        </fieldset>
      )}

      <label className="block mb-3">
        <span className="text-sm font-semibold text-gray-700">Public reason (shown to user)</span>
        <textarea
          value={publicReason}
          onChange={(e) => setPublicReason(e.target.value)}
          rows={2}
          maxLength={500}
          className="mt-1 w-full rounded-lg border border-gray-200 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
        />
      </label>

      <label className="block mb-4">
        <span className="text-sm font-semibold text-gray-700">Internal note (admins only)</span>
        <textarea
          value={internalNote}
          onChange={(e) => setInternalNote(e.target.value)}
          rows={2}
          maxLength={2000}
          className="mt-1 w-full rounded-lg border border-gray-200 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
        />
      </label>

      <div className="flex flex-wrap gap-2 justify-end">
        <Button variant="secondary" onClick={handleClear} disabled={submitting}>
          Clear current
        </Button>
        <Button variant="secondary" onClick={onClose} disabled={submitting}>
          Cancel
        </Button>
        <Button onClick={handleSubmit} disabled={submitting || !publicReason.trim()}>
          {submitting ? 'Applying…' : 'Apply'}
        </Button>
      </div>
    </Modal>
  );
}
