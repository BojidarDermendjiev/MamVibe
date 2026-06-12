import { useTranslation } from 'react-i18next';
import { Clock, ShieldCheck, CheckCircle2, AlertTriangle, XCircle, RotateCcw } from 'lucide-react';
import type { Payment } from '@/types/payment';
import { PaymentMethod, PaymentStatus } from '@/types/payment';
import { formatPrice } from '@/utils/currency';
import TabErrorState from './TabErrorState';

interface PurchasesTabProps {
  payments: Payment[];
  error: string | null;
  onRetry: () => void;
}

interface StatusBadge {
  labelKey: string;
  cls: string;
  icon: React.ElementType;
}

// Map every PaymentStatus to a visible badge. Held-in-escrow and disputed states
// surface most prominently because the buyer can take action (confirm received /
// raise a return). Legacy Completed and Released both mean "settled" so they
// share the success treatment.
const STATUS_BADGE: Record<PaymentStatus, StatusBadge> = {
  [PaymentStatus.Pending]: {
    labelKey: 'payment.status.pending',
    cls: 'bg-gray-100 text-gray-600 dark:bg-gray-700/60 dark:text-gray-300',
    icon: Clock,
  },
  [PaymentStatus.Completed]: {
    labelKey: 'payment.status.completed',
    cls: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-500/20 dark:text-emerald-300',
    icon: CheckCircle2,
  },
  [PaymentStatus.Failed]: {
    labelKey: 'payment.status.failed',
    cls: 'bg-red-100 text-red-700 dark:bg-red-500/20 dark:text-red-300',
    icon: XCircle,
  },
  [PaymentStatus.Cancelled]: {
    labelKey: 'payment.status.cancelled',
    cls: 'bg-gray-100 text-gray-600 dark:bg-gray-700/60 dark:text-gray-300',
    icon: XCircle,
  },
  [PaymentStatus.HeldInEscrow]: {
    labelKey: 'payment.status.heldInEscrow',
    cls: 'bg-amber-100 text-amber-700 dark:bg-amber-500/20 dark:text-amber-300',
    icon: ShieldCheck,
  },
  [PaymentStatus.Released]: {
    labelKey: 'payment.status.released',
    cls: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-500/20 dark:text-emerald-300',
    icon: CheckCircle2,
  },
  [PaymentStatus.RefundedFull]: {
    labelKey: 'payment.status.refundedFull',
    cls: 'bg-blue-100 text-blue-700 dark:bg-blue-500/20 dark:text-blue-300',
    icon: RotateCcw,
  },
  [PaymentStatus.RefundedProduct]: {
    labelKey: 'payment.status.refundedProduct',
    cls: 'bg-blue-100 text-blue-700 dark:bg-blue-500/20 dark:text-blue-300',
    icon: RotateCcw,
  },
  [PaymentStatus.Disputed]: {
    labelKey: 'payment.status.disputed',
    cls: 'bg-red-100 text-red-700 dark:bg-red-500/20 dark:text-red-300',
    icon: AlertTriangle,
  },
};

export default function PurchasesTab({ payments, error, onRetry }: PurchasesTabProps) {
  const { t } = useTranslation();

  if (error) return <TabErrorState onRetry={onRetry} />;

  return (
    <div role="tabpanel" id="panel-purchases" aria-labelledby="tab-purchases">
      {payments.length === 0 ? (
        <p className="text-center py-20 text-gray-400 dark:text-gray-500">{t('dashboard.no_purchases')}</p>
      ) : (
        <div className="space-y-3">
          {payments.map((p) => {
            const badge = STATUS_BADGE[p.status] ?? STATUS_BADGE[PaymentStatus.Pending];
            const BadgeIcon = badge.icon;
            return (
              <div
                key={p.id}
                className="bg-white dark:bg-[#2d2a42] rounded-xl p-4 border border-lavender/30 dark:border-white/10 flex items-center justify-between gap-3 hover:shadow-md transition-shadow duration-300"
              >
                <div className="min-w-0">
                  <p className="font-medium text-primary dark:text-white truncate">{p.itemTitle}</p>
                  <div className="flex flex-wrap items-center gap-2 mt-1">
                    <p className="text-xs text-gray-500 dark:text-gray-400">
                      {new Date(p.createdAt).toLocaleDateString()}
                    </p>
                    <span className={`inline-flex items-center gap-1 text-[10px] font-semibold px-2 py-0.5 rounded-full ${badge.cls}`}>
                      <BadgeIcon className="h-3 w-3" />
                      {t(badge.labelKey)}
                    </span>
                  </div>
                  {p.status === PaymentStatus.HeldInEscrow && p.heldUntil && (
                    <p className="text-[11px] text-gray-500 dark:text-gray-400 mt-1">
                      {t('payment.heldUntilHint', { date: new Date(p.heldUntil).toLocaleString() })}
                    </p>
                  )}
                </div>
                <div className="flex items-center gap-3">
                  <div className="text-right">
                    <p className="font-bold text-mauve dark:text-primary-light">{formatPrice(p.amount)}</p>
                    <p className="text-xs text-gray-500 dark:text-gray-400 capitalize">
                      {p.paymentMethod === PaymentMethod.Card ? 'Card'
                        : p.paymentMethod === PaymentMethod.Booking ? 'Free'
                        : 'On Spot'}
                    </p>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
