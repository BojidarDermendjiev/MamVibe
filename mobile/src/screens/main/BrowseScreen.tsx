import { useEffect, useState, useCallback } from 'react';
import {
  View,
  Text,
  TextInput,
  FlatList,
  StyleSheet,
  ActivityIndicator,
  TouchableOpacity,
  ScrollView,
  RefreshControl,
} from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { CompositeScreenProps } from '@react-navigation/native';
import type { BottomTabScreenProps } from '@react-navigation/bottom-tabs';
import { useItems } from '@/hooks/useItems';
import { itemsApi } from '@/api/itemsApi';
import ItemCard from '@/components/ItemCard';
import { useTheme } from '@/contexts/ThemeContext';
import type { Item, Category } from '@mamvibe/shared';
import { ListingType } from '@mamvibe/shared';
import type { MainTabParamList, RootStackParamList } from '@/navigation/types';

type Props = CompositeScreenProps<
  BottomTabScreenProps<MainTabParamList, 'Browse'>,
  NativeStackScreenProps<RootStackParamList>
>;

const SORT_OPTIONS = [
  { label: 'Newest',   value: 'newest' },
  { label: 'Price ↑', value: 'price_asc' },
  { label: 'Price ↓', value: 'price_desc' },
  { label: 'Popular',  value: 'popular' },
];

export default function BrowseScreen({ navigation }: Props) {
  const { colors } = useTheme();
  const insets = useSafeAreaInsets();
  const [categories, setCategories] = useState<Category[]>([]);
  const [selectedCategory, setSelectedCategory] = useState<string | undefined>(undefined);
  const [selectedListing, setSelectedListing] = useState<ListingType | undefined>(undefined);
  const [selectedSort, setSelectedSort] = useState('newest');
  const [refreshing, setRefreshing] = useState(false);

  const { items, loading, loadingMore, setFilter, searchTerm, setSearchTerm, refetch, loadNextPage } =
    useItems({ categoryId: selectedCategory, listingType: selectedListing, sortBy: selectedSort });

  useEffect(() => {
    itemsApi.getCategories().then(({ data }) => setCategories(data)).catch(() => {});
  }, []);

  const handleCategorySelect = (catId: string | undefined) => {
    setSelectedCategory(catId);
    setFilter({ categoryId: catId, page: 1 });
  };

  const handleListingFilter = (value: ListingType) => {
    const next = selectedListing === value ? undefined : value;
    setSelectedListing(next);
    setFilter({ listingType: next, page: 1 });
  };

  const handleSort = (value: string) => {
    setSelectedSort(value);
    setFilter({ sortBy: value, page: 1 });
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

  const renderItem = useCallback(
    ({ item, index }: { item: Item; index: number }) => (
      <View style={index % 2 === 0 ? s.cardLeft : s.cardRight}>
        <ItemCard item={item} onPress={handleItemPress} />
      </View>
    ),
    [handleItemPress],
  );

  return (
    <View style={[s.safe, { backgroundColor: colors.bg }]}>

      {/* ── Search bar ── */}
      <View style={[s.searchWrap, { paddingTop: insets.top + 10 }]}>
        <TextInput
          style={[s.searchInput, { backgroundColor: colors.input, borderColor: colors.inputBorder, color: colors.text }]}
          placeholder="Search items..."
          placeholderTextColor={colors.text2}
          value={searchTerm}
          onChangeText={(t) => { setSearchTerm(t); setFilter({ page: 1 }); }}
          returnKeyType="search"
          clearButtonMode="while-editing"
        />
      </View>

      {/* ── Single unified filter row ── */}
      <ScrollView
        horizontal
        showsHorizontalScrollIndicator={false}
        style={s.filterScroll}
        contentContainerStyle={s.filterContent}
      >
        {/* Categories */}
        <TouchableOpacity
          style={[s.chip, { backgroundColor: colors.card, borderColor: colors.border }, !selectedCategory && s.chipOn]}
          onPress={() => handleCategorySelect(undefined)}
        >
          <Text style={[s.chipTxt, { color: colors.text2 }, !selectedCategory && s.chipTxtOn]}>All</Text>
        </TouchableOpacity>
        {categories.map((cat) => (
          <TouchableOpacity
            key={cat.id}
            style={[s.chip, { backgroundColor: colors.card, borderColor: colors.border }, selectedCategory === cat.id && s.chipOn]}
            onPress={() => handleCategorySelect(cat.id)}
          >
            <Text style={[s.chipTxt, { color: colors.text2 }, selectedCategory === cat.id && s.chipTxtOn]}>
              {cat.name}
            </Text>
          </TouchableOpacity>
        ))}

        {/* Separator */}
        <View style={[s.sep, { backgroundColor: colors.border }]} />

        {/* Type */}
        <TouchableOpacity
          style={[s.chip, { backgroundColor: colors.card, borderColor: colors.border }, !selectedListing && s.chipOn]}
          onPress={() => { setSelectedListing(undefined); setFilter({ listingType: undefined, page: 1 }); }}
        >
          <Text style={[s.chipTxt, { color: colors.text2 }, !selectedListing && s.chipTxtOn]}>All Types</Text>
        </TouchableOpacity>
        <TouchableOpacity
          style={[s.chip, { backgroundColor: colors.card, borderColor: colors.border }, selectedListing === ListingType.Sell && s.chipOn]}
          onPress={() => handleListingFilter(ListingType.Sell)}
        >
          <Text style={[s.chipTxt, { color: colors.text2 }, selectedListing === ListingType.Sell && s.chipTxtOn]}>For Sale</Text>
        </TouchableOpacity>
        <TouchableOpacity
          style={[s.chip, { backgroundColor: colors.card, borderColor: colors.border }, selectedListing === ListingType.Donate && s.chipOn]}
          onPress={() => handleListingFilter(ListingType.Donate)}
        >
          <Text style={[s.chipTxt, { color: colors.text2 }, selectedListing === ListingType.Donate && s.chipTxtOn]}>Free</Text>
        </TouchableOpacity>

        {/* Separator */}
        <View style={[s.sep, { backgroundColor: colors.border }]} />

        {/* Sort */}
        {SORT_OPTIONS.map((opt) => (
          <TouchableOpacity
            key={opt.value}
            style={[s.chip, { backgroundColor: colors.card, borderColor: colors.border }, selectedSort === opt.value && s.chipOn]}
            onPress={() => handleSort(opt.value)}
          >
            <Text style={[s.chipTxt, { color: colors.text2 }, selectedSort === opt.value && s.chipTxtOn]}>
              {opt.label}
            </Text>
          </TouchableOpacity>
        ))}
      </ScrollView>

      {/* ── Grid ── */}
      {loading ? (
        <View style={s.center}>
          <ActivityIndicator size="large" color="#e91e8c" />
        </View>
      ) : items.length === 0 ? (
        <View style={s.center}>
          <Text style={s.emptyEmoji}>📦</Text>
          <Text style={[s.emptyText, { color: colors.text2 }]}>No items found</Text>
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
          ListFooterComponent={() => loadingMore ? <ActivityIndicator color="#e91e8c" style={{ paddingVertical: 20 }} /> : null}
          refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} tintColor="#e91e8c" />}
        />
      )}
    </View>
  );
}

