import { useTranslation } from 'react-i18next';
import type { EBill } from '@/types/ebill';
import EBillCard from '@/components/payment/EBillCard';
import TabErrorState from './TabErrorState';

interface EBillsTabProps {
  ebills: EBill[];
  error: string | null;
  onRetry: () => void;
}

export default function EBillsTab({ ebills, error, onRetry }: EBillsTabProps) {
  const { t } = useTranslation();

  if (error) return <TabErrorState onRetry={onRetry} />;

  return (
    <div role="tabpanel" id="panel-ebills" aria-labelledby="tab-ebills">
      {ebills.length === 0 ? (
        <div className="text-center py-20">
          <p className="text-gray-400">{t('ebill.empty')}</p>
          <p className="text-sm text-gray-300 mt-1">{t('ebill.empty_hint')}</p>
        </div>
      ) : (
        <div className="space-y-3">
          {ebills.map((bill) => <EBillCard key={bill.id} bill={bill} />)}
        </div>
      )}
    </div>
  );
}
