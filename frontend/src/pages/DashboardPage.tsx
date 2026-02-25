import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import { useDashboard, type DashboardTab } from '../hooks/useDashboard';
import { itemsApi } from '../api/itemsApi';
import { purchaseRequestsApi } from '../api/purchaseRequestsApi';
import { PurchaseRequestStatus } from '../types/purchaseRequest';
import { ListingType } from '../types/item';
import { useNotification } from '../contexts/NotificationContext';
import ItemCard from '../components/items/ItemCard';
import ShipmentCard from '../components/shipping/ShipmentCard';
import LoadingSpinner from '../components/common/LoadingSpinner';
import Avatar from '../components/common/Avatar';

export default function DashboardPage() {
  const { t } = useTranslation();
  const { tab, setTab, myItems, likedItems, payments, incomingRequests, myRequests, shipments, loading, removeLikedItem, refreshTab } = useDashboard();
  const { decrementPendingRequestCount } = useNotification();

  const handleListingLikeToggle = async (id: string) => {
    try {
      await itemsApi.toggleLike(id);
      refreshTab();
    } catch { /* ignore */ }
  };

  const handleLikedTabToggle = async (id: string) => {
    try {
      await itemsApi.toggleLike(id);
      removeLikedItem(id);
    } catch { /* ignore */ }
  };

  const handleAccept = async (requestId: string) => {
    try {
      await purchaseRequestsApi.accept(requestId);
      decrementPendingRequestCount();
      toast.success('Request accepted!');
      refreshTab();
    } catch {
      toast.error('Could not accept request.');
    }
  };

  const handleDecline = async (requestId: string) => {
    try {
      await purchaseRequestsApi.decline(requestId);
      decrementPendingRequestCount();
      toast.success('Request declined.');
      refreshTab();
    } catch {
      toast.error('Could not decline request.');
    }
  };

  const statusLabel = (status: number) => {
    if (status === PurchaseRequestStatus.Pending) return { text: 'Pending', cls: 'bg-yellow-100 text-yellow-800' };
    if (status === PurchaseRequestStatus.Accepted) return { text: 'Accepted', cls: 'bg-green-100 text-green-800' };
    if (status === PurchaseRequestStatus.Declined) return { text: 'Declined', cls: 'bg-red-100 text-red-800' };
    if (status === PurchaseRequestStatus.Completed) return { text: 'Completed', cls: 'bg-blue-100 text-blue-800' };
    return { text: 'Cancelled', cls: 'bg-gray-100 text-gray-600' };
  };

  const tabs: { key: DashboardTab; label: string }[] = [
    { key: 'listings', label: t('dashboard.my_listings') },
    { key: 'liked', label: t('dashboard.liked_items') },
    { key: 'purchases', label: t('dashboard.purchases') },
    { key: 'incoming-requests', label: t('dashboard.incoming_requests') },
    { key: 'my-requests', label: t('dashboard.my_requests') },
    { key: 'shipments', label: t('dashboard.my_shipments') },
  ];

  return (
    <div className="max-w-7xl mx-auto px-4 py-8 animate-fade-in">
      <h1 className="text-3xl font-bold text-primary-dark mb-6">{t('dashboard.title')}</h1>

      <div className="flex gap-1 bg-white rounded-lg p-1 border border-lavender/30 mb-8 w-fit">
        {tabs.map((t) => (
          <button
            key={t.key}
            onClick={() => setTab(t.key)}
            className={`px-4 py-2 rounded-md text-sm font-medium transition-all duration-300 ${
              tab === t.key ? 'bg-primary text-white shadow-md' : 'text-gray-500 hover:text-primary-dark hover:bg-lavender/20'
            }`}
          >
            {t.label}
          </button>
        ))}
      </div>

      {loading ? (
        <LoadingSpinner size="lg" className="py-20" />
      ) : (
        <>
          {tab === 'listings' && (
            myItems.length === 0 ? (
              <p className="text-center py-20 text-gray-400">{t('dashboard.no_listings')}</p>
            ) : (
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                {myItems.map((item) => <ItemCard key={item.id} item={item} onLikeToggle={handleListingLikeToggle} />)}
              </div>
            )
          )}
          {tab === 'liked' && (
            likedItems.length === 0 ? (
              <p className="text-center py-20 text-gray-400">{t('dashboard.no_liked')}</p>
            ) : (
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                {likedItems.map((item) => <ItemCard key={item.id} item={item} onLikeToggle={handleLikedTabToggle} />)}
              </div>
            )
          )}
          {tab === 'purchases' && (
            payments.length === 0 ? (
              <p className="text-center py-20 text-gray-400">{t('dashboard.no_purchases')}</p>
            ) : (
              <div className="space-y-3">
                {payments.map((p) => (
                  <div key={p.id} className="bg-white rounded-xl p-4 border border-lavender/30 flex items-center justify-between hover:shadow-md transition-shadow duration-300">
                    <div>
                      <p className="font-medium text-primary">{p.itemTitle}</p>
                      <p className="text-sm text-gray-400">{new Date(p.createdAt).toLocaleDateString()}</p>
                    </div>
                    <div className="text-right">
                      <p className="font-bold text-mauve">${p.amount.toFixed(2)}</p>
                      <p className="text-xs text-gray-400 capitalize">{p.paymentMethod === 0 ? 'Card' : 'On Spot'}</p>
                    </div>
                  </div>
                ))}
              </div>
            )
          )}

          {/* ── Incoming Requests (seller view) ── */}
          {tab === 'incoming-requests' && (
            incomingRequests.length === 0 ? (
              <p className="text-center py-20 text-gray-400">{t('dashboard.no_incoming_requests')}</p>
            ) : (
              <div className="space-y-3">
                {incomingRequests.map((r) => {
                  const { text, cls } = statusLabel(r.status);
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
                              onClick={() => handleAccept(r.id)}
                              className="px-3 py-1.5 text-sm font-medium bg-green-500 text-white rounded-lg hover:bg-green-600 transition-colors"
                            >
                              Accept
                            </button>
                            <button
                              onClick={() => handleDecline(r.id)}
                              className="px-3 py-1.5 text-sm font-medium bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors"
                            >
                              Decline
                            </button>
                          </>
                        )}
                        {r.status === PurchaseRequestStatus.Completed && r.shipmentId && (
                          <Link
                            to={`/shipments/${r.shipmentId}`}
                            className="px-3 py-1.5 text-sm font-medium bg-primary text-white rounded-lg hover:bg-primary-dark transition-colors whitespace-nowrap"
                          >
                            View Waybill
                          </Link>
                        )}
                      </div>
                    </div>
                  );
                })}
              </div>
            )
          )}

          {/* ── My Shipments ── */}
          {tab === 'shipments' && (
            shipments.length === 0 ? (
              <p className="text-center py-20 text-gray-400">{t('dashboard.no_shipments')}</p>
            ) : (
              <div className="space-y-3">
                {shipments.map((s) => <ShipmentCard key={s.id} shipment={s} />)}
              </div>
            )
          )}

          {/* ── My Requests (buyer view) ── */}
          {tab === 'my-requests' && (
            myRequests.length === 0 ? (
              <p className="text-center py-20 text-gray-400">{t('dashboard.no_my_requests')}</p>
            ) : (
              <div className="space-y-3">
                {myRequests.map((r) => {
                  const { text, cls } = statusLabel(r.status);
                  return (
                    <div key={r.id} className="bg-white rounded-xl p-4 border border-lavender/30 flex items-center gap-4 hover:shadow-md transition-shadow duration-300">
                      {r.itemPhotoUrl ? (
                        <img src={r.itemPhotoUrl} alt={r.itemTitle ?? ''} className="w-14 h-14 rounded-lg object-cover flex-shrink-0" />
                      ) : (
                        <div className="w-14 h-14 rounded-lg bg-lavender/20 flex-shrink-0" />
                      )}
                      <div className="flex-1 min-w-0">
                        <p className="font-medium text-primary truncate">{r.itemTitle}</p>
                        <p className="text-sm text-gray-500 mt-0.5">
                          {r.price != null && r.price > 0 ? `$${r.price.toFixed(2)}` : 'Free'}
                        </p>
                        <p className="text-xs text-gray-400 mt-0.5">{new Date(r.createdAt).toLocaleDateString()}</p>
                      </div>
                      <div className="flex items-center gap-2 flex-shrink-0">
                        <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${cls}`}>{text}</span>
                        {r.status === PurchaseRequestStatus.Accepted && r.listingType === ListingType.Sell && (
                          <Link
                            to={`/payment/${r.itemId}`}
                            className="px-3 py-1.5 text-sm font-medium bg-primary text-white rounded-lg hover:bg-primary-dark transition-colors whitespace-nowrap"
                          >
                            Complete Purchase
                          </Link>
                        )}
                        {r.status === PurchaseRequestStatus.Accepted && r.listingType === ListingType.Donate && (
                          <span className="px-2 py-1 text-xs font-medium bg-green-100 text-green-700 rounded-lg">
                            Booking Confirmed
                          </span>
                        )}
                        {r.status === PurchaseRequestStatus.Completed && (
                          <span className="px-2 py-1 text-xs font-medium bg-blue-100 text-blue-700 rounded-lg">
                            Order Complete
                          </span>
                        )}
                      </div>
                    </div>
                  );
                })}
              </div>
            )
          )}
        </>
      )}
    </div>
  );
}
