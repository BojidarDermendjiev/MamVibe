import { useTranslation } from 'react-i18next';
import type { Shipment } from '@/types/shipping';
import ShipmentCard from '@/components/shipping/ShipmentCard';
import TabErrorState from './TabErrorState';

interface ShipmentsTabProps {
  shipments: Shipment[];
  error: string | null;
  onRetry: () => void;
}

export default function ShipmentsTab({ shipments, error, onRetry }: ShipmentsTabProps) {
  const { t } = useTranslation();

  if (error) return <TabErrorState onRetry={onRetry} />;

  const toSend = shipments.filter(s => s.isCurrentUserSeller);
  const incoming = shipments.filter(s => !s.isCurrentUserSeller);

  if (shipments.length === 0) {
    return (
      <div role="tabpanel" id="panel-shipments" aria-labelledby="tab-shipments">
        <p className="text-center py-20 text-gray-400">{t('dashboard.no_shipments')}</p>
      </div>
    );
  }

  return (
    <div role="tabpanel" id="panel-shipments" aria-labelledby="tab-shipments">
      <div className="space-y-8">
        {toSend.length > 0 && (
          <section>
            <h2 className="text-sm font-semibold text-gray-500 uppercase tracking-wide mb-3">{t('shipping.to_send_section')}</h2>
            <div className="space-y-3">
              {toSend.map((s) => <ShipmentCard key={s.id} shipment={s} />)}
            </div>
          </section>
        )}
        {incoming.length > 0 && (
          <section>
            <h2 className="text-sm font-semibold text-gray-500 uppercase tracking-wide mb-3">{t('shipping.incoming_section')}</h2>
            <div className="space-y-3">
              {incoming.map((s) => <ShipmentCard key={s.id} shipment={s} />)}
            </div>
          </section>
        )}
      </div>
    </div>
  );
}
