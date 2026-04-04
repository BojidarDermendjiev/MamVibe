import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import toast from '@/utils/toast';
import { walletApi } from '../../api/walletApi';
import { formatEur } from '../../utils/currency';
import {
  WalletStatus,
  WalletTransactionKind,
  WalletTransactionStatus,
  WalletTransactionType,
  type AdminWalletDto,
  type WalletTransactionDto,
} from '../../types/wallet';
import type { PagedResult } from '../../types/item';
import LoadingSpinner from '../../components/common/LoadingSpinner';
import Button from '../../components/common/Button';

// ── Helpers ───────────────────────────────────────────────────────────────────

function kindLabel(kind: WalletTransactionKind): string {
  const map: Record<WalletTransactionKind, string> = {
    [WalletTransactionKind.TopUp]: 'Top Up',
    [WalletTransactionKind.Transfer]: 'Transfer',
    [WalletTransactionKind.ItemPayment]: 'Item Payment',
    [WalletTransactionKind.Withdrawal]: 'Withdrawal',
    [WalletTransactionKind.Refund]: 'Refund',
    [WalletTransactionKind.Fee]: 'Fee',
  };
  return map[kind] ?? String(kind);
}

function txStatusColor(status: WalletTransactionStatus): string {
  if (status === WalletTransactionStatus.Completed) return 'bg-green-100 text-green-700';
  if (status === WalletTransactionStatus.Pending) return 'bg-yellow-100 text-yellow-700';
  if (status === WalletTransactionStatus.Reversed) return 'bg-blue-100 text-blue-700';
  return 'bg-red-100 text-red-600';
}

function txStatusLabel(status: WalletTransactionStatus): string {
  if (status === WalletTransactionStatus.Completed) return 'Completed';
  if (status === WalletTransactionStatus.Pending) return 'Pending';
  if (status === WalletTransactionStatus.Reversed) return 'Reversed';
  return 'Failed';
}

function walletStatusColor(status: WalletStatus): string {
  if (status === WalletStatus.Active) return 'bg-green-100 text-green-700';
  if (status === WalletStatus.Frozen) return 'bg-blue-100 text-blue-700';
  if (status === WalletStatus.Suspended) return 'bg-yellow-100 text-yellow-700';
  return 'bg-red-100 text-red-600';
}

function walletStatusLabel(status: WalletStatus): string {
  if (status === WalletStatus.Active) return 'Active';
  if (status === WalletStatus.Frozen) return 'Frozen';
  if (status === WalletStatus.Suspended) return 'Suspended';
  return 'Closed';
}

// ── Reason modal ──────────────────────────────────────────────────────────────

function ReasonModal({
  title,
  label,
  onConfirm,
  onClose,
}: {
  title: string;
  label: string;
  onConfirm: (reason: string) => Promise<void>;
  onClose: () => void;
}) {
  const { t } = useTranslation();
  const [reason, setReason] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!reason.trim()) return;
    setLoading(true);
    await onConfirm(reason);
    setLoading(false);
  };

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
      <div className="bg-white dark:bg-[#2d2a42] rounded-xl p-6 w-full max-w-md shadow-xl">
        <h2 className="text-lg font-bold text-[#364153] dark:text-[#bdb9bc] mb-4">{title}</h2>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-600 dark:text-gray-400 mb-1">{label}</label>
            <textarea
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              maxLength={500}
              rows={3}
              className="w-full px-4 py-2.5 rounded-lg border border-lavender bg-white dark:bg-[#2a2740] dark:border-white/10 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-primary resize-none"
            />
          </div>
          <div className="flex gap-3">
            <Button type="submit" disabled={loading || !reason.trim()} className="flex-1">
              {loading ? t('common.loading') : t('common.confirm')}
            </Button>
            <Button type="button" variant="secondary" onClick={onClose} className="flex-1">
              {t('common.cancel')}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}

// ── Tabs ──────────────────────────────────────────────────────────────────────

type Tab = 'wallets' | 'transactions' | 'withdrawals';

// ── Wallets tab ───────────────────────────────────────────────────────────────

