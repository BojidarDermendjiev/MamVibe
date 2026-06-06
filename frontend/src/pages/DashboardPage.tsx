import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { usePageSEO } from '@/hooks/useSEO';
import toast from '@/utils/toast';
import { useDashboard, type DashboardTab } from '../hooks/useDashboard';
import { itemsApi } from '../api/itemsApi';
import { purchaseRequestsApi, type BuyerCheckResult } from '../api/purchaseRequestsApi';
import { offersApi } from '../api/offersApi';
import { savedSearchesApi } from '../api/savedSearchesApi';
import { bundlesApi } from '../api/bundlesApi';
import CreateBundleModal from '../components/bundles/CreateBundleModal';
import type { PurchaseRequest } from '../types/purchaseRequest';
import { useNotification } from '../contexts/NotificationContext';
import LoadingSpinner from '../components/common/LoadingSpinner';
import BuyerReputationModal from '../components/purchase/BuyerReputationModal';
import RateSellerModal from '../components/purchase/RateSellerModal';
import MyListingsTab from '../components/dashboard/MyListingsTab';
import FavoritesTab from '../components/dashboard/FavoritesTab';
import PurchasesTab from '../components/dashboard/PurchasesTab';
import IncomingRequestsTab from '../components/dashboard/IncomingRequestsTab';
import MyRequestsTab from '../components/dashboard/MyRequestsTab';
import ActiveOffersTab from '../components/dashboard/ActiveOffersTab';
import SentOffersTab from '../components/dashboard/SentOffersTab';
import ShipmentsTab from '../components/dashboard/ShipmentsTab';
import EBillsTab from '../components/dashboard/EBillsTab';
import FollowingFeedTab from '../components/dashboard/FollowingFeedTab';
import FollowingTab from '../components/dashboard/FollowingTab';
import SavedSearchesTab from '../components/dashboard/SavedSearchesTab';
import BundlesTab from '../components/dashboard/BundlesTab';

