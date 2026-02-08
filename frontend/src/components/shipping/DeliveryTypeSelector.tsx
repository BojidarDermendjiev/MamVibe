import { useTranslation } from 'react-i18next';
import { HiOfficeBuilding, HiHome, HiCube } from 'react-icons/hi';
import { DeliveryType } from '../../types/shipping';

interface DeliveryTypeSelectorProps {
  value: DeliveryType;
  onChange: (type: DeliveryType) => void;
}

export default function DeliveryTypeSelector({ value, onChange }: DeliveryTypeSelectorProps) {
  const { t } = useTranslation();

  const types = [
    { id: DeliveryType.Office, label: t('shipping.to_office'), icon: HiOfficeBuilding },
    { id: DeliveryType.Address, label: t('shipping.to_address'), icon: HiHome },
    { id: DeliveryType.Locker, label: t('shipping.to_locker'), icon: HiCube },
  ];

  return (
    <div className="space-y-3">
      <h3 className="font-medium text-primary">{t('shipping.delivery_type')}</h3>
      <div className="grid grid-cols-3 gap-3">
        {types.map((type) => {
          const Icon = type.icon;
          return (
            <button
              key={type.id}
              type="button"
              onClick={() => onChange(type.id)}
              className={`flex flex-col items-center gap-2 p-4 rounded-xl border-2 transition-all ${
                value === type.id
                  ? 'border-primary bg-primary/5'
                  : 'border-gray-200 hover:border-lavender'
              }`}
            >
              <div className="w-10 h-10 rounded-lg bg-lavender/20 flex items-center justify-center">
                <Icon className="h-5 w-5 text-primary" />
              </div>
              <p className="text-sm font-medium text-primary">{type.label}</p>
            </button>
          );
        })}
      </div>
    </div>
  );
}
