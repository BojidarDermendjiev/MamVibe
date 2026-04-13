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
import { SafeAreaView } from 'react-native-safe-area-context';
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
  { label: 'Newest', value: 'newest' },
  { label: 'Price ↑', value: 'price_asc' },
  { label: 'Price ↓', value: 'price_desc' },
  { label: 'Popular', value: 'popular' },
];

const LISTING_FILTERS = [
  { label: 'For Sale', value: ListingType.Sell },
  { label: 'Free',     value: ListingType.Donate },
];

export default function BrowseScreen({ navigation }: Props) {
  const { colors } = useTheme();
  const [categories, setCategories] = useState<Category[]>([]);
  const [selectedCategory, setSelectedCategory] = useState<string | undefined>(undefined);
  const [selectedListing, setSelectedListing] = useState<ListingType | undefined>(undefined);
  const [selectedSort, setSelectedSort] = useState('newest');
  const [refreshing, setRefreshing] = useState(false);

  const { items, loading, loadingMore, filter, setFilter, searchTerm, setSearchTerm, refetch, loadNextPage } =
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
    (item: Item) => {
      (navigation as any).navigate('ItemDetail', { itemId: item.id });
    },
    [navigation],
  );

  const onRefresh = async () => {
    setRefreshing(true);
    await refetch();
    setRefreshing(false);
  };

  const renderItem = useCallback(
    ({ item, index }: { item: Item; index: number }) => (
      <View style={index % 2 === 0 ? styles.cardLeft : styles.cardRight}>
        <ItemCard item={item} onPress={handleItemPress} />
      </View>
    ),
    [handleItemPress],
  );

  const renderFooter = () => {
    if (!loadingMore) return null;
    return (
      <View style={styles.footerLoader}>
        <ActivityIndicator color="#e91e8c" />
      </View>
    );
  };

  return (
    <SafeAreaView style={[styles.safe, { backgroundColor: colors.bg }]}>
      {/* Search bar */}
      <View style={[styles.searchContainer, { backgroundColor: colors.bg }]}>
        <TextInput
          style={[styles.searchInput, { backgroundColor: colors.input, borderColor: colors.inputBorder, color: colors.text }]}
          placeholder="Search items..."
          placeholderTextColor={colors.text2}
          value={searchTerm}
          onChangeText={(t) => { setSearchTerm(t); setFilter({ page: 1 }); }}
          returnKeyType="search"
          clearButtonMode="while-editing"
        />
      </View>

      {/* Category chips */}
      <ScrollView
        horizontal
        showsHorizontalScrollIndicator={false}
        style={styles.chipsScroll}
        contentContainerStyle={styles.chipsContent}
      >
        <TouchableOpacity
          style={[styles.chip, { backgroundColor: colors.card, borderColor: colors.border }, !selectedCategory && styles.chipActive]}
          onPress={() => handleCategorySelect(undefined)}
        >
          <Text style={[styles.chipText, { color: colors.text2 }, !selectedCategory && styles.chipTextActive]}>All</Text>
        </TouchableOpacity>
        {categories.map((cat) => (
          <TouchableOpacity
            key={cat.id}
            style={[styles.chip, { backgroundColor: colors.card, borderColor: colors.border }, selectedCategory === cat.id && styles.chipActive]}
            onPress={() => handleCategorySelect(cat.id)}
          >
            <Text style={[styles.chipText, { color: colors.text2 }, selectedCategory === cat.id && styles.chipTextActive]}>
              {cat.name}
            </Text>
          </TouchableOpacity>
        ))}
      </ScrollView>

      {/* Type + Sort bar */}
      <View style={[styles.filterBar, { backgroundColor: colors.section, borderColor: colors.border }]}>
        {/* TYPE pills */}
        <View style={styles.filterSegment}>
          <Text style={[styles.filterLabel, { color: colors.text3 }]}>TYPE</Text>
          <View style={styles.pillRow}>
            {LISTING_FILTERS.map((lf) => (
              <TouchableOpacity
                key={String(lf.value)}
                style={[styles.pill, { borderColor: colors.border }, selectedListing === lf.value && styles.pillActive]}
                onPress={() => handleListingFilter(lf.value as ListingType)}
              >
                <Text style={[styles.pillText, { color: colors.text2 }, selectedListing === lf.value && styles.pillTextActive]}>
                  {lf.label}
                </Text>
              </TouchableOpacity>
            ))}
          </View>
        </View>

        <View style={[styles.filterDivider, { backgroundColor: colors.border }]} />

        {/* SORT pills */}
        <View style={styles.filterSegment}>
          <Text style={[styles.filterLabel, { color: colors.text3 }]}>SORT</Text>
          <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={styles.pillRow}>
            {SORT_OPTIONS.map((s) => (
              <TouchableOpacity
                key={s.value}
                style={[styles.pill, { borderColor: colors.border }, selectedSort === s.value && styles.pillActive]}
                onPress={() => handleSort(s.value)}
              >
                <Text style={[styles.pillText, { color: colors.text2 }, selectedSort === s.value && styles.pillTextActive]}>
                  {s.label}
                </Text>
              </TouchableOpacity>
            ))}
          </ScrollView>
        </View>
      </View>

      {/* Grid */}
      {loading ? (
        <View style={styles.center}>
          <ActivityIndicator size="large" color="#e91e8c" />
        </View>
      ) : items.length === 0 ? (
        <View style={styles.center}>
          <Text style={styles.emptyEmoji}>📦</Text>
          <Text style={[styles.emptyText, { color: colors.text2 }]}>No items found</Text>
        </View>
      ) : (
        <FlatList
          data={items}
          keyExtractor={(item) => item.id}
          renderItem={renderItem}
          numColumns={2}
          contentContainerStyle={styles.listContent}
          onEndReached={loadNextPage}
          onEndReachedThreshold={0.3}
          ListFooterComponent={renderFooter}
          refreshControl={
            <RefreshControl refreshing={refreshing} onRefresh={onRefresh} tintColor="#e91e8c" />
          }
        />
      )}
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1 },
  searchContainer: {
    paddingHorizontal: 16,
    paddingTop: 10,
    paddingBottom: 8,
  },
  searchInput: {
    height: 44,
    borderRadius: 12,
    paddingHorizontal: 14,
    fontSize: 15,
    borderWidth: 1,
  },
  chipsScroll: { flexGrow: 0 },
  chipsContent: {
    paddingHorizontal: 16,
    paddingBottom: 10,
    gap: 8,
    flexDirection: 'row',
  },
  chip: {
    paddingHorizontal: 14,
    paddingVertical: 7,
    borderRadius: 99,
    borderWidth: 1,
  },
  chipActive: { backgroundColor: '#e91e8c', borderColor: '#e91e8c' },
  chipText: { fontSize: 13, fontWeight: '500' },
  chipTextActive: { color: '#fff', fontWeight: '600' },

  /* Filter bar — visually distinct from category chips above */
  filterBar: {
    flexDirection: 'row',
    alignItems: 'center',
    marginHorizontal: 16,
    marginBottom: 10,
    borderRadius: 12,
    borderWidth: 1,
    paddingHorizontal: 12,
    paddingVertical: 10,
    gap: 0,
  },
  filterSegment: { flex: 1, gap: 6 },
  filterLabel: {
    fontSize: 9,
    fontWeight: '700',
    letterSpacing: 1.2,
    textTransform: 'uppercase',
  },
  pillRow: { flexDirection: 'row', gap: 6 },
  pill: {
    paddingHorizontal: 10,
    paddingVertical: 4,
    borderRadius: 99,
    borderWidth: 1,
  },
  pillActive: { backgroundColor: '#e91e8c', borderColor: '#e91e8c' },
  pillText: { fontSize: 12, fontWeight: '500' },
  pillTextActive: { color: '#fff', fontWeight: '600' },
  filterDivider: {
    width: 1,
    height: '100%',
    alignSelf: 'stretch',
    marginHorizontal: 10,
  },
  listContent: {
    paddingHorizontal: 16,
    paddingTop: 4,
  },
  cardLeft: {
    flex: 1,
    marginRight: 8,
  },
  cardRight: {
    flex: 1,
    marginLeft: 8,
  },
  center: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
  emptyEmoji: {
    fontSize: 48,
    marginBottom: 12,
  },
  emptyText: {
    fontSize: 16,
    color: '#888',
  },
  footerLoader: {
    paddingVertical: 20,
    alignItems: 'center',
  },
});
