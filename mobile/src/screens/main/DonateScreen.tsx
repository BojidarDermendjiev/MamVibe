import { useState } from 'react';
import {
  View,
  Text,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
  Alert,
  TextInput,
  ScrollView,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useStripe } from '@stripe/stripe-react-native';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { RootStackParamList } from '@/navigation/types';
import { paymentsApi } from '@/api/paymentsApi';
import { useTheme } from '@/contexts/ThemeContext';

type Props = NativeStackScreenProps<RootStackParamList, 'Donate'>;

const PRESETS = [1, 3, 5, 10];
const PRIMARY = '#e91e8c';

export default function DonateScreen({ navigation }: Props) {
  const { colors } = useTheme();
  const { initPaymentSheet, presentPaymentSheet } = useStripe();

  const [selected, setSelected] = useState<number | null>(3);
  const [custom, setCustom] = useState('');
  const [loading, setLoading] = useState(false);

  const amount = custom ? parseFloat(custom) : selected;

  const handleDonate = async () => {
    if (!amount || isNaN(amount) || amount < 1) {
      Alert.alert('Amount required', 'Please select or enter an amount of at least 1 лв.');
      return;
    }
    setLoading(true);
    try {
      const { data } = await paymentsApi.createDonationIntent(amount);
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
      Alert.alert('Thank you! 💛', 'Your support keeps MamVibe running.', [
        { text: 'Back to Home', onPress: () => navigation.goBack() },
      ]);
    } catch {
      Alert.alert('Error', 'Something went wrong. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <SafeAreaView style={[s.safe, { backgroundColor: colors.bg }]} edges={['bottom']}>
      <ScrollView contentContainerStyle={s.scroll} showsVerticalScrollIndicator={false}>

        {/* Header */}
        <View style={s.header}>
          <Text style={s.heart}>💛</Text>
          <Text style={[s.title, { color: colors.text }]}>Support MamVibe</Text>
          <Text style={[s.subtitle, { color: colors.text2 }]}>
            MamVibe is free, ad-free, and always will be. A small contribution keeps the platform running and helps more families.
          </Text>
        </View>

        {/* Preset amounts */}
        <Text style={[s.label, { color: colors.text2 }]}>Choose an amount (лв)</Text>
        <View style={s.presets}>
          {PRESETS.map((v) => (
            <TouchableOpacity
              key={v}
              style={[
                s.preset,
                { backgroundColor: colors.card, borderColor: colors.border },
                selected === v && !custom && s.presetActive,
              ]}
              onPress={() => { setSelected(v); setCustom(''); }}
            >
              <Text style={[s.presetText, { color: colors.text }, selected === v && !custom && s.presetTextActive]}>
                {v} лв
              </Text>
            </TouchableOpacity>
          ))}
        </View>

        {/* Custom amount */}
        <Text style={[s.label, { color: colors.text2 }]}>Or enter custom amount</Text>
        <TextInput
          style={[s.input, { backgroundColor: colors.input, borderColor: custom ? PRIMARY : colors.inputBorder, color: colors.text }]}
          placeholder="e.g. 15"
          placeholderTextColor={colors.text2}
          keyboardType="decimal-pad"
          value={custom}
          onChangeText={(v) => { setCustom(v); setSelected(null); }}
        />

        {/* Summary */}
        {!!amount && !isNaN(amount) && (
          <View style={[s.summary, { backgroundColor: colors.section, borderColor: colors.border }]}>
            <Text style={[s.summaryText, { color: colors.text2 }]}>You're donating</Text>
            <Text style={s.summaryAmount}>{amount.toFixed(2)} лв</Text>
          </View>
        )}

        {/* Donate button */}
        <TouchableOpacity
          style={[s.btn, loading && s.btnDisabled]}
          onPress={handleDonate}
          disabled={loading}
        >
          {loading
            ? <ActivityIndicator color="#fff" />
            : <Text style={s.btnText}>Donate with Card  💳</Text>
          }
        </TouchableOpacity>

        <Text style={[s.note, { color: colors.text3 }]}>
          Payments are processed securely via Stripe. MamVibe never stores your card details.
        </Text>

      </ScrollView>
    </SafeAreaView>
  );
}

const s = StyleSheet.create({
  safe: { flex: 1 },
  scroll: { padding: 20, paddingBottom: 40 },

  header: { alignItems: 'center', marginBottom: 28 },
  heart: { fontSize: 52, marginBottom: 10 },
  title: { fontSize: 24, fontWeight: '800', textAlign: 'center', marginBottom: 10 },
  subtitle: { fontSize: 14, textAlign: 'center', lineHeight: 21, maxWidth: 300 },

  label: { fontSize: 12, fontWeight: '600', textTransform: 'uppercase', letterSpacing: 0.8, marginBottom: 10 },

  presets: { flexDirection: 'row', gap: 10, marginBottom: 20 },
  preset: {
    flex: 1,
    height: 52,
    borderRadius: 12,
    borderWidth: 1.5,
    alignItems: 'center',
    justifyContent: 'center',
  },
  presetActive: { backgroundColor: PRIMARY, borderColor: PRIMARY },
  presetText: { fontSize: 15, fontWeight: '700' },
  presetTextActive: { color: '#fff' },

  input: {
    height: 48,
    borderRadius: 12,
    borderWidth: 1.5,
    paddingHorizontal: 14,
    fontSize: 16,
    marginBottom: 20,
  },

  summary: {
    borderRadius: 12,
    borderWidth: 1,
    padding: 16,
    alignItems: 'center',
    marginBottom: 20,
  },
  summaryText: { fontSize: 13 },
  summaryAmount: { fontSize: 28, fontWeight: '800', color: PRIMARY, marginTop: 4 },

  btn: {
    height: 56,
    backgroundColor: PRIMARY,
    borderRadius: 14,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: 16,
  },
  btnDisabled: { opacity: 0.5 },
  btnText: { color: '#fff', fontSize: 17, fontWeight: '700' },

  note: { fontSize: 11, textAlign: 'center', lineHeight: 16 },
});
