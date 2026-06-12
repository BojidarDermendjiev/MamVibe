import { useState, useEffect, useCallback } from 'react';
import { itemsApi } from '../api/itemsApi';
import { paymentsApi } from '../api/paymentsApi';
import { purchaseRequestsApi } from '../api/purchaseRequestsApi';
import { offersApi } from '../api/offersApi';
import { followsApi } from '../api/followsApi';
import { savedSearchesApi } from '../api/savedSearchesApi';
import { shippingApi } from '../api/shippingApi';
import { ebillsApi } from '../api/ebillsApi';
import { bundlesApi } from '../api/bundlesApi';
import type { Item } from '../types/item';
import type { Payment } from '../types/payment';
import type { PurchaseRequest } from '../types/purchaseRequest';
import type { Offer } from '../types/offer';
import type { Shipment } from '../types/shipping';
import type { EBill } from '../types/ebill';
import type { FollowUserDto } from '../types/follow';
import type { SavedSearchDto } from '../types/savedSearch';
import type { BundleDto } from '../types/bundle';

export type DashboardTab = 'listings' | 'liked' | 'purchases' | 'incoming-requests' | 'my-requests' | 'received-offers' | 'sent-offers' | 'shipments' | 'ebills' | 'following-feed' | 'following' | 'followers' | 'saved-searches' | 'bundles' | 'payouts';

interface UseDashboardReturn {
  tab: DashboardTab;
  setTab: (tab: DashboardTab) => void;
  myItems: Item[];
  likedItems: Item[];
  payments: Payment[];
  incomingRequests: PurchaseRequest[];
  myRequests: PurchaseRequest[];
  receivedOffers: Offer[];
  sentOffers: Offer[];
  shipments: Shipment[];
  ebills: EBill[];
  followingFeed: Item[];
  following: FollowUserDto[];
  followers: FollowUserDto[];
  savedSearches: SavedSearchDto[];
  bundles: BundleDto[];
  loading: boolean;
  error: string | null;
  removeLikedItem: (id: string) => void;
  refreshTab: () => void;
}

// Initial tab selection — honour explicit deep links so we can route users
// returning from Stripe Connect onboarding straight to the payouts panel,
// and the legacy "#bank-payouts" hash used by the connect_required nudge.
function pickInitialTab(): DashboardTab {
  if (typeof window === 'undefined') return 'listings';
  if (window.location.hash === '#bank-payouts') return 'payouts';
  const params = new URLSearchParams(window.location.search);
  if (params.get('connect') === 'return' || params.get('connect') === 'refresh') return 'payouts';
  return 'listings';
}

export function useDashboard(): UseDashboardReturn {
  const [tab, setTab] = useState<DashboardTab>(pickInitialTab);
  const [myItems, setMyItems] = useState<Item[]>([]);
  const [likedItems, setLikedItems] = useState<Item[]>([]);
  const [payments, setPayments] = useState<Payment[]>([]);
  const [incomingRequests, setIncomingRequests] = useState<PurchaseRequest[]>([]);
  const [myRequests, setMyRequests] = useState<PurchaseRequest[]>([]);
  const [receivedOffers, setReceivedOffers] = useState<Offer[]>([]);
  const [sentOffers, setSentOffers] = useState<Offer[]>([]);
  const [shipments, setShipments] = useState<Shipment[]>([]);
  const [ebills, setEBills] = useState<EBill[]>([]);
  const [followingFeed, setFollowingFeed] = useState<Item[]>([]);
  const [following, setFollowing] = useState<FollowUserDto[]>([]);
  const [followers, setFollowers] = useState<FollowUserDto[]>([]);
  const [savedSearches, setSavedSearches] = useState<SavedSearchDto[]>([]);
  const [bundles, setBundles] = useState<BundleDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [refreshKey, setRefreshKey] = useState(0);

  const refreshTab = useCallback(() => {
    setRefreshKey((k) => k + 1);
  }, []);

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      setError(null);
      try {
        if (tab === 'listings') {
          const { data } = await itemsApi.getMyItems();
          setMyItems(data);
        } else if (tab === 'liked') {
          const { data } = await itemsApi.getLikedItems();
          setLikedItems(data);
        } else if (tab === 'purchases') {
          const { data } = await paymentsApi.getMyPayments();
          setPayments(data.items ?? []);
        } else if (tab === 'incoming-requests') {
          const { data } = await purchaseRequestsApi.getAsSeller();
          setIncomingRequests(data);
        } else if (tab === 'my-requests') {
          const { data } = await purchaseRequestsApi.getAsBuyer();
          setMyRequests(data);
        } else if (tab === 'received-offers') {
          const { data } = await offersApi.getReceived();
          setReceivedOffers(data);
        } else if (tab === 'sent-offers') {
          const { data } = await offersApi.getSent();
          setSentOffers(data);
        } else if (tab === 'shipments') {
          const { data } = await shippingApi.getMyShipments();
          setShipments(data);
        } else if (tab === 'ebills') {
          const { data } = await ebillsApi.getMyEBills();
          setEBills(data);
        } else if (tab === 'following-feed') {
          const { data } = await followsApi.getFeed(1, 24);
          setFollowingFeed(data.items ?? []);
        } else if (tab === 'following') {
          const { data } = await followsApi.getFollowing();
          setFollowing(data);
        } else if (tab === 'followers') {
          const { data } = await followsApi.getFollowers();
          setFollowers(data);
        } else if (tab === 'saved-searches') {
          const { data } = await savedSearchesApi.getMy();
          setSavedSearches(data);
        } else if (tab === 'bundles') {
          const [bundlesRes, itemsRes] = await Promise.all([bundlesApi.getMy(), itemsApi.getMyItems()]);
          setBundles(bundlesRes.data);
          setMyItems(itemsRes.data);
        }
        // 'payouts' tab — no parent-level data load; the BankPayoutsTab component
        // manages its own Stripe Connect status fetch on mount.
      } catch {
        setError('load_failed');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [tab, refreshKey]);

  const removeLikedItem = (id: string) => {
    setLikedItems((prev) => prev.filter((item) => item.id !== id));
  };

  return { tab, setTab, myItems, likedItems, payments, incomingRequests, myRequests, receivedOffers, sentOffers, shipments, ebills, followingFeed, following, followers, savedSearches, bundles, loading, error, removeLikedItem, refreshTab };
}
