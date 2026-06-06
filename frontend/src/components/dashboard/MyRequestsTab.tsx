import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import type { PurchaseRequest } from '@/types/purchaseRequest';
import { PurchaseRequestStatus } from '@/types/purchaseRequest';
import { ListingType } from '@/types/item';
import { formatPrice } from '@/utils/currency';
import TabErrorState from './TabErrorState';

interface MyRequestsTabProps {
  requests: PurchaseRequest[];
  error: string | null;
  onRetry: () => void;
  ratedRequestIds: Set<string>;
  onRateSeller: (requestId: string) => void;
}

function statusLabel(t: (key: string) => string, status: number) {
  if (status === PurchaseRequestStatus.Pending) return { text: t('dashboard.req_status_pending'), cls: 'bg-yellow-100 text-yellow-800' };
  if (status === PurchaseRequestStatus.Accepted) return { text: t('dashboard.req_status_accepted'), cls: 'bg-green-100 text-green-800' };
  if (status === PurchaseRequestStatus.Declined) return { text: t('dashboard.req_status_declined'), cls: 'bg-red-100 text-red-800' };
  if (status === PurchaseRequestStatus.Completed) return { text: t('dashboard.req_status_completed'), cls: 'bg-blue-100 text-blue-800' };
  return { text: t('dashboard.req_status_cancelled'), cls: 'bg-gray-100 text-gray-600' };
}

export default function MyRequestsTab({ requests, error, onRetry, ratedRequestIds, onRateSeller }: MyRequestsTabProps) {
  const { t } = useTranslation();

  if (error) return <TabErrorState onRetry={onRetry} />;

  return (
    <div role="tabpanel" id="panel-my-requests" aria-labelledby="tab-my-requests">
      {requests.length === 0 ? (
        <p className="text-center py-20 text-gray-400">{t('dashboard.no_my_requests')}</p>
      ) : (
        <div className="space-y-3">
          {requests.map((r) => {
            const { text, cls } = statusLabel(t, r.status);
            return (
              <div key={r.id} className="bg-white rounded-xl p-4 border border-lavender/30 flex items-center gap-4 hover:shadow-md transition-shadow duration-300">
                {(r.itemPhotoUrl || r.bundlePhotoUrl) ? (
                  <img src={r.itemPhotoUrl ?? r.bundlePhotoUrl ?? ''} alt={r.bundleTitle ?? r.itemTitle ?? ''} className="w-14 h-14 rounded-lg object-cover flex-shrink-0" />
                ) : (
                  <div className="w-14 h-14 rounded-lg bg-lavender/20 flex-shrink-0" />
                )}
                <div className="flex-1 min-w-0">
                  <p className="font-medium text-primary truncate">{r.bundleTitle ?? r.itemTitle}</p>
                  <p className="text-sm text-gray-500 mt-0.5">
                    {r.price != null && r.price > 0 ? formatPrice(r.price) : 'Free'}
                  </p>
                  <p className="text-xs text-gray-400 mt-0.5">{new Date(r.createdAt).toLocaleDateString()}</p>
                </div>
                <div className="flex items-center gap-2 flex-shrink-0">
                  <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${cls}`}>{text}</span>
                  {r.status === PurchaseRequestStatus.Accepted && (
                    <Link
                      to={r.bundleId ? `/payment/bundle/${r.bundleId}` : `/payment/${r.itemId}`}
                      className="px-3 py-1.5 text-sm font-medium bg-primary text-white rounded-lg hover:bg-primary-dark transition-colors whitespace-nowrap"
                    >
                      {r.listingType === ListingType.Donate
                        ? t('dashboard.req_complete_booking')
                        : t('dashboard.req_complete_purchase')}
                    </Link>
                  )}
                  {r.status === PurchaseRequestStatus.Completed && (
                    ratedRequestIds.has(r.id) ? (
                      <span className="px-2 py-1 text-xs font-medium bg-blue-100 text-blue-700 rounded-lg">
                        {t('rating.rated')}
                      </span>
                    ) : (
                      <button
                        onClick={() => onRateSeller(r.id)}
                        className="px-3 py-1.5 text-sm font-medium text-white rounded-lg transition-colors whitespace-nowrap"
                        style={{ backgroundColor: '#945c67' }}
                        onMouseEnter={e => (e.currentTarget.style.backgroundColor = '#7d4e58')}
                        onMouseLeave={e => (e.currentTarget.style.backgroundColor = '#945c67')}
                      >
                        {t('rating.rate_seller')}
                      </button>
                    )
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
