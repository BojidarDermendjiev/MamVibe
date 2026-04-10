import { useState, useEffect, useCallback } from 'react';
import { itemsApi } from '@/api/itemsApi';
import { paymentsApi } from '@/api/paymentsApi';
import { purchaseRequestsApi } from '@/api/purchaseRequestsApi';
import { shippingApi } from '@/api/shippingApi';
import { ebillsApi } from '@/api/ebillsApi';
import type { Item, Payment, PurchaseRequest, Shipment, EBill } from '@mamvibe/shared';

export type DashboardTab = 'my-requests' | 'incoming' | 'shipments' | 'ebills' | 'listings' | 'purchases';

interface DashboardData {
  myRequests: PurchaseRequest[];
  incomingRequests: PurchaseRequest[];
  shipments: Shipment[];
  ebills: EBill[];
  myItems: Item[];
  purchases: Payment[];
}

const EMPTY: DashboardData = {
  myRequests: [], incomingRequests: [], shipments: [], ebills: [], myItems: [], purchases: [],
};

export function useDashboard() {
  const [tab, setTab] = useState<DashboardTab>('my-requests');
  const [data, setData] = useState<DashboardData>(EMPTY);
  const [loading, setLoading] = useState(false);
  const [refreshKey, setRefreshKey] = useState(0);

  const refresh = useCallback(() => setRefreshKey((k) => k + 1), []);

  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      setLoading(true);
      try {
        let patch: Partial<DashboardData> = {};
        if (tab === 'my-requests') {
          const { data: d } = await purchaseRequestsApi.getAsBuyer();
          patch = { myRequests: d };
        } else if (tab === 'incoming') {
          const { data: d } = await purchaseRequestsApi.getAsSeller();
          patch = { incomingRequests: d };
        } else if (tab === 'shipments') {
          const { data: d } = await shippingApi.getMyShipments();
          patch = { shipments: d };
        } else if (tab === 'ebills') {
          const { data: d } = await ebillsApi.getMyEBills();
          patch = { ebills: d };
        } else if (tab === 'listings') {
          const { data: d } = await itemsApi.getMyItems();
          patch = { myItems: d };
        } else if (tab === 'purchases') {
          const { data: d } = await paymentsApi.getMyPayments();
          patch = { purchases: d };
        }
        if (!cancelled) setData((prev) => ({ ...prev, ...patch }));
      } catch {
        // silent — stale data stays
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    load();
    return () => { cancelled = true; };
  }, [tab, refreshKey]);

  return { tab, setTab, data, loading, refresh };
}
