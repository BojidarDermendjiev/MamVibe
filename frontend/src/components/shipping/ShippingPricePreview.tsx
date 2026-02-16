import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { shippingApi } from '../../api/shippingApi';
import type { CalculateShippingRequest, ShippingPriceResult } from '../../types/shipping';
import LoadingSpinner from '../common/LoadingSpinner';

interface ShippingPricePreviewProps {
  request: CalculateShippingRequest | null;
}

export default function ShippingPricePreview({ request }: ShippingPricePreviewProps) {
  const { t } = useTranslation();
  const [result, setResult] = useState<ShippingPriceResult | null>(null);
  const [loading, setLoading] = useState(false);

  const requestKey = request
    ? `${request.courierProvider}-${request.deliveryType}-${request.toCity}-${request.officeId}-${request.weight}-${request.isCod}-${request.codAmount}-${request.isInsured}-${request.insuredAmount}`
    : '';

  useEffect(() => {
    if (!request || !request.weight) {
      return;
    }

    let cancelled = false;
    const timer = setTimeout(() => {
      shippingApi
        .calculatePrice(request)
        .then((res) => { if (!cancelled) setResult(res.data); })
        .catch(() => { if (!cancelled) setResult(null); })
        .finally(() => { if (!cancelled) setLoading(false); });
    }, 500);

    return () => { cancelled = true; clearTimeout(timer); };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [requestKey]);

  if (loading) {
    return (
      <div className="bg-lavender/10 rounded-xl p-4 flex items-center gap-3">
        <LoadingSpinner size="sm" />
        <span className="text-sm text-gray-500">{t('shipping.calculating')}</span>
      </div>
    );
  }

  if (!result || !request || !request.weight) return null;

  return (
    <div className="bg-lavender/10 rounded-xl p-4">
      <div className="flex justify-between items-center">
        <span className="text-sm font-medium text-primary">{t('shipping.shipping_price')}</span>
        <span className="text-lg font-bold text-mauve">
          {result.price.toFixed(2)} {result.currency}
        </span>
      </div>
      {result.estimatedDelivery && (
        <p className="text-xs text-gray-500 mt-1">
          {t('shipping.estimated_delivery')}: {result.estimatedDelivery}
        </p>
      )}
    </div>
  );
}
