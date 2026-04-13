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

const TYPE_FILTERS = [
  { label: 'All',      value: undefined },
  { label: 'For Sale', value: ListingType.Sell },
  { label: 'Free',     value: ListingType.Donate },
] as const;

export default function BrowseScreen({ navigation }: Props) {
  const { colors } = useTheme();
  const insets = useSafeAreaInsets();

  const [categories, setCategories] = useState<Category[]>([]);
  const [selectedCategory, setSelectedCategory] = useState<string | undefined>(undefined);
  const [selectedType, setSelectedType] = useState<ListingType | undefined>(undefined);
  const [refreshing, setRefreshing] = useState(false);

  const { items, loading, loadingMore, setFilter, searchTerm, setSearchTerm, refetch, loadNextPage } =
    useItems({ categoryId: selectedCategory, listingType: selectedType, sortBy: 'newest' });

  useEffect(() => {
    itemsApi.getCategories().then(({ data }) => setCategories(data)).catch(() => {});
  }, []);

  const handleCategorySelect = (catId: string | undefined) => {
    setSelectedCategory(catId);
    setFilter({ categoryId: catId, page: 1 });
  };

  const handleTypeSelect = (value: ListingType | undefined) => {
    setSelectedType(value);
    setFilter({ listingType: value, page: 1 });
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

      {/* Search */}
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

      {/* Type toggle: All / For Sale / Free */}
      <View style={[s.typeBar, { backgroundColor: colors.section, borderColor: colors.border }]}>
        {TYPE_FILTERS.map((f) => {
          const active = selectedType === f.value;
          return (
            <TouchableOpacity
              key={String(f.value)}
              style={[s.typeBtn, active && s.typeBtnActive]}
              onPress={() => handleTypeSelect(f.value)}
            >
              <Text style={[s.typeBtnText, { color: colors.text2 }, active && s.typeBtnTextActive]}>
                {f.label}
              </Text>
            </TouchableOpacity>
          );
        })}
      </View>

      {/* Category chips */}
      <ScrollView
        horizontal
        showsHorizontalScrollIndicator={false}
        style={s.catScroll}
        contentContainerStyle={s.catContent}
      >
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
      </ScrollView>

      {/* Grid */}
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
          ListFooterComponent={() =>
            loadingMore ? <ActivityIndicator color="#e91e8c" style={{ paddingVertical: 20 }} /> : null
          }
          refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} tintColor="#e91e8c" />}
        />
      )}
    </View>
  );
}

const s = StyleSheet.create({
  safe: { flex: 1 },

  searchWrap: { paddingHorizontal: 16, paddingBottom: 10 },
  searchInput: { height: 44, borderRadius: 12, paddingHorizontal: 14, fontSize: 15, borderWidth: 1 },

  /* Type toggle — 3-segment pill bar */
  typeBar: {
    flexDirection: 'row',
    marginHorizontal: 16,
    marginBottom: 10,
    borderRadius: 12,
    borderWidth: 1,
    overflow: 'hidden',
  },
  typeBtn: { flex: 1, paddingVertical: 10, alignItems: 'center' },
  typeBtnActive: { backgroundColor: '#e91e8c' },
  typeBtnText: { fontSize: 13, fontWeight: '600' },
  typeBtnTextActive: { color: '#fff' },

  /* Category chips */
  catScroll: { flexGrow: 0 },
  catContent: { paddingHorizontal: 16, paddingBottom: 12, flexDirection: 'row', gap: 8 },
  chip: { paddingHorizontal: 14, paddingVertical: 7, borderRadius: 99, borderWidth: 1 },
  chipOn: { backgroundColor: '#e91e8c', borderColor: '#e91e8c' },
  chipTxt: { fontSize: 13, fontWeight: '500' },
  chipTxtOn: { color: '#fff', fontWeight: '600' },

  list: { paddingHorizontal: 16, paddingTop: 4 },
  cardLeft:  { flex: 1, marginRight: 8 },
  cardRight: { flex: 1, marginLeft: 8 },

  center: { flex: 1, alignItems: 'center', justifyContent: 'center' },
  emptyEmoji: { fontSize: 48, marginBottom: 12 },
  emptyText: { fontSize: 16 },
});
