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
import { useTranslation } from 'react-i18next';
import type { CompositeScreenProps } from '@react-navigation/native';
import type { BottomTabScreenProps } from '@react-navigation/bottom-tabs';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { MainTabParamList, RootStackParamList } from '@/navigation/types';
import { useTheme } from '@/contexts/ThemeContext';
import { ROSE, MIST, SAGE, SAND } from '@/constants/palette';

type Props = CompositeScreenProps<
  BottomTabScreenProps<MainTabParamList, 'Home'>,
  NativeStackScreenProps<RootStackParamList>
>;

const { width: W } = Dimensions.get('window');

const TITLE_WORDS = ['amazing', 'stylish', 'vibrant', 'special', 'lovely'];

const STEP_META = [
  { emoji: '📷', bg: 'rgba(245,237,229,0.8)', badge: '1' },
  { emoji: '🏷️', bg: 'rgba(142,170,137,0.25)', badge: '2' },
  { emoji: '🚚', bg: 'rgba(201,168,112,0.25)', badge: '3' },
  { emoji: '💝', bg: 'rgba(212,147,143,0.2)', badge: '4' },
];

const AGE_META = [
  { emoji: '👶', bg: 'rgba(245,237,229,0.8)' },
  { emoji: '😊', bg: 'rgba(142,170,137,0.25)' },
  { emoji: '👟', bg: 'rgba(201,168,112,0.25)' },
  { emoji: '👕', bg: 'rgba(212,147,143,0.2)' },
  { emoji: '🧒', bg: 'rgba(142,170,137,0.2)' },
];

export default function HomeScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const { colors, isDark } = useTheme();

  const STEPS = STEP_META.map((m, i) => ({
    ...m,
    title: t(`home.step${i}_title` as any),
    desc:  t(`home.step${i}_desc`  as any),
  }));

  const AGE_GROUPS = AGE_META.map((m, i) => ({
    ...m,
    label: t(`home.age${i}_label` as any),
    range: t(`home.age${i}_range` as any),
  }));

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

  const heroBg = isDark ? '#2a1c1b' : '#3d2424';
  const sectionBg = isDark ? colors.section : MIST;

  return (
    <ScrollView style={{ flex: 1, backgroundColor: colors.bg }} showsVerticalScrollIndicator={false}>

      {/* ── Hero ── */}
      <View style={[s.hero, { backgroundColor: heroBg }]}>
        <View style={s.heroContent}>
          <Text style={s.heroTitle}>{t('home.heroTitle')}</Text>
          <Animated.Text style={[s.heroWord, { opacity }]}>
            {TITLE_WORDS[titleIndex]}
          </Animated.Text>
          <Text style={s.heroSubtitle}>{t('home.heroSubtitle')}</Text>
          <View style={s.heroRow}>
            <TouchableOpacity style={s.heroBtn} onPress={() => navigation.navigate('Browse' as any)}>
              <Text style={s.heroBtnText}>{t('home.heroBtn')}</Text>
            </TouchableOpacity>
          </View>
        </View>
      </View>

      {/* ── How it works ── */}
      <View style={[s.section, { backgroundColor: sectionBg }]}>
        <Text style={[s.sectionTitle, { color: SAGE }]}>{t('home.howItWorks')}</Text>
        <Text style={[s.sectionSub, { color: colors.text2 }]}>{t('home.howItWorksSub')}</Text>
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
        <Text style={[s.sectionTitle, { color: SAGE }]}>{t('home.shopByAge')}</Text>
        <Text style={[s.sectionSub, { color: colors.text2 }]}>{t('home.shopByAgeSub')}</Text>
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
        <Text style={s.supportHeart}>💛</Text>
        <Text style={[s.supportTitle, { color: SAGE }]}>{t('home.supportTitle')}</Text>
        <Text style={[s.supportBody, { color: colors.text2 }]}>{t('home.supportBody')}</Text>
        <TouchableOpacity
          style={s.donateBtn}
          onPress={() => (navigation as any).navigate('Donate')}
          activeOpacity={0.85}
        >
          <Text style={s.donateBtnText}>{t('home.donateBtn')}</Text>
        </TouchableOpacity>
      </View>

    </ScrollView>
  );
}

