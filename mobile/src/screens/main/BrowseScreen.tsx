import { useState, useCallback, useRef, useEffect } from 'react';
import {
  View,
  Text,
  TextInput,
  FlatList,
  StyleSheet,
  ActivityIndicator,
  TouchableOpacity,
  RefreshControl,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useTranslation } from 'react-i18next';
import { useFocusEffect } from '@react-navigation/native';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { CompositeScreenProps } from '@react-navigation/native';
import type { BottomTabScreenProps } from '@react-navigation/bottom-tabs';
import { useItems } from '@/hooks/useItems';
import ItemCard from '@/components/ItemCard';
import { useTheme } from '@/contexts/ThemeContext';
import { ListingType } from '@mamvibe/shared';
import type { Item } from '@mamvibe/shared';
import type { MainTabParamList, RootStackParamList } from '@/navigation/types';

type Props = CompositeScreenProps<
  BottomTabScreenProps<MainTabParamList, 'Browse'>,
  NativeStackScreenProps<RootStackParamList>
>;

type TypeFilter = 'all' | 'sale' | 'free';

export default function BrowseScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const { colors } = useTheme();
  const [typeFilter, setTypeFilter] = useState<TypeFilter>('all');
  const [refreshing, setRefreshing] = useState(false);

  const listingType =
    typeFilter === 'sale' ? ListingType.Sell :
    typeFilter === 'free' ? ListingType.Donate :
    undefined;

  const { items, loading, loadingMore, setFilter, searchTerm, setSearchTerm, refetch, loadNextPage } =
    useItems({ listingType, sortBy: 'newest' });

  const handleType = (val: TypeFilter) => {
    setTypeFilter(val);
    setFilter({
      listingType: val === 'sale' ? ListingType.Sell : val === 'free' ? ListingType.Donate : undefined,
      page: 1,
    });
  };

  const handleItemPress = useCallback(
    (item: Item) => { (navigation as any).navigate('ItemDetail', { itemId: item.id }); },
    [navigation],
  );

  const onRefresh = async () => {
    setRefreshing(true);
    await refetch();
    setRefreshing(false);
  };

  const refetchRef = useRef(refetch);
  useEffect(() => { refetchRef.current = refetch; }, [refetch]);

  useFocusEffect(
    useCallback(() => { refetchRef.current(); }, []),
  );

  const renderItem = useCallback(
    ({ item, index }: { item: Item; index: number }) => (
      <View style={index % 2 === 0 ? s.cardLeft : s.cardRight}>
        <ItemCard item={item} onPress={handleItemPress} />
      </View>
    ),
    [handleItemPress],
  );

  return (
    <SafeAreaView style={[s.safe, { backgroundColor: colors.bg }]} edges={['top']}>

      {/* Search */}
      <View style={s.searchWrap}>
        <TextInput
          style={[s.searchInput, { backgroundColor: colors.input, borderColor: colors.inputBorder, color: colors.text }]}
          placeholder={t('browse.searchPlaceholder')}
          placeholderTextColor={colors.text2}
          value={searchTerm}
          onChangeText={(t) => { setSearchTerm(t); setFilter({ page: 1 }); }}
          returnKeyType="search"
          clearButtonMode="while-editing"
        />
      </View>

      {/* For Sale / Free toggle */}
      <View style={[s.toggle, { backgroundColor: colors.section, borderColor: colors.border }]}>
        {(['all', 'sale', 'free'] as TypeFilter[]).map((val) => {
          const label = val === 'all' ? t('browse.all') : val === 'sale' ? t('browse.forSale') : t('browse.free');
          const active = typeFilter === val;
          return (
            <TouchableOpacity
              key={val}
              style={[s.toggleBtn, active && s.toggleBtnActive]}
              onPress={() => handleType(val)}
              activeOpacity={0.8}
            >
              <Text style={[s.toggleTxt, { color: colors.text2 }, active && s.toggleTxtActive]}>
                {label}
              </Text>
            </TouchableOpacity>
          );
        })}
      </View>

      {/* Grid */}
      {loading ? (
        <View style={s.center}>
          <ActivityIndicator size="large" color="#d4938f" />
        </View>
      ) : items.length === 0 ? (
        <View style={s.center}>
          <Text style={s.emptyEmoji}>📦</Text>
          <Text style={[s.emptyText, { color: colors.text2 }]}>{t('browse.noItems')}</Text>
        </View>
      ) : (
        <FlatList
          data={items}
          keyExtractor={(item) => item.id}
          renderItem={renderItem}
          numColumns={2}
          contentContainerStyle={s.list}
          onEndReached={loadNextPage}
          onEndReachedThreshold={0.3}
          ListFooterComponent={() =>
            loadingMore ? <ActivityIndicator color="#d4938f" style={{ paddingVertical: 20 }} /> : null
          }
          refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} tintColor="#d4938f" />}
        />
      )}
    </SafeAreaView>
  );
}

const s = StyleSheet.create({
  safe: { flex: 1 },

  searchWrap: { paddingHorizontal: 16, paddingTop: 10, paddingBottom: 10 },
  searchInput: { height: 44, borderRadius: 12, paddingHorizontal: 14, fontSize: 15, borderWidth: 1 },

  toggle: {
    flexDirection: 'row',
    marginHorizontal: 16,
    marginBottom: 10,
    borderRadius: 10,
    borderWidth: 1,
    overflow: 'hidden',
  },
  toggleBtn: { flex: 1, paddingVertical: 7, alignItems: 'center' },
  toggleBtnActive: { backgroundColor: '#d4938f' },
  toggleTxt: { fontSize: 12, fontWeight: '600' },
  toggleTxtActive: { color: '#fff' },

  list: { paddingHorizontal: 16, paddingTop: 4 },
  cardLeft:  { flex: 1, marginRight: 8 },
  cardRight: { flex: 1, marginLeft: 8 },

  center: { flex: 1, alignItems: 'center', justifyContent: 'center' },
  emptyEmoji: { fontSize: 48, marginBottom: 12 },
  emptyText: { fontSize: 16 },
});
