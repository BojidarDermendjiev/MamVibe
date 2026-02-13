import { useState, useEffect, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { shippingApi } from '../../api/shippingApi';
import type { CourierOffice, CourierProvider } from '../../types/shipping';
import LoadingSpinner from '../common/LoadingSpinner';

interface OfficePickerProps {
  provider: CourierProvider;
  city?: string;
  value: string;
  onChange: (officeId: string, officeName: string) => void;
  lockersOnly?: boolean;
}

const MAX_VISIBLE = 50;

export default function OfficePicker({ provider, city, value, onChange, lockersOnly }: OfficePickerProps) {
  const { t } = useTranslation();
  const [offices, setOffices] = useState<CourierOffice[]>([]);
  const [filter, setFilter] = useState('');
  const [loading, setLoading] = useState(false);
  const [open, setOpen] = useState(false);
  const wrapperRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    setLoading(true);
    shippingApi
      .getOffices(provider, city || undefined)
      .then((res) => setOffices(res.data))
      .catch(() => setOffices([]))
      .finally(() => setLoading(false));
  }, [provider, city]);

  // Close dropdown on outside click
  useEffect(() => {
    const handleClick = (e: MouseEvent) => {
      if (wrapperRef.current && !wrapperRef.current.contains(e.target as Node)) {
        setOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClick);
    return () => document.removeEventListener('mousedown', handleClick);
  }, []);

  const filtered = offices.filter((o) => {
    if (lockersOnly !== undefined) {
      if (lockersOnly && !o.isLocker) return false;
      if (!lockersOnly && o.isLocker) return false;
    }
    if (!filter.trim()) return true;
    const q = filter.toLowerCase();
    return (
      o.id.toLowerCase().includes(q) ||
      o.name.toLowerCase().includes(q) ||
      (o.city && o.city.toLowerCase().includes(q)) ||
      (o.address && o.address.toLowerCase().includes(q))
    );
  });

  const visible = filtered.slice(0, MAX_VISIBLE);
  const hasMore = filtered.length > MAX_VISIBLE;

  const selectedOffice = offices.find((o) => o.id === value);

  const handleSelect = (office: CourierOffice) => {
    onChange(office.id, office.name);
    setFilter('');
    setOpen(false);
  };

  if (loading) return <LoadingSpinner size="sm" className="py-4" />;

  return (
    <div className="space-y-2" ref={wrapperRef}>
      <label className="block text-sm font-medium text-primary">{t('shipping.select_office')}</label>

      {/* Search input */}
      <input
        type="text"
        placeholder={t('shipping.filter_offices')}
        value={filter}
        onChange={(e) => { setFilter(e.target.value); setOpen(true); }}
        onFocus={() => setOpen(true)}
        className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-primary focus:border-primary"
      />

      {/* Selected value display */}
      {selectedOffice && !open && (
        <div className="px-3 py-2 bg-lavender/10 border border-lavender/30 rounded-lg text-sm">
          <span className="font-medium text-primary">#{selectedOffice.id}</span>
          {' '}{selectedOffice.name} {selectedOffice.isLocker ? '(APT)' : ''}
          {selectedOffice.address && <span className="text-gray-500"> — {selectedOffice.address}</span>}
        </div>
      )}

      {/* Dropdown list */}
      {open && (
        <div className="max-h-60 overflow-y-auto border border-gray-200 rounded-lg bg-white shadow-lg">
          {visible.length === 0 && (
            <div className="px-3 py-4 text-sm text-gray-400 text-center">
              {filter.trim() ? t('shipping.no_offices_found', 'No offices found') : t('shipping.choose_office')}
            </div>
          )}
          {visible.map((office) => (
            <button
              key={office.id}
              type="button"
              onClick={() => handleSelect(office)}
              className={`w-full text-left px-3 py-2 text-sm hover:bg-lavender/20 transition-colors border-b border-gray-50 last:border-b-0 ${
                office.id === value ? 'bg-lavender/10 font-medium' : ''
              }`}
            >
              <span className="text-primary font-medium">#{office.id}</span>
              {' '}{office.name} {office.isLocker ? '(APT)' : ''}
              {office.city && <span className="text-gray-500"> — {office.city}</span>}
              {office.address && <span className="text-gray-400 text-xs block ml-5">{office.address}</span>}
            </button>
          ))}
          {hasMore && (
            <div className="px-3 py-2 text-xs text-gray-400 text-center bg-gray-50">
              {t('shipping.type_to_filter', { count: filtered.length - MAX_VISIBLE, defaultValue: `+${filtered.length - MAX_VISIBLE} more — type to filter` })}
            </div>
          )}
        </div>
      )}
    </div>
  );
}
