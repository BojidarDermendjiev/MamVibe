import { useEffect, useRef, useState, useCallback } from 'react';
import {
  View,
  Text,
  ScrollView,
  TouchableOpacity,
  StyleSheet,
  Dimensions,
  Animated,
  ActivityIndicator,
} from 'react-native';
import type { CompositeScreenProps } from '@react-navigation/native';
import type { BottomTabScreenProps } from '@react-navigation/bottom-tabs';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { MainTabParamList, RootStackParamList } from '@/navigation/types';
import { useItems } from '@/hooks/useItems';
import ItemCard from '@/components/ItemCard';
import type { Item } from '@mamvibe/shared';

type Props = CompositeScreenProps<
  BottomTabScreenProps<MainTabParamList, 'Home'>,
  NativeStackScreenProps<RootStackParamList>
>;

const { width: W } = Dimensions.get('window');
const PRIMARY = '#e91e8c';
const PRIMARY_DARK = '#945c67';
const PEACH = '#E8724A';

const TITLE_WORDS = ['amazing', 'stylish', 'vibrant', 'special', 'lovely'];

const STEPS = [
  { emoji: '📷', title: 'Snap a Photo',      desc: 'Take a photo of items to donate or sell.',     bg: '#FDDDD6', badge: '1' },
  { emoji: '🏷️',  title: 'Set Your Terms',   desc: 'Donate for free or set a fair price.',          bg: '#D4EDE8', badge: '2' },
  { emoji: '🚚',  title: 'Arrange Pickup',   desc: 'Connect with families and arrange exchange.',   bg: '#F5E6C0', badge: '3' },
  { emoji: '💝',  title: 'Make a Difference', desc: 'Give items a second life for families.',        bg: '#FDDDE0', badge: '4' },
];

const AGE_GROUPS = [
  { emoji: '👶', label: 'Newborn',   range: '0–3 mo',  bg: '#FDDDD6' },
  { emoji: '😊', label: 'Infant',    range: '3–12 mo', bg: '#D4EDE8' },
  { emoji: '👟', label: 'Toddler',   range: '1–2 yr',  bg: '#F5E6C0' },
  { emoji: '👕', label: 'Preschool', range: '3–4 yr',  bg: '#FDDDE0' },
  { emoji: '🧒', label: 'Kids',      range: '5–6 yr',  bg: '#D4E8E8' },
];

