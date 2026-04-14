import { useEffect, useState, useCallback } from 'react';
import {
  View,
  Text,
  FlatList,
  StyleSheet,
  ActivityIndicator,
  TouchableOpacity,
  Alert,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { RootStackParamList } from '@/navigation/types';
import { itemsApi } from '@/api/itemsApi';
import ItemCard from '@/components/ItemCard';
import { useTheme } from '@/contexts/ThemeContext';
import type { Item } from '@mamvibe/shared';

type Props = NativeStackScreenProps<RootStackParamList, 'MyItems'>;

export default function MyItemsScreen({ navigation }: Props) {
  const { colors } = useTheme();
  const [items, setItems] = useState<Item[]>([]);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    try {
      setLoading(true);
      const { data } = await itemsApi.getMyItems();
      setItems(data);
    } catch {
      Alert.alert('Error', 'Could not load your items.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  const handlePress = useCallback(
    (item: Item) => navigation.navigate('ItemDetail', { itemId: item.id }),
    [navigation],
  );

  const handleDelete = (itemId: string) => {
    Alert.alert('Delete Item', 'Are you sure you want to delete this item?', [
      { text: 'Cancel', style: 'cancel' },
      {
        text: 'Delete',
        style: 'destructive',
        onPress: async () => {
          try {
            await itemsApi.delete(itemId);
            setItems((prev) => prev.filter((i) => i.id !== itemId));
          } catch {
            Alert.alert('Error', 'Could not delete item.');
          }
        },
      },
    ]);
  };

  if (loading) {
    return (
      <SafeAreaView style={[s.safe, { backgroundColor: colors.bg }]}>
        <ActivityIndicator size="large" color="#d4938f" style={{ marginTop: 40 }} />
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={[s.safe, { backgroundColor: colors.bg }]}>
      {items.length === 0 ? (
        <View style={s.empty}>
          <Text style={s.emptyEmoji}>📦</Text>
          <Text style={[s.emptyText, { color: colors.text2 }]}>You have no listings yet.</Text>
        </View>
      ) : (
        <FlatList
          data={items}
          keyExtractor={(item) => item.id}
          numColumns={2}
          contentContainerStyle={s.list}
          columnWrapperStyle={s.row}
          refreshing={loading}
          onRefresh={load}
          renderItem={({ item }) => (
            <View style={s.cardWrap}>
              <ItemCard item={item} onPress={handlePress} />
              <TouchableOpacity
                style={[s.deleteBtn, { borderColor: colors.border }]}
                onPress={() => handleDelete(item.id)}
              >
                <Text style={s.deleteBtnText}>Delete</Text>
              </TouchableOpacity>
            </View>
          )}
        />
      )}
    </SafeAreaView>
  );
}

const s = StyleSheet.create({
  safe: { flex: 1 },
  list: { padding: 16, paddingBottom: 32 },
  row: { gap: 12 },
  cardWrap: { flex: 1 },
  deleteBtn: {
    marginTop: 4,
    marginBottom: 12,
    borderWidth: 1,
    borderRadius: 8,
    paddingVertical: 6,
    alignItems: 'center',
  },
  deleteBtnText: { color: '#d4938f', fontSize: 12, fontWeight: '600' },
  empty: { flex: 1, alignItems: 'center', justifyContent: 'center' },
  emptyEmoji: { fontSize: 48, marginBottom: 12 },
  emptyText: { fontSize: 16 },
});
