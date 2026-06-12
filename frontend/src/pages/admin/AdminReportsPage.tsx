import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from '@/utils/toast';
import Button from '../../components/common/Button';
import LoadingSpinner from '../../components/common/LoadingSpinner';
import Modal from '../../components/common/Modal';
import { adminModerationApi } from '../../api/adminModerationApi';
import type {
  AbuseReportDetail,
  AbuseReportSummary,
  ReportStatus,
} from '../../types/moderation';

/**
 * Admin queue listing all user-submitted abuse reports. Click a row to see detail and
 * resolve (with or without an attached moderation action against the target user).
 */
export default function AdminReportsPage() {
  const { t } = useTranslation();
  const [items, setItems] = useState<AbuseReportSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [statusFilter, setStatusFilter] = useState<ReportStatus | ''>('Pending');
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [detail, setDetail] = useState<AbuseReportDetail | null>(null);
  const [resolving, setResolving] = useState(false);
  const [resolutionNote, setResolutionNote] = useState('');

  const fetch = async () => {
    setLoading(true);
    try {
      const { data } = await adminModerationApi.getReports({
        status: (statusFilter || undefined) as ReportStatus | undefined,
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

  const openDetail = async (id: string) => {
    setSelectedId(id);
    setDetail(null);
    try {
      const { data } = await adminModerationApi.getReport(id);
      setDetail(data);
    } catch {
      toast.error(t('common.error'));
    }
  };

  const close = () => {
    setSelectedId(null);
    setDetail(null);
    setResolutionNote('');
  };

  const resolveWith = async (status: ReportStatus) => {
    if (!detail) return;
    setResolving(true);
    try {
      await adminModerationApi.resolveReport(detail.id, {
        status,
        resolutionNote: resolutionNote.trim() || null,
      });
      toast.success('Report resolved');
      close();
      fetch();
    } catch {
      toast.error(t('common.error'));
    } finally {
      setResolving(false);
    }
  };

  return (
    <div>
      <h1 className="text-3xl font-bold text-[#364153] dark:text-[#bdb9bc] mb-6">{t('admin.reports', 'Reports')}</h1>

      <div className="mb-4 flex gap-2 items-center">
        <label className="text-sm text-[#364153] dark:text-[#bdb9bc]">{t('admin.filter_status', 'Status')}:</label>
        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value as ReportStatus | '')}
          className="rounded-lg border border-lavender bg-white dark:bg-[#2a2740] dark:border-white/10 dark:text-gray-100 text-sm px-3 py-1.5 focus:outline-none focus:ring-2 focus:ring-primary"
        >
          <option value="">All</option>
          <option value="Pending">Pending</option>
          <option value="UnderReview">Under review</option>
          <option value="Resolved">Resolved</option>
          <option value="Dismissed">Dismissed</option>
          <option value="Duplicate">Duplicate</option>
        </select>
      </div>

      {loading ? (
        <LoadingSpinner size="lg" className="py-20" />
      ) : items.length === 0 ? (
        <p className="text-sm text-gray-500 dark:text-gray-400">{t('admin.no_reports', 'No reports.')}</p>
      ) : (
        <div className="bg-white dark:bg-[#2d2a42] rounded-xl border border-lavender/30 dark:border-white/10 overflow-hidden">
          <table className="w-full">
            <thead>
              <tr className="bg-cream-dark dark:bg-[#3a3758] text-left">
                <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Target</th>
                <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Type</th>
                <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Reason</th>
                <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Status</th>
                <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Submitted</th>
                <th className="px-4 py-3"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-lavender/20 dark:divide-white/10">
              {items.map((r) => (
                <tr key={r.id} className="hover:bg-cream/50 dark:hover:bg-white/5">
                  <td className="px-4 py-3 text-sm font-mono text-[#364153] dark:text-[#bdb9bc]" title={r.targetUserId}>
                    {r.targetUserId.slice(0, 8)}…
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-500 dark:text-gray-400">{r.targetType}</td>
                  <td className="px-4 py-3 text-sm text-gray-500 dark:text-gray-400">{r.reason}</td>
                  <td className="px-4 py-3 text-sm text-gray-500 dark:text-gray-400">{r.status}</td>
                  <td className="px-4 py-3 text-sm text-gray-500 dark:text-gray-400">{new Date(r.createdAt).toLocaleString()}</td>
                  <td className="px-4 py-3">
                    <Button size="sm" variant="secondary" onClick={() => openDetail(r.id)}>
                      {t('common.review', 'Review')}
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {selectedId && (
        <Modal isOpen onClose={close} title={t('admin.report_detail', 'Report detail')}>
          {!detail ? (
            <LoadingSpinner className="py-6" />
          ) : (
            <div className="space-y-3 text-sm">
              <div><strong>Reporter:</strong> <span className="font-mono">{detail.reporterId}</span></div>
              <div><strong>Target user:</strong> <span className="font-mono">{detail.targetUserId}</span></div>
              <div><strong>Type:</strong> {detail.targetType} ({detail.targetId.slice(0, 12)}…)</div>
              <div><strong>Reason:</strong> {detail.reason}</div>
              <div><strong>Status:</strong> {detail.status}</div>
              <div><strong>Submitted:</strong> {new Date(detail.createdAt).toLocaleString()}</div>
              <div>
                <strong>Description:</strong>
                <p className="mt-1 whitespace-pre-wrap rounded-lg bg-cream/50 dark:bg-white/5 p-3 text-gray-700 dark:text-gray-300">
                  {detail.description}
                </p>
              </div>

              <label className="block">
                <span className="font-semibold">Resolution note</span>
                <textarea
                  value={resolutionNote}
                  onChange={(e) => setResolutionNote(e.target.value)}
                  rows={2}
                  maxLength={1000}
                  className="mt-1 w-full rounded-lg border border-gray-200 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
                />
              </label>

              <div className="flex flex-wrap gap-2 justify-end">
                <Button variant="secondary" onClick={() => resolveWith('Dismissed')} disabled={resolving}>
                  Dismiss
                </Button>
                <Button variant="secondary" onClick={() => resolveWith('Duplicate')} disabled={resolving}>
                  Duplicate
                </Button>
                <Button onClick={() => resolveWith('Resolved')} disabled={resolving}>
                  Mark resolved
                </Button>
              </div>
              <p className="text-xs text-gray-500 mt-1">
                To apply a moderation action against the target user, use the user moderation panel
                from the Users page after closing this dialog.
              </p>
            </div>
          )}
        </Modal>
      )}
    </div>
  );
}
