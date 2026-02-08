import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { CourierProvider, ShipmentStatus, type Shipment } from '../../types/shipping';

interface ShipmentCardProps {
  shipment: Shipment;
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

export default function ShipmentCard({ shipment }: ShipmentCardProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const courierNames: Record<number, string> = {
    [CourierProvider.Econt]: 'Econt',
    [CourierProvider.Speedy]: 'Speedy',
    [CourierProvider.BoxNow]: 'Box Now',
  };
  const courierName = courierNames[shipment.courierProvider] ?? 'Unknown';

  return (
    <div
      onClick={() => navigate(`/shipments/${shipment.id}`)}
      className="bg-white rounded-xl p-4 border border-lavender/30 hover:shadow-md transition-all cursor-pointer"
    >
      <div className="flex justify-between items-start mb-2">
        <div>
          <p className="font-semibold text-primary">{shipment.itemTitle ?? t('shipping.shipment')}</p>
          <p className="text-sm text-gray-500">{courierName} — {shipment.trackingNumber ?? '-'}</p>
        </div>
        <span
          className={`px-2 py-1 rounded-full text-xs font-medium ${statusColors[shipment.status] ?? 'bg-gray-100 text-gray-700'}`}
        >
          {t(`shipping.status_${statusKeys[shipment.status] ?? 'pending'}`)}
        </span>
      </div>
      <div className="flex justify-between items-center text-sm">
        <span className="text-gray-500">{shipment.recipientName}</span>
        <span className="font-medium text-mauve">{shipment.shippingPrice.toFixed(2)} BGN</span>
      </div>
      <p className="text-xs text-gray-400 mt-1">
        {new Date(shipment.createdAt).toLocaleDateString()}
      </p>
    </div>
  );
}
