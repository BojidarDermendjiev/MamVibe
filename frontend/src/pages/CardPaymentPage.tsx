import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { HiArrowLeft, HiShoppingCart } from 'react-icons/hi';
import toast from 'react-hot-toast';
import { useCartStore } from '../store/cartStore';
import { paymentsApi } from '../api/paymentsApi';
import { ListingType } from '../types/item';
import PaymentCardForm from '../components/payment/PaymentCardForm';

export default function CardPaymentPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { items, clearCart } = useCartStore();
  const [processing, setProcessing] = useState(false);

  const saleItems = items.filter((i) => i.listingType === ListingType.Sell);
  const donateItems = items.filter((i) => i.listingType === ListingType.Donate);
  const total = saleItems.reduce((sum, i) => sum + i.price, 0);

  const handlePay = async () => {
    setProcessing(true);
    try {
      // Book all donation items
      if (donateItems.length > 0) {
        await paymentsApi.bulkBooking(donateItems.map((i) => String(i.id)));
      }

      // Process card payment for sale items
      const { data } = await paymentsApi.bulkCheckout(saleItems.map((i) => String(i.id)));
      clearCart();
      window.location.href = data.sessionUrl;
    } catch (err: unknown) {
      const message =
        (err as { response?: { data?: { error?: string; details?: string } } })?.response?.data
          ?.error ||
        (err as { response?: { data?: { error?: string; details?: string } } })?.response?.data
          ?.details ||
        t('common.error');
      toast.error(message);
    } finally {
      setProcessing(false);
    }
  };

  if (items.length === 0 || saleItems.length === 0) {
    return (
      <div className="max-w-lg mx-auto px-4 py-16 text-center">
        <HiShoppingCart className="h-16 w-16 text-gray-300 mx-auto mb-4" />
        <p className="text-gray-500 text-lg mb-4">{t('cart.empty')}</p>
        <Link
          to="/browse"
          className="inline-block px-6 py-3 bg-primary text-white rounded-lg hover:bg-primary-dark transition-colors"
        >
          {t('nav.browse')}
        </Link>
      </div>
    );
  }

  return (
    <div className="max-w-lg mx-auto px-4 py-8">
      <button
        onClick={() => navigate('/checkout')}
        className="flex items-center gap-1 text-sm text-gray-500 hover:text-primary mb-6 transition-colors"
      >
        <HiArrowLeft className="h-4 w-4" />
        {t('common.back')}
      </button>

      <h1 className="text-3xl font-bold text-primary mb-6">{t('payment.card')}</h1>

      {/* Order total */}
      <div className="bg-white rounded-xl p-4 border border-lavender/30 mb-6">
        <div className="flex justify-between items-center">
          <span className="text-gray-600">
            {t('cart.total')} ({saleItems.length} {t('shipping.item').toLowerCase()}{saleItems.length > 1 ? 's' : ''})
          </span>
          <span className="text-xl font-bold text-primary">{total.toFixed(2)} лв.</span>
        </div>
      </div>

      {/* Card form */}
      <PaymentCardForm onSubmit={handlePay} isLoading={processing} />
    </div>
  );
}
