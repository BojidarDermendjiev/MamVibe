import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { CourierProvider, ShipmentStatus, type Shipment } from '../../types/shipping';
import { formatPrice } from '../../utils/currency';
import { shippingApi } from '../../api/shippingApi';
import toast from '../../utils/toast';

interface ShipmentCardProps {
  shipment: Shipment;
  currentUserId?: string;
}

const statusColors: Record<number, string> = {
  [ShipmentStatus.Pending]: 'bg-gray-100 text-gray-700',
  [ShipmentStatus.Created]: 'bg-blue-100 text-blue-700',
  [ShipmentStatus.PickedUp]: 'bg-indigo-100 text-indigo-700',
  [ShipmentStatus.InTransit]: 'bg-yellow-100 text-yellow-700',
  [ShipmentStatus.OutForDelivery]: 'bg-orange-100 text-orange-700',
  [ShipmentStatus.Delivered]: 'bg-green-100 text-green-700',
  [ShipmentStatus.Returned]: 'bg-red-100 text-red-700',
  [ShipmentStatus.Cancelled]: 'bg-red-100 text-red-700',
};

const statusKeys: Record<number, string> = {
  [ShipmentStatus.Pending]: 'pending',
  [ShipmentStatus.Created]: 'created',
  [ShipmentStatus.PickedUp]: 'picked_up',
  [ShipmentStatus.InTransit]: 'in_transit',
  [ShipmentStatus.OutForDelivery]: 'out_for_delivery',
  [ShipmentStatus.Delivered]: 'delivered',
  [ShipmentStatus.Returned]: 'returned',
  [ShipmentStatus.Cancelled]: 'cancelled',
};

export default function ShipmentCard({ shipment, currentUserId }: ShipmentCardProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [downloading, setDownloading] = useState(false);

  const isSeller = !!(currentUserId && shipment.sellerId && shipment.sellerId === currentUserId);
  const canPrintLabel = isSeller && (shipment.status === ShipmentStatus.Created || shipment.status === ShipmentStatus.PickedUp);

  const courierNames: Record<number, string> = {
    [CourierProvider.Econt]: 'Econt',
    [CourierProvider.Speedy]: 'Speedy',
    [CourierProvider.BoxNow]: 'Box Now',
    [CourierProvider.PigeonExpress]: 'Pigeon Express',
  };
  const courierName = courierNames[shipment.courierProvider] ?? 'Unknown';

  const handleDownloadLabel = async (e: React.MouseEvent) => {
    e.stopPropagation();
    setDownloading(true);
    try {
      const { data } = await shippingApi.getLabel(shipment.id);
      const url = URL.createObjectURL(data);
      const a = document.createElement('a');
      a.href = url;
      a.download = `label-${shipment.trackingNumber ?? shipment.id}.pdf`;
      a.click();
      URL.revokeObjectURL(url);
    } catch {
      toast.error(t('shipping.label_error'));
    } finally {
      setDownloading(false);
    }
  };

  return (
    <div
      onClick={() => navigate(`/shipments/${shipment.id}`)}
      className="bg-white rounded-xl p-4 border border-lavender/30 hover:shadow-md transition-all cursor-pointer"
    >
      <div className="flex justify-between items-start mb-2">
        <div className="flex-1 min-w-0 pr-3">
          <div className="flex items-center gap-2 mb-0.5">
            <p className="font-semibold text-primary truncate">{shipment.itemTitle ?? t('shipping.shipment')}</p>
            <span className={`shrink-0 px-1.5 py-0.5 rounded text-xs font-medium ${isSeller ? 'bg-mauve/10 text-mauve' : 'bg-blue-50 text-blue-600'}`}>
              {isSeller ? t('shipping.role_sender') : t('shipping.role_recipient')}
            </span>
          </div>
          <p className="text-sm text-gray-500">{courierName} — {shipment.trackingNumber ?? '-'}</p>
        </div>
        <span
          className={`shrink-0 px-2 py-1 rounded-full text-xs font-medium ${statusColors[shipment.status] ?? 'bg-gray-100 text-gray-700'}`}
        >
          {t(`shipping.status_${statusKeys[shipment.status] ?? 'pending'}`)}
        </span>
      </div>
      <div className="flex justify-between items-center text-sm">
        <span className="text-gray-500">{shipment.recipientName}</span>
        <span className="font-medium text-mauve">{formatPrice(shipment.shippingPrice)}</span>
      </div>
      <p className="text-xs text-gray-400 mt-1">
        {new Date(shipment.createdAt).toLocaleDateString()}
      </p>
      {canPrintLabel && (
        <div className="mt-3 flex justify-end" onClick={(e) => e.stopPropagation()}>
          <button
            onClick={handleDownloadLabel}
            disabled={downloading}
            className="px-3 py-1.5 text-sm font-medium bg-primary text-white rounded-lg hover:bg-primary-dark transition-colors disabled:opacity-60 disabled:cursor-wait flex items-center gap-1.5"
          >
            🖨️ {downloading ? '…' : t('shipping.download_label')}
          </button>
        </div>
      )}
    </div>
  );
}
