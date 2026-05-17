import { useEffect, useState, useCallback } from 'react';
import { adminApi, type AuditLog } from '../../api/adminApi';
import LoadingSpinner from '../../components/common/LoadingSpinner';

const ACTION_PREFIXES = ['All', 'Auth', 'Admin', 'Payment', 'Item', 'Shipping'];

function ActionBadge({ action }: { action: string }) {
  const prefix = action.split('.')[0];
  const colors: Record<string, string> = {
    Auth: 'bg-blue-100 text-blue-700 dark:bg-blue-900/40 dark:text-blue-300',
    Admin: 'bg-purple-100 text-purple-700 dark:bg-purple-900/40 dark:text-purple-300',
    Payment: 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/40 dark:text-yellow-300',
    Item: 'bg-green-100 text-green-700 dark:bg-green-900/40 dark:text-green-300',
    Shipping: 'bg-cyan-100 text-cyan-700 dark:bg-cyan-900/40 dark:text-cyan-300',
  };
  return (
    <span className={`px-2 py-0.5 rounded text-xs font-medium ${colors[prefix] ?? 'bg-gray-100 text-gray-600 dark:bg-gray-700 dark:text-gray-300'}`}>
      {action}
    </span>
  );
}

export default function AdminAuditLogPage() {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);

  const [actionFilter, setActionFilter] = useState('All');
  const [userIdFilter, setUserIdFilter] = useState('');
  const [successFilter, setSuccessFilter] = useState<'all' | 'true' | 'false'>('all');

  const pageSize = 50;
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

  const fetchLogs = useCallback(async (p = page) => {
    setLoading(true);
    try {
      const params = {
        page: p,
        pageSize,
        action: actionFilter !== 'All' ? actionFilter : undefined,
        userId: userIdFilter.trim() || undefined,
        success: successFilter !== 'all' ? successFilter === 'true' : undefined,
      };
      const { data } = await adminApi.getAuditLogs(params);
      setLogs(data.items);
      setTotalCount(data.totalCount);
    } catch { /* ignore */ } finally {
      setLoading(false);
    }
  }, [page, actionFilter, userIdFilter, successFilter]);

  useEffect(() => { fetchLogs(); }, [fetchLogs]);

  const applyFilters = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    fetchLogs(1);
  };

  const fmt = (iso: string) =>
    new Date(iso).toLocaleString(undefined, { dateStyle: 'short', timeStyle: 'medium' });

  return (
    <div>
      <h1 className="text-3xl font-bold text-[#364153] dark:text-[#bdb9bc] mb-6">
        Audit Log
      </h1>

      {/* Filters */}
      <form onSubmit={applyFilters} className="mb-6 flex flex-wrap gap-3 items-end">
        {/* Action prefix */}
        <div>
          <label className="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">Action</label>
          <select
            value={actionFilter}
            onChange={(e) => setActionFilter(e.target.value)}
            className="px-3 py-2 rounded-lg border border-lavender bg-white dark:bg-[#2a2740] dark:border-white/10 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
          >
            {ACTION_PREFIXES.map((a) => (
              <option key={a} value={a}>{a}</option>
            ))}
          </select>
        </div>

        {/* User ID */}
        <div>
          <label className="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">User ID</label>
          <input
            value={userIdFilter}
            onChange={(e) => setUserIdFilter(e.target.value)}
            placeholder="Filter by user ID…"
            className="px-3 py-2 rounded-lg border border-lavender bg-white dark:bg-[#2a2740] dark:border-white/10 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-primary w-56"
          />
        </div>

        {/* Success */}
        <div>
          <label className="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">Result</label>
          <select
            value={successFilter}
            onChange={(e) => setSuccessFilter(e.target.value as 'all' | 'true' | 'false')}
            className="px-3 py-2 rounded-lg border border-lavender bg-white dark:bg-[#2a2740] dark:border-white/10 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <option value="all">All</option>
            <option value="true">Success</option>
            <option value="false">Failure</option>
          </select>
        </div>

        <button
          type="submit"
          className="px-4 py-2 bg-primary text-white rounded-lg text-sm font-medium hover:bg-primary/90 transition-colors"
        >
          Apply
        </button>
      </form>

      {/* Table */}
      {loading ? (
        <LoadingSpinner size="lg" className="py-20" />
      ) : (
        <>
          <div className="bg-white dark:bg-[#2d2a42] rounded-xl border border-lavender/30 dark:border-white/10 overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-cream-dark dark:bg-[#3a3758] text-left">
                  <th className="px-4 py-3 font-medium text-[#364153] dark:text-[#bdb9bc] whitespace-nowrap">Time</th>
                  <th className="px-4 py-3 font-medium text-[#364153] dark:text-[#bdb9bc]">Action</th>
                  <th className="px-4 py-3 font-medium text-[#364153] dark:text-[#bdb9bc]">Result</th>
                  <th className="px-4 py-3 font-medium text-[#364153] dark:text-[#bdb9bc]">User ID</th>
                  <th className="px-4 py-3 font-medium text-[#364153] dark:text-[#bdb9bc]">Target</th>
                  <th className="px-4 py-3 font-medium text-[#364153] dark:text-[#bdb9bc]">IP</th>
                  <th className="px-4 py-3 font-medium text-[#364153] dark:text-[#bdb9bc]">Details</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-lavender/20 dark:divide-white/10">
                {logs.length === 0 ? (
                  <tr>
                    <td colSpan={7} className="px-4 py-12 text-center text-gray-400">
                      No audit log entries match the current filters.
                    </td>
                  </tr>
                ) : logs.map((log) => (
                  <tr key={log.id} className="hover:bg-cream/50 dark:hover:bg-white/5">
                    <td className="px-4 py-3 text-gray-500 dark:text-gray-400 whitespace-nowrap font-mono text-xs">
                      {fmt(log.createdAt)}
                    </td>
                    <td className="px-4 py-3">
                      <ActionBadge action={log.action} />
                    </td>
                    <td className="px-4 py-3">
                      <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${
                        log.success
                          ? 'bg-green-100 text-green-700 dark:bg-green-900/40 dark:text-green-300'
                          : 'bg-red-100 text-red-700 dark:bg-red-900/40 dark:text-red-300'
                      }`}>
                        {log.success ? '✓ OK' : '✗ Fail'}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-gray-500 dark:text-gray-400 font-mono text-xs max-w-[140px] truncate" title={log.userId}>
                      {log.userId || '—'}
                    </td>
                    <td className="px-4 py-3 text-gray-500 dark:text-gray-400 font-mono text-xs max-w-[120px] truncate" title={log.targetId ?? ''}>
                      {log.targetId ?? '—'}
                    </td>
                    <td className="px-4 py-3 text-gray-500 dark:text-gray-400 text-xs whitespace-nowrap">
                      {log.ipAddress ?? '—'}
                    </td>
                    <td className="px-4 py-3 text-gray-500 dark:text-gray-400 text-xs max-w-[200px] truncate" title={log.details ?? ''}>
                      {log.details ?? '—'}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          <div className="mt-4 flex items-center justify-between text-sm text-gray-500 dark:text-gray-400">
            <span>{totalCount} entries</span>
            <div className="flex gap-2">
              <button
                disabled={page <= 1}
                onClick={() => setPage((p) => p - 1)}
                className="px-3 py-1.5 rounded-lg border border-lavender/40 dark:border-white/10 hover:bg-cream dark:hover:bg-white/5 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
              >
                ← Prev
              </button>
              <span className="px-3 py-1.5">Page {page} / {totalPages}</span>
              <button
                disabled={page >= totalPages}
                onClick={() => setPage((p) => p + 1)}
                className="px-3 py-1.5 rounded-lg border border-lavender/40 dark:border-white/10 hover:bg-cream dark:hover:bg-white/5 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
              >
                Next →
              </button>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
