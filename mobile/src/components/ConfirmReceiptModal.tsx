import { useState } from 'react';
import {
  Modal,
  View,
  Text,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
  useWindowDimensions,
} from 'react-native';
import { formatPrice } from '@/utils/currency';

interface Props {
  visible: boolean;
  sellerName: string;
  itemTitle: string;
  amount: number;
  sellerRating?: number;
  onCancel: () => void;
  onConfirm: () => Promise<void>;
}

export default function ConfirmReceiptModal({
  visible,
  sellerName,
  itemTitle,
  amount,
  sellerRating,
  onCancel,
  onConfirm,
}: Props) {
  const [confirming, setConfirming] = useState(false);
  const { width } = useWindowDimensions();
  const cardWidth = Math.min(width * 0.9, 400);

  const handleConfirm = async () => {
    setConfirming(true);
    try {
      await onConfirm();
    } finally {
      setConfirming(false);
    }
  };

  return (
    <Modal visible={visible} transparent animationType="fade" onRequestClose={onCancel}>
      <View style={styles.backdrop}>
        <View style={[styles.card, { width: cardWidth }]}>
          <Text style={styles.icon}>📦</Text>

          <Text style={styles.title}>Confirm receipt</Text>
          <Text style={styles.subtitle}>
            Once confirmed, the payment is released to{' '}
            <Text style={styles.sellerName}>{sellerName}</Text>. This cannot be undone.
          </Text>

          <View style={styles.details}>
            <Row label="Item" value={itemTitle} />
            <Row
              label="Seller"
              value={sellerRating != null ? `${sellerName} · ⭐ ${sellerRating}` : sellerName}
            />
            <Row label="Amount" value={formatPrice(amount)} highlight />
            <Row label="Released to" value={`Stripe → ${sellerName}`} last />
          </View>

          <View style={styles.buttons}>
            <TouchableOpacity
              style={styles.btnCancel}
              onPress={onCancel}
              disabled={confirming}
              activeOpacity={0.7}
            >
              <Text style={styles.btnCancelText}>Cancel</Text>
            </TouchableOpacity>

            <TouchableOpacity
              style={[styles.btnConfirm, confirming && styles.btnDisabled]}
              onPress={handleConfirm}
              disabled={confirming}
              activeOpacity={0.75}
            >
              {confirming ? (
                <ActivityIndicator color="#fff" size="small" />
              ) : (
                <Text style={styles.btnConfirmText}>Confirm</Text>
              )}
            </TouchableOpacity>
          </View>
        </View>
      </View>
    </Modal>
  );
}

function Row({
  label,
  value,
  highlight,
  last,
}: {
  label: string;
  value: string;
  highlight?: boolean;
  last?: boolean;
}) {
  return (
    <View style={[styles.row, last && styles.rowLast]}>
      <Text style={styles.rowLabel}>{label}</Text>
      <Text style={[styles.rowValue, highlight && styles.rowValueHighlight]}>{value}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  backdrop: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.45)',
    alignItems: 'center',
    justifyContent: 'center',
    padding: 20,
  },
  card: {
    backgroundColor: '#fff',
    borderRadius: 20,
    padding: 24,
    alignItems: 'center',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 8 },
    shadowOpacity: 0.15,
    shadowRadius: 24,
    elevation: 12,
  },
  icon: { fontSize: 48, marginBottom: 12 },
  title: {
    fontSize: 20,
    fontWeight: '800',
    color: '#1a1a1a',
    marginBottom: 8,
    textAlign: 'center',
  },
  subtitle: {
    fontSize: 13,
    color: '#666',
    textAlign: 'center',
    lineHeight: 19,
    marginBottom: 20,
  },
  sellerName: { fontWeight: '700', color: '#1a1a1a' },

  details: {
    width: '100%',
    borderRadius: 12,
    backgroundColor: '#faf6f3',
    paddingHorizontal: 14,
    marginBottom: 22,
  },
  row: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingVertical: 11,
    borderBottomWidth: StyleSheet.hairlineWidth,
    borderBottomColor: '#e8ddd8',
  },
  rowLast: { borderBottomWidth: 0 },
  rowLabel: { fontSize: 13, color: '#888', flex: 1 },
  rowValue: { fontSize: 13, color: '#1a1a1a', fontWeight: '600', flex: 2, textAlign: 'right' },
  rowValueHighlight: { color: '#d4938f', fontSize: 15, fontWeight: '800' },

  buttons: { flexDirection: 'row', gap: 10, width: '100%' },
  btnCancel: {
    flex: 1,
    height: 48,
    borderRadius: 12,
    borderWidth: 1.5,
    borderColor: '#d4938f',
    alignItems: 'center',
    justifyContent: 'center',
  },
  btnCancelText: { color: '#d4938f', fontSize: 15, fontWeight: '600' },
  btnConfirm: {
    flex: 1,
    height: 48,
    borderRadius: 12,
    backgroundColor: '#d4938f',
    alignItems: 'center',
    justifyContent: 'center',
    shadowColor: '#d4938f',
    shadowOffset: { width: 0, height: 3 },
    shadowOpacity: 0.35,
    shadowRadius: 8,
    elevation: 4,
  },
  btnConfirmText: { color: '#fff', fontSize: 15, fontWeight: '700' },
  btnDisabled: { opacity: 0.55, shadowOpacity: 0 },
});