export default function HomeScreen({ navigation }: Props) {
  const { items, loading } = useItems({ sortBy: 'newest' });

  const [titleIndex, setTitleIndex] = useState(0);
  const opacity = useRef(new Animated.Value(1)).current;

  useEffect(() => {
    const id = setInterval(() => {
      Animated.sequence([
        Animated.timing(opacity, { toValue: 0, duration: 280, useNativeDriver: true }),
        Animated.timing(opacity, { toValue: 1, duration: 280, useNativeDriver: true }),
      ]).start();
      setTitleIndex((i) => (i + 1) % TITLE_WORDS.length);
    }, 2200);
    return () => clearInterval(id);
  }, [opacity]);

  const handleItemPress = useCallback(
    (item: Item) => navigation.navigate('ItemDetail', { itemId: item.id }),
    [navigation],
  );

  const recentItems = items.slice(0, 6);

  return (
    <ScrollView style={styles.root} showsVerticalScrollIndicator={false}>

      {/* ── Hero ── */}
      <View style={styles.hero}>
        <View style={styles.heroBg} />
        <View style={styles.heroContent}>
          <Text style={styles.heroTitle}>Give Baby Items{'\n'}a Second Life</Text>
          <Animated.Text style={[styles.heroWord, { opacity }]}>
            {TITLE_WORDS[titleIndex]}
          </Animated.Text>
          <Text style={styles.heroSubtitle}>
            Donate or sell baby clothes, strollers, and more to families who need them.
          </Text>
          <View style={styles.heroRow}>
            <TouchableOpacity
              style={styles.heroBtn}
              onPress={() => navigation.navigate('Browse' as any)}
            >
              <Text style={styles.heroBtnText}>Browse Items  →</Text>
            </TouchableOpacity>
          </View>
        </View>
      </View>

      {/* ── How it works ── */}
      <View style={[styles.section, { backgroundColor: '#FAF3EE' }]}>
        <Text style={styles.sectionTitle}>How It Works</Text>
        <Text style={styles.sectionSub}>List or find baby items in a few simple steps</Text>
        <View style={styles.stepsGrid}>
          {STEPS.map((step, i) => (
            <View key={i} style={styles.stepCard}>
              <View style={styles.stepIconWrap}>
                <View style={[styles.stepIcon, { backgroundColor: step.bg }]}>
                  <Text style={styles.stepEmoji}>{step.emoji}</Text>
                </View>
                <View style={styles.stepBadge}>
                  <Text style={styles.stepBadgeText}>{step.badge}</Text>
                </View>
              </View>
              <Text style={styles.stepTitle}>{step.title}</Text>
              <Text style={styles.stepDesc}>{step.desc}</Text>
            </View>
          ))}
        </View>
      </View>

      {/* ── Shop by Age ── */}
      <View style={[styles.section, { backgroundColor: '#FAF3EE', paddingBottom: 32 }]}>
        <Text style={styles.sectionTitle}>Shop by Age</Text>
        <Text style={styles.sectionSub}>Find the perfect size for your little one</Text>
        <ScrollView
          horizontal
          showsHorizontalScrollIndicator={false}
          style={{ marginTop: 16 }}
          contentContainerStyle={styles.ageScroll}
        >
          {AGE_GROUPS.map((g) => (
            <TouchableOpacity
              key={g.label}
              style={styles.ageCard}
              onPress={() => navigation.navigate('Browse' as any)}
            >
              <View style={[styles.ageIcon, { backgroundColor: g.bg }]}>
                <Text style={styles.ageEmoji}>{g.emoji}</Text>
              </View>
              <Text style={styles.ageLabel}>{g.label}</Text>
              <Text style={styles.ageRange}>{g.range}</Text>
            </TouchableOpacity>
          ))}
        </ScrollView>
      </View>

      {/* ── Recent Listings ── */}
      <View style={[styles.section, { backgroundColor: '#fff' }]}>
        <Text style={styles.sectionTitle}>Recent Listings</Text>
        {loading ? (
          <ActivityIndicator color={PRIMARY} style={{ marginTop: 20 }} />
        ) : recentItems.length === 0 ? (
          <Text style={styles.emptyText}>No listings yet</Text>
        ) : (
          <View style={styles.itemsGrid}>
            {recentItems.map((item, index) => (
              <View key={item.id} style={index % 2 === 0 ? styles.cardLeft : styles.cardRight}>
                <ItemCard item={item} onPress={handleItemPress} />
              </View>
            ))}
          </View>
        )}
        <TouchableOpacity
          style={styles.viewAllBtn}
          onPress={() => navigation.navigate('Browse' as any)}
        >
          <Text style={styles.viewAllText}>View all listings  →</Text>
        </TouchableOpacity>
      </View>

      {/* ── Support ── */}
      <View style={styles.supportSection}>
        <Text style={styles.supportHeart}>💜</Text>
        <Text style={styles.supportTitle}>MamVibe is free. Always.</Text>
        <Text style={styles.supportBody}>
          No ads, no sponsors, no catch. If this place has made your family's life even a little easier, a small coffee keeps the lights on.
        </Text>
      </View>

    </ScrollView>
  );
}

