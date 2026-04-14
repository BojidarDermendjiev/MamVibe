import { useCallback, useEffect, useState } from 'react';
import {
  View,
  Text,
  FlatList,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
  Alert,
  Modal,
  TextInput,
  RefreshControl,
  Linking,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useStripe } from '@stripe/stripe-react-native';
import { walletApi } from '@/api/walletApi';
import { formatEur } from '@/utils/currency';
import {
  WalletStatus,
  WalletTransactionKind,
  WalletTransactionStatus,
  WalletTransactionType,
  type WalletDto,
  type WalletTransactionDto,
  type PagedResult,
} from '@mamvibe/shared';

// ── Label helpers ──────────────────────────────────────────────────────────────

const KIND_LABELS: Record<number, string> = {
  [WalletTransactionKind.TopUp]: 'Top-Up',
  [WalletTransactionKind.Transfer]: 'Transfer',
  [WalletTransactionKind.ItemPayment]: 'Item Payment',
  [WalletTransactionKind.Withdrawal]: 'Withdrawal',
  [WalletTransactionKind.Refund]: 'Refund',
  [WalletTransactionKind.Fee]: 'Fee',
};

const STATUS_LABELS: Record<number, string> = {
  [WalletTransactionStatus.Pending]: 'Pending',
  [WalletTransactionStatus.Completed]: 'Completed',
  [WalletTransactionStatus.Failed]: 'Failed',
  [WalletTransactionStatus.Reversed]: 'Reversed',
};

function statusColor(s: WalletTransactionStatus) {
  if (s === WalletTransactionStatus.Completed) return '#8eaa89';
  if (s === WalletTransactionStatus.Pending) return '#c9a870';
  if (s === WalletTransactionStatus.Reversed) return '#8eaa89';
  return '#d4938f';
}

// ── Transaction row ────────────────────────────────────────────────────────────

function TxRow({ tx }: { tx: WalletTransactionDto }) {
  const isCredit = tx.type === WalletTransactionType.Credit;
  return (
    <View style={styles.txRow}>
      <View style={styles.txLeft}>
        <Text style={styles.txKind}>{KIND_LABELS[tx.kind] ?? String(tx.kind)}</Text>
        <Text style={styles.txDesc} numberOfLines={1}>
          {tx.description ?? tx.reference ?? '—'}
        </Text>
        <Text style={styles.txDate}>{new Date(tx.createdAt).toLocaleString('en-GB')}</Text>
      </View>
      <View style={styles.txRight}>
        <Text style={[styles.txAmount, { color: isCredit ? '#8eaa89' : '#d4938f' }]}>
          {isCredit ? '+' : '-'}{formatEur(tx.amount)}
        </Text>
        <Text style={styles.txBalance}>{formatEur(tx.balanceAfter)}</Text>
        <Text style={[styles.txStatus, { color: statusColor(tx.status) }]}>
          {STATUS_LABELS[tx.status] ?? String(tx.status)}
        </Text>
        {tx.receiptUrl && (
          <TouchableOpacity onPress={() => Linking.openURL(tx.receiptUrl!)}>
            <Text style={styles.txReceipt}>PDF ↗</Text>
          </TouchableOpacity>
        )}
      </View>
    </View>
  );
}

// ── Top-Up modal ───────────────────────────────────────────────────────────────

function TopUpModal({ onClose, onSuccess }: { onClose: () => void; onSuccess: () => void }) {
  const { initPaymentSheet, presentPaymentSheet } = useStripe();
  const [amount, setAmount] = useState('');
  const [step, setStep] = useState<'amount' | 'processing'>('amount');
  const [error, setError] = useState('');

  const handleNext = async () => {
    const parsed = parseFloat(amount);
    if (isNaN(parsed) || parsed < 0.01) { setError('Minimum amount is €0.01'); return; }
    if (parsed > 5000) { setError('Maximum amount is €5,000'); return; }
    setError('');
    setStep('processing');
    try {
      const { data } = await walletApi.createTopUp(parsed);
      const { error: initError } = await initPaymentSheet({
        paymentIntentClientSecret: data.clientSecret,
        merchantDisplayName: 'MamVibe',
        allowsDelayedPaymentMethods: false,
      });
      if (initError) { setError(initError.message ?? 'Could not initialise payment'); setStep('amount'); return; }

      const { error: presentError } = await presentPaymentSheet();
      if (presentError) {
        if (presentError.code !== 'Canceled') setError(presentError.message ?? 'Payment failed');
        setStep('amount');
        return;
      }
      onSuccess();
      onClose();
    } catch {
      setError('Top-up failed. Please try again.');
      setStep('amount');
    }
  };

  return (
    <Modal visible animationType="slide" transparent onRequestClose={onClose}>
      <View style={styles.overlay}>
        <View style={styles.sheet}>
          <Text style={styles.sheetTitle}>Top Up Wallet</Text>
          <Text style={styles.inputLabel}>Amount (EUR)</Text>
          <TextInput
            style={styles.input}
            placeholder="0.00"
            placeholderTextColor="#aaa"
            value={amount}
            onChangeText={setAmount}
            keyboardType="decimal-pad"
            editable={step === 'amount'}
          />
          {error ? <Text style={styles.errorText}>{error}</Text> : null}
          <View style={styles.sheetActions}>
            <TouchableOpacity style={styles.btnPrimary} onPress={handleNext} disabled={step === 'processing'}>
              {step === 'processing'
                ? <ActivityIndicator color="#fff" />
                : <Text style={styles.btnPrimaryText}>Continue to Payment</Text>}
            </TouchableOpacity>
            <TouchableOpacity style={styles.btnSecondary} onPress={onClose}>
              <Text style={styles.btnSecondaryText}>Cancel</Text>
            </TouchableOpacity>
          </View>
        </View>
      </View>
    </Modal>
  );
}

