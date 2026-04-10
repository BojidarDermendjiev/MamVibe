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
  { label: 'All', value: undefined },
  { label: 'For Sale', value: ListingType.Sell },
  { label: 'Free', value: ListingType.Donate },
];

export default function BrowseScreen({ navigation }: Props) {
  const [categories, setCategories] = useState<Category[]>([]);
  const [selectedCategory, setSelectedCategory] = useState<string | undefined>(undefined);
  const [selectedListing, setSelectedListing] = useState<number | undefined>(undefined);
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

  const handleListingFilter = (value: number | undefined) => {
    setSelectedListing(value);
    setFilter({ listingType: value, page: 1 });
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
    <SafeAreaView style={styles.safe}>
      {/* Search bar */}
      <View style={styles.searchContainer}>
        <TextInput
          style={styles.searchInput}
          placeholder="Search items..."
          placeholderTextColor="#aaa"
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
          style={[styles.chip, !selectedCategory && styles.chipActive]}
          onPress={() => handleCategorySelect(undefined)}
        >
          <Text style={[styles.chipText, !selectedCategory && styles.chipTextActive]}>All</Text>
        </TouchableOpacity>
        {categories.map((cat) => (
          <TouchableOpacity
            key={cat.id}
            style={[styles.chip, selectedCategory === cat.id && styles.chipActive]}
            onPress={() => handleCategorySelect(cat.id)}
          >
            <Text style={[styles.chipText, selectedCategory === cat.id && styles.chipTextActive]}>
              {cat.name}
            </Text>
          </TouchableOpacity>
        ))}
      </ScrollView>

      {/* Listing type filter */}
      <ScrollView
        horizontal
        showsHorizontalScrollIndicator={false}
        contentContainerStyle={styles.chipsContent}
        style={{ marginBottom: 4 }}
      >
        {LISTING_FILTERS.map((lf) => (
          <TouchableOpacity
            key={String(lf.value)}
            style={[styles.chip, styles.chipSmall, selectedListing === lf.value && styles.chipActive]}
            onPress={() => handleListingFilter(lf.value)}
          >
            <Text style={[styles.chipText, styles.chipTextSmall, selectedListing === lf.value && styles.chipTextActive]}>
              {lf.label}
            </Text>
          </TouchableOpacity>
        ))}
        <View style={styles.divider} />
        {SORT_OPTIONS.map((s) => (
          <TouchableOpacity
            key={s.value}
            style={[styles.chip, styles.chipSmall, selectedSort === s.value && styles.chipActive]}
            onPress={() => handleSort(s.value)}
          >
            <Text style={[styles.chipText, styles.chipTextSmall, selectedSort === s.value && styles.chipTextActive]}>
              {s.label}
            </Text>
          </TouchableOpacity>
        ))}
      </ScrollView>

      {/* Grid */}
      {loading ? (
        <View style={styles.center}>
          <ActivityIndicator size="large" color="#e91e8c" />
        </View>
      ) : items.length === 0 ? (
        <View style={styles.center}>
          <Text style={styles.emptyEmoji}>📦</Text>
          <Text style={styles.emptyText}>No items found</Text>
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
  divider: {
    width: 1,
    backgroundColor: '#ddd',
    marginHorizontal: 4,
    borderRadius: 1,
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
