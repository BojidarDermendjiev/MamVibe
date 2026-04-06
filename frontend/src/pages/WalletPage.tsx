import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { loadStripe } from '@stripe/stripe-js';
import { Elements, PaymentElement, useStripe, useElements } from '@stripe/react-stripe-js';
import toast from '@/utils/toast';
import { walletApi } from '../api/walletApi';
import { formatEur } from '../utils/currency';
import {
  WalletStatus,
  WalletTransactionKind,
  WalletTransactionStatus,
  WalletTransactionType,
  type WalletDto,
  type WalletTransactionDto,
} from '../types/wallet';
import type { PagedResult } from '../types/item';
import LoadingSpinner from '../components/common/LoadingSpinner';
import Button from '../components/common/Button';

const stripePromise = loadStripe(import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY ?? '');

// ── Helpers ───────────────────────────────────────────────────────────────────

function useKindLabel(kind: WalletTransactionKind): string {
  const { t } = useTranslation();
  const map: Record<WalletTransactionKind, string> = {
    [WalletTransactionKind.TopUp]: t('wallet.kind_topup'),
    [WalletTransactionKind.Transfer]: t('wallet.kind_transfer'),
    [WalletTransactionKind.ItemPayment]: t('wallet.kind_item_payment'),
    [WalletTransactionKind.Withdrawal]: t('wallet.kind_withdrawal'),
    [WalletTransactionKind.Refund]: t('wallet.kind_refund'),
    [WalletTransactionKind.Fee]: t('wallet.kind_fee'),
  };
  return map[kind] ?? String(kind);
}

function useTxStatusLabel(status: WalletTransactionStatus): string {
  const { t } = useTranslation();
  const map: Record<WalletTransactionStatus, string> = {
    [WalletTransactionStatus.Pending]: t('wallet.tx_status_pending'),
    [WalletTransactionStatus.Completed]: t('wallet.tx_status_completed'),
    [WalletTransactionStatus.Failed]: t('wallet.tx_status_failed'),
    [WalletTransactionStatus.Reversed]: t('wallet.tx_status_reversed'),
  };
  return map[status] ?? String(status);
}

function txStatusColor(status: WalletTransactionStatus): string {
  if (status === WalletTransactionStatus.Completed) return 'bg-green-100 text-green-700';
  if (status === WalletTransactionStatus.Pending) return 'bg-yellow-100 text-yellow-700';
  if (status === WalletTransactionStatus.Reversed) return 'bg-blue-100 text-blue-700';
  return 'bg-red-100 text-red-600';
}

function walletStatusColor(status: WalletStatus): string {
  if (status === WalletStatus.Active) return 'bg-green-100 text-green-700';
  if (status === WalletStatus.Frozen) return 'bg-blue-100 text-blue-700';
  if (status === WalletStatus.Suspended) return 'bg-yellow-100 text-yellow-700';
  return 'bg-red-100 text-red-600';
}

// ── Transaction row ───────────────────────────────────────────────────────────

function TxRow({ tx }: { tx: WalletTransactionDto }) {
  const kindLabel = useKindLabel(tx.kind);
  const statusLabel = useTxStatusLabel(tx.status);
  const isCredit = tx.type === WalletTransactionType.Credit;

  return (
    <tr className="hover:bg-cream/50 dark:hover:bg-white/5">
      <td className="px-4 py-3 text-sm text-gray-500 dark:text-gray-400">
        {new Date(tx.createdAt).toLocaleString()}
      </td>
      <td className="px-4 py-3 text-sm text-[#364153] dark:text-[#bdb9bc]">{kindLabel}</td>
      <td className="px-4 py-3 text-sm text-gray-500 dark:text-gray-400 max-w-[200px] truncate">
        {tx.description ?? tx.reference ?? '—'}
      </td>
      <td className="px-4 py-3 text-sm font-semibold text-right">
        <span className={isCredit ? 'text-green-600' : 'text-red-500'}>
          {isCredit ? '+' : '-'}{formatEur(tx.amount)}
        </span>
      </td>
      <td className="px-4 py-3 text-sm text-right text-gray-500 dark:text-gray-400">
        {formatEur(tx.balanceAfter)}
      </td>
      <td className="px-4 py-3">
        <span className={`px-2 py-1 rounded-full text-xs font-medium ${txStatusColor(tx.status)}`}>
          {statusLabel}
        </span>
      </td>
      <td className="px-4 py-3">
        {tx.receiptUrl && (
          <a
            href={tx.receiptUrl}
            target="_blank"
            rel="noopener noreferrer"
            className="text-xs text-primary hover:underline"
          >
            PDF
          </a>
        )}
      </td>
    </tr>
  );
}

