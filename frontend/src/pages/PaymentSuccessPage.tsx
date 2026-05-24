import { useEffect, useState } from 'react';
import { Link, useSearchParams, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { HiCheckCircle } from 'react-icons/hi';
import { purchaseRequestsApi } from '../api/purchaseRequestsApi';
import { PurchaseRequestStatus } from '../types/purchaseRequest';
import Button from '../components/common/Button';
import RateSellerModal from '../components/purchase/RateSellerModal';

export default function PaymentSuccessPage() {
  const { t } = useTranslation();
  const [searchParams] = useSearchParams();
  const location = useLocation();

  const isTestMode = (searchParams.get('session_id') ?? '').startsWith('test_');

  // itemId comes from router state (booking) or query param (Stripe redirect)
  const itemId: string | undefined =
    location.state?.itemId ?? searchParams.get('itemId') ?? undefined;

  const [rateModal, setRateModal] = useState<{ requestId: string; sellerName: string | null } | null>(null);
  const [rated, setRated] = useState(false);

  useEffect(() => {
    if (!itemId) return;
    purchaseRequestsApi.getAsBuyer().then(({ data }) => {
      const match = data.find(
        (r) => r.itemId === itemId && r.status === PurchaseRequestStatus.Completed
      );
      if (match) setRateModal({ requestId: match.id, sellerName: null });
    }).catch(() => {});
  }, [itemId]);

  return (
    <div className="min-h-[60vh] flex flex-col items-center justify-center text-center px-4">
      {isTestMode && (
        <div className="mb-6 px-4 py-2 rounded-lg bg-amber-100 border border-amber-300 text-amber-800 text-sm font-medium">
          ⚠️ Test mode — no real payment was processed.
        </div>
      )}
      <HiCheckCircle className="h-20 w-20 text-green-500 mb-4" />
      <h1 className="text-3xl font-bold text-primary mb-2">{t('payment.success_title')}</h1>
      <p className="text-gray-500 mb-8 max-w-md">{t('payment.success_msg')}</p>

      {itemId && !rated && !rateModal && (
        <button
          onClick={() => {
            purchaseRequestsApi.getAsBuyer().then(({ data }) => {
              const match = data.find(
                (r) => r.itemId === itemId && r.status === PurchaseRequestStatus.Completed
              );
              if (match) setRateModal({ requestId: match.id, sellerName: null });
            }).catch(() => {});
          }}
          className="mb-6 px-4 py-2 rounded-xl bg-yellow-400 hover:bg-yellow-500 text-white font-semibold text-sm transition-colors"
        >
          ⭐ {t('rating.rate_seller')}
        </button>
      )}

      {rated && (
        <p className="mb-6 text-sm font-medium text-green-600">⭐ {t('rating.rated')}</p>
      )}

      <div className="flex gap-3">
        <Link to="/dashboard"><Button variant="secondary">{t('nav.dashboard')}</Button></Link>
        <Link to="/browse"><Button>{t('nav.browse')}</Button></Link>
      </div>

      {rateModal && (
        <RateSellerModal
          purchaseRequestId={rateModal.requestId}
          sellerName={rateModal.sellerName}
          onClose={() => setRateModal(null)}
          onRated={() => { setRated(true); setRateModal(null); }}
        />
      )}
    </div>
  );
}
