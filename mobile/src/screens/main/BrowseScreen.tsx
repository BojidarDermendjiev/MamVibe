import { useEffect, useState, useCallback } from 'react';
import {
  View,
  Text,
  TextInput,
  FlatList,
  StyleSheet,
  ActivityIndicator,
  RefreshControl,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { CompositeScreenProps } from '@react-navigation/native';
import type { BottomTabScreenProps } from '@react-navigation/bottom-tabs';
import { useItems } from '@/hooks/useItems';
import ItemCard from '@/components/ItemCard';
import { useTheme } from '@/contexts/ThemeContext';
import type { Item } from '@mamvibe/shared';
import type { MainTabParamList, RootStackParamList } from '@/navigation/types';

type Props = CompositeScreenProps<
  BottomTabScreenProps<MainTabParamList, 'Browse'>,
  NativeStackScreenProps<RootStackParamList>
>;

export default function BrowseScreen({ navigation }: Props) {
  const { colors } = useTheme();

  const [refreshing, setRefreshing] = useState(false);

  const { items, loading, loadingMore, setFilter, searchTerm, setSearchTerm, refetch, loadNextPage } =
    useItems({ sortBy: 'newest' });

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
    <SafeAreaView style={[s.safe, { backgroundColor: colors.bg }]} edges={['top']}>

      {/* Search */}
      <View style={s.searchWrap}>
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
    </SafeAreaView>
  );
}

const s = StyleSheet.create({
  safe: { flex: 1 },

  searchWrap: { paddingHorizontal: 16, paddingTop: 10, paddingBottom: 10 },
  searchInput: { height: 44, borderRadius: 12, paddingHorizontal: 14, fontSize: 15, borderWidth: 1 },

  list: { paddingHorizontal: 16, paddingTop: 4 },
  cardLeft:  { flex: 1, marginRight: 8 },
  cardRight: { flex: 1, marginLeft: 8 },

  center: { flex: 1, alignItems: 'center', justifyContent: 'center' },
  emptyEmoji: { fontSize: 48, marginBottom: 12 },
  emptyText: { fontSize: 16 },
});
