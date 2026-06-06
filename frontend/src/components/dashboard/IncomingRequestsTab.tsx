import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import type { PurchaseRequest } from '@/types/purchaseRequest';
import { PurchaseRequestStatus } from '@/types/purchaseRequest';
import Avatar from '@/components/common/Avatar';
import TabErrorState from './TabErrorState';

interface IncomingRequestsTabProps {
  requests: PurchaseRequest[];
  error: string | null;
  onRetry: () => void;
  checkingId: string | null;
  onAccept: (request: PurchaseRequest) => void;
  onDecline: (requestId: string) => void;
}

function statusLabel(t: (key: string) => string, status: number) {
  if (status === PurchaseRequestStatus.Pending) return { text: t('dashboard.req_status_pending'), cls: 'bg-yellow-100 text-yellow-800' };
  if (status === PurchaseRequestStatus.Accepted) return { text: t('dashboard.req_status_accepted'), cls: 'bg-green-100 text-green-800' };
  if (status === PurchaseRequestStatus.Declined) return { text: t('dashboard.req_status_declined'), cls: 'bg-red-100 text-red-800' };
  if (status === PurchaseRequestStatus.Completed) return { text: t('dashboard.req_status_completed'), cls: 'bg-blue-100 text-blue-800' };
  return { text: t('dashboard.req_status_cancelled'), cls: 'bg-gray-100 text-gray-600' };
}

export default function IncomingRequestsTab({ requests, error, onRetry, checkingId, onAccept, onDecline }: IncomingRequestsTabProps) {
  const { t } = useTranslation();

  if (error) return <TabErrorState onRetry={onRetry} />;

  return (
    <div role="tabpanel" id="panel-incoming-requests" aria-labelledby="tab-incoming-requests">
      {requests.length === 0 ? (
        <p className="text-center py-20 text-gray-400">{t('dashboard.no_incoming_requests')}</p>
      ) : (
        <div className="space-y-3">
          {requests.map((r) => {
            const { text, cls } = statusLabel(t, r.status);
            return (
              <div key={r.id} className="bg-white rounded-xl p-4 border border-lavender/30 flex items-center gap-4 hover:shadow-md transition-shadow duration-300">
                {r.itemPhotoUrl ? (
                  <img src={r.itemPhotoUrl} alt={r.itemTitle ?? ''} className="w-14 h-14 rounded-lg object-cover flex-shrink-0" />
                ) : (
                  <div className="w-14 h-14 rounded-lg bg-lavender/20 flex-shrink-0" />
                )}
                <div className="flex-1 min-w-0">
                  <p className="font-medium text-primary truncate">{r.itemTitle}</p>
                  <div className="flex items-center gap-2 mt-1">
                    <Avatar src={r.buyerAvatarUrl} size="sm" />
                    <p className="text-sm text-gray-500">{r.buyerDisplayName}</p>
                  </div>
                  <p className="text-xs text-gray-400 mt-0.5">{new Date(r.createdAt).toLocaleDateString()}</p>
                </div>
                <div className="flex items-center gap-2 flex-shrink-0">
                  <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${cls}`}>{text}</span>
                  {r.status === PurchaseRequestStatus.Pending && (
                    <>
                      <button
                        onClick={() => onAccept(r)}
                        disabled={checkingId === r.id}
                        className="px-3 py-1.5 text-sm font-medium bg-green-500 text-white rounded-lg hover:bg-green-600 transition-colors disabled:opacity-60 disabled:cursor-wait"
                      >
                        {checkingId === r.id ? '…' : t('dashboard.req_accept')}
                      </button>
                      <button
                        onClick={() => onDecline(r.id)}
                        className="px-3 py-1.5 text-sm font-medium bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors"
                      >
                        {t('dashboard.req_decline')}
                      </button>
                    </>
                  )}
                  {r.status === PurchaseRequestStatus.Completed && r.shipmentId && (
                    <Link
                      to={`/shipments/${r.shipmentId}`}
                      className="px-3 py-1.5 text-sm font-medium bg-primary text-white rounded-lg hover:bg-primary-dark transition-colors whitespace-nowrap"
                    >
                      {t('dashboard.req_view_waybill')}
                    </Link>
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
