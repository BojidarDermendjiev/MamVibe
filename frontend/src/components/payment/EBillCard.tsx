import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { HiOutlineDocumentText, HiOutlineDownload, HiOutlineMail } from 'react-icons/hi';
import { ebillsApi } from '../../api/ebillsApi';
import { PaymentMethod } from '../../types/payment';
import type { EBill } from '../../types/ebill';
import { formatPrice } from '../../utils/currency';
import toast from '../../utils/toast';

interface EBillCardProps {
  bill: EBill;
}

export default function EBillCard({ bill }: EBillCardProps) {
  const { t } = useTranslation();
  const [resending, setResending] = useState(false);

  const methodLabel = () => {
    if (bill.paymentMethod === PaymentMethod.Card) return t('payment.card');
    return String(bill.paymentMethod);
  };

  const handleResend = async () => {
    setResending(true);
    try {
      await ebillsApi.resendEmail(bill.id);
      toast.success(t('ebill.resent_ok'));
    } catch {
      toast.error(t('common.error'));
    } finally {
      setResending(false);
    }
  };

  return (
    <div className="bg-white rounded-xl border border-lavender/30 p-4 hover:shadow-md transition-shadow duration-300">
      <div className="flex items-start gap-4">

        {/* Icon */}
        <div className="flex-shrink-0 w-10 h-10 rounded-lg bg-primary/10 flex items-center justify-center">
          <HiOutlineDocumentText className="w-5 h-5 text-primary" />
        </div>

        {/* Main info */}
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2 flex-wrap mb-0.5">
            <span className="text-xs font-mono font-semibold text-primary bg-primary/10 px-2 py-0.5 rounded-full">
              {bill.eBillNumber ?? '—'}
            </span>
            <span className="text-xs text-gray-400 bg-gray-100 px-2 py-0.5 rounded-full">
              {methodLabel()}
            </span>
          </div>

          <p className="font-medium text-gray-900 truncate mt-1">{bill.itemTitle ?? t('common.error')}</p>

          <p className="text-xs text-gray-400 mt-0.5">
            {t('ebill.seller')}: <span className="text-gray-600">{bill.sellerDisplayName ?? '—'}</span>
          </p>

          <p className="text-xs text-gray-400 mt-0.5">
            {t('ebill.issued')}: {new Date(bill.issuedAt).toLocaleDateString()}
          </p>
        </div>

        {/* Right: amount + actions */}
        <div className="flex-shrink-0 flex flex-col items-end gap-2">
          <p className="font-bold text-mauve text-base">{formatPrice(bill.amount)}</p>

          <div className="flex items-center gap-1.5">
            {/* Download */}
            {bill.receiptUrl ? (
              <a
                href={bill.receiptUrl}
                target="_blank"
                rel="noopener noreferrer"
                className="flex items-center gap-1 px-2.5 py-1.5 text-xs font-medium text-white bg-primary rounded-lg hover:bg-primary-dark transition-colors"
                title={t('ebill.download')}
              >
                <HiOutlineDownload className="w-3.5 h-3.5" />
                {t('ebill.download')}
              </a>
            ) : (
              <span
                className="flex items-center gap-1 px-2.5 py-1.5 text-xs font-medium text-gray-400 bg-gray-100 rounded-lg cursor-not-allowed"
                title={t('ebill.download')}
              >
                <HiOutlineDownload className="w-3.5 h-3.5" />
                {t('ebill.download')}
              </span>
            )}

            {/* Resend */}
            <button
              onClick={handleResend}
              disabled={resending}
              className="flex items-center gap-1 px-2.5 py-1.5 text-xs font-medium text-primary border border-primary/30 rounded-lg hover:bg-primary/5 transition-colors disabled:opacity-50 disabled:cursor-wait"
              title={t('ebill.resend')}
            >
              <HiOutlineMail className="w-3.5 h-3.5" />
              {resending ? '…' : t('ebill.resend')}
            </button>
          </div>
        </div>

      </div>
    </div>
  );
}
