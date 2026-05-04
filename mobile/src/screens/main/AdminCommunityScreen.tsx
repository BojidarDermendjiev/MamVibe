import { useState, useCallback, useEffect } from 'react';
import {
  View,
  Text,
  FlatList,
  StyleSheet,
  ActivityIndicator,
  TouchableOpacity,
  Alert,
  RefreshControl,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useTranslation } from 'react-i18next';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import { useTheme } from '@/contexts/ThemeContext';
import { ROSE } from '@/constants/palette';
import { doctorReviewsApi, type DoctorReviewDto } from '@/api/doctorReviewsApi';
import {
  childFriendlyPlacesApi,
  placeTypeLabel,
  type ChildFriendlyPlaceDto,
} from '@/api/childFriendlyPlacesApi';
import type { RootStackParamList } from '@/navigation/types';

type Props = NativeStackScreenProps<RootStackParamList, 'AdminCommunity'>;
type Tab = 'reviews' | 'places';

export default function AdminCommunityScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const { colors } = useTheme();
  const [tab, setTab] = useState<Tab>('reviews');
  const [reviews, setReviews] = useState<DoctorReviewDto[]>([]);
  const [places, setPlaces] = useState<ChildFriendlyPlaceDto[]>([]);
  const [loadingReviews, setLoadingReviews] = useState(true);
  const [loadingPlaces, setLoadingPlaces] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const loadAll = useCallback(async () => {
    const [r, p] = await Promise.allSettled([
      doctorReviewsApi.getPending(),
      childFriendlyPlacesApi.getPending(),
    ]);
    if (r.status === 'fulfilled') setReviews(r.value);
    if (p.status === 'fulfilled') setPlaces(p.value);
    setLoadingReviews(false);
    setLoadingPlaces(false);
  }, []);

  useEffect(() => { loadAll(); }, [loadAll]);

  const onRefresh = async () => {
    setRefreshing(true);
    await loadAll();
    setRefreshing(false);
  };

  const handleApproveReview = (id: string) => {
    Alert.alert(t('common.confirm'), t('common.approve') + '?', [
      { text: t('common.cancel'), style: 'cancel' },
      {
        text: t('common.approve'),
        onPress: async () => {
          try {
            await doctorReviewsApi.approve(id);
            setReviews((prev) => prev.filter((r) => r.id !== id));
          } catch {
            Alert.alert(t('common.error'), t('adminCommunity.approveError'));
          }
        },
      },
    ]);
  };

  const handleDeleteReview = (id: string) => {
    Alert.alert(t('common.deleteConfirmTitle'), t('common.deleteConfirmMsg'), [
      { text: t('common.cancel'), style: 'cancel' },
      {
        text: t('common.delete'),
        style: 'destructive',
        onPress: async () => {
          try {
            await doctorReviewsApi.adminDelete(id);
            setReviews((prev) => prev.filter((r) => r.id !== id));
          } catch {
            Alert.alert(t('common.error'), t('adminCommunity.deleteError'));
          }
        },
      },
    ]);
  };

  const handleApprovePlace = (id: string) => {
    Alert.alert(t('common.confirm'), t('common.approve') + '?', [
      { text: t('common.cancel'), style: 'cancel' },
      {
        text: t('common.approve'),
        onPress: async () => {
          try {
            await childFriendlyPlacesApi.approve(id);
            setPlaces((prev) => prev.filter((p) => p.id !== id));
          } catch {
            Alert.alert(t('common.error'), t('adminCommunity.approveError'));
          }
        },
      },
    ]);
  };

  const handleDeletePlace = (id: string) => {
    Alert.alert(t('common.deleteConfirmTitle'), t('common.deleteConfirmMsg'), [
      { text: t('common.cancel'), style: 'cancel' },
      {
        text: t('common.delete'),
        style: 'destructive',
        onPress: async () => {
          try {
            await childFriendlyPlacesApi.adminDelete(id);
            setPlaces((prev) => prev.filter((p) => p.id !== id));
          } catch {
            Alert.alert(t('common.error'), t('adminCommunity.deleteError'));
          }
        },
      },
    ]);
  };

  const renderReview = ({ item }: { item: DoctorReviewDto }) => (
    <View style={[s.card, { backgroundColor: colors.card, borderColor: colors.border }]}>
      <Text style={[s.cardTitle, { color: colors.text }]}>{item.doctorName}</Text>
      <Text style={[s.cardSub, { color: ROSE }]}>{item.specialization} · {item.city}</Text>
      <Text style={[s.cardContent, { color: colors.text }]} numberOfLines={3}>{item.content}</Text>
      <Text style={[s.cardMeta, { color: colors.text2 }]}>
        {'⭐'.repeat(item.rating)} · {item.authorDisplayName ?? t('doctorReviews.anonymous')}
      </Text>
      <View style={s.actions}>
        <TouchableOpacity
          style={[s.btn, s.approveBtn]}
          onPress={() => handleApproveReview(item.id)}
          activeOpacity={0.8}
        >
          <Text style={s.btnText}>✓ {t('common.approve')}</Text>
        </TouchableOpacity>
        <TouchableOpacity
          style={[s.btn, s.deleteBtn]}
          onPress={() => handleDeleteReview(item.id)}
          activeOpacity={0.8}
        >
          <Text style={s.btnText}>✕ {t('common.delete')}</Text>
        </TouchableOpacity>
      </View>
    </View>
  );

  const renderPlace = ({ item }: { item: ChildFriendlyPlaceDto }) => (
    <View style={[s.card, { backgroundColor: colors.card, borderColor: colors.border }]}>
      <Text style={[s.cardTitle, { color: colors.text }]}>{item.name}</Text>
      <Text style={[s.cardSub, { color: ROSE }]}>{placeTypeLabel[item.placeType]} · {item.city}</Text>
      <Text style={[s.cardContent, { color: colors.text }]} numberOfLines={3}>{item.description}</Text>
      <Text style={[s.cardMeta, { color: colors.text2 }]}>{item.authorDisplayName ?? '—'}</Text>
      <View style={s.actions}>
        <TouchableOpacity
          style={[s.btn, s.approveBtn]}
          onPress={() => handleApprovePlace(item.id)}
          activeOpacity={0.8}
        >
          <Text style={s.btnText}>✓ {t('common.approve')}</Text>
        </TouchableOpacity>
        <TouchableOpacity
          style={[s.btn, s.deleteBtn]}
          onPress={() => handleDeletePlace(item.id)}
          activeOpacity={0.8}
        >
          <Text style={s.btnText}>✕ {t('common.delete')}</Text>
        </TouchableOpacity>
      </View>
    </View>
  );

  const isLoading = tab === 'reviews' ? loadingReviews : loadingPlaces;
  const data = tab === 'reviews' ? reviews : places;
  const renderItem = tab === 'reviews' ? renderReview : renderPlace;

  return (
    <SafeAreaView style={[s.safe, { backgroundColor: colors.bg }]} edges={['top']}>
      {/* Header */}
      <View style={[s.header, { borderBottomColor: colors.border }]}>
        <TouchableOpacity onPress={() => navigation.goBack()} activeOpacity={0.7} style={s.backBtn}>
          <Text style={{ color: ROSE, fontSize: 17 }}>‹ Back</Text>
        </TouchableOpacity>
        <Text style={[s.title, { color: colors.text }]}>{t('adminCommunity.title')}</Text>
        <View style={{ width: 60 }} />
      </View>

      {/* Tabs */}
      <View style={[s.tabs, { backgroundColor: colors.section, borderColor: colors.border }]}>
        {(['reviews', 'places'] as Tab[]).map((tabKey) => {
          const label = tabKey === 'reviews'
            ? `${t('adminCommunity.pendingReviews')}${reviews.length > 0 ? ` (${reviews.length})` : ''}`
            : `${t('adminCommunity.pendingPlaces')}${places.length > 0 ? ` (${places.length})` : ''}`;
          return (
            <TouchableOpacity
              key={tabKey}
              style={[s.tabBtn, tab === tabKey && s.tabBtnActive]}
              onPress={() => setTab(tabKey)}
              activeOpacity={0.8}
            >
              <Text style={[s.tabText, { color: colors.text2 }, tab === tabKey && s.tabTextActive]}>
                {label}
              </Text>
            </TouchableOpacity>
          );
        })}
      </View>

      {isLoading ? (
        <View style={s.center}>
          <ActivityIndicator size="large" color={ROSE} />
        </View>
      ) : data.length === 0 ? (
        <View style={s.center}>
          <Text style={s.emptyEmoji}>✅</Text>
          <Text style={[s.emptyText, { color: colors.text2 }]}>{t('adminCommunity.noItems')}</Text>
        </View>
      ) : (
        <FlatList
          data={data as any[]}
          keyExtractor={(item) => item.id}
          renderItem={renderItem as any}
          contentContainerStyle={s.list}
          refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} tintColor={ROSE} />}
        />
      )}
    </SafeAreaView>
  );
}

