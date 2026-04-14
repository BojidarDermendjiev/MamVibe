import { useState } from 'react';
import {
  View,
  Text,
  Image,
  TouchableOpacity,
  StyleSheet,
  Dimensions,
} from 'react-native';
import { useTranslation } from 'react-i18next';
import { type Item, ListingType } from '@mamvibe/shared';
import { formatPrice } from '@/utils/currency';
import { itemsApi } from '@/api/itemsApi';
import { SERVER_URL } from '@/api/axiosClient';
import { useAuthStore } from '@/store/authStore';
import { useTheme } from '@/contexts/ThemeContext';

const CARD_WIDTH = (Dimensions.get('window').width - 48) / 2;

interface Props {
  item: Item;
  onPress: (item: Item) => void;
}

export default function ItemCard({ item, onPress }: Props) {
  const { t } = useTranslation();
  const [liked, setLiked] = useState(item.isLikedByCurrentUser);
  const [likeCount, setLikeCount] = useState(item.likeCount);
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const { colors } = useTheme();
  const photo = item.photos[0];
  const photoUri = photo?.url
    ? photo.url.startsWith('http') ? photo.url : `${SERVER_URL}${photo.url}`
    : null;

  const handleLike = async () => {
    if (!isAuthenticated) return;
    const next = !liked;
    setLiked(next);
    setLikeCount((c) => (next ? c + 1 : c - 1));
    try {
      await itemsApi.toggleLike(item.id);
    } catch {
      // revert on failure
      setLiked(!next);
      setLikeCount((c) => (next ? c - 1 : c + 1));
    }
  };

  const isDonate = item.listingType === ListingType.Donate;

  return (
    <TouchableOpacity style={[styles.card, { backgroundColor: colors.card }]} onPress={() => onPress(item)} activeOpacity={0.85}>
      <View style={styles.imageContainer}>
        {photoUri ? (
          <Image source={{ uri: photoUri }} style={styles.image} resizeMode="cover" />
        ) : (
          <View style={[styles.image, styles.imagePlaceholder]}>
            <Text style={styles.placeholderEmoji}>📦</Text>
          </View>
        )}
        <View style={[styles.badge, isDonate ? styles.badgeDonate : styles.badgeSell]}>
          <Text style={styles.badgeText}>{isDonate ? 'FREE' : 'SELL'}</Text>
        </View>
        <TouchableOpacity style={styles.likeBtn} onPress={handleLike}>
          <Text style={styles.likeIcon}>{liked ? '❤️' : '🤍'}</Text>
        </TouchableOpacity>
      </View>

      <View style={styles.info}>
        <Text style={[styles.title, { color: colors.text }]} numberOfLines={1}>{item.title}</Text>
        <Text style={[styles.category, { color: colors.text2 }]} numberOfLines={1}>{item.categoryName}</Text>
        <View style={styles.footer}>
          <Text style={styles.price}>
            {isDonate ? t('items.free') : formatPrice(item.price)}
          </Text>
          <Text style={[styles.views, { color: colors.text3 }]}>👁 {item.viewCount}</Text>
        </View>
        <Text style={[styles.likeCount, { color: colors.text3 }]}>🤍 {likeCount}</Text>
      </View>
    </TouchableOpacity>
  );
}

const styles = StyleSheet.create({
  card: {
    width: CARD_WIDTH,
    backgroundColor: '#fff',
    borderRadius: 12,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.06,
    shadowRadius: 8,
    elevation: 3,
    overflow: 'hidden',
    marginBottom: 16,
  },
  imageContainer: {
    position: 'relative',
    aspectRatio: 4 / 3,
    backgroundColor: '#f5f5f5',
  },
  image: {
    width: '100%',
    height: '100%',
  },
  imagePlaceholder: {
    alignItems: 'center',
    justifyContent: 'center',
  },
  placeholderEmoji: {
    fontSize: 32,
  },
  badge: {
    position: 'absolute',
    top: 6,
    left: 6,
    paddingHorizontal: 6,
    paddingVertical: 2,
    borderRadius: 99,
  },
  badgeDonate: {
    backgroundColor: '#8eaa89',
  },
  badgeSell: {
    backgroundColor: '#d4938f',
  },
  badgeText: {
    color: '#fff',
    fontSize: 10,
    fontWeight: '700',
    letterSpacing: 0.5,
  },
  likeBtn: {
    position: 'absolute',
    top: 4,
    right: 6,
    padding: 4,
  },
  likeIcon: {
    fontSize: 18,
  },
  info: {
    padding: 8,
  },
  title: {
    fontSize: 13,
    fontWeight: '600',
    color: '#1a1a1a',
  },
  category: {
    fontSize: 11,
    color: '#888',
    marginTop: 1,
  },
  footer: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginTop: 4,
  },
  price: {
    fontSize: 12,
    fontWeight: '700',
    color: '#c9a870',
    flexShrink: 1,
    marginRight: 4,
  },
  views: {
    fontSize: 11,
    color: '#aaa',
  },
  likeCount: {
    fontSize: 11,
    color: '#aaa',
    marginTop: 2,
  },
});
