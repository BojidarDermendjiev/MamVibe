import { useEffect, useRef, useState } from 'react';
import {
  View,
  Text,
  ScrollView,
  TouchableOpacity,
  StyleSheet,
  Dimensions,
  Animated,
} from 'react-native';
import type { CompositeScreenProps } from '@react-navigation/native';
import type { BottomTabScreenProps } from '@react-navigation/bottom-tabs';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { MainTabParamList, RootStackParamList } from '@/navigation/types';
import { useTheme } from '@/contexts/ThemeContext';

type Props = CompositeScreenProps<
  BottomTabScreenProps<MainTabParamList, 'Home'>,
  NativeStackScreenProps<RootStackParamList>
>;

const { width: W } = Dimensions.get('window');
const PRIMARY    = '#e91e8c';
const PRIMARY_DARK = '#945c67';
const PEACH      = '#E8724A';

const TITLE_WORDS = ['amazing', 'stylish', 'vibrant', 'special', 'lovely'];

const STEPS = [
  { emoji: '📷', title: 'Snap a Photo',       desc: 'Take a photo of items to donate or sell.',   bg: '#FDDDD6', badge: '1' },
  { emoji: '🏷️',  title: 'Set Your Terms',    desc: 'Donate for free or set a fair price.',        bg: '#D4EDE8', badge: '2' },
  { emoji: '🚚',  title: 'Arrange Pickup',    desc: 'Connect with families and arrange exchange.', bg: '#F5E6C0', badge: '3' },
  { emoji: '💝',  title: 'Make a Difference', desc: 'Give items a beautiful second life.',         bg: '#FDDDE0', badge: '4' },
];

const AGE_GROUPS = [
  { emoji: '👶', label: 'Newborn',   range: '0–3 mo',  bg: '#FDDDD6' },
  { emoji: '😊', label: 'Infant',    range: '3–12 mo', bg: '#D4EDE8' },
  { emoji: '👟', label: 'Toddler',   range: '1–2 yr',  bg: '#F5E6C0' },
  { emoji: '👕', label: 'Preschool', range: '3–4 yr',  bg: '#FDDDE0' },
  { emoji: '🧒', label: 'Kids',      range: '5–6 yr',  bg: '#D4E8E8' },
];

export default function HomeScreen({ navigation }: Props) {
  const { colors, isDark } = useTheme();

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

  const sectionBg = isDark ? '#201d30' : '#FAF3EE';

  return (
    <ScrollView style={{ flex: 1, backgroundColor: colors.bg }} showsVerticalScrollIndicator={false}>

      {/* ── Hero ── */}
      <View style={[s.hero, { backgroundColor: isDark ? '#2b1d35' : '#5c3d5e' }]}>
        <View style={s.heroContent}>
          <Text style={s.heroTitle}>Give Baby Items{'\n'}a Second Life</Text>
          <Animated.Text style={[s.heroWord, { opacity }]}>
            {TITLE_WORDS[titleIndex]}
          </Animated.Text>
          <Text style={s.heroSubtitle}>
            Donate or sell baby clothes, strollers, and more to families who need them.
          </Text>
          <View style={s.heroRow}>
            <TouchableOpacity style={s.heroBtn} onPress={() => navigation.navigate('Browse' as any)}>
              <Text style={s.heroBtnText}>Browse Items  →</Text>
            </TouchableOpacity>
          </View>
        </View>
      </View>

      {/* ── How it works ── */}
      <View style={[s.section, { backgroundColor: sectionBg }]}>
        <Text style={[s.sectionTitle, { color: PRIMARY_DARK }]}>How It Works</Text>
        <Text style={[s.sectionSub, { color: colors.text2 }]}>List or find baby items in a few simple steps</Text>
        <View style={s.stepsGrid}>
          {STEPS.map((step, i) => (
            <View key={i} style={[s.stepCard, { backgroundColor: colors.card }]}>
              <View style={s.stepIconWrap}>
                <View style={[s.stepIcon, { backgroundColor: step.bg }]}>
                  <Text style={s.stepEmoji}>{step.emoji}</Text>
                </View>
                <View style={s.stepBadge}>
                  <Text style={s.stepBadgeText}>{step.badge}</Text>
                </View>
              </View>
              <Text style={[s.stepTitle, { color: colors.text }]}>{step.title}</Text>
              <Text style={[s.stepDesc, { color: colors.text2 }]}>{step.desc}</Text>
            </View>
          ))}
        </View>
      </View>

      {/* ── Shop by Age ── */}
      <View style={[s.section, { backgroundColor: sectionBg, paddingBottom: 32 }]}>
        <Text style={[s.sectionTitle, { color: PRIMARY_DARK }]}>Shop by Age</Text>
        <Text style={[s.sectionSub, { color: colors.text2 }]}>Find the perfect size for your little one</Text>
        {/* 2-col wrap grid — matches the web layout */}
        <View style={s.ageGrid}>
          {AGE_GROUPS.map((g) => (
            <TouchableOpacity
              key={g.label}
              style={[s.ageCard, { backgroundColor: colors.card, borderColor: colors.border }]}
              onPress={() => navigation.navigate('Browse' as any)}
            >
              <View style={[s.ageIcon, { backgroundColor: g.bg }]}>
                <Text style={s.ageEmoji}>{g.emoji}</Text>
              </View>
              <Text style={[s.ageLabel, { color: colors.text }]}>{g.label}</Text>
              <Text style={[s.ageRange, { color: colors.text2 }]}>{g.range}</Text>
            </TouchableOpacity>
          ))}
        </View>
      </View>

      {/* ── Support ── */}
      <View style={[s.supportSection, { backgroundColor: colors.section, borderTopColor: colors.border }]}>
        <Text style={s.supportHeart}>💜</Text>
        <Text style={[s.supportTitle, { color: PRIMARY_DARK }]}>MamVibe is free. Always.</Text>
        <Text style={[s.supportBody, { color: colors.text2 }]}>
          No ads, no sponsors, no catch. If this place has made your family's life even a little easier, a small coffee keeps the lights on.
        </Text>
      </View>

    </ScrollView>
  );
}

