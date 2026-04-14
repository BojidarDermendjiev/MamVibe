import { useEffect, useState } from 'react';
import {
  View,
  Text,
  ScrollView,
  StyleSheet,
  ActivityIndicator,
  TouchableOpacity,
  Alert,
  Linking,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import { shippingApi } from '@/api/shippingApi';
import { formatPrice } from '@/utils/currency';
import {
  CourierProvider,
  ShipmentStatus,
  DeliveryType,
  type Shipment,
  type TrackingEvent,
} from '@mamvibe/shared';
import type { RootStackParamList } from '@/navigation/types';

type Props = NativeStackScreenProps<RootStackParamList, 'ShipmentDetail'>;

const COURIER_NAMES: Record<number, string> = {
  [CourierProvider.Econt]: 'Econt',
  [CourierProvider.Speedy]: 'Speedy',
  [CourierProvider.BoxNow]: 'BoxNow',
};

const STATUS_LABELS: Record<number, string> = {
  [ShipmentStatus.Pending]: 'Pending',
  [ShipmentStatus.Created]: 'Created',
  [ShipmentStatus.PickedUp]: 'Picked Up',
  [ShipmentStatus.InTransit]: 'In Transit',
  [ShipmentStatus.OutForDelivery]: 'Out for Delivery',
  [ShipmentStatus.Delivered]: 'Delivered',
  [ShipmentStatus.Returned]: 'Returned',
  [ShipmentStatus.Cancelled]: 'Cancelled',
};

function statusColor(s: ShipmentStatus): string {
  if (s === ShipmentStatus.Delivered) return '#8eaa89';
  if (s === ShipmentStatus.Cancelled || s === ShipmentStatus.Returned) return '#d4938f';
  if (s === ShipmentStatus.InTransit || s === ShipmentStatus.OutForDelivery) return '#8eaa89';
  return '#c9a870';
}

function InfoRow({ label, value }: { label: string; value: string }) {
  if (!value) return null;
  return (
    <View style={styles.infoRow}>
      <Text style={styles.infoLabel}>{label}</Text>
      <Text style={styles.infoValue}>{value}</Text>
    </View>
  );
}

function TrackingTimeline({ shipmentId }: { shipmentId: string }) {
  const [events, setEvents] = useState<TrackingEvent[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    shippingApi.trackShipment(shipmentId)
      .then(({ data }) => setEvents(data))
      .catch(() => {})
      .finally(() => setLoading(false));
  }, [shipmentId]);

  if (loading) return <ActivityIndicator color="#d4938f" style={{ marginVertical: 16 }} />;
  if (events.length === 0) return <Text style={styles.emptyTracking}>No tracking events yet.</Text>;

  return (
    <View style={styles.timeline}>
      {events.map((ev, i) => (
        <View key={i} style={styles.timelineItem}>
          <View style={styles.timelineDotCol}>
            <View style={[styles.timelineDot, i === 0 && styles.timelineDotActive]} />
            {i < events.length - 1 && <View style={styles.timelineLine} />}
          </View>
          <View style={styles.timelineContent}>
            <Text style={styles.timelineDesc}>{ev.description}</Text>
            {ev.location && <Text style={styles.timelineLoc}>📍 {ev.location}</Text>}
            <Text style={styles.timelineTime}>
              {new Date(ev.timestamp).toLocaleString('en-GB')}
            </Text>
          </View>
        </View>
      ))}
    </View>
  );
}

