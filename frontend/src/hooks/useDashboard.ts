import { useState, useEffect, useCallback } from 'react';
import { itemsApi } from '../api/itemsApi';
import { paymentsApi } from '../api/paymentsApi';
import { purchaseRequestsApi } from '../api/purchaseRequestsApi';
import { shippingApi } from '../api/shippingApi';
import type { Item } from '../types/item';
import type { Payment } from '../types/payment';
import type { PurchaseRequest } from '../types/purchaseRequest';
import type { Shipment } from '../types/shipping';

export type DashboardTab = 'listings' | 'liked' | 'purchases' | 'incoming-requests' | 'my-requests' | 'shipments';

interface UseDashboardReturn {
  tab: DashboardTab;
  setTab: (tab: DashboardTab) => void;
  myItems: Item[];
  likedItems: Item[];
  payments: Payment[];
  incomingRequests: PurchaseRequest[];
  myRequests: PurchaseRequest[];
  shipments: Shipment[];
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
  const [shipments, setShipments] = useState<Shipment[]>([]);
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
        } else if (tab === 'shipments') {
          const { data } = await shippingApi.getMyShipments();
          setShipments(data);
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

  return { tab, setTab, myItems, likedItems, payments, incomingRequests, myRequests, shipments, loading, removeLikedItem, refreshTab };
}
