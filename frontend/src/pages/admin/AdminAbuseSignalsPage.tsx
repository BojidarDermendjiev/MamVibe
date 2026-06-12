import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from '@/utils/toast';
import Button from '../../components/common/Button';
import LoadingSpinner from '../../components/common/LoadingSpinner';
import axiosClient from '../../api/axiosClient';

interface AbuseSignal {
  id: string;
  type: string;
  subjectUserId: string;
  score: number;
  details: string | null;
  evidenceTargetId: string | null;
  acknowledged: boolean;
  acknowledgedByAdminId: string | null;
  acknowledgedAt: string | null;
  createdAt: string;
}

interface PagedSignals {
  items: AbuseSignal[];
  totalCount: number;
  page: number;
  pageSize: number;
}

/**
 * Read-only feed of auto-detected abuse signals (failed-login bursts, mass listing creation,
 * spam keywords, multi-account same IP, accumulated report thresholds). Admins acknowledge
 * signals here; the underlying user is moderated via the Users page.
 */
export default function AdminAbuseSignalsPage() {
  const { t } = useTranslation();
  const [items, setItems] = useState<AbuseSignal[]>([]);
  const [includeAcknowledged, setIncludeAcknowledged] = useState(false);
  const [loading, setLoading] = useState(true);

  const fetch = async () => {
    setLoading(true);
    try {
      const { data } = await axiosClient.get<PagedSignals>('/admin/abuse-signals', {
        params: { includeAcknowledged, page: 1, pageSize: 50 },
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
  }, [includeAcknowledged]);

  const acknowledge = async (id: string) => {
    try {
      await axiosClient.post(`/admin/abuse-signals/${id}/acknowledge`);
      toast.success('Signal acknowledged');
      fetch();
    } catch {
      toast.error(t('common.error'));
    }
  };

  return (
    <div>
      <h1 className="text-3xl font-bold text-[#364153] dark:text-[#bdb9bc] mb-6">{t('admin.abuse_signals', 'Abuse signals')}</h1>

      <label className="inline-flex items-center gap-2 mb-4 text-sm text-[#364153] dark:text-[#bdb9bc]">
        <input
          type="checkbox"
          checked={includeAcknowledged}
          onChange={(e) => setIncludeAcknowledged(e.target.checked)}
          className="accent-primary"
        />
        {t('admin.include_acknowledged', 'Include acknowledged')}
      </label>

      {loading ? (
        <LoadingSpinner size="lg" className="py-20" />
      ) : items.length === 0 ? (
        <p className="text-sm text-gray-500 dark:text-gray-400">{t('admin.no_signals', 'No active signals.')}</p>
      ) : (
        <div className="bg-white dark:bg-[#2d2a42] rounded-xl border border-lavender/30 dark:border-white/10 overflow-hidden">
          <table className="w-full">
            <thead>
              <tr className="bg-cream-dark dark:bg-[#3a3758] text-left">
                <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Subject</th>
                <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Type</th>
                <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Score</th>
                <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Details</th>
                <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Raised</th>
                <th className="px-4 py-3"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-lavender/20 dark:divide-white/10">
              {items.map((s) => (
                <tr key={s.id} className={s.acknowledged ? 'opacity-60' : ''}>
                  <td className="px-4 py-3 text-sm font-mono text-[#364153] dark:text-[#bdb9bc]" title={s.subjectUserId}>
                    {s.subjectUserId.slice(0, 8)}…
                  </td>
                  <td className="px-4 py-3 text-sm">{s.type}</td>
                  <td className="px-4 py-3 text-sm font-semibold">{s.score}</td>
                  <td className="px-4 py-3 text-xs text-gray-500 dark:text-gray-400 max-w-md truncate" title={s.details ?? ''}>
                    {s.details ?? '—'}
                  </td>
                  <td className="px-4 py-3 text-xs text-gray-500 dark:text-gray-400">{new Date(s.createdAt).toLocaleString()}</td>
                  <td className="px-4 py-3">
                    {!s.acknowledged && (
                      <Button size="sm" variant="secondary" onClick={() => acknowledge(s.id)}>
                        {t('admin.acknowledge', 'Acknowledge')}
                      </Button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
