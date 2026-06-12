import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
  ListChecks,
  Heart,
  ShoppingBag,
  Package,
  Receipt,
  Layers,
  Inbox,
  Send,
  Tag,
  MessageSquare,
  Rss,
  UserCheck,
  Users,
  Bookmark,
  Menu,
  X,
  Wallet,
} from 'lucide-react';
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
import BankPayoutsTab from '../components/dashboard/BankPayoutsTab';

interface NavItem {
  key: DashboardTab;
  labelKey: string;
  icon: React.ElementType;
}

interface NavGroup {
  groupKey: string;
  items: NavItem[];
}

const NAV_GROUPS: NavGroup[] = [
  {
    groupKey: 'dashboard.nav_group_activity',
    items: [
      { key: 'listings',  labelKey: 'dashboard.my_listings',  icon: ListChecks },
      { key: 'liked',     labelKey: 'dashboard.liked_items',  icon: Heart },
      { key: 'purchases', labelKey: 'dashboard.purchases',    icon: ShoppingBag },
      { key: 'shipments', labelKey: 'dashboard.my_shipments', icon: Package },
      { key: 'ebills',    labelKey: 'dashboard.my_ebills',    icon: Receipt },
      { key: 'bundles',   labelKey: 'dashboard.my_bundles',   icon: Layers },
      { key: 'payouts',   labelKey: 'dashboard.bank_payouts', icon: Wallet },
    ],
  },
  {
    groupKey: 'dashboard.nav_group_requests',
    items: [
      { key: 'incoming-requests', labelKey: 'dashboard.incoming_requests', icon: Inbox },
      { key: 'my-requests',       labelKey: 'dashboard.my_requests',       icon: Send },
      { key: 'received-offers',   labelKey: 'dashboard.received_offers',   icon: Tag },
      { key: 'sent-offers',       labelKey: 'dashboard.sent_offers',       icon: MessageSquare },
    ],
  },
  {
    groupKey: 'dashboard.nav_group_community',
    items: [
      { key: 'following-feed',  labelKey: 'dashboard.following_feed', icon: Rss },
      { key: 'following',       labelKey: 'dashboard.following',      icon: UserCheck },
      { key: 'followers',       labelKey: 'dashboard.followers',      icon: Users },
      { key: 'saved-searches',  labelKey: 'dashboard.saved_searches', icon: Bookmark },
    ],
  },
];

