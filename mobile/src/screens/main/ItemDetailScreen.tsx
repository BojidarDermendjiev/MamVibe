import { useEffect, useRef, useState } from 'react';
import {
  View,
  Text,
  Image,
  ScrollView,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
  Alert,
  Dimensions,
  FlatList,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { RootStackParamList } from '@/navigation/types';
import { itemsApi } from '@/api/itemsApi';
import { useAuthStore } from '@/store/authStore';
import { formatPrice } from '@/utils/currency';
import { type Item, ListingType } from '@mamvibe/shared';

type Props = NativeStackScreenProps<RootStackParamList, 'ItemDetail'>;

const { width: SCREEN_WIDTH } = Dimensions.get('window');

export default function ItemDetailScreen({ route, navigation }: Props) {
  const { itemId } = route.params;
  const { user } = useAuthStore();
  const [item, setItem] = useState<Item | null>(null);
  const [loading, setLoading] = useState(true);
  const [activePhoto, setActivePhoto] = useState(0);
  const [liked, setLiked] = useState(false);
  const [likeCount, setLikeCount] = useState(0);
  const viewCounted = useRef(false);

  useEffect(() => {
    itemsApi.getById(itemId)
      .then(({ data }) => {
        setItem(data);
        setLiked(data.isLikedByCurrentUser);
        setLikeCount(data.likeCount);
        if (!viewCounted.current) {
          viewCounted.current = true;
          itemsApi.incrementView(itemId).catch(() => {});
        }
      })
      .catch(() => {
        Alert.alert('Error', 'Item not found');
        navigation.goBack();
      })
      .finally(() => setLoading(false));
  }, [itemId]);

  const handleLike = async () => {
    if (!user) return;
    const next = !liked;
    setLiked(next);
    setLikeCount((c) => (next ? c + 1 : c - 1));
    try {
      await itemsApi.toggleLike(itemId);
    } catch {
      setLiked(!next);
      setLikeCount((c) => (next ? c - 1 : c + 1));
    }
  };

  const handleRequestPurchase = () => {
    if (!item) return;
    // Navigate to full checkout flow (delivery + payment method + Stripe)
    (navigation as any).navigate('Payment', { itemId: item.id });
  };

  const handleDelete = async () => {
    if (!item) return;
    Alert.alert('Delete Item', 'Are you sure you want to delete this item?', [
      { text: 'Cancel', style: 'cancel' },
      {
        text: 'Delete',
        style: 'destructive',
        onPress: async () => {
          try {
            await itemsApi.delete(item.id);
            navigation.goBack();
          } catch {
            Alert.alert('Error', 'Could not delete item.');
          }
        },
      },
    ]);
  };

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color="#e91e8c" />
      </View>
    );
  }

  if (!item) return null;

  const isOwner = user?.id === item.userId;
  const isDonate = item.listingType === ListingType.Donate;
  const photos = item.photos;

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <ScrollView showsVerticalScrollIndicator={false}>
        {/* Photo gallery */}
        <View style={styles.galleryContainer}>
          {photos.length > 0 ? (
            <>
              <FlatList
                data={photos}
                keyExtractor={(p) => p.id}
                horizontal
                pagingEnabled
                showsHorizontalScrollIndicator={false}
                onScroll={(e) => {
                  const idx = Math.round(e.nativeEvent.contentOffset.x / SCREEN_WIDTH);
                  setActivePhoto(idx);
                }}
                scrollEventThrottle={16}
                renderItem={({ item: photo }) => (
                  <Image source={{ uri: photo.url }} style={styles.mainPhoto} resizeMode="cover" />
                )}
              />
              {photos.length > 1 && (
                <View style={styles.dots}>
                  {photos.map((_, i) => (
                    <View key={i} style={[styles.dot, i === activePhoto && styles.dotActive]} />
                  ))}
                </View>
              )}
            </>
          ) : (
            <View style={[styles.mainPhoto, styles.photoPlaceholder]}>
              <Text style={{ fontSize: 64 }}>📦</Text>
            </View>
          )}
        </View>

        <View style={styles.content}>
          {/* Badge + like row */}
          <View style={styles.badgeRow}>
            <View style={[styles.badge, isDonate ? styles.badgeDonate : styles.badgeSell]}>
              <Text style={styles.badgeText}>{isDonate ? 'FREE' : 'FOR SALE'}</Text>
            </View>
            <TouchableOpacity onPress={handleLike} style={styles.likeBtn}>
              <Text style={styles.likeIcon}>{liked ? '❤️' : '🤍'}</Text>
              <Text style={styles.likeCountText}>{likeCount}</Text>
            </TouchableOpacity>
          </View>

          <Text style={styles.title}>{item.title}</Text>
          <Text style={styles.category}>{item.categoryName}</Text>

          <Text style={styles.price}>
            {isDonate ? 'Free' : formatPrice(item.price)}
          </Text>

          {/* Stats */}
          <View style={styles.statsRow}>
            <Text style={styles.stat}>👁 {item.viewCount} views</Text>
            <Text style={styles.stat}>
              🕒 {new Date(item.createdAt).toLocaleDateString('en-GB')}
            </Text>
          </View>

          {/* Description */}
          <Text style={styles.sectionLabel}>Description</Text>
          <Text style={styles.description}>{item.description}</Text>

          {/* Seller */}
          <Text style={styles.sectionLabel}>Seller</Text>
          <View style={styles.sellerCard}>
            <View style={styles.sellerAvatar}>
              {item.userAvatarUrl ? (
                <Image source={{ uri: item.userAvatarUrl }} style={styles.avatarImg} />
              ) : (
                <Text style={styles.avatarFallback}>
                  {item.userDisplayName.charAt(0).toUpperCase()}
                </Text>
              )}
            </View>
            <Text style={styles.sellerName}>{item.userDisplayName}</Text>
          </View>

          {/* Owner actions */}
          {isOwner && (
            <TouchableOpacity style={styles.deleteBtn} onPress={handleDelete}>
              <Text style={styles.deleteBtnText}>Delete Item</Text>
            </TouchableOpacity>
          )}

          {/* Buyer actions */}
          {!isOwner && user && item.isActive && (
            <View style={styles.actions}>
              <TouchableOpacity
                style={[styles.actionBtn, styles.actionBtnOutline]}
                onPress={() =>
                  (navigation as any).navigate('ChatTab', {
                    screen: 'Conversation',
                    params: {
                      userId: item.userId,
                      displayName: item.userDisplayName,
                      avatarUrl: item.userAvatarUrl,
                    },
                  })
                }
              >
                <Text style={styles.actionBtnOutlineText}>💬 Contact Seller</Text>
              </TouchableOpacity>
              <TouchableOpacity
                style={[styles.actionBtn, isDonate ? styles.actionBtnGreen : styles.actionBtnPrimary]}
                onPress={handleRequestPurchase}
              >
                <Text style={styles.actionBtnText}>
                  {isDonate ? 'Book for Free' : 'Buy Now'}
                </Text>
              </TouchableOpacity>
            </View>
          )}

          {!isOwner && user && !item.isActive && (
            <View style={styles.unavailableBanner}>
              <Text style={styles.unavailableText}>This item is no longer available</Text>
            </View>
          )}
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: '#fff' },
  center: { flex: 1, alignItems: 'center', justifyContent: 'center' },
  galleryContainer: { position: 'relative' },
  mainPhoto: {
    width: SCREEN_WIDTH,
    height: SCREEN_WIDTH,
    backgroundColor: '#f5f5f5',
  },
  photoPlaceholder: {
    alignItems: 'center',
    justifyContent: 'center',
  },
  dots: {
    position: 'absolute',
    bottom: 10,
    width: '100%',
    flexDirection: 'row',
    justifyContent: 'center',
    gap: 5,
  },
  dot: {
    width: 6,
    height: 6,
    borderRadius: 3,
    backgroundColor: 'rgba(255,255,255,0.5)',
  },
  dotActive: {
    backgroundColor: '#fff',
  },
  content: {
    padding: 20,
  },
  badgeRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 10,
  },
  badge: {
    paddingHorizontal: 10,
    paddingVertical: 4,
    borderRadius: 99,
  },
  badgeSell: { backgroundColor: '#e91e8c' },
  badgeDonate: { backgroundColor: '#22c55e' },
  badgeText: { color: '#fff', fontSize: 11, fontWeight: '700', letterSpacing: 0.5 },
  likeBtn: { flexDirection: 'row', alignItems: 'center', gap: 4 },
  likeIcon: { fontSize: 22 },
  likeCountText: { fontSize: 14, color: '#555' },
  title: { fontSize: 22, fontWeight: '700', color: '#1a1a1a', marginBottom: 4 },
  category: { fontSize: 13, color: '#888', marginBottom: 12 },
  price: { fontSize: 24, fontWeight: '800', color: '#e91e8c', marginBottom: 12 },
  statsRow: { flexDirection: 'row', gap: 16, marginBottom: 20 },
  stat: { fontSize: 13, color: '#aaa' },
  sectionLabel: { fontSize: 13, fontWeight: '700', color: '#333', marginBottom: 8, textTransform: 'uppercase', letterSpacing: 0.5 },
  description: { fontSize: 15, color: '#444', lineHeight: 22, marginBottom: 24 },
  sellerCard: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    padding: 14,
    backgroundColor: '#fdf4f8',
    borderRadius: 12,
    marginBottom: 24,
  },
  sellerAvatar: {
    width: 42,
    height: 42,
    borderRadius: 21,
    backgroundColor: '#e91e8c',
    alignItems: 'center',
    justifyContent: 'center',
    overflow: 'hidden',
  },
  avatarImg: { width: 42, height: 42 },
  avatarFallback: { color: '#fff', fontSize: 18, fontWeight: '700' },
  sellerName: { fontSize: 15, fontWeight: '600', color: '#1a1a1a' },
  actions: { gap: 10 },
  actionBtn: {
    height: 50,
    borderRadius: 12,
    alignItems: 'center',
    justifyContent: 'center',
  },
  actionBtnPrimary: { backgroundColor: '#e91e8c' },
  actionBtnGreen: { backgroundColor: '#22c55e' },
  actionBtnOutline: { borderWidth: 1.5, borderColor: '#e91e8c' },
  actionBtnDisabled: { opacity: 0.5 },
  actionBtnText: { color: '#fff', fontSize: 16, fontWeight: '600' },
  actionBtnOutlineText: { color: '#e91e8c', fontSize: 16, fontWeight: '600' },
  deleteBtn: {
    height: 48,
    borderRadius: 12,
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 1,
    borderColor: '#ef4444',
  },
  deleteBtnText: { color: '#ef4444', fontSize: 15, fontWeight: '600' },
  unavailableBanner: {
    padding: 16,
    backgroundColor: '#f5f5f5',
    borderRadius: 12,
    alignItems: 'center',
  },
  unavailableText: { color: '#888', fontSize: 15 },
});
