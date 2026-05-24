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
    } else {
      // Paid item: go to card page (delivery details passed in router state)
      navigate(`/payment/${itemId}/card`, { state: { delivery } });
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
          {isDonate ? t('items.free') : formatPrice(item.price)}
        </p>
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

        <ShippingPricePreview request={shippingRequest} />
      </div>

      <Button fullWidth size="lg" isLoading={processing} onClick={handleSubmit}>
        {isDonate ? t('payment.confirm_booking') : t('payment.continue_to_card')}
      </Button>
    </div>
  );
}
