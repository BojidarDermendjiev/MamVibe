import { useState, useEffect } from 'react';
import { useParams, useNavigate, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { HiArrowLeft } from 'react-icons/hi';
import toast from 'react-hot-toast';
import { itemsApi } from '../api/itemsApi';
import { paymentsApi } from '../api/paymentsApi';
import { type Item, ListingType } from '../types/item';
import type { PaymentDeliveryRequest } from '../types/shipping';
import LoadingSpinner from '../components/common/LoadingSpinner';
import PaymentCardForm from '../components/payment/PaymentCardForm';

export default function PaymentItemCardPage() {
  const { itemId } = useParams<{ itemId: string }>();
  const { t } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();
  const delivery = location.state?.delivery as PaymentDeliveryRequest | undefined;
  const [item, setItem] = useState<Item | null>(null);
  const [loading, setLoading] = useState(true);
  const [processing, setProcessing] = useState(false);

  // Redirect back if no delivery info (direct URL access without going through PaymentPage)
  useEffect(() => {
    if (!delivery && itemId) navigate(`/payment/${itemId}`, { replace: true });
  }, [delivery, itemId, navigate]);

  useEffect(() => {
    if (!itemId) return;
    itemsApi.getById(itemId).then((res) => {
      setItem(res.data);
      setLoading(false);
    }).catch(() => {
      toast.error('Item not found');
      navigate('/browse');
    });
  }, [itemId, navigate]);

  const handlePay = async () => {
    if (!itemId || !delivery) return;
    setProcessing(true);
    try {
      const { data } = await paymentsApi.createCheckout(itemId, delivery);
      const parsed = new URL(data.sessionUrl);
      const allowedOrigins = ['https://checkout.stripe.com', window.location.origin];
      if (!allowedOrigins.includes(parsed.origin)) throw new Error('Invalid redirect');
      window.location.href = data.sessionUrl;
    } catch (err: unknown) {
      const message =
        (err as { response?: { data?: { error?: string; details?: string } } })?.response?.data?.error
        || (err as { response?: { data?: { error?: string; details?: string } } })?.response?.data?.details
        || t('common.error');
      toast.error(message);
      setProcessing(false);
    }
  };

  if (loading) return <LoadingSpinner size="lg" className="py-20" />;
  if (!item) return null;

  return (
    <div className="max-w-lg mx-auto px-4 py-8">
      <button
        onClick={() => navigate(`/payment/${itemId}`)}
        className="flex items-center gap-2 text-sm text-gray-500 hover:text-primary mb-6 transition-colors"
      >
        <HiArrowLeft className="h-4 w-4" />
        {t('common.back')}
      </button>

      <h1 className="text-3xl font-bold text-primary mb-6">{t('payment.card')}</h1>

      {/* Item summary */}
      <div className="bg-white rounded-xl p-6 border border-lavender/30 mb-6">
        <h2 className="font-semibold text-primary mb-1">{item.title}</h2>
        <p className="text-2xl font-bold text-mauve">
          {item.listingType === ListingType.Donate ? t('items.free') : `$${item.price?.toFixed(2)}`}
        </p>
      </div>

      {/* Card form */}
      <div className="bg-white rounded-xl p-6 border border-lavender/30">
        <PaymentCardForm onSubmit={handlePay} isLoading={processing} />
      </div>
    </div>
  );
}
