import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { usePageSEO } from '@/hooks/useSEO';
import toast from '@/utils/toast';
import { useDashboard, type DashboardTab } from '../hooks/useDashboard';
import EBillCard from '../components/payment/EBillCard';
import { itemsApi } from '../api/itemsApi';
import { purchaseRequestsApi, type BuyerCheckResult } from '../api/purchaseRequestsApi';
import { offersApi } from '../api/offersApi';
import { savedSearchesApi } from '../api/savedSearchesApi';
import { bundlesApi } from '../api/bundlesApi';
import CreateBundleModal from '../components/bundles/CreateBundleModal';
import { PurchaseRequestStatus } from '../types/purchaseRequest';
import type { PurchaseRequest } from '../types/purchaseRequest';
import { OfferStatus } from '../types/offer';
import type { Offer } from '../types/offer';
import { PaymentMethod } from '../types/payment';
import { ListingType } from '../types/item';
import { useNotification } from '../contexts/NotificationContext';
import { formatPrice } from '../utils/currency';
import ItemCard from '../components/items/ItemCard';
import ShipmentCard from '../components/shipping/ShipmentCard';
import LoadingSpinner from '../components/common/LoadingSpinner';
import Avatar from '../components/common/Avatar';
import BuyerReputationModal from '../components/purchase/BuyerReputationModal';
import RateSellerModal from '../components/purchase/RateSellerModal';