export default function DashboardPage() {
  const { t } = useTranslation();
  const { tab, setTab, myItems, likedItems, payments, incomingRequests, myRequests, receivedOffers, sentOffers, shipments, ebills, followingFeed, following, followers, savedSearches, bundles, loading, error, removeLikedItem, refreshTab } = useDashboard();
  const [showCreateBundle, setShowCreateBundle] = useState(false);
  const [checkingId, setCheckingId] = useState<string | null>(null);
  const [ratedRequestIds, setRatedRequestIds] = useState<Set<string>>(new Set());
  const [rateModal, setRateModal] = useState<{ requestId: string; sellerName: string | null } | null>(null);
  const [reputationModal, setReputationModal] = useState<{ request: PurchaseRequest; result: BuyerCheckResult } | null>(null);

  // Noindex: private, user-specific page. Should never appear in search results.
  usePageSEO({ title: "My Dashboard", description: "Manage your MamVibe listings, purchases, and messages.", index: false });
  const { decrementPendingRequestCount } = useNotification();

  const handleListingLikeToggle = async (id: string) => {
    try {
      await itemsApi.toggleLike(id);
      refreshTab();
    } catch { toast.error(t('common.error')); }
  };

  const handleBump = async (id: string) => {
    try {
      await itemsApi.bump(id);
      toast.success(t('items.bump_success'));
      refreshTab();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error ?? t('items.bump_error');
      toast.error(msg);
    }
  };

  const handleLikedTabToggle = async (id: string) => {
    try {
      await itemsApi.toggleLike(id);
      removeLikedItem(id);
    } catch { toast.error(t('common.error')); }
  };

  const doAccept = async (requestId: string) => {
    try {
      await purchaseRequestsApi.accept(requestId);
      decrementPendingRequestCount();
      toast.success('Request accepted!');
      refreshTab();
    } catch { toast.error('Could not accept request.'); }
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
    } catch { toast.error('Could not decline request.'); }
  };

  const handleAcceptOffer = async (offerId: string) => {
    try { await offersApi.accept(offerId); toast.success(t('offer.accepted')); refreshTab(); }
    catch { toast.error(t('offer.action_error')); }
  };

  const handleDeclineOffer = async (offerId: string) => {
    try { await offersApi.decline(offerId); toast.success(t('offer.declined')); refreshTab(); }
    catch { toast.error(t('offer.action_error')); }
  };

  const handleCounterOffer = async (offerId: string, price: number) => {
    try {
      await offersApi.counter(offerId, price);
      toast.success(t('offer.countered'));
      refreshTab();
    } catch { toast.error(t('offer.action_error')); }
  };

  const handleAcceptCounter = async (offerId: string) => {
    try { await offersApi.acceptCounter(offerId); toast.success(t('offer.counter_accepted')); refreshTab(); }
    catch { toast.error(t('offer.action_error')); }
  };

  const handleDeclineCounter = async (offerId: string) => {
    try { await offersApi.declineCounter(offerId); toast.success(t('offer.counter_declined')); refreshTab(); }
    catch { toast.error(t('offer.action_error')); }
  };

  const handleCancelOffer = async (offerId: string) => {
    try { await offersApi.cancel(offerId); toast.success(t('offer.cancelled')); refreshTab(); }
    catch { toast.error(t('offer.action_error')); }
  };

  const handleDeleteSavedSearch = async (id: string) => {
    try { await savedSearchesApi.delete(id); toast.success(t('saved_search.deleted')); refreshTab(); }
    catch { toast.error(t('saved_search.delete_error')); }
  };

  const handleDeleteBundle = async (id: string) => {
    try { await bundlesApi.delete(id); toast.success(t('bundle.deleted')); refreshTab(); }
    catch { toast.error(t('bundle.delete_error')); }
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
      {rateModal && (
        <RateSellerModal
          purchaseRequestId={rateModal.requestId}
          sellerName={rateModal.sellerName}
          onClose={() => setRateModal(null)}
          onRated={() => setRatedRequestIds(prev => new Set(prev).add(rateModal.requestId))}
        />
      )}
      {reputationModal && (
        <BuyerReputationModal
          buyerName={reputationModal.request.buyerDisplayName}
          buyerAvatarUrl={reputationModal.request.buyerAvatarUrl}
          result={reputationModal.result}
          onAccept={async () => { setReputationModal(null); await doAccept(reputationModal.request.id); }}
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

      <div role="tablist" aria-label={t('dashboard.title')} className="flex gap-1 bg-white rounded-lg p-1 border border-lavender/30 mb-8 w-fit mx-auto flex-wrap">
        {tabs.map((tabItem) => (
          <button
            key={tabItem.key}
            role="tab"
            id={`tab-${tabItem.key}`}
            aria-selected={tab === tabItem.key}
            aria-controls={`panel-${tabItem.key}`}
            onClick={() => setTab(tabItem.key)}
            className={`px-4 py-2 rounded-md text-sm font-medium transition-all duration-300 ${
              tab === tabItem.key ? 'bg-primary text-white shadow-md' : 'text-gray-500 hover:text-gray-900 dark:hover:text-white hover:bg-lavender/20'
            }`}
          >
            {tabItem.label}
          </button>
        ))}
      </div>

      {loading ? (
        <LoadingSpinner size="lg" className="py-20" />
      ) : (
        <>
          {tab === 'listings' && <MyListingsTab items={myItems} error={error} onRetry={refreshTab} onLikeToggle={handleListingLikeToggle} onBump={handleBump} />}
          {tab === 'liked' && <FavoritesTab items={likedItems} error={error} onRetry={refreshTab} onLikeToggle={handleLikedTabToggle} />}
          {tab === 'purchases' && <PurchasesTab payments={payments} error={error} onRetry={refreshTab} />}
          {tab === 'incoming-requests' && (
            <IncomingRequestsTab
              requests={incomingRequests}
              error={error}
              onRetry={refreshTab}
              checkingId={checkingId}
              onAccept={handleAccept}
              onDecline={handleDecline}
            />
          )}
          {tab === 'my-requests' && (
            <MyRequestsTab
              requests={myRequests}
              error={error}
              onRetry={refreshTab}
              ratedRequestIds={ratedRequestIds}
              onRateSeller={(requestId) => setRateModal({ requestId, sellerName: null })}
            />
          )}
          {tab === 'received-offers' && (
            <ActiveOffersTab
              offers={receivedOffers}
              error={error}
              onRetry={refreshTab}
              onAccept={handleAcceptOffer}
              onDecline={handleDeclineOffer}
              onCounter={handleCounterOffer}
            />
          )}
          {tab === 'sent-offers' && (
            <SentOffersTab
              offers={sentOffers}
              error={error}
              onRetry={refreshTab}
              onAcceptCounter={handleAcceptCounter}
              onDeclineCounter={handleDeclineCounter}
              onCancel={handleCancelOffer}
            />
          )}
          {tab === 'shipments' && <ShipmentsTab shipments={shipments} error={error} onRetry={refreshTab} />}
          {tab === 'ebills' && <EBillsTab ebills={ebills} error={error} onRetry={refreshTab} />}
          {tab === 'following-feed' && <FollowingFeedTab items={followingFeed} error={error} onRetry={refreshTab} />}
          {tab === 'following' && (
            <FollowingTab
              users={following}
              error={error}
              onRetry={refreshTab}
              emptyKey="dashboard.following_empty"
              panelId="panel-following"
              tabId="tab-following"
            />
          )}
          {tab === 'followers' && (
            <FollowingTab
              users={followers}
              error={error}
              onRetry={refreshTab}
              emptyKey="dashboard.followers_empty"
              panelId="panel-followers"
              tabId="tab-followers"
            />
          )}
          {tab === 'saved-searches' && <SavedSearchesTab savedSearches={savedSearches} error={error} onRetry={refreshTab} onDelete={handleDeleteSavedSearch} />}
          {tab === 'bundles' && <BundlesTab bundles={bundles} error={error} onRetry={refreshTab} onDelete={handleDeleteBundle} onCreateBundle={() => setShowCreateBundle(true)} />}
        </>
      )}
    </div>
  );
}
