import { useState, useEffect, useCallback } from 'react';
import { itemsApi } from '../api/itemsApi';
import { paymentsApi } from '../api/paymentsApi';
import { purchaseRequestsApi } from '../api/purchaseRequestsApi';
import type { Item } from '../types/item';
import type { Payment } from '../types/payment';
import type { PurchaseRequest } from '../types/purchaseRequest';

export type DashboardTab = 'listings' | 'liked' | 'purchases' | 'incoming-requests' | 'my-requests';

interface UseDashboardReturn {
  tab: DashboardTab;
  setTab: (tab: DashboardTab) => void;
  myItems: Item[];
  likedItems: Item[];
  payments: Payment[];
  incomingRequests: PurchaseRequest[];
  myRequests: PurchaseRequest[];
  loading: boolean;
  removeLikedItem: (id: string) => void;
  refreshTab: () => void;
}

export function useDashboard(): UseDashboardReturn {
  const [tab, setTab] = useState<DashboardTab>('listings');
  const [myItems, setMyItems] = useState<Item[]>([]);
  const [likedItems, setLikedItems] = useState<Item[]>([]);
  const [payments, setPayments] = useState<Payment[]>([]);
  const [incomingRequests, setIncomingRequests] = useState<PurchaseRequest[]>([]);
  const [myRequests, setMyRequests] = useState<PurchaseRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshKey, setRefreshKey] = useState(0);

  const refreshTab = useCallback(() => {
    setRefreshKey((k) => k + 1);
  }, []);

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      try {
        if (tab === 'listings') {
          const { data } = await itemsApi.getMyItems();
          setMyItems(data);
        } else if (tab === 'liked') {
          const { data } = await itemsApi.getLikedItems();
          setLikedItems(data);
        } else if (tab === 'purchases') {
          const { data } = await paymentsApi.getMyPayments();
          setPayments(data);
        } else if (tab === 'incoming-requests') {
          const { data } = await purchaseRequestsApi.getAsSeller();
          setIncomingRequests(data);
        } else if (tab === 'my-requests') {
          const { data } = await purchaseRequestsApi.getAsBuyer();
          setMyRequests(data);
        }
      } catch {
        /* ignore */
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [tab, refreshKey]);

  const removeLikedItem = (id: string) => {
    setLikedItems((prev) => prev.filter((item) => item.id !== id));
  };

  return { tab, setTab, myItems, likedItems, payments, incomingRequests, myRequests, loading, removeLikedItem, refreshTab };
}
