import { useState } from 'react';
import {
  View,
  Text,
  ScrollView,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  KeyboardAvoidingView,
  Platform,
  ActivityIndicator,
  Alert,
  Image,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import { ordersApi } from '@/api/ordersApi';
import { formatPrice } from '@/utils/currency';
import type { RootStackParamList } from '@/navigation/types';

type Props = NativeStackScreenProps<RootStackParamList, 'LeaveReview'>;

const RATING_LABELS: Record<number, string> = {
  1: 'Poor · 1/5',
  2: 'Fair · 2/5',
  3: 'Good · 3/5',
  4: 'Very good · 4/5',
  5: 'Excellent · 5/5',
};

const TAGS = [
  'Fast shipping',
  'As described',
  'Great photos',
  'Friendly seller',
  'Good packaging',
  'Fair price',
];

export default function LeaveReviewScreen({ route, navigation }: Props) {
  const { paymentId, sellerName, sellerAvatarUrl, itemTitle, itemPrice } = route.params;

  const [rating, setRating] = useState(0);
  const [selectedTags, setSelectedTags] = useState<string[]>([]);
  const [content, setContent] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const toggleTag = (tag: string) => {
    setSelectedTags((prev) =>
      prev.includes(tag) ? prev.filter((t) => t !== tag) : [...prev, tag]
    );
  };

  const handleSubmit = async () => {
    if (rating === 0) {
      Alert.alert('Rating required', 'Please select a star rating before submitting.');
      return;
    }
    setSubmitting(true);
    try {
      await ordersApi.submitSellerReview(paymentId, {
        rating,
        tags: selectedTags,
        content: content.trim(),
      });
      navigation.goBack();
    } catch {
      Alert.alert('Error', 'Could not submit your review. Please try again.');
    } finally {
      setSubmitting(false);
    }
  };

  const initial = sellerName.charAt(0).toUpperCase();

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <KeyboardAvoidingView
        style={styles.kav}
        behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
        keyboardVerticalOffset={Platform.OS === 'ios' ? 90 : 80}
      >
        <ScrollView
          contentContainerStyle={styles.scroll}
          keyboardShouldPersistTaps="handled"
          showsVerticalScrollIndicator={false}
        >
          {/* Seller info */}
          <View style={styles.sellerRow}>
            <View style={styles.avatar}>
              {sellerAvatarUrl ? (
                <Image source={{ uri: sellerAvatarUrl }} style={styles.avatarImg} />
              ) : (
                <Text style={styles.avatarLetter}>{initial}</Text>
              )}
            </View>
            <View style={styles.sellerInfo}>
              <Text style={styles.sellerName}>{sellerName}</Text>
              <Text style={styles.itemSubtitle}>
                {itemTitle}
                {itemPrice != null ? ` · ${formatPrice(itemPrice)}` : ''}
              </Text>
            </View>
          </View>

          {/* Star rating */}
          <Text style={styles.sectionLabel}>OVERALL RATING</Text>
          <View style={styles.starsRow}>
            {[1, 2, 3, 4, 5].map((star) => (
              <TouchableOpacity
                key={star}
                onPress={() => setRating(star)}
                style={styles.starBtn}
                activeOpacity={0.7}
                hitSlop={{ top: 8, bottom: 8, left: 4, right: 4 }}
              >
                <Text style={[styles.star, star <= rating && styles.starActive]}>★</Text>
              </TouchableOpacity>
            ))}
          </View>
          {rating > 0 && (
            <Text style={styles.ratingLabel}>{RATING_LABELS[rating]}</Text>
          )}

          {/* Tags */}
          <Text style={styles.sectionLabel}>WHAT DID YOU LOVE?</Text>
          <View style={styles.tagsRow}>
            {TAGS.map((tag) => {
              const active = selectedTags.includes(tag);
              return (
                <TouchableOpacity
                  key={tag}
                  onPress={() => toggleTag(tag)}
                  style={[styles.tag, active && styles.tagActive]}
                  activeOpacity={0.75}
                >
                  <Text style={[styles.tagText, active && styles.tagTextActive]}>{tag}</Text>
                </TouchableOpacity>
              );
            })}
          </View>

          {/* Review text */}
          <Text style={styles.sectionLabel}>YOUR REVIEW</Text>
          <TextInput
            style={styles.textArea}
            placeholder="Share your experience with this seller…"
            placeholderTextColor="#bbb"
            value={content}
            onChangeText={setContent}
            multiline
            maxLength={1000}
            textAlignVertical="top"
          />
          {content.length > 800 && (
            <Text style={styles.charCount}>{content.length}/1000</Text>
          )}
        </ScrollView>

        {/* Submit button — always visible above keyboard */}
        <View style={styles.footer}>
          <TouchableOpacity
            style={[styles.submitBtn, (rating === 0 || submitting) && styles.submitBtnDisabled]}
            onPress={handleSubmit}
            disabled={rating === 0 || submitting}
            activeOpacity={0.8}
          >
            {submitting ? (
              <ActivityIndicator color="#fff" />
            ) : (
              <Text style={styles.submitText}>Submit Review</Text>
            )}
          </TouchableOpacity>
        </View>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: '#fafafa' },
  kav: { flex: 1 },
  scroll: { padding: 20, gap: 14, paddingBottom: 12 },

  sellerRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 14,
    backgroundColor: '#fff',
    borderRadius: 14,
    padding: 16,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 4,
    elevation: 2,
  },
  avatar: {
    width: 52,
    height: 52,
    borderRadius: 26,
    backgroundColor: '#d4938f',
    alignItems: 'center',
    justifyContent: 'center',
    overflow: 'hidden',
    flexShrink: 0,
  },
  avatarImg: { width: 52, height: 52 },
  avatarLetter: { color: '#fff', fontSize: 22, fontWeight: '700' },
  sellerInfo: { flex: 1 },
  sellerName: { fontSize: 16, fontWeight: '700', color: '#1a1a1a' },
  itemSubtitle: { fontSize: 12, color: '#888', marginTop: 2 },

  sectionLabel: {
    fontSize: 11,
    fontWeight: '700',
    color: '#aaa',
    letterSpacing: 0.8,
    textTransform: 'uppercase',
    marginTop: 4,
  },

  starsRow: {
    flexDirection: 'row',
    justifyContent: 'center',
    gap: 6,
    marginTop: 2,
  },
  starBtn: {
    padding: 4,
    minWidth: 44,
    minHeight: 44,
    alignItems: 'center',
    justifyContent: 'center',
  },
  star: { fontSize: 38, color: '#e0e0e0' },
  starActive: { color: '#f5c518' },
  ratingLabel: {
    textAlign: 'center',
    fontSize: 13,
    color: '#888',
    fontWeight: '500',
    marginTop: -6,
  },

  tagsRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
    marginTop: 2,
  },
  tag: {
    paddingHorizontal: 14,
    paddingVertical: 8,
    borderRadius: 20,
    borderWidth: 1.5,
    borderColor: '#e0d0cc',
    backgroundColor: '#fff',
  },
  tagActive: { borderColor: '#d4938f', backgroundColor: 'rgba(212,147,143,0.1)' },
  tagText: { fontSize: 13, color: '#888', fontWeight: '500' },
  tagTextActive: { color: '#d4938f', fontWeight: '600' },

  textArea: {
    backgroundColor: '#fff',
    borderRadius: 14,
    borderWidth: 1.5,
    borderColor: '#e8ddd8',
    padding: 14,
    fontSize: 15,
    color: '#1a1a1a',
    minHeight: 130,
    lineHeight: 22,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.04,
    shadowRadius: 4,
    elevation: 1,
  },
  charCount: { fontSize: 11, color: '#bbb', textAlign: 'right', marginTop: -8 },

  footer: {
    paddingHorizontal: 20,
    paddingTop: 10,
    paddingBottom: 16,
    backgroundColor: '#fafafa',
    borderTopWidth: StyleSheet.hairlineWidth,
    borderTopColor: '#e8d8cc',
  },
  submitBtn: {
    height: 52,
    borderRadius: 14,
    backgroundColor: '#d4938f',
    alignItems: 'center',
    justifyContent: 'center',
    shadowColor: '#d4938f',
    shadowOffset: { width: 0, height: 3 },
    shadowOpacity: 0.35,
    shadowRadius: 8,
    elevation: 4,
  },
  submitBtnDisabled: { opacity: 0.4, shadowOpacity: 0 },
  submitText: { color: '#fff', fontSize: 16, fontWeight: '700' },
});
