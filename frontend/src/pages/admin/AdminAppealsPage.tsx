import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from '@/utils/toast';
import Button from '../../components/common/Button';
import LoadingSpinner from '../../components/common/LoadingSpinner';
import Modal from '../../components/common/Modal';
import { adminAppealsApi } from '../../api/appealsApi';
import type { Appeal, AppealStatus, AppealSummary } from '../../types/moderation';

/**
 * Admin queue for moderation appeals. Approving an appeal also clears the user's
 * active moderation state (server-side, in the same workflow).
 */
export default function AdminAppealsPage() {
  const { t } = useTranslation();
  const [items, setItems] = useState<AppealSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [statusFilter, setStatusFilter] = useState<AppealStatus | ''>('Pending');
  const [detail, setDetail] = useState<Appeal | null>(null);
  const [note, setNote] = useState('');
  const [deciding, setDeciding] = useState(false);

  const fetch = async () => {
    setLoading(true);
    try {
      const { data } = await adminAppealsApi.list({
        status: statusFilter || undefined,
        page: 1,
        pageSize: 50,
      });
      setItems(data.items);
    } catch {
      toast.error(t('common.error'));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetch();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [statusFilter]);

  const open = async (id: string) => {
    setDetail(null);
    setNote('');
    try {
      const { data } = await adminAppealsApi.get(id);
      setDetail(data);
    } catch {
      toast.error(t('common.error'));
    }
  };

  const decide = async (decision: 'Approved' | 'Rejected') => {
    if (!detail) return;
    setDeciding(true);
    try {
      await adminAppealsApi.decide(detail.id, { status: decision, decisionNote: note.trim() || null });
      toast.success(`Appeal ${decision.toLowerCase()}`);
      setDetail(null);
      fetch();
    } catch {
      toast.error(t('common.error'));
    } finally {
      setDeciding(false);
    }
  };

  return (
    <div>
      <h1 className="text-3xl font-bold text-[#364153] dark:text-[#bdb9bc] mb-6">{t('admin.appeals', 'Appeals')}</h1>

      <div className="mb-4 flex gap-2 items-center">
        <label className="text-sm text-[#364153] dark:text-[#bdb9bc]">{t('admin.filter_status', 'Status')}:</label>
        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value as AppealStatus | '')}
          className="rounded-lg border border-lavender bg-white dark:bg-[#2a2740] dark:border-white/10 dark:text-gray-100 text-sm px-3 py-1.5 focus:outline-none focus:ring-2 focus:ring-primary"
        >
          <option value="">All</option>
          <option value="Pending">Pending</option>
          <option value="UnderReview">Under review</option>
          <option value="Approved">Approved</option>
          <option value="Rejected">Rejected</option>
        </select>
      </div>

      {loading ? (
        <LoadingSpinner size="lg" className="py-20" />
      ) : items.length === 0 ? (
        <p className="text-sm text-gray-500 dark:text-gray-400">{t('admin.no_appeals', 'No appeals.')}</p>
      ) : (
        <div className="bg-white dark:bg-[#2d2a42] rounded-xl border border-lavender/30 dark:border-white/10 overflow-hidden">
          <table className="w-full">
            <thead>
              <tr className="bg-cream-dark dark:bg-[#3a3758] text-left">
                <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">User</th>
                <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Status</th>
                <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Submitted</th>
                <th className="px-4 py-3"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-lavender/20 dark:divide-white/10">
              {items.map((a) => (
                <tr key={a.id}>
                  <td className="px-4 py-3 text-sm font-mono">{a.userId.slice(0, 8)}…</td>
                  <td className="px-4 py-3 text-sm">{a.status}</td>
                  <td className="px-4 py-3 text-sm text-gray-500 dark:text-gray-400">{new Date(a.createdAt).toLocaleString()}</td>
                  <td className="px-4 py-3">
                    <Button size="sm" variant="secondary" onClick={() => open(a.id)}>
                      {t('common.review', 'Review')}
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {detail && (
        <Modal isOpen onClose={() => setDetail(null)} title={t('admin.appeal_detail', 'Appeal detail')}>
          <div className="space-y-3 text-sm">
            <div><strong>User:</strong> <span className="font-mono">{detail.userId}</span></div>
            <div><strong>Status:</strong> {detail.status}</div>
            <div>
              <strong>Statement:</strong>
              <p className="mt-1 whitespace-pre-wrap rounded-lg bg-cream/50 dark:bg-white/5 p-3 text-gray-700 dark:text-gray-300">
                {detail.userStatement}
              </p>
            </div>
            <label className="block">
              <span className="font-semibold">Decision note</span>
              <textarea
                value={note}
                onChange={(e) => setNote(e.target.value)}
                rows={2}
                maxLength={2000}
                className="mt-1 w-full rounded-lg border border-gray-200 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
              />
            </label>
            <div className="flex flex-wrap gap-2 justify-end">
              <Button variant="secondary" onClick={() => decide('Rejected')} disabled={deciding}>
                Reject
              </Button>
              <Button onClick={() => decide('Approved')} disabled={deciding}>
                Approve (clears moderation)
              </Button>
            </div>
          </div>
        </Modal>
      )}
    </div>
  );
}
