import { useEffect, useState } from 'react';
import {
  View,
  Text,
  ScrollView,
  FlatList,
  StyleSheet,
  ActivityIndicator,
  TouchableOpacity,
  Alert,
  Linking,
  RefreshControl,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import {
  PurchaseRequestStatus,
  ShipmentStatus,
  CourierProvider,
  ListingType,
  PaymentMethod,
  type PurchaseRequest,
  type Shipment,
  type EBill,
} from '@mamvibe/shared';
import { useDashboard, type DashboardTab } from '@/hooks/useDashboard';
import { useSignalR } from '@/contexts/SignalRContext';
import { purchaseRequestsApi } from '@/api/purchaseRequestsApi';
import { formatPrice } from '@/utils/currency';
import type { RootStackParamList } from '@/navigation/types';

type Props = NativeStackScreenProps<RootStackParamList, 'Dashboard'>;

const TABS: { key: DashboardTab; label: string }[] = [
  { key: 'my-requests', label: 'My Requests' },
  { key: 'incoming', label: 'Incoming' },
  { key: 'shipments', label: 'Shipments' },
  { key: 'ebills', label: 'E-Bills' },
];

const COURIER_NAMES: Record<number, string> = {
  [CourierProvider.Econt]: 'Econt',
  [CourierProvider.Speedy]: 'Speedy',
  [CourierProvider.BoxNow]: 'BoxNow',
};

const SHIPMENT_STATUS_LABELS: Record<number, string> = {
  [ShipmentStatus.Pending]: 'Pending',
  [ShipmentStatus.Created]: 'Created',
  [ShipmentStatus.PickedUp]: 'Picked Up',
  [ShipmentStatus.InTransit]: 'In Transit',
  [ShipmentStatus.OutForDelivery]: 'Out for Delivery',
  [ShipmentStatus.Delivered]: 'Delivered',
  [ShipmentStatus.Returned]: 'Returned',
  [ShipmentStatus.Cancelled]: 'Cancelled',
};

const PR_STATUS_LABELS: Record<number, string> = {
  [PurchaseRequestStatus.Pending]: 'Pending',
  [PurchaseRequestStatus.Accepted]: 'Accepted',
  [PurchaseRequestStatus.Declined]: 'Declined',
  [PurchaseRequestStatus.Cancelled]: 'Cancelled',
  [PurchaseRequestStatus.Completed]: 'Completed',
};

const PAYMENT_METHOD_LABELS: Record<number, string> = {
  [PaymentMethod.Card]: 'Card',
  [PaymentMethod.OnSpot]: 'On Spot',
  [PaymentMethod.Booking]: 'Booking',
  [PaymentMethod.Wallet]: 'Wallet',
};

function prStatusColor(s: number): string {
  if (s === PurchaseRequestStatus.Accepted) return '#8eaa89';
  if (s === PurchaseRequestStatus.Declined || s === PurchaseRequestStatus.Cancelled) return '#d4938f';
  if (s === PurchaseRequestStatus.Completed) return '#8eaa89';
  return '#c9a870';
}

function shipmentStatusColor(s: number): string {
  if (s === ShipmentStatus.Delivered) return '#8eaa89';
  if (s === ShipmentStatus.Cancelled || s === ShipmentStatus.Returned) return '#d4938f';
  if (s === ShipmentStatus.InTransit || s === ShipmentStatus.OutForDelivery) return '#8eaa89';
  return '#c9a870';
}

function StatusBadge({ label, color }: { label: string; color: string }) {
  return (
    <View style={[styles.badge, { backgroundColor: color + '18' }]}>
      <Text style={[styles.badgeText, { color }]}>{label}</Text>
    </View>
  );
}

function MyRequestCard({
  req,
  onPayNow,
}: {
  req: PurchaseRequest;
  onPayNow: (itemId: string) => void;
}) {
  const isDonate = req.listingType === ListingType.Donate;
  const canPay = req.status === PurchaseRequestStatus.Accepted && !isDonate;

  return (
    <View style={styles.card}>
      <View style={styles.cardHeader}>
        <Text style={styles.cardTitle} numberOfLines={1}>{req.itemTitle ?? 'Item'}</Text>
        <StatusBadge
          label={PR_STATUS_LABELS[req.status] ?? String(req.status)}
          color={prStatusColor(req.status)}
        />
      </View>
      {req.price != null && (
        <Text style={styles.cardMeta}>Price: {formatPrice(req.price)}</Text>
      )}
      {isDonate && <Text style={styles.cardMeta}>Donation</Text>}
      <Text style={styles.cardDate}>{new Date(req.createdAt).toLocaleDateString('en-GB')}</Text>
      {canPay && (
        <TouchableOpacity style={styles.btnPrimary} onPress={() => onPayNow(req.itemId)}>
          <Text style={styles.btnPrimaryText}>Pay Now</Text>
        </TouchableOpacity>
      )}
      {req.status === PurchaseRequestStatus.Accepted && isDonate && (
        <View style={styles.confirmedRow}>
          <Text style={styles.confirmedText}>✅ Booking Confirmed</Text>
        </View>
      )}
    </View>
  );
}

function IncomingRequestCard({
  req,
  onAccept,
  onDecline,
  onViewShipment,
  actionLoading,
}: {
  req: PurchaseRequest;
  onAccept: (id: string) => void;
  onDecline: (id: string) => void;
  onViewShipment: (shipmentId: string) => void;
  actionLoading: string | null;
}) {
  const isPending = req.status === PurchaseRequestStatus.Pending;
  const isLoading = actionLoading === req.id;

  return (
    <View style={styles.card}>
      <View style={styles.cardHeader}>
        <Text style={styles.cardTitle} numberOfLines={1}>{req.itemTitle ?? 'Item'}</Text>
        <StatusBadge
          label={PR_STATUS_LABELS[req.status] ?? String(req.status)}
          color={prStatusColor(req.status)}
        />
      </View>
      <Text style={styles.cardMeta}>From: {req.buyerDisplayName ?? 'Buyer'}</Text>
      {req.price != null && <Text style={styles.cardMeta}>Price: {formatPrice(req.price)}</Text>}
      <Text style={styles.cardDate}>{new Date(req.createdAt).toLocaleDateString('en-GB')}</Text>

      {isPending && (
        <View style={styles.actionRow}>
          <TouchableOpacity
            style={[styles.btnAccept, isLoading && styles.btnDisabled]}
            disabled={isLoading}
            onPress={() => onAccept(req.id)}
          >
            {isLoading
              ? <ActivityIndicator color="#fff" size="small" />
              : <Text style={styles.btnAcceptText}>Accept</Text>}
          </TouchableOpacity>
          <TouchableOpacity
            style={[styles.btnDecline, isLoading && styles.btnDisabled]}
            disabled={isLoading}
            onPress={() => onDecline(req.id)}
          >
            <Text style={styles.btnDeclineText}>Decline</Text>
          </TouchableOpacity>
        </View>
      )}

      {req.shipmentId && req.status === PurchaseRequestStatus.Completed && (
        <TouchableOpacity style={styles.btnSecondary} onPress={() => onViewShipment(req.shipmentId!)}>
          <Text style={styles.btnSecondaryText}>📦 View Shipment</Text>
        </TouchableOpacity>
      )}
    </View>
  );
}

function ShipmentCard({ shipment, onPress }: { shipment: Shipment; onPress: () => void }) {
  return (
    <TouchableOpacity style={styles.card} onPress={onPress} activeOpacity={0.75}>
      <View style={styles.cardHeader}>
        <Text style={styles.cardTitle} numberOfLines={1}>{shipment.itemTitle ?? 'Shipment'}</Text>
        <StatusBadge
          label={SHIPMENT_STATUS_LABELS[shipment.status] ?? String(shipment.status)}
          color={shipmentStatusColor(shipment.status)}
        />
      </View>
      <Text style={styles.cardMeta}>{COURIER_NAMES[shipment.courierProvider]} · {formatPrice(shipment.shippingPrice)}</Text>
      {shipment.trackingNumber && (
        <Text style={styles.cardMeta}>#{shipment.trackingNumber}</Text>
      )}
      <Text style={styles.cardDate}>{new Date(shipment.createdAt).toLocaleDateString('en-GB')}</Text>
    </TouchableOpacity>
  );
}

function EBillCard({ bill }: { bill: EBill }) {
  return (
    <View style={styles.card}>
      <View style={styles.cardHeader}>
        <Text style={styles.cardTitle} numberOfLines={1}>{bill.itemTitle ?? 'Purchase'}</Text>
        <Text style={styles.billAmount}>{formatPrice(bill.amount)}</Text>
      </View>
      {bill.eBillNumber && <Text style={styles.cardMeta}>#{bill.eBillNumber}</Text>}
      <Text style={styles.cardMeta}>Seller: {bill.sellerDisplayName ?? '—'}</Text>
      <Text style={styles.cardMeta}>Method: {PAYMENT_METHOD_LABELS[bill.paymentMethod]}</Text>
      <Text style={styles.cardDate}>{new Date(bill.issuedAt).toLocaleDateString('en-GB')}</Text>
      {bill.receiptUrl && (
        <TouchableOpacity
          style={styles.btnSecondary}
          onPress={() => Linking.openURL(bill.receiptUrl!).catch(() => Alert.alert('Error', 'Could not open receipt.'))}
        >
          <Text style={styles.btnSecondaryText}>📄 Open Receipt</Text>
        </TouchableOpacity>
      )}
    </View>
  );
}

function EmptyState({ message }: { message: string }) {
  return (
    <View style={styles.empty}>
      <Text style={styles.emptyText}>{message}</Text>
    </View>
  );
}

export default function DashboardScreen({ navigation }: Props) {
  const { tab, setTab, data, loading, refresh } = useDashboard();
  const { onPurchaseRequest, onPurchaseRequestUpdated, onShipmentStatusChanged } = useSignalR();
  const [actionLoading, setActionLoading] = useState<string | null>(null);

  useEffect(() => {
    const unsub1 = onPurchaseRequest(() => {
      if (tab === 'incoming') refresh();
    });
    const unsub2 = onPurchaseRequestUpdated(() => {
      if (tab === 'my-requests') refresh();
    });
    const unsub3 = onShipmentStatusChanged(() => {
      if (tab === 'shipments') refresh();
    });
    return () => { unsub1(); unsub2(); unsub3(); };
  }, [tab, refresh, onPurchaseRequest, onPurchaseRequestUpdated, onShipmentStatusChanged]);

  const handleAccept = (id: string) => {
    Alert.alert('Accept Request', 'Accept this purchase request?', [
      { text: 'Cancel', style: 'cancel' },
      {
        text: 'Accept',
        onPress: async () => {
          setActionLoading(id);
          try {
            await purchaseRequestsApi.accept(id);
            refresh();
          } catch {
            Alert.alert('Error', 'Could not accept request.');
          } finally {
            setActionLoading(null);
          }
        },
      },
    ]);
  };

  const handleDecline = (id: string) => {
    Alert.alert('Decline Request', 'Decline this purchase request?', [
      { text: 'Cancel', style: 'cancel' },
      {
        text: 'Decline',
        style: 'destructive',
        onPress: async () => {
          setActionLoading(id);
          try {
            await purchaseRequestsApi.decline(id);
            refresh();
          } catch {
            Alert.alert('Error', 'Could not decline request.');
          } finally {
            setActionLoading(null);
          }
        },
      },
    ]);
  };

  const refreshControl = (
    <RefreshControl refreshing={loading} onRefresh={refresh} tintColor="#d4938f" />
  );

  const renderContent = () => {
    if (loading && !data.myRequests.length && !data.incomingRequests.length &&
        !data.shipments.length && !data.ebills.length) {
      return <ActivityIndicator color="#d4938f" style={{ marginTop: 40 }} />;
    }

    if (tab === 'my-requests') {
      if (!data.myRequests.length) return <EmptyState message="No purchase requests yet." />;
      return (
        <FlatList
          data={data.myRequests}
          keyExtractor={(r) => r.id}
          renderItem={({ item }) => (
            <MyRequestCard
              req={item}
              onPayNow={(itemId) => (navigation as any).navigate('Payment', { itemId })}
            />
          )}
          contentContainerStyle={styles.list}
          refreshControl={refreshControl}
        />
      );
    }

    if (tab === 'incoming') {
      if (!data.incomingRequests.length) return <EmptyState message="No incoming requests." />;
      return (
        <FlatList
          data={data.incomingRequests}
          keyExtractor={(r) => r.id}
          renderItem={({ item }) => (
            <IncomingRequestCard
              req={item}
              onAccept={handleAccept}
              onDecline={handleDecline}
              onViewShipment={(shipmentId) => navigation.navigate('ShipmentDetail', { shipmentId })}
              actionLoading={actionLoading}
            />
          )}
          contentContainerStyle={styles.list}
          refreshControl={refreshControl}
        />
      );
    }

    if (tab === 'shipments') {
      if (!data.shipments.length) return <EmptyState message="No shipments yet." />;
      return (
        <FlatList
          data={data.shipments}
          keyExtractor={(s) => s.id}
          renderItem={({ item }) => (
            <ShipmentCard
              shipment={item}
              onPress={() => navigation.navigate('ShipmentDetail', { shipmentId: item.id })}
            />
          )}
          contentContainerStyle={styles.list}
          refreshControl={refreshControl}
        />
      );
    }

    if (tab === 'ebills') {
      if (!data.ebills.length) return <EmptyState message="No e-bills yet." />;
      return (
        <FlatList
          data={data.ebills}
          keyExtractor={(b) => b.id}
          renderItem={({ item }) => <EBillCard bill={item} />}
          contentContainerStyle={styles.list}
          refreshControl={refreshControl}
        />
      );
    }

    return null;
  };

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      {/* Tab bar */}
      <ScrollView
        horizontal
        showsHorizontalScrollIndicator={false}
        style={styles.tabBar}
        contentContainerStyle={styles.tabBarContent}
      >
        {TABS.map((t) => (
          <TouchableOpacity
            key={t.key}
            style={[styles.tabItem, tab === t.key && styles.tabItemActive]}
            onPress={() => setTab(t.key)}
          >
            <Text style={[styles.tabLabel, tab === t.key && styles.tabLabelActive]}>{t.label}</Text>
          </TouchableOpacity>
        ))}
      </ScrollView>

      {/* Content */}
      <View style={styles.content}>
        {renderContent()}
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: '#fafafa' },
  content: { flex: 1 },

  tabBar: { backgroundColor: '#fff', borderBottomWidth: StyleSheet.hairlineWidth, borderBottomColor: '#f5ede5', flexGrow: 0 },
  tabBarContent: { paddingHorizontal: 12, paddingVertical: 4 },
  tabItem: { paddingHorizontal: 16, paddingVertical: 10, marginHorizontal: 2, borderRadius: 20 },
  tabItemActive: { backgroundColor: 'rgba(245,237,229,0.8)' },
  tabLabel: { fontSize: 14, color: '#8eaa89', fontWeight: '500' },
  tabLabelActive: { color: '#d4938f', fontWeight: '700' },

  list: { padding: 16, gap: 12, paddingBottom: 32 },

  card: {
    backgroundColor: '#fff',
    borderRadius: 14,
    padding: 16,
    gap: 6,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 4,
    elevation: 2,
  },
  cardHeader: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', gap: 8 },
  cardTitle: { flex: 1, fontSize: 15, fontWeight: '700', color: '#1a1a1a' },
  cardMeta: { fontSize: 13, color: '#666' },
  cardDate: { fontSize: 12, color: '#bbb' },

  badge: { borderRadius: 8, paddingHorizontal: 8, paddingVertical: 3 },
  badgeText: { fontSize: 11, fontWeight: '700' },

  billAmount: { fontSize: 16, fontWeight: '800', color: '#1a1a1a' },

  actionRow: { flexDirection: 'row', gap: 10, marginTop: 4 },
  btnAccept: {
    flex: 1, height: 40, borderRadius: 10,
    backgroundColor: '#8eaa89', alignItems: 'center', justifyContent: 'center',
  },
  btnAcceptText: { color: '#fff', fontSize: 14, fontWeight: '600' },
  btnDecline: {
    flex: 1, height: 40, borderRadius: 10,
    borderWidth: 1.5, borderColor: '#d4938f', alignItems: 'center', justifyContent: 'center',
  },
  btnDeclineText: { color: '#d4938f', fontSize: 14, fontWeight: '600' },
  btnDisabled: { opacity: 0.5 },

  btnPrimary: {
    height: 42, borderRadius: 10,
    backgroundColor: '#d4938f', alignItems: 'center', justifyContent: 'center', marginTop: 4,
  },
  btnPrimaryText: { color: '#fff', fontSize: 14, fontWeight: '600' },

  btnSecondary: {
    height: 40, borderRadius: 10, borderWidth: 1.5,
    borderColor: '#d4938f', alignItems: 'center', justifyContent: 'center', marginTop: 4,
  },
  btnSecondaryText: { color: '#d4938f', fontSize: 14, fontWeight: '600' },

  confirmedRow: { marginTop: 4 },
  confirmedText: { fontSize: 13, color: '#8eaa89', fontWeight: '600' },

  empty: { flex: 1, alignItems: 'center', justifyContent: 'center', paddingTop: 80 },
  emptyText: { fontSize: 15, color: '#aaa' },
});
