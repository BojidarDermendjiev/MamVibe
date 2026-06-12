import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import toast from '@/utils/toast';
import { itemsApi } from '../api/itemsApi';
import { paymentsApi } from '../api/paymentsApi';
import { type Item, ListingType } from '../types/item';
import { formatPrice } from '../utils/currency';
import { CourierProvider, DeliveryType } from '../types/shipping';
import type { CalculateShippingRequest, PaymentDeliveryRequest } from '../types/shipping';
import Button from '../components/common/Button';
import LoadingSpinner from '../components/common/LoadingSpinner';
import CourierSelector from '../components/shipping/CourierSelector';
import DeliveryTypeSelector from '../components/shipping/DeliveryTypeSelector';
import OfficePicker from '../components/shipping/OfficePicker';
import ShippingPricePreview from '../components/shipping/ShippingPricePreview';
import StripeCardForm from '../components/payment/StripeCardForm';

export default function PaymentPage() {
  const { itemId } = useParams<{ itemId: string }>();
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [item, setItem] = useState<Item | null>(null);
  const [loading, setLoading] = useState(true);
  const [processing, setProcessing] = useState(false);
  const [paymentMethod, setPaymentMethod] = useState<'card' | 'cod'>('card');
  const [shippingPrice, setShippingPrice] = useState(0);
  // Holds the Stripe PaymentIntent client secret once delivery info has been
  // submitted. While set, we render the embedded <StripeCardForm /> instead of
  // the delivery form. Cleared on "back" so the user can edit delivery details.
  const [clientSecret, setClientSecret] = useState<string | null>(null);

  // Delivery state
  const [courier, setCourier] = useState<CourierProvider>(CourierProvider.Econt);
  const [deliveryType, setDeliveryType] = useState<DeliveryType>(DeliveryType.Office);
  const [officeId, setOfficeId] = useState('');
  const [officeName, setOfficeName] = useState('');
  const [city, setCity] = useState('');
  const [address, setAddress] = useState('');
  const [recipientName, setRecipientName] = useState('');
  const [recipientPhone, setRecipientPhone] = useState('');

  const isDonate = item?.listingType === ListingType.Donate;

  // Reset delivery type to Office if donate item had Address selected (donate = pickup only)
  useEffect(() => {
    if (isDonate && deliveryType === DeliveryType.Address) {
      setDeliveryType(DeliveryType.Office);
    }
  }, [isDonate, deliveryType]);

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

  const isCod = !isDonate && paymentMethod === 'cod';

  const shippingRequest: CalculateShippingRequest | null =
    recipientName && recipientPhone
      ? {
          courierProvider: courier,
          deliveryType,
          toCity: deliveryType === DeliveryType.Address ? city : undefined,
          officeId: deliveryType !== DeliveryType.Address ? officeId : undefined,
          weight: 1,
          isCod,
          codAmount: isCod ? (item?.price ?? 0) : 0,
          isInsured: false,
          insuredAmount: 0,
        }
      : null;

  const handleSubmit = async () => {
    if (!itemId) return;

    // Validate delivery fields
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

    const delivery: PaymentDeliveryRequest = {
      courierProvider: courier,
      deliveryType,
      recipientName,
      recipientPhone,
      city: city || undefined,
      address: address || undefined,
      officeId: officeId || undefined,
      officeName: officeName || undefined,
      weight: 1,
    };

    if (isDonate) {
      // Free item: create booking + shipment directly
      setProcessing(true);
      try {
        await paymentsApi.createBooking(itemId, delivery);
        toast.success(t('payment.booking_success'));
        navigate('/payment/success', { state: { itemId } });
      } catch (err: unknown) {
        const message = (err as { response?: { data?: { error?: string; details?: string } } })?.response?.data?.error
          || (err as { response?: { data?: { error?: string; details?: string } } })?.response?.data?.details
          || t('common.error');
        toast.error(message);
      } finally {
        setProcessing(false);
      }
    } else if (paymentMethod === 'cod') {
      // Cash on delivery: create payment + shipment, courier collects at delivery
      setProcessing(true);
      try {
        await paymentsApi.createCod(itemId, delivery);
        toast.success(t('payment.cod_success'));
        navigate('/payment/success', { state: { itemId } });
      } catch (err: unknown) {
        const message = (err as { response?: { data?: { error?: string; details?: string } } })?.response?.data?.error
          || (err as { response?: { data?: { error?: string; details?: string } } })?.response?.data?.details
          || t('common.error');
        toast.error(message);
      } finally {
        setProcessing(false);
      }
    } else {
      // Paid item, card method: create a PaymentIntent and switch the UI to the
      // embedded <StripeCardForm />. Card data is collected by Stripe inside its
      // own iframe (Stripe Elements) — our app never sees PAN/CVV/expiry, so PCI
      // scope stays at SAQ A. The user stays on this page; on success Stripe.js
      // resolves and we navigate to /payment/success ourselves.
      setProcessing(true);
      try {
        const { data } = await paymentsApi.createPaymentIntent(itemId, delivery);
        if (!data.clientSecret) throw new Error('No client secret returned');
        setClientSecret(data.clientSecret);
      } catch (err: unknown) {
        const message = (err as { response?: { data?: { error?: string; details?: string } } })?.response?.data?.error
          || (err as { response?: { data?: { error?: string; details?: string } } })?.response?.data?.details
          || t('common.error');
        toast.error(message);
      } finally {
        setProcessing(false);
      }
    }
  };

  if (loading) return <LoadingSpinner size="lg" className="py-20" />;
  if (!item) return null;

  if (clientSecret) {
    return (
      <div className="max-w-lg mx-auto px-4 py-8">
        <h1 className="text-3xl font-bold text-primary dark:text-white mb-2">{t('payment.card_details')}</h1>
        <p className="text-sm text-gray-500 dark:text-gray-400 mb-6">
          {t('payment.total')}: <span className="font-semibold text-mauve">{formatPrice((item.price ?? 0) + shippingPrice)}</span>
        </p>
        <div className="bg-white dark:bg-gray-800 rounded-xl p-6 border border-lavender/30 dark:border-gray-700">
          <StripeCardForm
            clientSecret={clientSecret}
            onSuccess={() => navigate('/payment/success', { state: { itemId } })}
            onCancel={() => setClientSecret(null)}
            submitLabel={t('card.pay_now')}
          />
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-lg mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold text-primary mb-6">{t('payment.title')}</h1>

      {/* Item summary */}
      <div className="bg-white rounded-xl p-6 border border-lavender/30 mb-6">
        <h2 className="font-semibold text-primary mb-1">{item.title}</h2>
        {isDonate ? (
          <p className="text-2xl font-bold text-mauve">{t('items.free')}</p>
        ) : (
          <div className="space-y-1 mt-1">
            <div className="flex justify-between text-sm text-gray-500">
              <span>{t('items.price')}</span>
              <span>{formatPrice(item.price)}</span>
            </div>
            {shippingPrice > 0 && (
              <div className="flex justify-between text-sm text-gray-500">
                <span>{t('payment.shipping_fee')}</span>
                <span>{formatPrice(shippingPrice)}</span>
              </div>
            )}
            <div className="flex justify-between text-lg font-bold text-mauve border-t border-lavender/30 pt-1 mt-1">
              <span>{t('payment.order_total')}</span>
              <span>{formatPrice((item.price ?? 0) + shippingPrice)}</span>
            </div>
          </div>
        )}
      </div>

      {/* Step 1: Delivery Options */}
      <div className="bg-white rounded-xl p-6 border border-lavender/30 mb-6 space-y-5">
        <h3 className="text-lg font-semibold text-primary">{t('payment.delivery')}</h3>

        <CourierSelector value={courier} onChange={setCourier} />
        <DeliveryTypeSelector
          value={deliveryType}
          onChange={setDeliveryType}
          exclude={isDonate ? [DeliveryType.Address] : []}
        />

        {deliveryType !== DeliveryType.Address ? (
          <OfficePicker
            provider={courier}
            city={city || undefined}
            value={officeId}
            onChange={(id, name) => { setOfficeId(id); setOfficeName(name); }}
          />
        ) : (
          <div className="space-y-3">
            <div>
              <label className="block text-sm font-medium text-primary mb-1">{t('shipping.city')}</label>
              <input
                type="text"
                value={city}
                onChange={(e) => setCity(e.target.value)}
                className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary focus:border-primary"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-primary mb-1">{t('shipping.address')}</label>
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
            <label className="block text-sm font-medium text-primary mb-1">{t('payment.recipient_name')}</label>
            <input
              type="text"
              value={recipientName}
              onChange={(e) => setRecipientName(e.target.value)}
              className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary focus:border-primary"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-primary mb-1">{t('payment.recipient_phone')}</label>
            <input
              type="text"
              value={recipientPhone}
              onChange={(e) => setRecipientPhone(e.target.value)}
              className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary focus:border-primary"
            />
          </div>
        </div>

        <ShippingPricePreview request={shippingRequest} onPriceChange={setShippingPrice} />
      </div>

      {/* Step 2: Payment Method (sell items only) */}
      {!isDonate && (
        <div className="bg-white rounded-xl p-6 border border-lavender/30 mb-6 space-y-3">
          <h3 className="text-lg font-semibold text-primary">{t('payment.method')}</h3>
          <div className="grid grid-cols-2 gap-3">
            <button
              type="button"
              onClick={() => setPaymentMethod('card')}
              className={`flex flex-col items-center gap-2 p-4 rounded-xl border-2 transition-all ${
                paymentMethod === 'card'
                  ? 'border-primary bg-primary/5'
                  : 'border-lavender/40 hover:border-primary/40'
              }`}
            >
              <span className="text-2xl">💳</span>
              <span className="text-sm font-semibold text-primary">{t('payment.card')}</span>
              <span className="text-xs text-gray-500 text-center">{t('payment.card_desc')}</span>
            </button>
            <button
              type="button"
              onClick={() => setPaymentMethod('cod')}
              className={`flex flex-col items-center gap-2 p-4 rounded-xl border-2 transition-all ${
                paymentMethod === 'cod'
                  ? 'border-primary bg-primary/5'
                  : 'border-lavender/40 hover:border-primary/40'
              }`}
            >
              <span className="text-2xl">💵</span>
              <span className="text-sm font-semibold text-primary">{t('payment.cod')}</span>
              <span className="text-xs text-gray-500 text-center">{t('payment.cod_desc')}</span>
            </button>
          </div>
        </div>
      )}

      <Button fullWidth size="lg" isLoading={processing} onClick={handleSubmit}>
        {isDonate
          ? t('payment.confirm_booking')
          : paymentMethod === 'cod'
          ? t('payment.cod_confirm')
          : t('payment.continue_to_card')}
      </Button>
    </div>
  );
}
