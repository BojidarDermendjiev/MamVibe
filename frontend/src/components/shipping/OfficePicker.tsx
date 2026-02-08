import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { shippingApi } from '../../api/shippingApi';
import type { CourierOffice, CourierProvider } from '../../types/shipping';
import LoadingSpinner from '../common/LoadingSpinner';

interface OfficePickerProps {
  provider: CourierProvider;
  city?: string;
  value: string;
  onChange: (officeId: string, officeName: string) => void;
}

export default function OfficePicker({ provider, city, value, onChange }: OfficePickerProps) {
  const { t } = useTranslation();
  const [offices, setOffices] = useState<CourierOffice[]>([]);
  const [filter, setFilter] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    setLoading(true);
    shippingApi
      .getOffices(provider, city || undefined)
      .then((res) => setOffices(res.data))
      .catch(() => setOffices([]))
      .finally(() => setLoading(false));
  }, [provider, city]);

  const filtered = offices.filter(
    (o) =>
      o.name.toLowerCase().includes(filter.toLowerCase()) ||
      (o.address && o.address.toLowerCase().includes(filter.toLowerCase()))
  );

  if (loading) return <LoadingSpinner size="sm" className="py-4" />;

  return (
    <div className="space-y-2">
      <label className="block text-sm font-medium text-primary">{t('shipping.select_office')}</label>
      <input
        type="text"
        placeholder={t('shipping.filter_offices')}
        value={filter}
        onChange={(e) => setFilter(e.target.value)}
        className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary focus:border-primary"
      />
      <select
        value={value}
        onChange={(e) => {
          const office = offices.find((o) => o.id === e.target.value);
          onChange(e.target.value, office?.name ?? '');
        }}
        className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary focus:border-primary"
      >
        <option value="">{t('shipping.choose_office')}</option>
        {filtered.map((office) => (
          <option key={office.id} value={office.id}>
            {office.name} {office.isLocker ? '(APT)' : ''} — {office.address}
          </option>
        ))}
      </select>
    </div>
  );
}