export default function DashboardPage() {
  const { t } = useTranslation();
  const { tab, setTab, myItems, likedItems, payments, incomingRequests, myRequests, receivedOffers, sentOffers, shipments, ebills, followingFeed, following, followers, savedSearches, bundles, loading, removeLikedItem, refreshTab } = useDashboard();
  const [showCreateBundle, setShowCreateBundle] = useState(false);

  // Noindex: private, user-specific page. Should never appear in search results.
  usePageSEO({ title: "My Dashboard", description: "Manage your MamVibe listings, purchases, and messages.", index: false });
  const { decrementPendingRequestCount } = useNotification();

  const handleListingLikeToggle = async (id: string) => {
    try {
      await itemsApi.toggleLike(id);
      refreshTab();
    } catch { /* ignore */ }
  };

  const handleBump = async (id: string) => {
    try {
      await itemsApi.bump(id);
      toast.success(t('items.bump_success'));
      refreshTab();
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { error?: string } } })?.response?.data?.error ??
        t('items.bump_error');
      toast.error(msg);
    }
  };

  const handleLikedTabToggle = async (id: string) => {
    try {
      await itemsApi.toggleLike(id);
      removeLikedItem(id);
    } catch { /* ignore */ }
  };

  const [checkingId, setCheckingId] = useState<string | null>(null);
  const [counteringOfferId, setCounteringOfferId] = useState<string | null>(null);
  const [counterPrice, setCounterPrice] = useState('');
  const [ratedRequestIds, setRatedRequestIds] = useState<Set<string>>(new Set());
  const [rateModal, setRateModal] = useState<{ requestId: string; sellerName: string | null } | null>(null);
  const [reputationModal, setReputationModal] = useState<{
    request: PurchaseRequest;
    result: BuyerCheckResult;
  } | null>(null);

  const doAccept = async (requestId: string) => {
    try {
      await purchaseRequestsApi.accept(requestId);
      decrementPendingRequestCount();
      toast.success('Request accepted!');
      refreshTab();
    } catch {
      toast.error('Could not accept request.');
    }
  };

  const handleAccept = async (request: PurchaseRequest) => {
    setCheckingId(request.id);
    try {
      const { data } = await purchaseRequestsApi.checkBuyer(request.id);
      if (data.hasReports) {
        setReputationModal({ request, result: data });
      } else {
        await doAccept(request.id);
      }
    } catch {
      // If the check itself fails, proceed with the accept (don't block seller)
      await doAccept(request.id);
    } finally {
      setCheckingId(null);
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

  const handleAcceptOffer = async (offerId: string) => {
    try {
      await offersApi.accept(offerId);
      toast.success(t('offer.accepted'));
      refreshTab();
    } catch { toast.error(t('offer.action_error')); }
  };

  const handleDeclineOffer = async (offerId: string) => {
    try {
      await offersApi.decline(offerId);
      toast.success(t('offer.declined'));
      refreshTab();
    } catch { toast.error(t('offer.action_error')); }
  };

  const handleCounterOffer = async (offerId: string) => {
    const parsed = parseFloat(counterPrice);
    if (isNaN(parsed) || parsed <= 0) { toast.error(t('offer.invalid_price')); return; }
    try {
      await offersApi.counter(offerId, parsed);
      toast.success(t('offer.countered'));
      setCounteringOfferId(null);
      setCounterPrice('');
      refreshTab();
    } catch { toast.error(t('offer.action_error')); }
  };

  const handleAcceptCounter = async (offerId: string) => {
    try {
      await offersApi.acceptCounter(offerId);
      toast.success(t('offer.counter_accepted'));
      refreshTab();
    } catch { toast.error(t('offer.action_error')); }
  };

  const handleDeclineCounter = async (offerId: string) => {
    try {
      await offersApi.declineCounter(offerId);
      toast.success(t('offer.counter_declined'));
      refreshTab();
    } catch { toast.error(t('offer.action_error')); }
  };

  const handleCancelOffer = async (offerId: string) => {
    try {
      await offersApi.cancel(offerId);
      toast.success(t('offer.cancelled'));
      refreshTab();
    } catch { toast.error(t('offer.action_error')); }
  };

  const handleDeleteSavedSearch = async (id: string) => {
    try {
      await savedSearchesApi.delete(id);
      toast.success(t('saved_search.deleted'));
      refreshTab();
    } catch { toast.error(t('saved_search.delete_error')); }
  };

  const handleDeleteBundle = async (id: string) => {
    try {
      await bundlesApi.delete(id);
      toast.success(t('bundle.deleted'));
      refreshTab();
    } catch { toast.error(t('bundle.delete_error')); }
  };

  const offerStatusLabel = (status: number) => {
    if (status === OfferStatus.Pending)   return { text: t('offer.status_pending'),   cls: 'bg-yellow-100 text-yellow-800' };
    if (status === OfferStatus.Accepted)  return { text: t('offer.status_accepted'),  cls: 'bg-green-100 text-green-800' };
    if (status === OfferStatus.Declined)  return { text: t('offer.status_declined'),  cls: 'bg-red-100 text-red-800' };
    if (status === OfferStatus.Countered) return { text: t('offer.status_countered'), cls: 'bg-blue-100 text-blue-800' };
    if (status === OfferStatus.Expired)   return { text: t('offer.status_expired'),   cls: 'bg-gray-100 text-gray-500' };
    return { text: t('offer.status_cancelled'), cls: 'bg-gray-100 text-gray-500' };
  };

  const statusLabel = (status: number) => {
    if (status === PurchaseRequestStatus.Pending) return { text: t('dashboard.req_status_pending'), cls: 'bg-yellow-100 text-yellow-800' };
    if (status === PurchaseRequestStatus.Accepted) return { text: t('dashboard.req_status_accepted'), cls: 'bg-green-100 text-green-800' };
    if (status === PurchaseRequestStatus.Declined) return { text: t('dashboard.req_status_declined'), cls: 'bg-red-100 text-red-800' };
    if (status === PurchaseRequestStatus.Completed) return { text: t('dashboard.req_status_completed'), cls: 'bg-blue-100 text-blue-800' };
    return { text: t('dashboard.req_status_cancelled'), cls: 'bg-gray-100 text-gray-600' };
  };

  const tabs: { key: DashboardTab; label: string }[] = [
    { key: 'listings', label: t('dashboard.my_listings') },
    { key: 'liked', label: t('dashboard.liked_items') },
    { key: 'purchases', label: t('dashboard.purchases') },
    { key: 'incoming-requests', label: t('dashboard.incoming_requests') },
    { key: 'my-requests', label: t('dashboard.my_requests') },
    { key: 'received-offers', label: t('dashboard.received_offers') },
    { key: 'sent-offers', label: t('dashboard.sent_offers') },
    { key: 'shipments', label: t('dashboard.my_shipments') },
    { key: 'ebills', label: t('dashboard.my_ebills') },
    { key: 'following-feed', label: t('dashboard.following_feed') },
    { key: 'following', label: t('dashboard.following') },
    { key: 'followers', label: t('dashboard.followers') },
    { key: 'saved-searches', label: t('dashboard.saved_searches') },
    { key: 'bundles', label: t('dashboard.my_bundles') },
  ];

  return (
    <div className="max-w-7xl mx-auto px-4 py-8 animate-fade-in">
      {/* ── Rate Seller Modal ── */}
      {rateModal && (
        <RateSellerModal
          purchaseRequestId={rateModal.requestId}
          sellerName={rateModal.sellerName}
          onClose={() => setRateModal(null)}
          onRated={() => setRatedRequestIds(prev => new Set(prev).add(rateModal.requestId))}
        />
      )}

      {/* ── Buyer Reputation Modal ── */}
      {reputationModal && (
        <BuyerReputationModal
          buyerName={reputationModal.request.buyerDisplayName}
          buyerAvatarUrl={reputationModal.request.buyerAvatarUrl}
          result={reputationModal.result}
          onAccept={async () => {
            setReputationModal(null);
            await doAccept(reputationModal.request.id);
          }}
          onCancel={() => setReputationModal(null)}
        />
      )}
      <CreateBundleModal
        isOpen={showCreateBundle}
        onClose={() => setShowCreateBundle(false)}
        myItems={myItems}
        onCreated={() => { setShowCreateBundle(false); setTab('bundles'); refreshTab(); }}
      />

      <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-6 text-center">{t('dashboard.title')}</h1>

      <div className="flex gap-1 bg-white rounded-lg p-1 border border-lavender/30 mb-8 w-fit mx-auto">
        {tabs.map((t) => (
          <button
            key={t.key}
            onClick={() => setTab(t.key)}
            className={`px-4 py-2 rounded-md text-sm font-medium transition-all duration-300 ${
              tab === t.key ? 'bg-primary text-white shadow-md' : 'text-gray-500 hover:text-gray-900 dark:hover:text-white hover:bg-lavender/20'
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
                {myItems.map((item) => <ItemCard key={item.id} item={item} onLikeToggle={handleListingLikeToggle} onBump={handleBump} showStatus />)}
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
                    <div className="flex items-center gap-3">
                      <div className="text-right">
                        <p className="font-bold text-mauve">{formatPrice(p.amount)}</p>
                        <p className="text-xs text-gray-400 capitalize">
                          {p.paymentMethod === PaymentMethod.Card ? 'Card'
                            : p.paymentMethod === PaymentMethod.Booking ? 'Free'
                            : 'On Spot'}
                        </p>
                      </div>
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
                              onClick={() => handleAccept(r)}
                              disabled={checkingId === r.id}
                              className="px-3 py-1.5 text-sm font-medium bg-green-500 text-white rounded-lg hover:bg-green-600 transition-colors disabled:opacity-60 disabled:cursor-wait"
                            >
                              {checkingId === r.id ? '…' : t('dashboard.req_accept')}
                            </button>
                            <button
                              onClick={() => handleDecline(r.id)}
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
            )
          )}

          {/* ── Received Offers (seller view) ── */}
          {tab === 'received-offers' && (
            receivedOffers.length === 0 ? (
              <p className="text-center py-20 text-gray-400">{t('dashboard.no_received_offers')}</p>
            ) : (
              <div className="space-y-3">
                {receivedOffers.map((offer: Offer) => {
                  const { text, cls } = offerStatusLabel(offer.status);
                  return (
                    <div key={offer.id} className="bg-white rounded-xl p-4 border border-lavender/30 flex items-center gap-4 hover:shadow-md transition-shadow duration-300">
                      {offer.itemPhotoUrl ? (
                        <img src={offer.itemPhotoUrl} alt={offer.itemTitle ?? ''} className="w-14 h-14 rounded-lg object-cover flex-shrink-0" />
                      ) : (
                        <div className="w-14 h-14 rounded-lg bg-lavender/20 flex-shrink-0" />
                      )}
                      <div className="flex-1 min-w-0">
                        <p className="font-medium text-primary truncate">{offer.itemTitle}</p>
                        <p className="text-sm text-gray-500">
                          {t('offer.buyer')}: {offer.buyerDisplayName}
                          {' · '}{t('offer.offered')}: <span className="font-semibold text-mauve">{formatPrice(offer.offeredPrice)}</span>
                          {offer.itemPrice != null && (
                            <span className="text-gray-400 text-xs ml-1">({t('offer.list_price')}: {formatPrice(offer.itemPrice)})</span>
                          )}
                        </p>
                        <p className="text-xs text-gray-400 mt-0.5">{new Date(offer.createdAt).toLocaleDateString()}</p>
                      </div>
                      <div className="flex items-center gap-2 flex-shrink-0 flex-wrap justify-end">
                        <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${cls}`}>{text}</span>
                        {offer.status === OfferStatus.Pending && (
                          <>
                            <button onClick={() => handleAcceptOffer(offer.id)} className="px-3 py-1.5 text-sm font-medium bg-green-500 text-white rounded-lg hover:bg-green-600 transition-colors">{t('offer.accept')}</button>
                            <button onClick={() => { setCounteringOfferId(offer.id); setCounterPrice(''); }} className="px-3 py-1.5 text-sm font-medium bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-colors">{t('offer.counter')}</button>
                            <button onClick={() => handleDeclineOffer(offer.id)} className="px-3 py-1.5 text-sm font-medium bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors">{t('offer.decline')}</button>
                          </>
                        )}
                        {counteringOfferId === offer.id && (
                          <div className="flex items-center gap-2 mt-2 w-full">
                            <input
                              type="number" step="0.01" min="0.01"
                              value={counterPrice}
                              onChange={e => setCounterPrice(e.target.value)}
                              placeholder="0.00"
                              className="w-28 px-2 py-1.5 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                            />
                            <button onClick={() => handleCounterOffer(offer.id)} className="px-3 py-1.5 text-sm font-medium bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-colors">{t('common.send')}</button>
                            <button onClick={() => setCounteringOfferId(null)} className="px-2 py-1.5 text-sm text-gray-400 hover:text-gray-600 transition-colors">{t('common.cancel')}</button>
                          </div>
                        )}
                      </div>
                    </div>
                  );
                })}
              </div>
            )
          )}

          {/* ── Sent Offers (buyer view) ── */}
          {tab === 'sent-offers' && (
            sentOffers.length === 0 ? (
              <p className="text-center py-20 text-gray-400">{t('dashboard.no_sent_offers')}</p>
            ) : (
              <div className="space-y-3">
                {sentOffers.map((offer: Offer) => {
                  const { text, cls } = offerStatusLabel(offer.status);
                  return (
                    <div key={offer.id} className="bg-white rounded-xl p-4 border border-lavender/30 flex items-center gap-4 hover:shadow-md transition-shadow duration-300">
                      {offer.itemPhotoUrl ? (
                        <img src={offer.itemPhotoUrl} alt={offer.itemTitle ?? ''} className="w-14 h-14 rounded-lg object-cover flex-shrink-0" />
                      ) : (
                        <div className="w-14 h-14 rounded-lg bg-lavender/20 flex-shrink-0" />
                      )}
                      <div className="flex-1 min-w-0">
                        <p className="font-medium text-primary truncate">{offer.itemTitle}</p>
                        <p className="text-sm text-gray-500">
                          {t('offer.offered')}: <span className="font-semibold text-mauve">{formatPrice(offer.offeredPrice)}</span>
                          {offer.counterPrice != null && (
                            <span className="ml-2 text-blue-600">{t('offer.counter_price')}: <span className="font-semibold">{formatPrice(offer.counterPrice)}</span></span>
                          )}
                        </p>
                        <p className="text-xs text-gray-400 mt-0.5">{new Date(offer.createdAt).toLocaleDateString()}</p>
                      </div>
                      <div className="flex items-center gap-2 flex-shrink-0">
                        <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${cls}`}>{text}</span>
                        {offer.status === OfferStatus.Countered && (
                          <>
                            <button onClick={() => handleAcceptCounter(offer.id)} className="px-3 py-1.5 text-sm font-medium bg-green-500 text-white rounded-lg hover:bg-green-600 transition-colors">{t('offer.accept_counter')}</button>
                            <button onClick={() => handleDeclineCounter(offer.id)} className="px-3 py-1.5 text-sm font-medium bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors">{t('offer.decline_counter')}</button>
                          </>
                        )}
                        {(offer.status === OfferStatus.Pending || offer.status === OfferStatus.Countered) && (
                          <button onClick={() => handleCancelOffer(offer.id)} className="px-3 py-1.5 text-sm font-medium border border-gray-300 text-gray-500 rounded-lg hover:bg-gray-50 transition-colors">{t('offer.cancel')}</button>
                        )}
                      </div>
                    </div>
                  );
                })}
              </div>
            )
          )}

          {/* ── My Shipments ── */}
          {tab === 'shipments' && (() => {
            const toSend = shipments.filter(s => s.isCurrentUserSeller);
            const incoming = shipments.filter(s => !s.isCurrentUserSeller);
            if (shipments.length === 0) {
              return <p className="text-center py-20 text-gray-400">{t('dashboard.no_shipments')}</p>;
            }
            return (
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
            );
          })()}

          {/* ── E-Bills ── */}
          {tab === 'ebills' && (
            ebills.length === 0 ? (
              <div className="text-center py-20">
                <p className="text-gray-400">{t('ebill.empty')}</p>
                <p className="text-sm text-gray-300 mt-1">{t('ebill.empty_hint')}</p>
              </div>
            ) : (
              <div className="space-y-3">
                {ebills.map((bill) => <EBillCard key={bill.id} bill={bill} />)}
              </div>
            )
          )}

          {/* ── Following Feed ── */}
          {tab === 'following-feed' && (
            followingFeed.length === 0 ? (
              <div className="text-center py-20">
                <p className="text-gray-400">{t('dashboard.following_feed_empty')}</p>
                <p className="text-sm text-gray-300 mt-1">{t('dashboard.following_feed_empty_hint')}</p>
              </div>
            ) : (
              <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                {followingFeed.map((item) => (
                  <ItemCard key={item.id} item={item} />
                ))}
              </div>
            )
          )}

          {/* ── Following ── */}
          {tab === 'following' && (
            following.length === 0 ? (
              <p className="text-center py-20 text-gray-400">{t('dashboard.following_empty')}</p>
            ) : (
              <div className="space-y-3">
                {following.map((u) => (
                  <div key={u.id} className="bg-white dark:bg-[#2d2a42] rounded-xl p-4 border border-lavender/30 flex items-center gap-3">
                    <Avatar src={u.avatarUrl} size="md" />
                    <div className="flex-1 min-w-0">
                      <p className="font-medium text-primary">{u.displayName}</p>
                      <p className="text-xs text-gray-400 mt-0.5">
                        {t('follow.follower_count', { count: u.followerCount })} · {t('follow.item_count', { count: u.itemCount })}
                      </p>
                    </div>
                    <Link
                      to={`/profile/${u.id}`}
                      className="text-xs text-mauve hover:underline flex-shrink-0"
                    >
                      {t('follow.view_profile')}
                    </Link>
                  </div>
                ))}
              </div>
            )
          )}

          {/* ── Followers ── */}
          {tab === 'followers' && (
            followers.length === 0 ? (
              <p className="text-center py-20 text-gray-400">{t('dashboard.followers_empty')}</p>
            ) : (
              <div className="space-y-3">
                {followers.map((u) => (
                  <div key={u.id} className="bg-white dark:bg-[#2d2a42] rounded-xl p-4 border border-lavender/30 flex items-center gap-3">
                    <Avatar src={u.avatarUrl} size="md" />
                    <div className="flex-1 min-w-0">
                      <p className="font-medium text-primary">{u.displayName}</p>
                      <p className="text-xs text-gray-400 mt-0.5">
                        {t('follow.follower_count', { count: u.followerCount })} · {t('follow.item_count', { count: u.itemCount })}
                      </p>
                    </div>
                    <Link
                      to={`/profile/${u.id}`}
                      className="text-xs text-mauve hover:underline flex-shrink-0"
                    >
                      {t('follow.view_profile')}
                    </Link>
                  </div>
                ))}
              </div>
            )
          )}

          {/* ── Saved Searches ── */}
          {tab === 'saved-searches' && (
            savedSearches.length === 0 ? (
              <p className="text-center py-20 text-gray-400">{t('dashboard.saved_searches_empty')}</p>
            ) : (
              <div className="space-y-3">
                {savedSearches.map((s) => (
                  <div key={s.id} className="bg-white dark:bg-[#2d2a42] rounded-xl p-4 border border-lavender/30 flex items-center gap-3">
                    <div className="flex-1 min-w-0">
                      <p className="font-medium text-primary">{s.name}</p>
                      <p className="text-xs text-gray-400 mt-0.5">
                        {[s.categoryName, s.searchTerm, s.maxPrice != null ? `≤ ${formatPrice(s.maxPrice)}` : null].filter(Boolean).join(' · ')}
                      </p>
                    </div>
                    <button
                      onClick={() => handleDeleteSavedSearch(s.id)}
                      className="text-xs text-red-400 hover:text-red-600 flex-shrink-0 transition-colors"
                    >
                      {t('common.delete')}
                    </button>
                  </div>
                ))}
              </div>
            )
          )}

          {/* ── My Bundles ── */}
          {tab === 'bundles' && (
            <>
              <div className="flex justify-end mb-4">
                <button
                  onClick={() => setShowCreateBundle(true)}
                  className="px-4 py-2 rounded-lg bg-mauve text-white text-sm font-medium hover:bg-mauve/90 transition-colors"
                >
                  + {t('bundle.create_btn')}
                </button>
              </div>
              {bundles.length === 0 ? (
                <p className="text-center py-20 text-gray-400">{t('dashboard.bundles_empty')}</p>
              ) : (
                <div className="space-y-3">
                  {bundles.map((b) => (
                    <div key={b.id} className="bg-white dark:bg-[#2d2a42] rounded-xl p-4 border border-lavender/30 flex items-center gap-3 hover:shadow-md transition-shadow">
                      <div className="flex-shrink-0 flex -space-x-2">
                        {b.items.slice(0, 3).map((item) =>
                          item.photoUrl ? (
                            <img key={item.itemId} src={item.photoUrl} alt={item.title} className="w-12 h-12 rounded-lg object-cover border-2 border-white dark:border-[#2d2a42]" />
                          ) : (
                            <div key={item.itemId} className="w-12 h-12 rounded-lg bg-lavender/20 border-2 border-white dark:border-[#2d2a42]" />
                          )
                        )}
                      </div>
                      <div className="flex-1 min-w-0">
                        <Link to={`/bundles/${b.id}`} className="font-medium text-primary hover:underline truncate block">{b.title}</Link>
                        <p className="text-xs text-gray-400 mt-0.5">
                          {t('bundle.items_count', { count: b.items.length })} · {formatPrice(b.price)}
                          {b.isSold && <span className="ml-2 text-red-500">{t('bundle.sold_badge')}</span>}
                        </p>
                      </div>
                      {!b.isSold && (
                        <button
                          onClick={() => handleDeleteBundle(b.id)}
                          className="text-xs text-red-400 hover:text-red-600 flex-shrink-0 transition-colors"
                        >
                          {t('common.delete')}
                        </button>
                      )}
                    </div>
                  ))}
                </div>
              )}
            </>
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
                              ⭐ {t('rating.rated')}
                            </span>
                          ) : (
                            <button
                              onClick={() => setRateModal({ requestId: r.id, sellerName: null })}
                              className="px-3 py-1.5 text-sm font-medium text-white rounded-lg transition-colors whitespace-nowrap"
                              style={{ backgroundColor: '#945c67' }}
                              onMouseEnter={e => (e.currentTarget.style.backgroundColor = '#7d4e58')}
                              onMouseLeave={e => (e.currentTarget.style.backgroundColor = '#945c67')}
                            >
                              ⭐ {t('rating.rate_seller')}
                            </button>
                          )
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
