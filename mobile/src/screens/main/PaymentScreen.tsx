import { useEffect, useState } from 'react';
import {
  View,
  Text,
  ScrollView,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
  Alert,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import { useStripe } from '@stripe/stripe-react-native';
import { itemsApi } from '@/api/itemsApi';
import { paymentsApi } from '@/api/paymentsApi';
import { walletApi } from '@/api/walletApi';
import { formatPrice, formatEur } from '@/utils/currency';
import type { Item } from '@mamvibe/shared';
import { ListingType, CourierProvider, DeliveryType } from '@mamvibe/shared';
import type { PaymentDeliveryRequest } from '@mamvibe/shared';
import type { RootStackParamList } from '@/navigation/types';

type Props = NativeStackScreenProps<RootStackParamList, 'Payment'>;
type PayMethod = 'card' | 'onspot' | 'wallet';

const COURIERS = [
  { label: 'Econt', value: CourierProvider.Econt },
  { label: 'Speedy', value: CourierProvider.Speedy },
  { label: 'BoxNow', value: CourierProvider.BoxNow },
] as const;

const DELIVERY_TYPES = [
  { label: 'Office / Locker', value: DeliveryType.Office },
  { label: 'Home Address', value: DeliveryType.Address },
] as const;

function ChipRow<T extends number>({
  options,
  value,
  onChange,
}: {
  options: readonly { label: string; value: T }[];
  value: T;
  onChange: (v: T) => void;
}) {
  return (
    <View style={styles.chipRow}>
      {options.map((o) => (
        <TouchableOpacity
          key={o.value}
          style={[styles.chip, value === o.value && styles.chipActive]}
          onPress={() => onChange(o.value)}
        >
          <Text style={[styles.chipText, value === o.value && styles.chipTextActive]}>
            {o.label}
          </Text>
        </TouchableOpacity>
      ))}
    </View>
  );
}

function Field({
  label,
  value,
  onChange,
  placeholder,
  keyboardType = 'default',
}: {
  label: string;
  value: string;
  onChange: (v: string) => void;
  placeholder?: string;
  keyboardType?: 'default' | 'phone-pad' | 'email-address';
}) {
  return (
    <View style={styles.field}>
      <Text style={styles.fieldLabel}>{label}</Text>
      <TextInput
        style={styles.input}
        value={value}
        onChangeText={onChange}
        placeholder={placeholder}
        placeholderTextColor="#bbb"
        keyboardType={keyboardType}
      />
    </View>
  );
}

export default function PaymentScreen({ route, navigation }: Props) {
  const { itemId } = route.params;
  const { initPaymentSheet, presentPaymentSheet } = useStripe();

  const [item, setItem] = useState<Item | null>(null);
  const [loading, setLoading] = useState(true);
  const [processing, setProcessing] = useState(false);
  const [walletBalance, setWalletBalance] = useState<number | null>(null);

  // Delivery
  const [courier, setCourier] = useState<CourierProvider>(CourierProvider.Econt);
  const [deliveryType, setDeliveryType] = useState<DeliveryType>(DeliveryType.Office);
  const [officeId, setOfficeId] = useState('');
  const [city, setCity] = useState('');
  const [address, setAddress] = useState('');
  const [recipientName, setRecipientName] = useState('');
  const [recipientPhone, setRecipientPhone] = useState('');

  // Payment method
  const [method, setMethod] = useState<PayMethod>('card');

  const isDonate = item?.listingType === ListingType.Donate;

  useEffect(() => {
    Promise.all([
      itemsApi.getById(itemId),
      walletApi.getWallet().catch(() => null),
    ]).then(([itemRes, walletRes]) => {
      setItem(itemRes.data);
      if (walletRes) setWalletBalance(walletRes.data.balance);
    }).catch(() => {
      Alert.alert('Error', 'Item not found');
      navigation.goBack();
    }).finally(() => setLoading(false));
  }, [itemId]);

  const buildDelivery = (): PaymentDeliveryRequest | null => {
    if (!recipientName.trim() || !recipientPhone.trim()) return null;
    if (deliveryType === DeliveryType.Address && (!city.trim() || !address.trim())) return null;
    if (deliveryType === DeliveryType.Office && !officeId.trim()) return null;
    return {
      courierProvider: courier,
      deliveryType,
      recipientName,
      recipientPhone,
      city: city || undefined,
      address: address || undefined,
      officeId: officeId || undefined,
      weight: 1,
    };
  };

  const handleSubmit = async () => {
    const delivery = buildDelivery();
    if (!delivery) {
      Alert.alert('Missing info', 'Please fill in all delivery fields.');
      return;
    }
    setProcessing(true);
    try {
      if (isDonate) {
        await paymentsApi.createBooking(itemId, delivery);
        Alert.alert('Booked!', 'Your booking request was sent to the seller.');
        navigation.goBack();
        return;
      }

      if (method === 'onspot') {
        await paymentsApi.createOnSpot(itemId, delivery);
        Alert.alert('Done!', 'On-spot payment registered.');
        navigation.goBack();
        return;
      }

      if (method === 'wallet') {
        await walletApi.payForItem(itemId);
        Alert.alert('Paid!', 'Payment made from your wallet.');
        navigation.goBack();
        return;
      }

      // Card — use Stripe PaymentSheet
      const { data } = await paymentsApi.createPaymentIntent(itemId);
      const { error: initErr } = await initPaymentSheet({
        paymentIntentClientSecret: data.clientSecret,
        merchantDisplayName: 'MamVibe',
      });
      if (initErr) { Alert.alert('Error', initErr.message); return; }

      const { error: presentErr } = await presentPaymentSheet();
      if (presentErr) {
        if (presentErr.code !== 'Canceled') Alert.alert('Payment failed', presentErr.message);
        return;
      }
      Alert.alert('Paid!', 'Your card payment was successful.');
      navigation.goBack();
    } catch (err: any) {
      const msg = err.response?.data?.error ?? err.response?.data?.details ?? 'Something went wrong.';
      Alert.alert('Error', msg);
    } finally {
      setProcessing(false);
    }
  };

  if (loading) {
    return <View style={styles.center}><ActivityIndicator size="large" color="#d4938f" /></View>;
  }
  if (!item) return null;

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <ScrollView contentContainerStyle={styles.scroll} keyboardShouldPersistTaps="handled">

        {/* Item summary */}
        <View style={styles.card}>
          <Text style={styles.cardTitle}>{item.title}</Text>
          <Text style={styles.cardPrice}>
            {isDonate ? 'Free' : formatPrice(item.price)}
          </Text>
        </View>

        {/* Delivery */}
        <Text style={styles.sectionLabel}>Delivery</Text>
        <View style={styles.card}>
          <Text style={styles.fieldLabel}>Courier</Text>
          <ChipRow options={COURIERS} value={courier} onChange={setCourier} />

          <Text style={[styles.fieldLabel, { marginTop: 14 }]}>Delivery Type</Text>
          <ChipRow options={DELIVERY_TYPES} value={deliveryType} onChange={setDeliveryType} />

          {deliveryType === DeliveryType.Office ? (
            <Field label="Office ID" value={officeId} onChange={setOfficeId} placeholder="e.g. 1234" />
          ) : (
            <>
              <Field label="City" value={city} onChange={setCity} placeholder="Sofia" />
              <Field label="Address" value={address} onChange={setAddress} placeholder="Street, number" />
            </>
          )}

          <Field label="Recipient Name" value={recipientName} onChange={setRecipientName} placeholder="Full name" />
          <Field label="Recipient Phone" value={recipientPhone} onChange={setRecipientPhone} placeholder="+359..." keyboardType="phone-pad" />
        </View>

        {/* Payment method — sell items only */}
        {!isDonate && (
          <>
            <Text style={styles.sectionLabel}>Payment Method</Text>
            {(
              [
                { key: 'card', icon: '💳', title: 'Credit / Debit Card', desc: 'Secure online payment' },
                { key: 'wallet', icon: '👜', title: `Wallet${walletBalance !== null ? ` — ${formatEur(walletBalance)}` : ''}`, desc: 'Pay from your MamVibe balance' },
                { key: 'onspot', icon: '📍', title: 'Pay On-Spot', desc: 'Pay cash when you meet' },
              ] as const
            ).map((opt) => (
              <TouchableOpacity
                key={opt.key}
                style={[styles.methodCard, method === opt.key && styles.methodCardActive]}
                onPress={() => setMethod(opt.key)}
              >
                <Text style={styles.methodIcon}>{opt.icon}</Text>
                <View style={styles.methodBody}>
                  <Text style={styles.methodTitle}>{opt.title}</Text>
                  <Text style={styles.methodDesc}>{opt.desc}</Text>
                </View>
                <View style={[styles.radio, method === opt.key && styles.radioActive]}>
                  {method === opt.key && <View style={styles.radioDot} />}
                </View>
              </TouchableOpacity>
            ))}
          </>
        )}

        {/* Submit */}
        <TouchableOpacity
          style={[styles.submitBtn, processing && styles.submitBtnDisabled]}
          onPress={handleSubmit}
          disabled={processing}
        >
          {processing
            ? <ActivityIndicator color="#fff" />
            : <Text style={styles.submitText}>
                {isDonate
                  ? 'Confirm Booking'
                  : method === 'card'
                  ? 'Pay with Card'
                  : method === 'wallet'
                  ? 'Pay from Wallet'
                  : 'Register On-Spot Payment'}
              </Text>}
        </TouchableOpacity>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: '#fafafa' },
  center: { flex: 1, alignItems: 'center', justifyContent: 'center' },
  scroll: { padding: 16, gap: 8, paddingBottom: 40 },

  card: { backgroundColor: '#fff', borderRadius: 14, padding: 16, marginBottom: 8, shadowColor: '#000', shadowOffset: { width: 0, height: 1 }, shadowOpacity: 0.05, shadowRadius: 4, elevation: 2 },
  cardTitle: { fontSize: 17, fontWeight: '600', color: '#1a1a1a', marginBottom: 4 },
  cardPrice: { fontSize: 22, fontWeight: '800', color: '#c9a870' },

  sectionLabel: { fontSize: 12, fontWeight: '700', color: '#555', textTransform: 'uppercase', letterSpacing: 0.6, marginTop: 8, marginBottom: 4 },

  chipRow: { flexDirection: 'row', gap: 8, flexWrap: 'wrap', marginTop: 6 },
  chip: { paddingHorizontal: 14, paddingVertical: 7, borderRadius: 99, borderWidth: 1, borderColor: '#ddd', backgroundColor: '#fff' },
  chipActive: { backgroundColor: '#d4938f', borderColor: '#d4938f' },
  chipText: { fontSize: 13, color: '#555', fontWeight: '500' },
  chipTextActive: { color: '#fff', fontWeight: '600' },

  field: { marginTop: 14 },
  fieldLabel: { fontSize: 13, fontWeight: '600', color: '#444', marginBottom: 6 },
  input: { height: 46, borderWidth: 1, borderColor: '#e0e0e0', borderRadius: 10, paddingHorizontal: 14, fontSize: 15, color: '#1a1a1a', backgroundColor: '#fafafa' },

  methodCard: { flexDirection: 'row', alignItems: 'center', backgroundColor: '#fff', borderRadius: 14, padding: 14, marginBottom: 8, borderWidth: 1.5, borderColor: '#eee', gap: 12, shadowColor: '#000', shadowOffset: { width: 0, height: 1 }, shadowOpacity: 0.04, shadowRadius: 3, elevation: 1 },
  methodCardActive: { borderColor: '#d4938f', backgroundColor: 'rgba(212,147,143,0.06)' },
  methodIcon: { fontSize: 26 },
  methodBody: { flex: 1 },
  methodTitle: { fontSize: 14, fontWeight: '600', color: '#1a1a1a' },
  methodDesc: { fontSize: 12, color: '#888', marginTop: 1 },
  radio: { width: 20, height: 20, borderRadius: 10, borderWidth: 2, borderColor: '#ddd', alignItems: 'center', justifyContent: 'center' },
  radioActive: { borderColor: '#d4938f' },
  radioDot: { width: 10, height: 10, borderRadius: 5, backgroundColor: '#d4938f' },

  submitBtn: { height: 54, backgroundColor: '#d4938f', borderRadius: 14, alignItems: 'center', justifyContent: 'center', marginTop: 16 },
  submitBtnDisabled: { opacity: 0.5 },
  submitText: { color: '#fff', fontSize: 17, fontWeight: '700' },
});