const s = StyleSheet.create({
  safe: { flex: 1 },

  searchWrap: {
    paddingHorizontal: 16,
    paddingBottom: 10,
  },
  searchInput: {
    height: 44,
    borderRadius: 12,
    paddingHorizontal: 14,
    fontSize: 15,
    borderWidth: 1,
  },

  filterScroll: { flexGrow: 0 },
  filterContent: {
    paddingHorizontal: 16,
    paddingBottom: 12,
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },

  chip: {
    paddingHorizontal: 14,
    paddingVertical: 7,
    borderRadius: 99,
    borderWidth: 1,
  },
  chipOn: { backgroundColor: '#e91e8c', borderColor: '#e91e8c' },
  chipTxt: { fontSize: 13, fontWeight: '500' },
  chipTxtOn: { color: '#fff', fontWeight: '600' },

  sep: {
    width: 1,
    height: 20,
    marginHorizontal: 4,
  },

  list: { paddingHorizontal: 16, paddingTop: 4 },
  cardLeft:  { flex: 1, marginRight: 8 },
  cardRight: { flex: 1, marginLeft: 8 },

  center: { flex: 1, alignItems: 'center', justifyContent: 'center' },
  emptyEmoji: { fontSize: 48, marginBottom: 12 },
  emptyText: { fontSize: 16 },
});
