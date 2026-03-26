import { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { HiArrowLeft } from 'react-icons/hi';
import { Heart } from 'lucide-react';
import toast from '@/utils/toast';
import { paymentsApi } from '../api/paymentsApi';
import PaymentCardForm from '../components/payment/PaymentCardForm';

export default function DonationCardPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();
  const amount: number = location.state?.amount ?? 10;
  const [processing, setProcessing] = useState(false);

  const handlePay = async () => {
    setProcessing(true);
    try {
      const { data } = await paymentsApi.createDonationCheckout(amount);
      const parsed = new URL(data.sessionUrl);
      const allowedOrigins = ['https://checkout.stripe.com', window.location.origin];
      if (!allowedOrigins.includes(parsed.origin)) throw new Error('Invalid redirect');
      window.location.href = data.sessionUrl;
    } catch (err: unknown) {
      const message =
        (err as { response?: { data?: { error?: string } } })?.response?.data?.error
        || t('common.error');
      toast.error(message);
      setProcessing(false);
    }
  };

  const payLabel = t('donate.card_pay').replace('{amount}', amount.toFixed(2));

  return (
    <div className="max-w-lg mx-auto px-4 py-8">
      <button
        onClick={() => navigate('/donate')}
        className="flex items-center gap-2 text-sm text-gray-500 hover:text-primary mb-6 transition-colors"
      >
        <HiArrowLeft className="h-4 w-4" />
        {t('common.back')}
      </button>

      <h1 className="text-3xl font-bold text-primary mb-6">{t('donate.card_title')}</h1>

      {/* Amount summary */}
      <div className="bg-white rounded-xl p-5 border border-lavender/40 mb-6 flex items-center gap-4">
        <div className="w-10 h-10 rounded-full bg-primary/10 flex items-center justify-center flex-shrink-0">
          <Heart className="w-5 h-5 text-primary fill-primary/30" />
        </div>
        <div>
          <p className="text-xs text-gray-500 font-medium uppercase tracking-wide">
            {t('donate.card_amount')}
          </p>
          <p className="text-2xl font-bold text-primary">{amount.toFixed(2)} лв.</p>
        </div>
      </div>

      {/* Card form */}
      <div className="bg-white rounded-xl p-6 border border-lavender/40">
        <PaymentCardForm
          onSubmit={handlePay}
          isLoading={processing}
          submitLabel={payLabel}
        />
      </div>
    </div>
  );
}