function WalletsTab() {
  const { t } = useTranslation();
  const [data, setData] = useState<PagedResult<AdminWalletDto> | null>(null);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [freezeTarget, setFreezeTarget] = useState<AdminWalletDto | null>(null);

  const fetchWallets = useCallback(async (p: number) => {
    setLoading(true);
    try {
      const { data: res } = await walletApi.admin.getWallets(p, 20);
      setData(res);
    } catch { /* ignore */ } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchWallets(1); }, [fetchWallets]);

  const handleFreeze = async (reason: string) => {
    if (!freezeTarget) return;
    try {
      await walletApi.admin.freezeWallet(freezeTarget.id, reason);
      toast.success(t('wallet.admin_freeze_success'));
      setFreezeTarget(null);
      fetchWallets(page);
    } catch {
      toast.error(t('common.error'));
    }
  };

  const handleUnfreeze = async (wallet: AdminWalletDto) => {
    try {
      await walletApi.admin.unfreezeWallet(wallet.id);
      toast.success(t('wallet.admin_unfreeze_success'));
      fetchWallets(page);
    } catch {
      toast.error(t('common.error'));
    }
  };

  if (loading) return <LoadingSpinner size="lg" className="py-10" />;

  return (
    <>
      <div className="bg-white dark:bg-[#2d2a42] rounded-xl border border-lavender/30 dark:border-white/10 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="bg-cream-dark dark:bg-[#3a3758] text-left">
                <th className="px-4 py-3 text-xs font-medium text-[#364153] dark:text-[#bdb9bc]">{t('wallet.admin_user')}</th>
                <th className="px-4 py-3 text-xs font-medium text-[#364153] dark:text-[#bdb9bc]">{t('wallet.balance')}</th>
                <th className="px-4 py-3 text-xs font-medium text-[#364153] dark:text-[#bdb9bc]">{t('wallet.status')}</th>
                <th className="px-4 py-3 text-xs font-medium text-[#364153] dark:text-[#bdb9bc]">{t('wallet.admin_tx_count')}</th>
                <th className="px-4 py-3 text-xs font-medium text-[#364153] dark:text-[#bdb9bc]">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-lavender/20 dark:divide-white/10">
              {data?.items.map((w) => (
                <tr key={w.id} className="hover:bg-cream/50 dark:hover:bg-white/5">
                  <td className="px-4 py-3">
                    <p className="text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">{w.userDisplayName}</p>
                    <p className="text-xs text-gray-400">{w.userEmail}</p>
                  </td>
                  <td className="px-4 py-3 text-sm font-semibold text-[#364153] dark:text-[#bdb9bc]">
                    {formatEur(w.balance)}
                  </td>
                  <td className="px-4 py-3">
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${walletStatusColor(w.status)}`}>
                      {walletStatusLabel(w.status)}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-500">{w.transactionCount}</td>
                  <td className="px-4 py-3">
                    <div className="flex gap-2">
                      {w.status === WalletStatus.Active ? (
                        <Button size="sm" variant="danger" onClick={() => setFreezeTarget(w)}>
                          {t('wallet.admin_freeze')}
                        </Button>
                      ) : w.status === WalletStatus.Frozen ? (
                        <Button size="sm" variant="secondary" onClick={() => handleUnfreeze(w)}>
                          {t('wallet.admin_unfreeze')}
                        </Button>
                      ) : null}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {data && data.totalPages > 1 && (
          <div className="flex items-center justify-between px-4 py-3 border-t border-lavender/20 dark:border-white/10">
            <Button size="sm" variant="secondary" disabled={page <= 1} onClick={() => { const p = page - 1; setPage(p); fetchWallets(p); }}>
              {t('common.back')}
            </Button>
            <span className="text-sm text-gray-500">{page} / {data.totalPages}</span>
            <Button size="sm" variant="secondary" disabled={page >= data.totalPages} onClick={() => { const p = page + 1; setPage(p); fetchWallets(p); }}>
              {t('common.next')}
            </Button>
          </div>
        )}
      </div>

      {freezeTarget && (
        <ReasonModal
          title={t('wallet.admin_freeze')}
          label={t('wallet.admin_freeze_reason')}
          onConfirm={handleFreeze}
          onClose={() => setFreezeTarget(null)}
        />
      )}
    </>
  );
}

// ── Transactions tab ──────────────────────────────────────────────────────────

function TransactionsTab() {
  const { t } = useTranslation();
  const [data, setData] = useState<PagedResult<WalletTransactionDto> | null>(null);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [refundTarget, setRefundTarget] = useState<WalletTransactionDto | null>(null);

  const fetchTx = useCallback(async (p: number) => {
    setLoading(true);
    try {
      const { data: res } = await walletApi.admin.getTransactions({ page: p, pageSize: 20 });
      setData(res);
    } catch { /* ignore */ } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchTx(1); }, [fetchTx]);

  const handleRefund = async (reason: string) => {
    if (!refundTarget) return;
    try {
      await walletApi.admin.refundTransaction(refundTarget.id, reason);
      toast.success(t('wallet.admin_refund_success'));
      setRefundTarget(null);
      fetchTx(page);
    } catch {
      toast.error(t('common.error'));
    }
  };

  if (loading) return <LoadingSpinner size="lg" className="py-10" />;

  return (
    <>
      <div className="bg-white dark:bg-[#2d2a42] rounded-xl border border-lavender/30 dark:border-white/10 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="bg-cream-dark dark:bg-[#3a3758] text-left">
                <th className="px-4 py-3 text-xs font-medium text-[#364153] dark:text-[#bdb9bc]">{t('wallet.date')}</th>
                <th className="px-4 py-3 text-xs font-medium text-[#364153] dark:text-[#bdb9bc]">Type</th>
                <th className="px-4 py-3 text-xs font-medium text-[#364153] dark:text-[#bdb9bc]">{t('wallet.description')}</th>
                <th className="px-4 py-3 text-xs font-medium text-right text-[#364153] dark:text-[#bdb9bc]">{t('wallet.amount')}</th>
                <th className="px-4 py-3 text-xs font-medium text-right text-[#364153] dark:text-[#bdb9bc]">{t('wallet.balance_after')}</th>
                <th className="px-4 py-3 text-xs font-medium text-[#364153] dark:text-[#bdb9bc]">{t('wallet.status')}</th>
                <th className="px-4 py-3 text-xs font-medium text-[#364153] dark:text-[#bdb9bc]">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-lavender/20 dark:divide-white/10">
              {data?.items.map((tx) => {
                const isCredit = tx.type === WalletTransactionType.Credit;
                const canRefund = tx.status === WalletTransactionStatus.Completed && isCredit === false;
                return (
                  <tr key={tx.id} className="hover:bg-cream/50 dark:hover:bg-white/5">
                    <td className="px-4 py-3 text-xs text-gray-500 dark:text-gray-400 whitespace-nowrap">
                      {new Date(tx.createdAt).toLocaleString()}
                    </td>
                    <td className="px-4 py-3 text-sm text-[#364153] dark:text-[#bdb9bc]">{kindLabel(tx.kind)}</td>
                    <td className="px-4 py-3 text-xs text-gray-500 dark:text-gray-400 max-w-[160px] truncate">
                      {tx.description ?? tx.reference ?? '—'}
                    </td>
                    <td className="px-4 py-3 text-sm font-semibold text-right">
                      <span className={isCredit ? 'text-green-600' : 'text-red-500'}>
                        {isCredit ? '+' : '-'}{formatEur(tx.amount)}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-sm text-right text-gray-500">{formatEur(tx.balanceAfter)}</td>
                    <td className="px-4 py-3">
                      <span className={`px-2 py-1 rounded-full text-xs font-medium ${txStatusColor(tx.status)}`}>
                        {txStatusLabel(tx.status)}
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex gap-1 items-center">
                        {canRefund && (
                          <Button size="sm" variant="secondary" onClick={() => setRefundTarget(tx)}>
                            {t('wallet.admin_refund')}
                          </Button>
                        )}
                        {tx.receiptUrl && (
                          <a href={tx.receiptUrl} target="_blank" rel="noopener noreferrer"
                            className="text-xs text-primary hover:underline px-1">PDF</a>
                        )}
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>

        {data && data.totalPages > 1 && (
          <div className="flex items-center justify-between px-4 py-3 border-t border-lavender/20 dark:border-white/10">
            <Button size="sm" variant="secondary" disabled={page <= 1} onClick={() => { const p = page - 1; setPage(p); fetchTx(p); }}>
              {t('common.back')}
            </Button>
            <span className="text-sm text-gray-500">{page} / {data.totalPages}</span>
            <Button size="sm" variant="secondary" disabled={page >= data.totalPages} onClick={() => { const p = page + 1; setPage(p); fetchTx(p); }}>
              {t('common.next')}
            </Button>
          </div>
        )}
      </div>

      {refundTarget && (
        <ReasonModal
          title={t('wallet.admin_refund')}
          label={t('wallet.admin_refund_reason')}
          onConfirm={handleRefund}
          onClose={() => setRefundTarget(null)}
        />
      )}
    </>
  );
}

// ── Withdrawals tab ───────────────────────────────────────────────────────────

function WithdrawalsTab() {
  const { t } = useTranslation();
  const [data, setData] = useState<PagedResult<WalletTransactionDto> | null>(null);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [rejectTarget, setRejectTarget] = useState<WalletTransactionDto | null>(null);

  const fetchWithdrawals = useCallback(async (p: number) => {
    setLoading(true);
    try {
      const { data: res } = await walletApi.admin.getPendingWithdrawals(p, 20);
      setData(res);
    } catch { /* ignore */ } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchWithdrawals(1); }, [fetchWithdrawals]);

  const handleApprove = async (tx: WalletTransactionDto) => {
    try {
      await walletApi.admin.approveWithdrawal(tx.id);
      toast.success(t('wallet.admin_approve_success'));
      fetchWithdrawals(page);
    } catch {
      toast.error(t('common.error'));
    }
  };

  const handleReject = async (reason: string) => {
    if (!rejectTarget) return;
    try {
      await walletApi.admin.rejectWithdrawal(rejectTarget.id, reason);
      toast.success(t('wallet.admin_reject_success'));
      setRejectTarget(null);
      fetchWithdrawals(page);
    } catch {
      toast.error(t('common.error'));
    }
  };

  if (loading) return <LoadingSpinner size="lg" className="py-10" />;

  return (
    <>
      <div className="bg-white dark:bg-[#2d2a42] rounded-xl border border-lavender/30 dark:border-white/10 overflow-hidden">
        {!data || data.items.length === 0 ? (
          <p className="text-center text-gray-400 py-10">No pending withdrawals</p>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="bg-cream-dark dark:bg-[#3a3758] text-left">
                    <th className="px-4 py-3 text-xs font-medium text-[#364153] dark:text-[#bdb9bc]">{t('wallet.date')}</th>
                    <th className="px-4 py-3 text-xs font-medium text-[#364153] dark:text-[#bdb9bc]">{t('wallet.admin_wallet_id')}</th>
                    <th className="px-4 py-3 text-xs font-medium text-right text-[#364153] dark:text-[#bdb9bc]">{t('wallet.amount')}</th>
                    <th className="px-4 py-3 text-xs font-medium text-[#364153] dark:text-[#bdb9bc]">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-lavender/20 dark:divide-white/10">
                  {data.items.map((tx) => (
                    <tr key={tx.id} className="hover:bg-cream/50 dark:hover:bg-white/5">
                      <td className="px-4 py-3 text-xs text-gray-500 whitespace-nowrap">
                        {new Date(tx.createdAt).toLocaleString()}
                      </td>
                      <td className="px-4 py-3 text-xs text-gray-400 font-mono">{tx.walletId}</td>
                      <td className="px-4 py-3 text-sm font-semibold text-right text-red-500">
                        -{formatEur(tx.amount)}
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex gap-2">
                          <Button size="sm" onClick={() => handleApprove(tx)}>
                            {t('wallet.admin_approve')}
                          </Button>
                          <Button size="sm" variant="danger" onClick={() => setRejectTarget(tx)}>
                            {t('wallet.admin_reject')}
                          </Button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {data.totalPages > 1 && (
              <div className="flex items-center justify-between px-4 py-3 border-t border-lavender/20 dark:border-white/10">
                <Button size="sm" variant="secondary" disabled={page <= 1} onClick={() => { const p = page - 1; setPage(p); fetchWithdrawals(p); }}>
                  {t('common.back')}
                </Button>
                <span className="text-sm text-gray-500">{page} / {data.totalPages}</span>
                <Button size="sm" variant="secondary" disabled={page >= data.totalPages} onClick={() => { const p = page + 1; setPage(p); fetchWithdrawals(p); }}>
                  {t('common.next')}
                </Button>
              </div>
            )}
          </>
        )}
      </div>

      {rejectTarget && (
        <ReasonModal
          title={t('wallet.admin_reject')}
          label={t('wallet.admin_reject_reason')}
          onConfirm={handleReject}
          onClose={() => setRejectTarget(null)}
        />
      )}
    </>
  );
}

// ── Main page ─────────────────────────────────────────────────────────────────

export default function AdminWalletsPage() {
  const { t } = useTranslation();
  const [activeTab, setActiveTab] = useState<Tab>('wallets');

  const tabs: { id: Tab; label: string }[] = [
    { id: 'wallets', label: t('wallet.admin_wallets') },
    { id: 'transactions', label: t('wallet.admin_transactions') },
    { id: 'withdrawals', label: t('wallet.admin_withdrawals') },
  ];

  return (
    <div>
      <h1 className="text-3xl font-bold text-[#364153] dark:text-[#bdb9bc] mb-6">{t('wallet.admin_title')}</h1>

      {/* Tab bar */}
      <div className="flex gap-1 mb-6 border-b border-lavender/30 dark:border-white/10">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            onClick={() => setActiveTab(tab.id)}
            className={`px-4 py-2 text-sm font-medium transition-colors rounded-t-lg -mb-px border-b-2 ${
              activeTab === tab.id
                ? 'border-primary text-primary'
                : 'border-transparent text-gray-500 hover:text-primary'
            }`}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {activeTab === 'wallets' && <WalletsTab />}
      {activeTab === 'transactions' && <TransactionsTab />}
      {activeTab === 'withdrawals' && <WithdrawalsTab />}
    </div>
  );
}
