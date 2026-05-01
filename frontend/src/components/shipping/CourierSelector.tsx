import { useTranslation } from 'react-i18next';
import { CourierProvider } from '../../types/shipping';

interface CourierSelectorProps {
  value: CourierProvider;
  onChange: (provider: CourierProvider) => void;
}

export default function CourierSelector({ value, onChange }: CourierSelectorProps) {
  const { t } = useTranslation();

  const couriers = [
    { id: CourierProvider.Econt, label: t('shipping.econt'), desc: t('shipping.econt_desc') },
    { id: CourierProvider.Speedy, label: t('shipping.speedy'), desc: t('shipping.speedy_desc') },
    { id: CourierProvider.BoxNow, label: t('shipping.boxnow'), desc: t('shipping.boxnow_desc') },
    { id: CourierProvider.PigeonExpress, label: t('shipping.pigeon'), desc: t('shipping.pigeon_desc') },
  ];

  return (
    <div className="space-y-3">
      <h3 className="font-medium text-primary">{t('shipping.courier')}</h3>
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
        {couriers.map((courier) => (
          <button
            key={courier.id}
            type="button"
            onClick={() => onChange(courier.id)}
            className={`flex flex-col items-center gap-2 p-4 rounded-xl border-2 transition-all ${
              value === courier.id
                ? 'border-primary bg-primary/5'
                : 'border-gray-200 hover:border-lavender'
            }`}
          >
            <p className="font-medium text-primary">{courier.label}</p>
            <p className="text-sm text-gray-500">{courier.desc}</p>
          </button>
        ))}
      </div>
    </div>
  );
}