const s = StyleSheet.create({
  safe: { flex: 1 },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: 16,
    paddingVertical: 12,
    borderBottomWidth: StyleSheet.hairlineWidth,
  },
  backBtn: { width: 60 },
  title: { flex: 1, textAlign: 'center', fontSize: 18, fontWeight: '700' },
  tabs: {
    flexDirection: 'row',
    marginHorizontal: 16,
    marginVertical: 12,
    borderRadius: 10,
    borderWidth: 1,
    overflow: 'hidden',
  },
  tabBtn: { flex: 1, paddingVertical: 9, alignItems: 'center' },
  tabBtnActive: { backgroundColor: ROSE },
  tabText: { fontSize: 12, fontWeight: '600' },
  tabTextActive: { color: '#fff' },
  list: { padding: 16 },
  center: { flex: 1, alignItems: 'center', justifyContent: 'center' },
  emptyEmoji: { fontSize: 48, marginBottom: 12 },
  emptyText: { fontSize: 16 },

  card: {
    borderRadius: 14,
    borderWidth: StyleSheet.hairlineWidth,
    padding: 14,
    marginBottom: 12,
  },
  cardTitle: { fontSize: 16, fontWeight: '700', marginBottom: 2 },
  cardSub: { fontSize: 13, marginBottom: 8 },
  cardContent: { fontSize: 14, lineHeight: 20, marginBottom: 8 },
  cardMeta: { fontSize: 12, marginBottom: 12 },
  actions: { flexDirection: 'row', gap: 10 },
  btn: { flex: 1, height: 40, borderRadius: 10, alignItems: 'center', justifyContent: 'center' },
  approveBtn: { backgroundColor: '#4caf50' },
  deleteBtn: { backgroundColor: '#e53935' },
  btnText: { color: '#fff', fontSize: 14, fontWeight: '600' },
});