export default function ShipmentDetailScreen({ route, navigation }: Props) {
  const { shipmentId } = route.params;
  const [shipment, setShipment] = useState<Shipment | null>(null);
  const [loading, setLoading] = useState(true);
  const [cancelling, setCancelling] = useState(false);

  useEffect(() => {
    shippingApi.getMyShipments()
      .then(({ data }) => {
        const found = data.find((s) => s.id === shipmentId) ?? null;
        setShipment(found);
      })
      .catch(() => Alert.alert('Error', 'Could not load shipment'))
      .finally(() => setLoading(false));
  }, [shipmentId]);

  const handleCancel = () => {
    Alert.alert('Cancel Shipment', 'Are you sure you want to cancel this shipment?', [
      { text: 'No', style: 'cancel' },
      {
        text: 'Yes, Cancel',
        style: 'destructive',
        onPress: async () => {
          setCancelling(true);
          try {
            await shippingApi.cancelShipment(shipmentId);
            setShipment((prev) => prev ? { ...prev, status: ShipmentStatus.Cancelled } : null);
          } catch {
            Alert.alert('Error', 'Could not cancel shipment.');
          } finally {
            setCancelling(false);
          }
        },
      },
    ]);
  };

  const handleDownloadLabel = () => {
    const url = shippingApi.getLabelUrl(shipmentId);
    Linking.openURL(url).catch(() => Alert.alert('Error', 'Could not open label URL.'));
  };

  if (loading) {
    return <View style={styles.center}><ActivityIndicator size="large" color="#d4938f" /></View>;
  }

  if (!shipment) {
    return (
      <View style={styles.center}>
        <Text style={styles.notFound}>Shipment not found.</Text>
        <TouchableOpacity onPress={() => navigation.goBack()}>
          <Text style={styles.backLink}>← Go Back</Text>
        </TouchableOpacity>
      </View>
    );
  }

  const canCancel = shipment.status === ShipmentStatus.Pending || shipment.status === ShipmentStatus.Created;

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <ScrollView contentContainerStyle={styles.scroll}>

        {/* Status banner */}
        <View style={[styles.statusBanner, { backgroundColor: statusColor(shipment.status) }]}>
          <Text style={styles.statusBannerText}>{STATUS_LABELS[shipment.status] ?? String(shipment.status)}</Text>
          {shipment.trackingNumber && (
            <Text style={styles.trackingNum}>#{shipment.trackingNumber}</Text>
          )}
        </View>

        {/* Details */}
        <View style={styles.card}>
          {shipment.itemTitle && <InfoRow label="Item" value={shipment.itemTitle} />}
          <InfoRow label="Courier" value={COURIER_NAMES[shipment.courierProvider] ?? String(shipment.courierProvider)} />
          <InfoRow label="Recipient" value={shipment.recipientName} />
          <InfoRow label="Phone" value={shipment.recipientPhone} />
          {shipment.deliveryAddress
            ? <InfoRow label="Address" value={`${shipment.deliveryAddress}${shipment.city ? ', ' + shipment.city : ''}`} />
            : null}
          {shipment.officeName && <InfoRow label="Office" value={shipment.officeName} />}
          <InfoRow label="Shipping Price" value={formatPrice(shipment.shippingPrice)} />
          <InfoRow label="Weight" value={`${shipment.weight} kg`} />
          {shipment.isCod && <InfoRow label="Cash on Delivery" value={formatPrice(shipment.codAmount)} />}
          {shipment.isInsured && <InfoRow label="Insurance" value={formatPrice(shipment.insuredAmount)} />}
        </View>

        {/* Actions */}
        <View style={styles.actions}>
          <TouchableOpacity style={styles.btnSecondary} onPress={handleDownloadLabel}>
            <Text style={styles.btnSecondaryText}>📄 Download Label</Text>
          </TouchableOpacity>
          {canCancel && (
            <TouchableOpacity
              style={[styles.btnDanger, cancelling && styles.btnDisabled]}
              onPress={handleCancel}
              disabled={cancelling}
            >
              {cancelling
                ? <ActivityIndicator color="#fff" />
                : <Text style={styles.btnDangerText}>Cancel Shipment</Text>}
            </TouchableOpacity>
          )}
        </View>

        {/* Tracking timeline */}
        {shipment.trackingNumber && (
          <>
            <Text style={styles.sectionLabel}>Tracking Events</Text>
            <View style={styles.card}>
              <TrackingTimeline shipmentId={shipment.id} />
            </View>
          </>
        )}
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: '#fafafa' },
  center: { flex: 1, alignItems: 'center', justifyContent: 'center', gap: 12 },
  notFound: { fontSize: 16, color: '#888' },
  backLink: { color: '#d4938f', fontSize: 15 },
  scroll: { padding: 16, gap: 12, paddingBottom: 40 },

  statusBanner: {
    borderRadius: 14,
    padding: 18,
    alignItems: 'center',
    gap: 4,
  },
  statusBannerText: { color: '#fff', fontSize: 18, fontWeight: '800', letterSpacing: 0.5 },
  trackingNum: { color: 'rgba(255,255,255,0.85)', fontSize: 13, fontWeight: '500' },

  card: {
    backgroundColor: '#fff',
    borderRadius: 14,
    padding: 16,
    gap: 2,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 4,
    elevation: 2,
  },
  infoRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    paddingVertical: 8,
    borderBottomWidth: StyleSheet.hairlineWidth,
    borderBottomColor: '#f0f0f0',
  },
  infoLabel: { fontSize: 13, color: '#888', flex: 1 },
  infoValue: { fontSize: 13, color: '#1a1a1a', fontWeight: '600', flex: 2, textAlign: 'right' },

  sectionLabel: {
    fontSize: 12,
    fontWeight: '700',
    color: '#555',
    textTransform: 'uppercase',
    letterSpacing: 0.6,
    marginTop: 4,
  },

  actions: { flexDirection: 'row', gap: 10 },
  btnSecondary: {
    flex: 1, height: 46, borderRadius: 12, borderWidth: 1.5,
    borderColor: '#d4938f', alignItems: 'center', justifyContent: 'center',
  },
  btnSecondaryText: { color: '#d4938f', fontSize: 14, fontWeight: '600' },
  btnDanger: {
    flex: 1, height: 46, borderRadius: 12,
    backgroundColor: '#d4938f', alignItems: 'center', justifyContent: 'center',
  },
  btnDangerText: { color: '#fff', fontSize: 14, fontWeight: '600' },
  btnDisabled: { opacity: 0.5 },

  // Timeline
  emptyTracking: { color: '#aaa', fontSize: 14, textAlign: 'center', paddingVertical: 12 },
  timeline: { gap: 0 },
  timelineItem: { flexDirection: 'row', gap: 12, paddingBottom: 16 },
  timelineDotCol: { alignItems: 'center', width: 16 },
  timelineDot: {
    width: 14, height: 14, borderRadius: 7,
    backgroundColor: '#ddd', borderWidth: 2, borderColor: '#fff',
    shadowColor: '#000', shadowOffset: { width: 0, height: 1 }, shadowOpacity: 0.15, shadowRadius: 2,
    elevation: 2,
  },
  timelineDotActive: { backgroundColor: '#d4938f' },
  timelineLine: { width: 2, flex: 1, backgroundColor: '#f0f0f0', marginTop: 4 },
  timelineContent: { flex: 1, paddingBottom: 4 },
  timelineDesc: { fontSize: 14, color: '#1a1a1a', fontWeight: '500' },
  timelineLoc: { fontSize: 12, color: '#888', marginTop: 2 },
  timelineTime: { fontSize: 11, color: '#bbb', marginTop: 3 },
});
