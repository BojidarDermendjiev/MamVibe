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
  SafeAreaView,
} from 'react-native';
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
      <View style={styles.searchContainer}>
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

      {/* Listing type + Sort — side-by-side with labels */}
      <View style={styles.filterRow}>
        <View style={styles.filterGroup}>
          <Text style={[styles.filterLabel, { color: colors.text3 }]}>TYPE</Text>
          <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={styles.filterChips}>
            {LISTING_FILTERS.map((lf) => (
              <TouchableOpacity
                key={String(lf.value)}
                style={[styles.chip, styles.chipSmall, { backgroundColor: colors.card, borderColor: colors.border }, selectedListing === lf.value && styles.chipActive]}
                onPress={() => handleListingFilter(lf.value as ListingType)}
              >
                <Text style={[styles.chipText, styles.chipTextSmall, { color: colors.text2 }, selectedListing === lf.value && styles.chipTextActive]}>
                  {lf.label}
                </Text>
              </TouchableOpacity>
            ))}
          </ScrollView>
        </View>

        <View style={[styles.filterDivider, { backgroundColor: colors.border }]} />

        <View style={styles.filterGroup}>
          <Text style={[styles.filterLabel, { color: colors.text3 }]}>SORT</Text>
          <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={styles.filterChips}>
            {SORT_OPTIONS.map((s) => (
              <TouchableOpacity
                key={s.value}
                style={[styles.chip, styles.chipSmall, { backgroundColor: colors.card, borderColor: colors.border }, selectedSort === s.value && styles.chipActive]}
                onPress={() => handleSort(s.value)}
              >
                <Text style={[styles.chipText, styles.chipTextSmall, { color: colors.text2 }, selectedSort === s.value && styles.chipTextActive]}>
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
  safe: {
    flex: 1,
    backgroundColor: '#fafafa',
  },
  searchContainer: {
    paddingHorizontal: 16,
    paddingTop: 12,
    paddingBottom: 8,
  },
  searchInput: {
    height: 42,
    backgroundColor: '#fff',
    borderRadius: 10,
    paddingHorizontal: 14,
    fontSize: 15,
    color: '#1a1a1a',
    borderWidth: 1,
    borderColor: '#e8e8e8',
  },
  chipsScroll: {
    flexGrow: 0,
  },
  chipsContent: {
    paddingHorizontal: 16,
    paddingBottom: 8,
    gap: 8,
    flexDirection: 'row',
  },
  chip: {
    paddingHorizontal: 14,
    paddingVertical: 7,
    borderRadius: 99,
    backgroundColor: '#fff',
    borderWidth: 1,
    borderColor: '#e0e0e0',
  },
  chipSmall: {
    paddingHorizontal: 10,
    paddingVertical: 5,
  },
  chipActive: {
    backgroundColor: '#e91e8c',
    borderColor: '#e91e8c',
  },
  chipText: {
    fontSize: 13,
    color: '#555',
    fontWeight: '500',
  },
  chipTextSmall: {
    fontSize: 12,
  },
  chipTextActive: {
    color: '#fff',
    fontWeight: '600',
  },
  filterRow: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    paddingHorizontal: 16,
    marginBottom: 6,
  },
  filterGroup: {
    flex: 1,
    overflow: 'hidden',
  },
  filterLabel: {
    fontSize: 9,
    fontWeight: '700',
    color: '#bbb',
    letterSpacing: 1,
    marginBottom: 5,
  },
  filterChips: {
    gap: 6,
    flexDirection: 'row',
  },
  filterDivider: {
    width: 1,
    backgroundColor: '#e0e0e0',
    marginHorizontal: 10,
    marginTop: 2,
    alignSelf: 'stretch',
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
