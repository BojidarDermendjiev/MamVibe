import { useState, useEffect, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { adminApi } from '../../api/adminApi';
import { shippingApi } from '../../api/shippingApi';
import {
  CourierProvider,
  DeliveryType,
  type Shipment,
  type TrackingEvent,
} from '../../types/shipping';
import type { Payment as PaymentType } from '../../types/payment';
import CourierSelector from '../../components/shipping/CourierSelector';
import DeliveryTypeSelector from '../../components/shipping/DeliveryTypeSelector';
import OfficePicker from '../../components/shipping/OfficePicker';
import ShippingPricePreview from '../../components/shipping/ShippingPricePreview';
import ShipmentCard from '../../components/shipping/ShipmentCard';
import Button from '../../components/common/Button';
import LoadingSpinner from '../../components/common/LoadingSpinner';

export default function AdminShippingPage() {
  const { t } = useTranslation();
  const [payments, setPayments] = useState<PaymentType[]>([]);
  const [shipments, setShipments] = useState<Shipment[]>([]);
  const [loading, setLoading] = useState(true);
  const [creating, setCreating] = useState(false);
  const [trackingData, setTrackingData] = useState<Record<string, TrackingEvent[]>>({});
  const [trackingLoading, setTrackingLoading] = useState<Record<string, boolean>>({});

  // Form state
  const [selectedPaymentId, setSelectedPaymentId] = useState('');
  const [courier, setCourier] = useState<CourierProvider>(CourierProvider.Econt);
  const [deliveryType, setDeliveryType] = useState<DeliveryType>(DeliveryType.Office);
  const [recipientName, setRecipientName] = useState('');
  const [recipientPhone, setRecipientPhone] = useState('');
  const [address, setAddress] = useState('');
  const [city, setCity] = useState('');
  const [officeId, setOfficeId] = useState('');
  const [officeName, setOfficeName] = useState('');
  const [weight, setWeight] = useState(1);
  const [isCod, setIsCod] = useState(false);
  const [codAmount, setCodAmount] = useState(0);
  const [isInsured, setIsInsured] = useState(false);
  const [insuredAmount, setInsuredAmount] = useState(0);

  useEffect(() => {
    Promise.all([
      adminApi.getAllPayments(),
      adminApi.getAllShipments(),
    ])
      .then(([paymentsRes, shipmentsRes]) => {
        setPayments(paymentsRes.data);
        setShipments(shipmentsRes.data);
      })
      .catch(() => toast.error(t('common.error')))
      .finally(() => setLoading(false));
  }, []);

  const priceRequest = useMemo(() => {
    if (!weight) return null;
    return {
      courierProvider: courier,
      deliveryType,
      toCity: city || undefined,
      officeId: officeId || undefined,
      weight,
      isCod,
      codAmount,
      isInsured,
      insuredAmount,
    };
  }, [courier, deliveryType, city, officeId, weight, isCod, codAmount, isInsured, insuredAmount]);

  const handleCreate = async () => {
    if (!selectedPaymentId) {
      toast.error(t('shipping.select_payment'));
      return;
    }
    setCreating(true);
    try {
      const { data } = await shippingApi.createShipment({
        paymentId: selectedPaymentId,
        courierProvider: courier,
        deliveryType,
        recipientName,
        recipientPhone,
        deliveryAddress: address || undefined,
        city: city || undefined,
        officeId: officeId || undefined,
        officeName: officeName || undefined,
        weight,
        isCod,
        codAmount,
        isInsured,
        insuredAmount,
      });
      setShipments((prev) => [data, ...prev]);
      toast.success(t('shipping.created'));
      setSelectedPaymentId('');
      setRecipientName('');
      setRecipientPhone('');
      setAddress('');
      setCity('');
      setOfficeId('');
      setOfficeName('');
    } catch {
      toast.error(t('common.error'));
    } finally {
      setCreating(false);
    }
  };

  const handleTrack = async (shipmentId: string) => {
    if (trackingData[shipmentId]) {
      // Toggle off
      setTrackingData((prev) => {
        const next = { ...prev };
        delete next[shipmentId];
        return next;
      });
      return;
    }
    setTrackingLoading((prev) => ({ ...prev, [shipmentId]: true }));
    try {
      const { data } = await adminApi.trackShipment(shipmentId);
      setTrackingData((prev) => ({ ...prev, [shipmentId]: data }));
    } catch {
      toast.error(t('common.error'));
    } finally {
      setTrackingLoading((prev) => ({ ...prev, [shipmentId]: false }));
    }
  };

  if (loading) return <LoadingSpinner size="lg" className="py-20" />;

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold text-primary mb-6">{t('shipping.admin_title')}</h1>

      {/* Create shipment form */}
      <div className="bg-white rounded-xl p-6 border border-lavender/30 mb-8 space-y-6">
        <h2 className="text-xl font-semibold text-primary">{t('shipping.create_shipment')}</h2>

        {/* Payment selector */}
        <div>
          <label className="block text-sm font-medium text-primary mb-1">{t('shipping.select_payment')}</label>
          <select
            value={selectedPaymentId}
            onChange={(e) => setSelectedPaymentId(e.target.value)}
            className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary focus:border-primary"
          >
            <option value="">{t('shipping.choose_payment')}</option>
            {payments.map((p) => (
              <option key={p.id} value={p.id}>
                {p.itemTitle} — {p.amount.toFixed(2)} BGN
              </option>
            ))}
          </select>
        </div>

        <CourierSelector value={courier} onChange={setCourier} />
        <DeliveryTypeSelector value={deliveryType} onChange={setDeliveryType} />

        {/* Recipient info */}
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-primary mb-1">{t('shipping.recipient_name')}</label>
            <input
              type="text"
              value={recipientName}
              onChange={(e) => setRecipientName(e.target.value)}
              className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary focus:border-primary"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-primary mb-1">{t('shipping.recipient_phone')}</label>
            <input
              type="text"
              value={recipientPhone}
              onChange={(e) => setRecipientPhone(e.target.value)}
              className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary focus:border-primary"
            />
          </div>
        </div>

        {/* Address fields */}
        {deliveryType === DeliveryType.Address && (
          <div className="grid grid-cols-2 gap-4">
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

        {/* Office picker */}
        {(deliveryType === DeliveryType.Office || deliveryType === DeliveryType.Locker) && (
          <OfficePicker
            provider={courier}
            city={city || undefined}
            value={officeId}
            onChange={(id, name) => {
              setOfficeId(id);
              setOfficeName(name);
            }}
          />
        )}

        {/* Weight */}
        <div>
          <label className="block text-sm font-medium text-primary mb-1">{t('shipping.weight')} (kg)</label>
          <input
            type="number"
            step="0.1"
            min="0.1"
            value={weight}
            onChange={(e) => setWeight(Number(e.target.value))}
            className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary focus:border-primary"
          />
        </div>

        {/* COD */}
        <div className="flex items-center gap-4">
          <label className="flex items-center gap-2">
            <input type="checkbox" checked={isCod} onChange={(e) => setIsCod(e.target.checked)} />
            <span className="text-sm font-medium text-primary">{t('shipping.cod')}</span>
          </label>
          {isCod && (
            <input
              type="number"
              step="0.01"
              min="0"
              value={codAmount}
              onChange={(e) => setCodAmount(Number(e.target.value))}
              placeholder={t('shipping.cod_amount')}
              className="px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary focus:border-primary"
            />
          )}
        </div>

        {/* Insurance */}
        <div className="flex items-center gap-4">
          <label className="flex items-center gap-2">
            <input type="checkbox" checked={isInsured} onChange={(e) => setIsInsured(e.target.checked)} />
            <span className="text-sm font-medium text-primary">{t('shipping.insurance')}</span>
          </label>
          {isInsured && (
            <input
              type="number"
              step="0.01"
              min="0"
              value={insuredAmount}
              onChange={(e) => setInsuredAmount(Number(e.target.value))}
              placeholder={t('shipping.insured_amount')}
              className="px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary focus:border-primary"
            />
          )}
        </div>

        <ShippingPricePreview request={priceRequest} />

        <Button fullWidth size="lg" isLoading={creating} onClick={handleCreate}>
          {t('shipping.create_shipment')}
        </Button>
      </div>

      {/* Existing shipments */}
      <h2 className="text-xl font-semibold text-primary mb-4">{t('admin.all_shipments')}</h2>
      {shipments.length === 0 ? (
        <p className="text-gray-500">{t('shipping.no_shipments')}</p>
      ) : (
        <div className="space-y-3">
          {shipments.map((shipment) => (
            <div key={shipment.id}>
              <div className="flex items-center gap-2">
                <div className="flex-1">
                  <ShipmentCard shipment={shipment} />
                </div>
                <Button
                  size="sm"
                  variant="secondary"
                  isLoading={trackingLoading[shipment.id]}
                  onClick={() => handleTrack(shipment.id)}
                >
                  {t('admin.track_shipment')}
                </Button>
              </div>
              {trackingData[shipment.id] && (
                <div className="ml-4 mt-2 mb-4 bg-cream rounded-lg p-4 border border-lavender/20">
                  <h4 className="text-sm font-medium text-primary mb-2">{t('shipping.tracking')}</h4>
                  {trackingData[shipment.id].length === 0 ? (
                    <p className="text-sm text-gray-500">{t('shipping.no_tracking')}</p>
                  ) : (
                    <div className="space-y-2">
                      {trackingData[shipment.id].map((event, idx) => (
                        <div key={idx} className="flex gap-3 text-sm">
                          <span className="text-gray-400 whitespace-nowrap">
                            {new Date(event.timestamp).toLocaleString()}
                          </span>
                          <span className="text-primary">{event.description}</span>
                          {event.location && (
                            <span className="text-gray-400">({event.location})</span>
                          )}
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
