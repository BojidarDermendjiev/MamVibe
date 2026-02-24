import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { HiCreditCard, HiLocationMarker } from 'react-icons/hi';
import toast from 'react-hot-toast';
import { itemsApi } from '../api/itemsApi';
import { paymentsApi } from '../api/paymentsApi';
import { type Item, ListingType } from '../types/item';
import { CourierProvider, DeliveryType } from '../types/shipping';
import type { CalculateShippingRequest, PaymentDeliveryRequest } from '../types/shipping';
import Button from '../components/common/Button';
import LoadingSpinner from '../components/common/LoadingSpinner';
import CourierSelector from '../components/shipping/CourierSelector';
import DeliveryTypeSelector from '../components/shipping/DeliveryTypeSelector';
import OfficePicker from '../components/shipping/OfficePicker';
import ShippingPricePreview from '../components/shipping/ShippingPricePreview';

export default function PaymentPage() {
  const { itemId } = useParams<{ itemId: string }>();
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [item, setItem] = useState<Item | null>(null);
  const [loading, setLoading] = useState(true);
  const [processing, setProcessing] = useState(false);

  // Delivery state
  const [courier, setCourier] = useState<CourierProvider>(CourierProvider.Econt);
  const [deliveryType, setDeliveryType] = useState<DeliveryType>(DeliveryType.Office);
  const [officeId, setOfficeId] = useState('');
  const [officeName, setOfficeName] = useState('');
  const [city, setCity] = useState('');
  const [address, setAddress] = useState('');
  const [recipientName, setRecipientName] = useState('');
  const [recipientPhone, setRecipientPhone] = useState('');

  // Payment method (sell items only)
  const [method, setMethod] = useState<'card' | 'onspot'>('card');

  const isDonate = item?.listingType === ListingType.Donate;

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

    // Card payment → go to dedicated card page, passing delivery in state
    if (!isDonate && method === 'card') {
      navigate(`/payment/${itemId}/card`, { state: { delivery } });
      return;
    }

    setProcessing(true);
    try {
      if (isDonate) {
        await paymentsApi.createBooking(itemId, delivery);
        toast.success(t('payment.booking_success'));
        navigate('/payment/success');
      } else {
        await paymentsApi.createOnSpot(itemId, delivery);
        toast.success('On-spot payment registered!');
        navigate('/payment/success');
      }
    } catch (err: unknown) {
      const message = (err as { response?: { data?: { error?: string; details?: string } } })?.response?.data?.error
        || (err as { response?: { data?: { error?: string; details?: string } } })?.response?.data?.details
        || t('common.error');
      toast.error(message);
    } finally {
      setProcessing(false);
    }
  };

  if (loading) return <LoadingSpinner size="lg" className="py-20" />;
  if (!item) return null;

  return (
    <div className="max-w-lg mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold text-primary mb-6">{t('payment.title')}</h1>

      {/* Item summary */}
      <div className="bg-white rounded-xl p-6 border border-lavender/30 mb-6">
        <h2 className="font-semibold text-primary mb-1">{item.title}</h2>
        <p className="text-2xl font-bold text-mauve">
          {isDonate ? t('items.free') : `$${item.price?.toFixed(2)}`}
        </p>
      </div>

      {/* Step 1: Delivery Options */}
      <div className="bg-white rounded-xl p-6 border border-lavender/30 mb-6 space-y-5">
        <h3 className="text-lg font-semibold text-primary">{t('payment.delivery')}</h3>

        <CourierSelector value={courier} onChange={setCourier} />
        <DeliveryTypeSelector value={deliveryType} onChange={setDeliveryType} />

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

        <ShippingPricePreview request={shippingRequest} />
      </div>

      {/* Step 2: Payment method (sell items only) */}
      {!isDonate && (
        <div className="space-y-3 mb-6">
          <h3 className="font-medium text-primary">{t('payment.method')}</h3>
          <div className="flex gap-3">
            <button
              onClick={() => setMethod('card')}
              className={`flex-1 flex items-center gap-3 p-4 rounded-xl border-2 transition-all ${
                method === 'card' ? 'border-primary bg-primary/5' : 'border-gray-200 hover:border-lavender'
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
                method === 'onspot' ? 'border-primary bg-primary/5' : 'border-gray-200 hover:border-lavender'
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

      <Button fullWidth size="lg" isLoading={processing} onClick={handleSubmit}>
        {isDonate
          ? t('payment.confirm_booking')
          : method === 'card'
          ? t('payment.continue_to_card')
          : t('payment.proceed')}
      </Button>
    </div>
  );
}