// ── Top-Up modal (Stripe Elements) ───────────────────────────────────────────

function TopUpForm({ onSuccess, onClose }: { onSuccess: () => void; onClose: () => void }) {
  const { t } = useTranslation();
  const stripe = useStripe();
  const elements = useElements();
  const [processing, setProcessing] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!stripe || !elements) return;
    setProcessing(true);
    const { error } = await stripe.confirmPayment({
      elements,
      confirmParams: { return_url: `${window.location.origin}/wallet` },
      redirect: 'if_required',
    });
    if (error) {
      toast.error(error.message ?? t('wallet.top_up_error'));
      setProcessing(false);
    } else {
      toast.success(t('wallet.top_up_success'));
      onSuccess();
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <PaymentElement />
      <div className="flex gap-3 pt-2">
        <Button type="submit" disabled={processing || !stripe} className="flex-1">
          {processing ? t('wallet.top_up_processing') : t('wallet.top_up')}
        </Button>
        <Button type="button" variant="secondary" onClick={onClose} className="flex-1">
          {t('common.cancel')}
        </Button>
      </div>
    </form>
  );
}

function TopUpModal({ onSuccess, onClose }: { onSuccess: () => void; onClose: () => void }) {
  const { t } = useTranslation();
  const [amount, setAmount] = useState('');
  const [clientSecret, setClientSecret] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleAmountSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const parsed = parseFloat(amount);
    if (isNaN(parsed) || parsed < 0.01) { setError(t('wallet.top_up_min')); return; }
    if (parsed > 5000) { setError(t('wallet.top_up_max')); return; }
    setLoading(true);
    setError('');
    try {
      const { data } = await walletApi.createTopUp(parsed);
      setClientSecret(data.clientSecret);
    } catch {
      setError(t('wallet.top_up_error'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
      <div className="bg-white dark:bg-[#2d2a42] rounded-xl p-6 w-full max-w-md shadow-xl">
        <h2 className="text-xl font-bold text-[#364153] dark:text-[#bdb9bc] mb-4">{t('wallet.top_up_title')}</h2>

        {!clientSecret ? (
          <form onSubmit={handleAmountSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-600 dark:text-gray-400 mb-1">
                {t('wallet.top_up_amount')}
              </label>
              <input
                type="number"
                step="0.01"
                min="0.01"
                max="5000"
                value={amount}
                onChange={(e) => setAmount(e.target.value)}
                placeholder="0.00"
                className="w-full px-4 py-2.5 rounded-lg border border-lavender bg-white dark:bg-[#2a2740] dark:border-white/10 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-primary [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none"
              />
              {error && <p className="text-red-500 text-xs mt-1">{error}</p>}
            </div>
            <div className="flex gap-3">
              <Button type="submit" disabled={loading} className="flex-1">
                {loading ? t('common.loading') : t('common.next')}
              </Button>
              <Button type="button" variant="secondary" onClick={onClose} className="flex-1">
                {t('common.cancel')}
              </Button>
            </div>
          </form>
        ) : (
          <Elements stripe={stripePromise} options={{ clientSecret }}>
            <TopUpForm onSuccess={() => { onSuccess(); onClose(); }} onClose={onClose} />
          </Elements>
        )}
      </div>
    </div>
  );
}

// ── Transfer modal ────────────────────────────────────────────────────────────

function TransferModal({ onSuccess, onClose }: { onSuccess: () => void; onClose: () => void }) {
  const { t } = useTranslation();
  const [email, setEmail] = useState('');
  const [amount, setAmount] = useState('');
  const [note, setNote] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    const parsed = parseFloat(amount);
    if (!email) { setError(t('auth.email')); return; }
    if (isNaN(parsed) || parsed <= 0) { setError(t('wallet.top_up_min')); return; }
    setLoading(true);
    try {
      await walletApi.transfer(email, parsed, note || undefined);
      toast.success(t('wallet.transfer_success'));
      onSuccess();
      onClose();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error;
      setError(msg ?? t('wallet.transfer_error'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
      <div className="bg-white dark:bg-[#2d2a42] rounded-xl p-6 w-full max-w-md shadow-xl">
        <h2 className="text-xl font-bold text-[#364153] dark:text-[#bdb9bc] mb-4">{t('wallet.transfer_title')}</h2>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-600 dark:text-gray-400 mb-1">{t('wallet.transfer_email')}</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full px-4 py-2.5 rounded-lg border border-lavender bg-white dark:bg-[#2a2740] dark:border-white/10 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-primary [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-600 dark:text-gray-400 mb-1">{t('wallet.transfer_amount')}</label>
            <input
              type="number"
              step="0.01"
              min="0.01"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              placeholder="0.00"
              className="w-full px-4 py-2.5 rounded-lg border border-lavender bg-white dark:bg-[#2a2740] dark:border-white/10 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-primary [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-600 dark:text-gray-400 mb-1">{t('wallet.transfer_note')}</label>
            <input
              type="text"
              value={note}
              maxLength={200}
              onChange={(e) => setNote(e.target.value)}
              className="w-full px-4 py-2.5 rounded-lg border border-lavender bg-white dark:bg-[#2a2740] dark:border-white/10 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-primary [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none"
            />
          </div>
          {error && <p className="text-red-500 text-sm">{error}</p>}
          <div className="flex gap-3">
            <Button type="submit" disabled={loading} className="flex-1">
              {loading ? t('common.loading') : t('wallet.transfer')}
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

// ── Withdraw modal ────────────────────────────────────────────────────────────

function WithdrawModal({ onSuccess, onClose }: { onSuccess: () => void; onClose: () => void }) {
  const { t } = useTranslation();
  const [amount, setAmount] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    const parsed = parseFloat(amount);
    if (isNaN(parsed) || parsed < 1) { setError(t('wallet.top_up_min')); return; }
    setLoading(true);
    try {
      await walletApi.withdraw(parsed);
      toast.success(t('wallet.withdraw_success'));
      onSuccess();
      onClose();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error;
      setError(msg ?? t('wallet.withdraw_error'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
      <div className="bg-white dark:bg-[#2d2a42] rounded-xl p-6 w-full max-w-md shadow-xl">
        <h2 className="text-xl font-bold text-[#364153] dark:text-[#bdb9bc] mb-4">{t('wallet.withdraw_title')}</h2>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-600 dark:text-gray-400 mb-1">{t('wallet.withdraw_amount')}</label>
            <input
              type="number"
              step="0.01"
              min="1"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              placeholder="0.00"
              className="w-full px-4 py-2.5 rounded-lg border border-lavender bg-white dark:bg-[#2a2740] dark:border-white/10 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-primary [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none"
            />
          </div>
          <p className="text-xs text-gray-400">{t('wallet.withdraw_note')}</p>
          {error && <p className="text-red-500 text-sm">{error}</p>}
          <div className="flex gap-3">
            <Button type="submit" disabled={loading} className="flex-1">
              {loading ? t('common.loading') : t('wallet.withdraw')}
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

// ── Main page ─────────────────────────────────────────────────────────────────

type Modal = 'topup' | 'transfer' | 'withdraw' | null;

export default function WalletPage() {
  const { t } = useTranslation();
  const [wallet, setWallet] = useState<WalletDto | null>(null);
  const [txData, setTxData] = useState<PagedResult<WalletTransactionDto> | null>(null);
  const [loading, setLoading] = useState(true);
  const [txPage, setTxPage] = useState(1);
  const [modal, setModal] = useState<Modal>(null);

  const fetchWallet = useCallback(async () => {
    try {
      const { data } = await walletApi.getWallet();
      setWallet(data);
    } catch { /* ignore */ }
  }, []);

  const fetchTx = useCallback(async (page: number) => {
    try {
      const { data } = await walletApi.getTransactions(page, 20);
      setTxData(data);
    } catch { /* ignore */ }
  }, []);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      setLoading(true);
      await Promise.all([fetchWallet(), fetchTx(1)]);
      if (!cancelled) setLoading(false);
    })();
    return () => { cancelled = true; };
  }, [fetchWallet, fetchTx]);

  const handleModalSuccess = () => {
    fetchWallet();
    fetchTx(txPage);
  };

  const walletStatusLabel: Record<WalletStatus, string> = {
    [WalletStatus.Active]: t('wallet.status_active'),
    [WalletStatus.Frozen]: t('wallet.status_frozen'),
    [WalletStatus.Suspended]: t('wallet.status_suspended'),
    [WalletStatus.Closed]: t('wallet.status_closed'),
  };

  if (loading) return <LoadingSpinner size="lg" className="py-20" />;

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold text-primary mb-6">{t('wallet.title')}</h1>

      {/* Balance card */}
      {wallet && (
        <div className="bg-white dark:bg-[#2d2a42] rounded-xl p-6 border border-lavender/30 dark:border-white/10 mb-6">
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <div>
              <p className="text-sm text-gray-500 dark:text-gray-400">{t('wallet.balance')}</p>
              <p className="text-4xl font-bold text-primary mt-1">{formatEur(wallet.balance)}</p>
              <div className="mt-2">
                <span className={`px-2 py-1 rounded-full text-xs font-medium ${walletStatusColor(wallet.status)}`}>
                  {walletStatusLabel[wallet.status]}
                </span>
              </div>
            </div>

            {wallet.status === WalletStatus.Active && (
              <div className="flex gap-2 flex-wrap">
                <Button size="sm" onClick={() => setModal('topup')}>{t('wallet.top_up')}</Button>
                <Button size="sm" variant="secondary" onClick={() => setModal('transfer')}>{t('wallet.transfer')}</Button>
                <Button size="sm" variant="secondary" onClick={() => setModal('withdraw')}>{t('wallet.withdraw')}</Button>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Transactions */}
      <div className="bg-white dark:bg-[#2d2a42] rounded-xl border border-lavender/30 dark:border-white/10 overflow-hidden">
        <div className="px-4 py-3 border-b border-lavender/20 dark:border-white/10">
          <h2 className="font-semibold text-[#364153] dark:text-[#bdb9bc]">{t('wallet.transactions')}</h2>
        </div>

        {!txData || txData.items.length === 0 ? (
          <p className="text-center text-gray-400 py-10">{t('wallet.no_transactions')}</p>
        ) : (
          <>
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
                    <th className="px-4 py-3 text-xs font-medium text-[#364153] dark:text-[#bdb9bc]">{t('wallet.receipt')}</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-lavender/20 dark:divide-white/10">
                  {txData.items.map((tx) => <TxRow key={tx.id} tx={tx} />)}
                </tbody>
              </table>
            </div>

            {/* Pagination */}
            {txData.totalPages > 1 && (
              <div className="flex items-center justify-between px-4 py-3 border-t border-lavender/20 dark:border-white/10">
                <Button
                  size="sm"
                  variant="secondary"
                  disabled={txPage <= 1}
                  onClick={() => { const p = txPage - 1; setTxPage(p); fetchTx(p); }}
                >
                  {t('common.back')}
                </Button>
                <span className="text-sm text-gray-500">{txPage} / {txData.totalPages}</span>
                <Button
                  size="sm"
                  variant="secondary"
                  disabled={txPage >= txData.totalPages}
                  onClick={() => { const p = txPage + 1; setTxPage(p); fetchTx(p); }}
                >
                  {t('common.next')}
                </Button>
              </div>
            )}
          </>
        )}
      </div>

      {/* Modals */}
      {modal === 'topup' && (
        <TopUpModal onSuccess={handleModalSuccess} onClose={() => setModal(null)} />
      )}
      {modal === 'transfer' && (
        <TransferModal onSuccess={handleModalSuccess} onClose={() => setModal(null)} />
      )}
      {modal === 'withdraw' && (
        <WithdrawModal onSuccess={handleModalSuccess} onClose={() => setModal(null)} />
      )}
    </div>
  );
}