const AGE_CARD_W = (W - 20 * 2 - 12) / 2; // 2-col with 12px gap

const s = StyleSheet.create({
  /* Hero */
  hero: { minHeight: 280, overflow: 'hidden' },
  heroContent: { padding: 28, paddingTop: 40, paddingBottom: 36 },
  heroTitle: { fontSize: 26, fontWeight: '800', color: '#fff', lineHeight: 34, marginBottom: 10 },
  heroWord: { fontSize: 28, fontWeight: '800', color: '#F4A261', marginBottom: 12, letterSpacing: 0.5 },
  heroSubtitle: { fontSize: 14, color: 'rgba(255,255,255,0.8)', lineHeight: 21, marginBottom: 24 },
  heroRow: { flexDirection: 'row' },
  heroBtn: { borderWidth: 2, borderColor: '#fff', borderRadius: 10, paddingHorizontal: 20, paddingVertical: 10 },
  heroBtnText: { color: '#fff', fontWeight: '700', fontSize: 14 },

  /* Section */
  section: { paddingHorizontal: 20, paddingTop: 32, paddingBottom: 20 },
  sectionTitle: { fontSize: 22, fontWeight: '800', marginBottom: 4 },
  sectionSub: { fontSize: 13, marginBottom: 4 },

  /* Steps 2×2 grid */
  stepsGrid: { flexDirection: 'row', flexWrap: 'wrap', gap: 12, marginTop: 20 },
  stepCard: {
    width: (W - 52) / 2,
    borderRadius: 16,
    padding: 16,
    alignItems: 'center',
    shadowColor: '#000',
    shadowOpacity: 0.08,
    shadowOffset: { width: 0, height: 2 },
    shadowRadius: 8,
    elevation: 3,
  },
  stepIconWrap: { position: 'relative', marginBottom: 10 },
  stepIcon: { width: 64, height: 64, borderRadius: 16, alignItems: 'center', justifyContent: 'center' },
  stepEmoji: { fontSize: 28 },
  stepBadge: { position: 'absolute', top: -6, right: -6, width: 22, height: 22, borderRadius: 11, backgroundColor: PEACH, alignItems: 'center', justifyContent: 'center' },
  stepBadgeText: { color: '#fff', fontSize: 11, fontWeight: '700' },
  stepTitle: { fontSize: 13, fontWeight: '700', textAlign: 'center', marginBottom: 4 },
  stepDesc: { fontSize: 11, textAlign: 'center', lineHeight: 16 },

  /* Age — 2-col wrap grid */
  ageGrid: { flexDirection: 'row', flexWrap: 'wrap', gap: 12, marginTop: 16 },
  ageCard: {
    width: AGE_CARD_W,
    borderRadius: 16,
    padding: 16,
    alignItems: 'center',
    shadowColor: '#000',
    shadowOpacity: 0.06,
    shadowOffset: { width: 0, height: 2 },
    shadowRadius: 6,
    elevation: 2,
    borderWidth: 1,
  },
  ageIcon: { width: 52, height: 52, borderRadius: 14, alignItems: 'center', justifyContent: 'center', marginBottom: 10 },
  ageEmoji: { fontSize: 24 },
  ageLabel: { fontSize: 13, fontWeight: '700', textAlign: 'center' },
  ageRange: { fontSize: 11, marginTop: 2, textAlign: 'center' },

  /* Support */
  supportSection: { paddingHorizontal: 28, paddingVertical: 40, alignItems: 'center', borderTopWidth: 1 },
  supportHeart: { fontSize: 40, marginBottom: 12 },
  supportTitle: { fontSize: 20, fontWeight: '800', textAlign: 'center', marginBottom: 10 },
  supportBody: { fontSize: 13, textAlign: 'center', lineHeight: 20, maxWidth: 300 },
});