const AGE_CARD_W = (W - 20 * 2 - 12) / 2;

const s = StyleSheet.create({
  hero: { minHeight: 280, overflow: 'hidden' },
  heroContent: { padding: 28, paddingTop: 40, paddingBottom: 36 },
  heroTitle: { fontSize: 26, fontWeight: '800', color: '#fff', lineHeight: 34, marginBottom: 10 },
  heroWord: { fontSize: 28, fontWeight: '800', color: SAND, marginBottom: 12, letterSpacing: 0.5 },
  heroSubtitle: { fontSize: 14, color: 'rgba(255,255,255,0.8)', lineHeight: 21, marginBottom: 24 },
  heroRow: { flexDirection: 'row' },
  heroBtn: { borderWidth: 2, borderColor: '#fff', borderRadius: 10, paddingHorizontal: 20, paddingVertical: 10 },
  heroBtnText: { color: '#fff', fontWeight: '700', fontSize: 14 },

  section: { paddingHorizontal: 20, paddingTop: 32, paddingBottom: 20 },
  sectionTitle: { fontSize: 22, fontWeight: '800', marginBottom: 4 },
  sectionSub: { fontSize: 13, marginBottom: 4 },

  stepsGrid: { flexDirection: 'row', flexWrap: 'wrap', gap: 12, marginTop: 20 },
  stepCard: {
    width: (W - 52) / 2,
    borderRadius: 16,
    padding: 16,
    alignItems: 'center',
    shadowColor: '#000',
    shadowOpacity: 0.06,
    shadowOffset: { width: 0, height: 2 },
    shadowRadius: 8,
    elevation: 3,
  },
  stepIconWrap: { position: 'relative', marginBottom: 10 },
  stepIcon: { width: 64, height: 64, borderRadius: 16, alignItems: 'center', justifyContent: 'center' },
  stepEmoji: { fontSize: 28 },
  stepBadge: { position: 'absolute', top: -6, right: -6, width: 22, height: 22, borderRadius: 11, backgroundColor: SAND, alignItems: 'center', justifyContent: 'center' },
  stepBadgeText: { color: '#fff', fontSize: 11, fontWeight: '700' },
  stepTitle: { fontSize: 13, fontWeight: '700', textAlign: 'center', marginBottom: 4 },
  stepDesc: { fontSize: 11, textAlign: 'center', lineHeight: 16 },

  ageGrid: { flexDirection: 'row', flexWrap: 'wrap', gap: 12, marginTop: 16 },
  ageCard: {
    width: AGE_CARD_W,
    borderRadius: 16,
    padding: 16,
    alignItems: 'center',
    shadowColor: '#000',
    shadowOpacity: 0.05,
    shadowOffset: { width: 0, height: 2 },
    shadowRadius: 6,
    elevation: 2,
    borderWidth: 1,
  },
  ageIcon: { width: 52, height: 52, borderRadius: 14, alignItems: 'center', justifyContent: 'center', marginBottom: 10 },
  ageEmoji: { fontSize: 24 },
  ageLabel: { fontSize: 13, fontWeight: '700', textAlign: 'center' },
  ageRange: { fontSize: 11, marginTop: 2, textAlign: 'center' },

  supportSection: { paddingHorizontal: 28, paddingVertical: 40, paddingBottom: 48, alignItems: 'center', borderTopWidth: 1 },
  supportHeart: { fontSize: 40, marginBottom: 12 },
  supportTitle: { fontSize: 20, fontWeight: '800', textAlign: 'center', marginBottom: 10 },
  supportBody: { fontSize: 13, textAlign: 'center', lineHeight: 20, maxWidth: 300, marginBottom: 20 },
  donateBtn: { backgroundColor: ROSE, borderRadius: 12, paddingHorizontal: 24, paddingVertical: 14 },
  donateBtnText: { color: '#fff', fontSize: 15, fontWeight: '700' },
});
