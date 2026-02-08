import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { shippingApi } from '../../api/shippingApi';
import type { TrackingEvent } from '../../types/shipping';
import LoadingSpinner from '../common/LoadingSpinner';

interface ShipmentTrackerProps {
  shipmentId: string;
}

export default function ShipmentTracker({ shipmentId }: ShipmentTrackerProps) {
  const { t } = useTranslation();
  const [events, setEvents] = useState<TrackingEvent[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    shippingApi
      .trackShipment(shipmentId)
      .then((res) => setEvents(res.data))
      .catch(() => setEvents([]))
      .finally(() => setLoading(false));
  }, [shipmentId]);

  if (loading) return <LoadingSpinner size="sm" className="py-4" />;

  if (events.length === 0) {
    return <p className="text-sm text-gray-500 py-4">{t('shipping.no_tracking')}</p>;
  }

  return (
    <div className="space-y-0">
      <h3 className="font-medium text-primary mb-3">{t('shipping.tracking')}</h3>
      <div className="relative pl-6">
        <div className="absolute left-2 top-0 bottom-0 w-0.5 bg-lavender/30" />
        {events.map((event, idx) => (
          <div key={idx} className="relative pb-4 last:pb-0">
            <div
              className={`absolute left-[-16px] top-1 w-3 h-3 rounded-full border-2 ${
                idx === 0
                  ? 'bg-primary border-primary'
                  : 'bg-white border-lavender'
              }`}
            />
            <div className="ml-2">
              <p className="text-sm font-medium text-primary">{event.description}</p>
              <div className="flex gap-3 text-xs text-gray-500 mt-0.5">
                <span>{new Date(event.timestamp).toLocaleString()}</span>
                {event.location && <span>{event.location}</span>}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
