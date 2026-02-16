import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { HiCreditCard, HiLocationMarker, HiShoppingCart } from 'react-icons/hi';
import toast from 'react-hot-toast';
import { useCartStore } from '../store/cartStore';
import { paymentsApi } from '../api/paymentsApi';
import { ListingType } from '../types/item';
import { CourierProvider, DeliveryType } from '../types/shipping';
import type { CalculateShippingRequest } from '../types/shipping';
import Button from '../components/common/Button';
import CourierSelector from '../components/shipping/CourierSelector';
import DeliveryTypeSelector from '../components/shipping/DeliveryTypeSelector';
import OfficePicker from '../components/shipping/OfficePicker';
import ShippingPricePreview from '../components/shipping/ShippingPricePreview';

export default function CheckoutPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { items, clearCart } = useCartStore();
  const [processing, setProcessing] = useState(false);

  // Delivery state
  const [courier, setCourier] = useState<CourierProvider>(CourierProvider.Econt);
  const [deliveryType, setDeliveryType] = useState<DeliveryType>(DeliveryType.Office);
  const [officeId, setOfficeId] = useState('');
  const [, setOfficeName] = useState('');
  const [city, setCity] = useState('');
  const [address, setAddress] = useState('');
  const [recipientName, setRecipientName] = useState('');
  const [recipientPhone, setRecipientPhone] = useState('');

  // Payment method
  const [method, setMethod] = useState<'card' | 'onspot'>('card');

  const donateItems = items.filter((i) => i.listingType === ListingType.Donate);
  const saleItems = items.filter((i) => i.listingType === ListingType.Sell);
  const hasSaleItems = saleItems.length > 0;
  const total = saleItems.reduce((sum, i) => sum + i.price, 0);

  const shippingRequest: CalculateShippingRequest | null =
    recipientName && recipientPhone
      ? {
          courierProvider: courier,
          deliveryType,
          toCity: deliveryType === DeliveryType.Address ? city : undefined,
          officeId: deliveryType !== DeliveryType.Address ? officeId : undefined,
          weight: 1,
          isCod: false,
          codAmount: 0,
          isInsured: false,
          insuredAmount: 0,
        }
      : null;

  const handleSubmit = async () => {
    if (items.length === 0) return;

    if (!recipientName.trim() || !recipientPhone.trim()) {
      toast.error(t('common.error'));
      return;
    }
    if (deliveryType === DeliveryType.Address && (!city.trim() || !address.trim())) {
      toast.error(t('common.error'));
      return;
    }
    if (deliveryType !== DeliveryType.Address && !officeId) {
      toast.error(t('common.error'));
      return;
    }

    // If card payment selected, navigate to card input page
    if (hasSaleItems && method === 'card') {
      navigate('/checkout/card');
      return;
    }

    setProcessing(true);
    try {
      // Book all donation items
      if (donateItems.length > 0) {
        await paymentsApi.bulkBooking(donateItems.map((i) => String(i.id)));
      }

      // Handle sale items (on-spot)
      if (hasSaleItems) {
        await paymentsApi.bulkOnSpot(saleItems.map((i) => String(i.id)));
      }

      clearCart();
      toast.success(t('checkout.success'));
      navigate('/payment/success');
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

  if (items.length === 0) {
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
      <h1 className="text-3xl font-bold text-primary mb-6">{t('checkout.title')}</h1>

      {/* Cart summary */}
      <div className="bg-white rounded-xl p-6 border border-lavender/30 mb-6">
        <h3 className="font-semibold text-primary mb-3">{t('checkout.order_summary')}</h3>
        <div className="space-y-3">
          {items.map((item) => (
            <div key={item.id} className="flex items-center gap-3">
              {item.imageUrl ? (
                <img
                  src={item.imageUrl}
                  alt={item.title}
                  className="h-12 w-12 rounded-lg object-cover"
                />
              ) : (
                <div className="h-12 w-12 rounded-lg bg-cream-dark flex items-center justify-center text-gray-400">
                  <HiShoppingCart className="h-5 w-5" />
                </div>
              )}
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-text truncate">{item.title}</p>
                <p className="text-xs text-gray-500">{item.categoryName}</p>
              </div>
              <p className="text-sm font-bold text-primary">
                {item.listingType === ListingType.Donate
                  ? t('items.free')
                  : `${item.price.toFixed(2)} лв.`}
              </p>
            </div>
          ))}
        </div>
        {hasSaleItems && (
          <div className="border-t border-gray-100 mt-3 pt-3 flex justify-between">
            <span className="font-medium text-gray-700">{t('cart.total')}</span>
            <span className="text-lg font-bold text-primary">{total.toFixed(2)} лв.</span>
          </div>
        )}
      </div>

      {/* Step 1: Delivery Options */}
      <div className="bg-white rounded-xl p-6 border border-lavender/30 mb-6 space-y-5">
        <h3 className="text-lg font-semibold text-primary">{t('payment.delivery')}</h3>

        <CourierSelector value={courier} onChange={(v) => { setCourier(v); setOfficeId(''); setOfficeName(''); }} />
        <DeliveryTypeSelector value={deliveryType} onChange={(v) => { setDeliveryType(v); setOfficeId(''); setOfficeName(''); }} />

        {deliveryType !== DeliveryType.Address ? (
          <OfficePicker
            provider={courier}
            city={city || undefined}
            value={officeId}
            onChange={(id, name) => {
              setOfficeId(id);
              setOfficeName(name);
            }}
            lockersOnly={deliveryType === DeliveryType.Locker}
          />
        ) : (
          <div className="space-y-3">
            <div>
              <label className="block text-sm font-medium text-primary mb-1">
                {t('shipping.city')}
              </label>
              <input
                type="text"
                value={city}
                onChange={(e) => setCity(e.target.value)}
                className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary focus:border-primary"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-primary mb-1">
                {t('shipping.address')}
              </label>
              <input
                type="text"
                value={address}
                onChange={(e) => setAddress(e.target.value)}
                className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary focus:border-primary"
              />
            </div>
          </div>
        )}

        <div className="space-y-3">
          <div>
            <label className="block text-sm font-medium text-primary mb-1">
              {t('payment.recipient_name')}
            </label>
            <input
              type="text"
              value={recipientName}
              onChange={(e) => setRecipientName(e.target.value)}
              className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary focus:border-primary"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-primary mb-1">
              {t('payment.recipient_phone')}
            </label>
            <input
              type="text"
              value={recipientPhone}
              onChange={(e) => setRecipientPhone(e.target.value)}
              className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary focus:border-primary"
            />
          </div>
        </div>

        <ShippingPricePreview request={shippingRequest} />
      </div>

      {/* Step 2: Payment method (only if sale items) */}
      {hasSaleItems && (
        <div className="space-y-3 mb-6">
          <h3 className="font-medium text-primary">{t('payment.method')}</h3>
          <div className="flex gap-3">
            <button
              onClick={() => setMethod('card')}
              className={`flex-1 flex items-center gap-3 p-4 rounded-xl border-2 transition-all ${
                method === 'card'
                  ? 'border-primary bg-primary/5'
                  : 'border-gray-200 hover:border-lavender'
              }`}
            >
              <div className="w-10 h-10 rounded-lg bg-lavender/20 flex items-center justify-center">
                <HiCreditCard className="h-5 w-5 text-primary" />
              </div>
              <div className="text-left">
                <p className="font-medium text-primary">{t('payment.card')}</p>
                <p className="text-sm text-gray-500">{t('payment.card_desc')}</p>
              </div>
            </button>
            <button
              onClick={() => setMethod('onspot')}
              className={`flex-1 flex items-center gap-3 p-4 rounded-xl border-2 transition-all ${
                method === 'onspot'
                  ? 'border-primary bg-primary/5'
                  : 'border-gray-200 hover:border-lavender'
              }`}
            >
              <div className="w-10 h-10 rounded-lg bg-peach/30 flex items-center justify-center">
                <HiLocationMarker className="h-5 w-5 text-mauve" />
              </div>
              <div className="text-left">
                <p className="font-medium text-primary">{t('payment.on_spot')}</p>
                <p className="text-sm text-gray-500">{t('payment.on_spot_desc')}</p>
              </div>
            </button>
          </div>
        </div>
      )}

      {/* Submit */}
      <Button fullWidth size="lg" isLoading={processing} onClick={handleSubmit}>
        {hasSaleItems && method === 'card'
          ? t('payment.proceed')
          : !hasSaleItems
            ? t('payment.confirm_booking')
            : t('checkout.place_order')}
      </Button>
    </div>
  );
}