const styles = StyleSheet.create({
  root: { flex: 1, backgroundColor: '#fff' },

  /* Hero */
  hero: { minHeight: 280, overflow: 'hidden' },
  heroBg: {
    ...StyleSheet.absoluteFillObject,
    backgroundColor: '#2b1d35',
  },
  heroContent: { padding: 28, paddingTop: 36, paddingBottom: 36 },
  heroTitle: { fontSize: 26, fontWeight: '800', color: '#fff', lineHeight: 34, marginBottom: 10 },
  heroWord: {
    fontSize: 28,
    fontWeight: '800',
    color: '#F4A261',
    marginBottom: 12,
    letterSpacing: 0.5,
  },
  heroSubtitle: { fontSize: 14, color: 'rgba(255,255,255,0.8)', lineHeight: 21, marginBottom: 24 },
  heroRow: { flexDirection: 'row', gap: 10 },
  heroBtn: {
    borderWidth: 2,
    borderColor: '#fff',
    borderRadius: 10,
    paddingHorizontal: 20,
    paddingVertical: 10,
  },
  heroBtnText: { color: '#fff', fontWeight: '700', fontSize: 14 },

  /* Sections */
  section: { paddingHorizontal: 20, paddingTop: 32, paddingBottom: 20 },
  sectionTitle: { fontSize: 22, fontWeight: '800', color: PRIMARY_DARK, marginBottom: 4 },
  sectionSub: { fontSize: 13, color: '#888', marginBottom: 4 },

  /* How it works */
  stepsGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 16,
    marginTop: 20,
  },
  stepCard: {
    width: (W - 56) / 2,
    alignItems: 'center',
  },
  stepIconWrap: { position: 'relative', marginBottom: 10 },
  stepIcon: {
    width: 64,
    height: 64,
    borderRadius: 16,
    alignItems: 'center',
    justifyContent: 'center',
  },
  stepEmoji: { fontSize: 28 },
  stepBadge: {
    position: 'absolute',
    top: -6,
    right: -6,
    width: 22,
    height: 22,
    borderRadius: 11,
    backgroundColor: PEACH,
    alignItems: 'center',
    justifyContent: 'center',
  },
  stepBadgeText: { color: '#fff', fontSize: 11, fontWeight: '700' },
  stepTitle: { fontSize: 13, fontWeight: '700', color: '#2b1d35', textAlign: 'center', marginBottom: 4 },
  stepDesc: { fontSize: 11, color: '#777', textAlign: 'center', lineHeight: 16 },

  /* Age groups */
  ageScroll: { paddingHorizontal: 4, gap: 12, flexDirection: 'row' },
  ageCard: {
    width: 88,
    backgroundColor: '#fff',
    borderRadius: 16,
    padding: 12,
    alignItems: 'center',
    shadowColor: '#000',
    shadowOpacity: 0.06,
    shadowOffset: { width: 0, height: 2 },
    shadowRadius: 6,
    elevation: 2,
    borderWidth: 1,
    borderColor: '#f0f0f0',
  },
  ageIcon: { width: 48, height: 48, borderRadius: 12, alignItems: 'center', justifyContent: 'center', marginBottom: 8 },
  ageEmoji: { fontSize: 22 },
  ageLabel: { fontSize: 12, fontWeight: '700', color: '#2b1d35', textAlign: 'center' },
  ageRange: { fontSize: 10, color: '#aaa', marginTop: 2, textAlign: 'center' },

  /* Recent items */
  itemsGrid: { flexDirection: 'row', flexWrap: 'wrap', marginTop: 16 },
  cardLeft: { flex: 1, marginRight: 8 },
  cardRight: { flex: 1, marginLeft: 8 },
  emptyText: { color: '#aaa', textAlign: 'center', marginTop: 20, fontSize: 14 },
  viewAllBtn: {
    marginTop: 12,
    alignSelf: 'center',
    paddingHorizontal: 24,
    paddingVertical: 10,
    borderRadius: 99,
    borderWidth: 2,
    borderColor: PRIMARY,
  },
  viewAllText: { color: PRIMARY, fontWeight: '700', fontSize: 13 },

  /* Support */
  supportSection: {
    backgroundColor: '#fff',
    paddingHorizontal: 28,
    paddingVertical: 40,
    alignItems: 'center',
    borderTopWidth: 1,
    borderTopColor: '#f0f0f0',
  },
  supportHeart: { fontSize: 40, marginBottom: 12 },
  supportTitle: { fontSize: 20, fontWeight: '800', color: PRIMARY_DARK, textAlign: 'center', marginBottom: 10 },
  supportBody: { fontSize: 13, color: '#777', textAlign: 'center', lineHeight: 20, maxWidth: 300, marginBottom: 28 },
});
