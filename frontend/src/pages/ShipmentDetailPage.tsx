import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { shippingApi } from '../api/shippingApi';
import { CourierProvider, ShipmentStatus, type Shipment } from '../types/shipping';
import ShipmentTracker from '../components/shipping/ShipmentTracker';
import Button from '../components/common/Button';
import LoadingSpinner from '../components/common/LoadingSpinner';
import { formatPrice } from '../utils/currency';

export default function ShipmentDetailPage() {
  const { shipmentId } = useParams<{ shipmentId: string }>();
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [shipment, setShipment] = useState<Shipment | null>(null);
  const [loading, setLoading] = useState(true);
  const [cancelling, setCancelling] = useState(false);

  useEffect(() => {
    if (!shipmentId) return;
    shippingApi
      .getMyShipments()
      .then((res) => {
        const found = res.data.find((s) => s.id === shipmentId);
        setShipment(found ?? null);
      })
      .catch(() => toast.error(t('common.error')))
      .finally(() => setLoading(false));
  }, [shipmentId, t]);

  const handleDownloadLabel = async () => {
    if (!shipmentId) return;
    try {
      const response = await shippingApi.getLabel(shipmentId);
      const blob = new Blob([response.data], { type: 'application/pdf' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `label-${shipment?.trackingNumber ?? shipmentId}.pdf`;
      a.click();
      URL.revokeObjectURL(url);
    } catch {
      toast.error(t('common.error'));
    }
  };

  const handleCancel = async () => {
    if (!shipmentId) return;
    setCancelling(true);
    try {
      await shippingApi.cancelShipment(shipmentId);
      setShipment((prev) => (prev ? { ...prev, status: ShipmentStatus.Cancelled } : null));
      toast.success(t('shipping.cancelled'));
    } catch {
      toast.error(t('common.error'));
    } finally {
      setCancelling(false);
    }
  };

  if (loading) return <LoadingSpinner size="lg" className="py-20" />;
  if (!shipment) {
    return (
      <div className="max-w-lg mx-auto px-4 py-20 text-center">
        <p className="text-gray-500">{t('shipping.not_found')}</p>
        <Button variant="ghost" className="mt-4" onClick={() => navigate(-1)}>
          {t('common.back')}
        </Button>
      </div>
    );
  }

  const courierNames: Record<number, string> = {
    [CourierProvider.Econt]: 'Econt',
    [CourierProvider.Speedy]: 'Speedy',
    [CourierProvider.BoxNow]: 'Box Now',
  };
  const courierName = courierNames[shipment.courierProvider] ?? 'Unknown';
  const canCancel =
    shipment.status === ShipmentStatus.Pending || shipment.status === ShipmentStatus.Created;

  return (
    <div className="max-w-2xl mx-auto px-4 py-8">
      <button onClick={() => navigate(-1)} className="text-primary hover:underline mb-4 inline-block">
        &larr; {t('common.back')}
      </button>

      <h1 className="text-3xl font-bold text-primary mb-6">{t('shipping.shipment_details')}</h1>

      <div className="bg-white rounded-xl p-6 border border-lavender/30 space-y-4 mb-6">
        {shipment.itemTitle && (
          <div>
            <span className="text-sm text-gray-500">{t('shipping.item')}</span>
            <p className="font-semibold text-primary">{shipment.itemTitle}</p>
          </div>
        )}

        <div className="grid grid-cols-2 gap-4">
          <div>
            <span className="text-sm text-gray-500">{t('shipping.courier')}</span>
            <p className="font-medium text-primary">{courierName}</p>
          </div>
          <div>
            <span className="text-sm text-gray-500">{t('shipping.tracking_number')}</span>
            <p className="font-medium text-primary">{shipment.trackingNumber ?? '-'}</p>
          </div>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <span className="text-sm text-gray-500">{t('shipping.recipient_name')}</span>
            <p className="font-medium text-primary">{shipment.recipientName}</p>
          </div>
          <div>
            <span className="text-sm text-gray-500">{t('shipping.recipient_phone')}</span>
            <p className="font-medium text-primary">{shipment.recipientPhone}</p>
          </div>
        </div>

        {shipment.deliveryAddress && (
          <div>
            <span className="text-sm text-gray-500">{t('shipping.address')}</span>
            <p className="font-medium text-primary">
              {shipment.deliveryAddress}, {shipment.city}
            </p>
          </div>
        )}

        {shipment.officeName && (
          <div>
            <span className="text-sm text-gray-500">{t('shipping.office')}</span>
            <p className="font-medium text-primary">{shipment.officeName}</p>
          </div>
        )}

        <div className="grid grid-cols-3 gap-4">
          <div>
            <span className="text-sm text-gray-500">{t('shipping.shipping_price')}</span>
            <p className="font-bold text-mauve">{formatPrice(shipment.shippingPrice)}</p>
          </div>
          <div>
            <span className="text-sm text-gray-500">{t('shipping.weight')}</span>
            <p className="font-medium text-primary">{shipment.weight} kg</p>
          </div>
          <div>
            <span className="text-sm text-gray-500">{t('shipping.status_label')}</span>
            <p className="font-medium text-primary">{t(`shipping.status_${shipment.status}`)}</p>
          </div>
        </div>

        {shipment.isCod && (
          <div>
            <span className="text-sm text-gray-500">{t('shipping.cod')}</span>
            <p className="font-medium text-primary">{formatPrice(shipment.codAmount)}</p>
          </div>
        )}

        {shipment.isInsured && (
          <div>
            <span className="text-sm text-gray-500">{t('shipping.insurance')}</span>
            <p className="font-medium text-primary">{formatPrice(shipment.insuredAmount)}</p>
          </div>
        )}
      </div>

      {/* Actions */}
      <div className="flex gap-3 mb-6">
        <Button variant="secondary" onClick={handleDownloadLabel}>
          {t('shipping.download_label')}
        </Button>
        {canCancel && (
          <Button variant="danger" isLoading={cancelling} onClick={handleCancel}>
            {t('shipping.cancel_shipment')}
          </Button>
        )}
      </div>

      {/* Tracking timeline */}
      {shipment.trackingNumber && <ShipmentTracker shipmentId={shipment.id} />}
    </div>
  );
}