export default function DashboardPage() {
  const { t } = useTranslation();
  const {
    tab, setTab,
    myItems, likedItems, payments,
    incomingRequests, myRequests,
    receivedOffers, sentOffers,
    shipments, ebills,
    followingFeed, following, followers,
    savedSearches, bundles,
    loading, error,
    removeLikedItem, refreshTab,
  } = useDashboard();

  const [showCreateBundle, setShowCreateBundle] = useState(false);
  const [checkingId, setCheckingId] = useState<string | null>(null);
  const [ratedRequestIds, setRatedRequestIds] = useState<Set<string>>(new Set());
  const [rateModal, setRateModal] = useState<{ requestId: string; sellerName: string | null } | null>(null);
  const [reputationModal, setReputationModal] = useState<{ request: PurchaseRequest; result: BuyerCheckResult } | null>(null);
  const [mobileNavOpen, setMobileNavOpen] = useState(false);

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

  const handleNavSelect = (key: DashboardTab) => {
    setTab(key);
    setMobileNavOpen(false);
  };

  // ── Sidebar nav (shared between desktop sidebar and mobile drawer) ─────────
  const SidebarContent = () => (
    <nav
      role="navigation"
      aria-label={t('dashboard.title')}
      className="flex flex-col gap-1 w-full"
    >
      {NAV_GROUPS.map((group) => (
        <div key={group.groupKey} className="mb-2">
          <p className="px-3 py-1.5 text-[10px] font-semibold uppercase tracking-widest text-mauve/60 dark:text-[#bdb9bc]/40 select-none">
            {t(group.groupKey)}
          </p>
          {group.items.map(({ key, labelKey, icon: Icon }) => {
            const isActive = tab === key;
            return (
              <button
                key={key}
                id={`tab-${key}`}
                role="tab"
                aria-selected={isActive}
                aria-controls={`panel-${key}`}
                aria-current={isActive ? 'page' : undefined}
                onClick={() => handleNavSelect(key)}
                className={[
                  'flex items-center gap-3 w-full px-3 py-2.5 rounded-lg text-sm font-medium text-left',
                  'transition-all duration-200',
                  isActive
                    ? 'bg-primary text-white shadow-sm'
                    : 'text-gray-600 dark:text-[#bdb9bc] hover:bg-lavender/20 dark:hover:bg-[#3a3655]/60 hover:text-primary dark:hover:text-white',
                ].join(' ')}
              >
                <Icon
                  size={16}
                  className={isActive ? 'text-white' : 'text-mauve/70 dark:text-[#bdb9bc]/60'}
                  aria-hidden="true"
                />
                <span>{t(labelKey)}</span>
              </button>
            );
          })}
        </div>
      ))}
    </nav>
  );

  return (
    <div className="max-w-7xl mx-auto px-4 py-8 animate-fade-in">
      {/* ── Modals ─────────────────────────────────────────────────────────── */}
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

      {/* ── Mobile: hamburger toggle bar ───────────────────────────────────── */}
      <div className="md:hidden flex items-center justify-between mb-4 bg-white dark:bg-[#2d2a42] rounded-xl px-4 py-3 border border-lavender/30 dark:border-[#3a3655]">
        <span className="text-sm font-semibold text-primary dark:text-white">
          {t('dashboard.title')}
        </span>
        <button
          onClick={() => setMobileNavOpen(v => !v)}
          aria-label={mobileNavOpen ? 'Close navigation' : 'Open navigation'}
          aria-expanded={mobileNavOpen}
          className="p-1.5 rounded-lg text-gray-600 dark:text-[#bdb9bc] hover:bg-lavender/20 dark:hover:bg-[#3a3655]/60 transition-colors"
        >
          {mobileNavOpen ? <X size={20} aria-hidden="true" /> : <Menu size={20} aria-hidden="true" />}
        </button>
      </div>

      {/* ── Mobile: expanded drawer ────────────────────────────────────────── */}
      {mobileNavOpen && (
        <div className="md:hidden bg-white dark:bg-[#2d2a42] rounded-xl border border-lavender/30 dark:border-[#3a3655] p-3 mb-4">
          <SidebarContent />
        </div>
      )}

      {/* ── Main two-column layout ─────────────────────────────────────────── */}
      <div className="flex gap-6 items-start">

        {/* ── Desktop sidebar ───────────────────────────────────────────────── */}
        <aside className="hidden md:block w-[220px] shrink-0">
          <div className="bg-white dark:bg-[#2d2a42] rounded-xl border border-lavender/30 dark:border-[#3a3655] p-3 sticky top-24">
            <p className="px-3 py-2 mb-1 text-xs font-bold uppercase tracking-widest text-primary dark:text-[#bdb9bc]/70">
              {t('dashboard.title')}
            </p>
            <div className="h-px bg-lavender/30 dark:bg-[#3a3655] mb-2" />
            <SidebarContent />
          </div>
        </aside>

        {/* ── Tab content panel ─────────────────────────────────────────────── */}
        <main className="flex-1 min-w-0">
          {loading ? (
            <LoadingSpinner size="lg" className="py-20" />
          ) : (
            <>
              {tab === 'listings' && (
                <div id="panel-listings" role="tabpanel" aria-labelledby="tab-listings">
                  <MyListingsTab items={myItems} error={error} onRetry={refreshTab} onLikeToggle={handleListingLikeToggle} onBump={handleBump} />
                </div>
              )}
              {tab === 'liked' && (
                <div id="panel-liked" role="tabpanel" aria-labelledby="tab-liked">
                  <FavoritesTab items={likedItems} error={error} onRetry={refreshTab} onLikeToggle={handleLikedTabToggle} />
                </div>
              )}
              {tab === 'purchases' && (
                <div id="panel-purchases" role="tabpanel" aria-labelledby="tab-purchases">
                  <PurchasesTab payments={payments} error={error} onRetry={refreshTab} />
                </div>
              )}
              {tab === 'incoming-requests' && (
                <div id="panel-incoming-requests" role="tabpanel" aria-labelledby="tab-incoming-requests">
                  <IncomingRequestsTab
                    requests={incomingRequests}
                    error={error}
                    onRetry={refreshTab}
                    checkingId={checkingId}
                    onAccept={handleAccept}
                    onDecline={handleDecline}
                  />
                </div>
              )}
              {tab === 'my-requests' && (
                <div id="panel-my-requests" role="tabpanel" aria-labelledby="tab-my-requests">
                  <MyRequestsTab
                    requests={myRequests}
                    error={error}
                    onRetry={refreshTab}
                    ratedRequestIds={ratedRequestIds}
                    onRateSeller={(requestId) => setRateModal({ requestId, sellerName: null })}
                  />
                </div>
              )}
              {tab === 'received-offers' && (
                <div id="panel-received-offers" role="tabpanel" aria-labelledby="tab-received-offers">
                  <ActiveOffersTab
                    offers={receivedOffers}
                    error={error}
                    onRetry={refreshTab}
                    onAccept={handleAcceptOffer}
                    onDecline={handleDeclineOffer}
                    onCounter={handleCounterOffer}
                  />
                </div>
              )}
              {tab === 'sent-offers' && (
                <div id="panel-sent-offers" role="tabpanel" aria-labelledby="tab-sent-offers">
                  <SentOffersTab
                    offers={sentOffers}
                    error={error}
                    onRetry={refreshTab}
                    onAcceptCounter={handleAcceptCounter}
                    onDeclineCounter={handleDeclineCounter}
                    onCancel={handleCancelOffer}
                  />
                </div>
              )}
              {tab === 'shipments' && (
                <div id="panel-shipments" role="tabpanel" aria-labelledby="tab-shipments">
                  <ShipmentsTab shipments={shipments} error={error} onRetry={refreshTab} />
                </div>
              )}
              {tab === 'ebills' && (
                <div id="panel-ebills" role="tabpanel" aria-labelledby="tab-ebills">
                  <EBillsTab ebills={ebills} error={error} onRetry={refreshTab} />
                </div>
              )}
              {tab === 'following-feed' && (
                <div id="panel-following-feed" role="tabpanel" aria-labelledby="tab-following-feed">
                  <FollowingFeedTab items={followingFeed} error={error} onRetry={refreshTab} />
                </div>
              )}
              {tab === 'following' && (
                <div id="panel-following" role="tabpanel" aria-labelledby="tab-following">
                  <FollowingTab
                    users={following}
                    error={error}
                    onRetry={refreshTab}
                    emptyKey="dashboard.following_empty"
                    panelId="panel-following"
                    tabId="tab-following"
                  />
                </div>
              )}
              {tab === 'followers' && (
                <div id="panel-followers" role="tabpanel" aria-labelledby="tab-followers">
                  <FollowingTab
                    users={followers}
                    error={error}
                    onRetry={refreshTab}
                    emptyKey="dashboard.followers_empty"
                    panelId="panel-followers"
                    tabId="tab-followers"
                  />
                </div>
              )}
              {tab === 'saved-searches' && (
                <div id="panel-saved-searches" role="tabpanel" aria-labelledby="tab-saved-searches">
                  <SavedSearchesTab savedSearches={savedSearches} error={error} onRetry={refreshTab} onDelete={handleDeleteSavedSearch} />
                </div>
              )}
              {tab === 'bundles' && (
                <div id="panel-bundles" role="tabpanel" aria-labelledby="tab-bundles">
                  <BundlesTab bundles={bundles} error={error} onRetry={refreshTab} onDelete={handleDeleteBundle} onCreateBundle={() => setShowCreateBundle(true)} />
                </div>
              )}
              {tab === 'payouts' && <BankPayoutsTab />}
            </>
          )}
        </main>
      </div>
    </div>
  );
}
