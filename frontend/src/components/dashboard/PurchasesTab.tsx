import { useTranslation } from 'react-i18next';
import type { Payment } from '@/types/payment';
import { PaymentMethod } from '@/types/payment';
import { formatPrice } from '@/utils/currency';
import TabErrorState from './TabErrorState';

interface PurchasesTabProps {
  payments: Payment[];
  error: string | null;
  onRetry: () => void;
}

export default function PurchasesTab({ payments, error, onRetry }: PurchasesTabProps) {
  const { t } = useTranslation();

  if (error) return <TabErrorState onRetry={onRetry} />;

  return (
    <div role="tabpanel" id="panel-purchases" aria-labelledby="tab-purchases">
      {payments.length === 0 ? (
        <p className="text-center py-20 text-gray-400">{t('dashboard.no_purchases')}</p>
      ) : (
        <div className="space-y-3">
          {payments.map((p) => (
            <div key={p.id} className="bg-white rounded-xl p-4 border border-lavender/30 flex items-center justify-between hover:shadow-md transition-shadow duration-300">
              <div>
                <p className="font-medium text-primary">{p.itemTitle}</p>
                <p className="text-sm text-gray-400">{new Date(p.createdAt).toLocaleDateString()}</p>
              </div>
              <div className="flex items-center gap-3">
                <div className="text-right">
                  <p className="font-bold text-mauve">{formatPrice(p.amount)}</p>
                  <p className="text-xs text-gray-400 capitalize">
                    {p.paymentMethod === PaymentMethod.Card ? 'Card'
                      : p.paymentMethod === PaymentMethod.Booking ? 'Free'
                      : 'On Spot'}
                  </p>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