// ── Transfer modal ─────────────────────────────────────────────────────────────

function TransferModal({ onClose, onSuccess }: { onClose: () => void; onSuccess: () => void }) {
  const [email, setEmail] = useState('');
  const [amount, setAmount] = useState('');
  const [note, setNote] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async () => {
    const parsed = parseFloat(amount);
    if (!email.trim()) { setError('Email is required'); return; }
    if (isNaN(parsed) || parsed <= 0) { setError('Enter a valid amount'); return; }
    setError('');
    setLoading(true);
    try {
      await walletApi.transfer(email, parsed, note.trim() || undefined);
      Alert.alert('Done!', 'Transfer sent.');
      onSuccess();
      onClose();
    } catch (err: any) {
      setError(err.response?.data?.error ?? 'Transfer failed');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal visible animationType="slide" transparent onRequestClose={onClose}>
      <View style={styles.overlay}>
        <View style={styles.sheet}>
          <Text style={styles.sheetTitle}>Transfer Funds</Text>
          <Text style={styles.inputLabel}>Recipient Email</Text>
          <TextInput style={styles.input} placeholder="user@example.com" placeholderTextColor="#aaa" value={email} onChangeText={setEmail} keyboardType="email-address" autoCapitalize="none" />
          <Text style={styles.inputLabel}>Amount (EUR)</Text>
          <TextInput style={styles.input} placeholder="0.00" placeholderTextColor="#aaa" value={amount} onChangeText={setAmount} keyboardType="decimal-pad" />
          <Text style={styles.inputLabel}>Note (optional)</Text>
          <TextInput style={styles.input} placeholder="What's this for?" placeholderTextColor="#aaa" value={note} onChangeText={setNote} maxLength={200} />
          {error ? <Text style={styles.errorText}>{error}</Text> : null}
          <View style={styles.sheetActions}>
            <TouchableOpacity style={styles.btnPrimary} onPress={handleSubmit} disabled={loading}>
              {loading ? <ActivityIndicator color="#fff" /> : <Text style={styles.btnPrimaryText}>Send</Text>}
            </TouchableOpacity>
            <TouchableOpacity style={styles.btnSecondary} onPress={onClose}>
              <Text style={styles.btnSecondaryText}>Cancel</Text>
            </TouchableOpacity>
          </View>
        </View>
      </View>
    </Modal>
  );
}

// ── Withdraw modal ─────────────────────────────────────────────────────────────

function WithdrawModal({ onClose, onSuccess }: { onClose: () => void; onSuccess: () => void }) {
  const [amount, setAmount] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async () => {
    const parsed = parseFloat(amount);
    if (isNaN(parsed) || parsed < 1) { setError('Minimum withdrawal is €1.00'); return; }
    setError('');
    setLoading(true);
    try {
      await walletApi.withdraw(parsed);
      Alert.alert('Submitted', 'Your withdrawal request has been submitted.');
      onSuccess();
      onClose();
    } catch (err: any) {
      setError(err.response?.data?.error ?? 'Withdrawal failed');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal visible animationType="slide" transparent onRequestClose={onClose}>
      <View style={styles.overlay}>
        <View style={styles.sheet}>
          <Text style={styles.sheetTitle}>Withdraw</Text>
          <Text style={styles.inputLabel}>Amount (EUR)</Text>
          <TextInput style={styles.input} placeholder="0.00" placeholderTextColor="#aaa" value={amount} onChangeText={setAmount} keyboardType="decimal-pad" />
          <Text style={styles.inputHint}>Funds will be sent to your registered IBAN.</Text>
          {error ? <Text style={styles.errorText}>{error}</Text> : null}
          <View style={styles.sheetActions}>
            <TouchableOpacity style={styles.btnPrimary} onPress={handleSubmit} disabled={loading}>
              {loading ? <ActivityIndicator color="#fff" /> : <Text style={styles.btnPrimaryText}>Request Withdrawal</Text>}
            </TouchableOpacity>
            <TouchableOpacity style={styles.btnSecondary} onPress={onClose}>
              <Text style={styles.btnSecondaryText}>Cancel</Text>
            </TouchableOpacity>
          </View>
        </View>
      </View>
    </Modal>
  );
}

// ── Main screen ────────────────────────────────────────────────────────────────

type ActiveModal = 'topup' | 'transfer' | 'withdraw' | null;

const WALLET_STATUS_LABELS: Record<number, string> = {
  [WalletStatus.Active]: 'Active',
  [WalletStatus.Frozen]: 'Frozen',
  [WalletStatus.Suspended]: 'Suspended',
  [WalletStatus.Closed]: 'Closed',
};

export default function WalletScreen() {
  const [wallet, setWallet] = useState<WalletDto | null>(null);
  const [txData, setTxData] = useState<PagedResult<WalletTransactionDto> | null>(null);
  const [page, setPage] = useState(1);
  const [loadingMore, setLoadingMore] = useState(false);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [modal, setModal] = useState<ActiveModal>(null);

  const fetchWallet = useCallback(async () => {
    const { data } = await walletApi.getWallet();
    setWallet(data);
  }, []);

  const fetchTx = useCallback(async (p: number, append = false) => {
    if (append) setLoadingMore(true);
    const { data } = await walletApi.getTransactions(p, 20);
    setTxData((prev) =>
      append && prev ? { ...data, items: [...prev.items, ...data.items] } : data,
    );
    if (append) setLoadingMore(false);
  }, []);

  useEffect(() => {
    Promise.all([fetchWallet(), fetchTx(1)]).finally(() => setLoading(false));
  }, [fetchWallet, fetchTx]);

  const onRefresh = async () => {
    setRefreshing(true);
    setPage(1);
    await Promise.all([fetchWallet(), fetchTx(1)]);
    setRefreshing(false);
  };

  const onModalSuccess = () => {
    setPage(1);
    fetchWallet().catch(() => {});
    fetchTx(1).catch(() => {});
  };

  const onEndReached = () => {
    if (!txData || page >= txData.totalPages || loadingMore) return;
    const next = page + 1;
    setPage(next);
    fetchTx(next, true).catch(() => {});
  };

  const isActive = wallet?.status === WalletStatus.Active;

  if (loading) {
    return <View style={styles.center}><ActivityIndicator size="large" color="#d4938f" /></View>;
  }

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <FlatList
        data={txData?.items ?? []}
        keyExtractor={(t) => t.id}
        renderItem={({ item }) => <TxRow tx={item} />}
        onEndReached={onEndReached}
        onEndReachedThreshold={0.3}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} tintColor="#d4938f" />}
        ListHeaderComponent={
          <View>
            {/* Balance card */}
            <View style={styles.balanceCard}>
              <Text style={styles.balanceLabel}>Balance</Text>
              <Text style={styles.balanceAmount}>{formatEur(wallet?.balance)}</Text>
              <View style={[styles.statusBadge, isActive ? styles.statusActive : styles.statusInactive]}>
                <Text style={styles.statusText}>{WALLET_STATUS_LABELS[wallet?.status ?? 0]}</Text>
              </View>

              {isActive && (
                <View style={styles.actionRow}>
                  <TouchableOpacity style={styles.actionBtn} onPress={() => setModal('topup')}>
                    <Text style={styles.actionBtnIcon}>＋</Text>
                    <Text style={styles.actionBtnLabel}>Top Up</Text>
                  </TouchableOpacity>
                  <TouchableOpacity style={styles.actionBtn} onPress={() => setModal('transfer')}>
                    <Text style={styles.actionBtnIcon}>→</Text>
                    <Text style={styles.actionBtnLabel}>Transfer</Text>
                  </TouchableOpacity>
                  <TouchableOpacity style={styles.actionBtn} onPress={() => setModal('withdraw')}>
                    <Text style={styles.actionBtnIcon}>↓</Text>
                    <Text style={styles.actionBtnLabel}>Withdraw</Text>
                  </TouchableOpacity>
                </View>
              )}
            </View>

            <Text style={styles.sectionLabel}>Transaction History</Text>

            {(!txData || txData.items.length === 0) && (
              <View style={styles.empty}>
                <Text style={styles.emptyEmoji}>💸</Text>
                <Text style={styles.emptyText}>No transactions yet</Text>
              </View>
            )}
          </View>
        }
        ListFooterComponent={loadingMore ? <ActivityIndicator color="#d4938f" style={{ padding: 16 }} /> : null}
        ItemSeparatorComponent={() => <View style={styles.separator} />}
      />

      {modal === 'topup'    && <TopUpModal    onClose={() => setModal(null)} onSuccess={onModalSuccess} />}
      {modal === 'transfer' && <TransferModal onClose={() => setModal(null)} onSuccess={onModalSuccess} />}
      {modal === 'withdraw' && <WithdrawModal onClose={() => setModal(null)} onSuccess={onModalSuccess} />}
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: '#fafafa' },
  center: { flex: 1, alignItems: 'center', justifyContent: 'center' },

  balanceCard: {
    margin: 16,
    padding: 24,
    backgroundColor: '#d4938f',
    borderRadius: 20,
    shadowColor: '#d4938f',
    shadowOffset: { width: 0, height: 6 },
    shadowOpacity: 0.3,
    shadowRadius: 12,
    elevation: 8,
  },
  balanceLabel: { color: 'rgba(255,255,255,0.8)', fontSize: 14, fontWeight: '500' },
  balanceAmount: { color: '#fff', fontSize: 40, fontWeight: '800', marginVertical: 4 },
  statusBadge: { alignSelf: 'flex-start', paddingHorizontal: 10, paddingVertical: 3, borderRadius: 99, marginBottom: 20 },
  statusActive: { backgroundColor: 'rgba(255,255,255,0.2)' },
  statusInactive: { backgroundColor: 'rgba(0,0,0,0.2)' },
  statusText: { color: '#fff', fontSize: 12, fontWeight: '600' },
  actionRow: { flexDirection: 'row', gap: 12 },
  actionBtn: { flex: 1, backgroundColor: 'rgba(255,255,255,0.2)', borderRadius: 14, paddingVertical: 12, alignItems: 'center', gap: 4 },
  actionBtnIcon: { color: '#fff', fontSize: 20, fontWeight: '600' },
  actionBtnLabel: { color: '#fff', fontSize: 12, fontWeight: '600' },

  sectionLabel: { marginHorizontal: 16, marginBottom: 4, fontSize: 13, fontWeight: '700', color: '#8eaa89', textTransform: 'uppercase', letterSpacing: 0.5 },

  txRow: { flexDirection: 'row', justifyContent: 'space-between', paddingHorizontal: 16, paddingVertical: 12, backgroundColor: '#fff' },
  txLeft: { flex: 1, marginRight: 12 },
  txKind: { fontSize: 14, fontWeight: '600', color: '#1a1a1a' },
  txDesc: { fontSize: 12, color: '#888', marginTop: 1 },
  txDate: { fontSize: 11, color: '#aaa', marginTop: 3 },
  txRight: { alignItems: 'flex-end', gap: 2 },
  txAmount: { fontSize: 15, fontWeight: '700' },
  txBalance: { fontSize: 12, color: '#aaa' },
  txStatus: { fontSize: 11, fontWeight: '600' },
  txReceipt: { fontSize: 11, color: '#d4938f', marginTop: 2 },

  separator: { height: StyleSheet.hairlineWidth, backgroundColor: '#f0f0f0' },
  empty: { alignItems: 'center', paddingVertical: 40 },
  emptyEmoji: { fontSize: 48, marginBottom: 8 },
  emptyText: { fontSize: 15, color: '#aaa' },

  // Modals
  overlay: { flex: 1, backgroundColor: 'rgba(0,0,0,0.45)', justifyContent: 'flex-end' },
  sheet: { backgroundColor: '#fff', borderTopLeftRadius: 20, borderTopRightRadius: 20, padding: 24, paddingBottom: 36 },
  sheetTitle: { fontSize: 20, fontWeight: '700', color: '#1a1a1a', marginBottom: 20 },
  inputLabel: { fontSize: 13, fontWeight: '600', color: '#444', marginBottom: 6, marginTop: 12 },
  input: { height: 48, borderWidth: 1, borderColor: '#e0e0e0', borderRadius: 10, paddingHorizontal: 14, fontSize: 16, color: '#1a1a1a', backgroundColor: '#fafafa' },
  inputHint: { fontSize: 12, color: '#aaa', marginTop: 6 },
  errorText: { color: '#d4938f', fontSize: 13, marginTop: 8 },
  sheetActions: { marginTop: 24, gap: 10 },
  btnPrimary: { height: 50, backgroundColor: '#d4938f', borderRadius: 12, alignItems: 'center', justifyContent: 'center' },
  btnPrimaryText: { color: '#fff', fontSize: 16, fontWeight: '600' },
  btnSecondary: { height: 50, borderWidth: 1, borderColor: '#f5ede5', borderRadius: 12, alignItems: 'center', justifyContent: 'center' },
  btnSecondaryText: { color: '#8eaa89', fontSize: 16 },
});
