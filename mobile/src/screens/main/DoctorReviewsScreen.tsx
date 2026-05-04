import { useState, useCallback, useRef, useEffect } from 'react';
import {
  View,
  Text,
  FlatList,
  StyleSheet,
  ActivityIndicator,
  TouchableOpacity,
  RefreshControl,
  Modal,
  ScrollView,
  TextInput,
  Alert,
  Switch,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useTranslation } from 'react-i18next';
import { useFocusEffect } from '@react-navigation/native';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import { useAuthStore } from '@/store/authStore';
import { useTheme } from '@/contexts/ThemeContext';
import { ROSE } from '@/constants/palette';
import { doctorReviewsApi, type DoctorReviewDto, type CreateDoctorReviewDto } from '@/api/doctorReviewsApi';
import type { RootStackParamList } from '@/navigation/types';

type Props = NativeStackScreenProps<RootStackParamList, 'DoctorReviews'>;

function StarRating({ value, onChange }: { value: number; onChange?: (v: number) => void }) {
  return (
    <View style={{ flexDirection: 'row', gap: 6 }}>
      {[1, 2, 3, 4, 5].map((star) => (
        <TouchableOpacity
          key={star}
          onPress={() => onChange?.(star)}
          disabled={!onChange}
          activeOpacity={0.7}
        >
          <Text style={{ fontSize: onChange ? 28 : 18, color: star <= value ? '#f5a623' : '#ccc' }}>★</Text>
        </TouchableOpacity>
      ))}
    </View>
  );
}

function ReviewCard({
  review,
  userId,
  isAdmin,
  onDelete,
}: {
  review: DoctorReviewDto;
  userId: string | undefined;
  isAdmin: boolean;
  onDelete: (id: string) => void;
}) {
  const { colors } = useTheme();
  const { t } = useTranslation();
  const canDelete = isAdmin || review.userId === userId;
  const date = new Date(review.createdAt).toLocaleDateString();

  return (
    <View style={[card.wrap, { backgroundColor: colors.card, borderColor: colors.border }]}>
      <View style={card.topRow}>
        <View style={{ flex: 1 }}>
          <Text style={[card.doctorName, { color: colors.text }]}>{review.doctorName}</Text>
          <Text style={[card.spec, { color: ROSE }]}>{review.specialization}</Text>
        </View>
        <StarRating value={review.rating} />
      </View>

      {(review.clinicName || review.city) ? (
        <Text style={[card.meta, { color: colors.text2 }]}>
          {[review.clinicName, review.city].filter(Boolean).join(' · ')}
        </Text>
      ) : null}

      <Text style={[card.content, { color: colors.text }]}>{review.content}</Text>

      {review.superdocUrl ? (
        <Text style={[card.link, { color: ROSE }]} numberOfLines={1}>
          🔗 {review.superdocUrl}
        </Text>
      ) : null}

      <View style={card.footer}>
        <Text style={[card.author, { color: colors.text2 }]}>
          {review.isAnonymous ? t('doctorReviews.anonymous') : (review.authorDisplayName ?? '—')}
          {'  ·  '}{date}
        </Text>
        {canDelete && (
          <TouchableOpacity onPress={() => onDelete(review.id)} activeOpacity={0.7}>
            <Text style={card.del}>🗑️</Text>
          </TouchableOpacity>
        )}
      </View>
    </View>
  );
}

const card = StyleSheet.create({
  wrap: { borderRadius: 14, borderWidth: StyleSheet.hairlineWidth, padding: 14, marginBottom: 12 },
  topRow: { flexDirection: 'row', alignItems: 'flex-start', marginBottom: 4 },
  doctorName: { fontSize: 16, fontWeight: '700' },
  spec: { fontSize: 13, marginTop: 2 },
  meta: { fontSize: 13, marginBottom: 8 },
  content: { fontSize: 14, lineHeight: 20, marginBottom: 8 },
  link: { fontSize: 12, marginBottom: 8 },
  footer: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between' },
  author: { fontSize: 12 },
  del: { fontSize: 20 },
});

const EMPTY_FORM: CreateDoctorReviewDto = {
  doctorName: '',
  specialization: '',
  clinicName: '',
  city: '',
  rating: 0,
  content: '',
  superdocUrl: '',
  isAnonymous: false,
};

export default function DoctorReviewsScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const { colors } = useTheme();
  const user = useAuthStore((s) => s.user);
  const isAdmin = user?.roles?.includes('Admin') ?? false;

  const [reviews, setReviews] = useState<DoctorReviewDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [modalVisible, setModalVisible] = useState(false);
  const [form, setForm] = useState<CreateDoctorReviewDto>(EMPTY_FORM);
  const [submitting, setSubmitting] = useState(false);
  const [submitSuccess, setSubmitSuccess] = useState(false);

  const load = useCallback(async () => {
    try {
      const data = await doctorReviewsApi.getAll();
      setReviews(data);
    } finally {
      setLoading(false);
    }
  }, []);

  const refetchRef = useRef(load);
  useEffect(() => { refetchRef.current = load; }, [load]);
  useFocusEffect(useCallback(() => { refetchRef.current(); }, []));

  const onRefresh = async () => {
    setRefreshing(true);
    await load();
    setRefreshing(false);
  };

  const handleDelete = (id: string) => {
    Alert.alert(t('common.deleteConfirmTitle'), t('common.deleteConfirmMsg'), [
      { text: t('common.cancel'), style: 'cancel' },
      {
        text: t('common.delete'),
        style: 'destructive',
        onPress: async () => {
          try {
            if (isAdmin) {
              await doctorReviewsApi.adminDelete(id);
            } else {
              await doctorReviewsApi.delete(id);
            }
            setReviews((prev) => prev.filter((r) => r.id !== id));
          } catch {
            Alert.alert(t('common.error'), t('doctorReviews.deleteError'));
          }
        },
      },
    ]);
  };

  const handleSubmit = async () => {
    if (!form.doctorName || !form.specialization || !form.city || !form.content || form.rating === 0) {
      Alert.alert(t('common.validation'), 'Please fill all required fields and select a rating.');
      return;
    }
    setSubmitting(true);
    try {
      await doctorReviewsApi.create({
        ...form,
        clinicName: form.clinicName || undefined,
        superdocUrl: form.superdocUrl || undefined,
      });
      setSubmitSuccess(true);
    } catch {
      Alert.alert(t('common.error'), t('doctorReviews.submitError'));
    } finally {
      setSubmitting(false);
    }
  };

  const closeModal = () => {
    setModalVisible(false);
    setSubmitSuccess(false);
    setForm(EMPTY_FORM);
  };

  const f = (key: keyof CreateDoctorReviewDto) => (val: string) =>
    setForm((prev) => ({ ...prev, [key]: val }));

  return (
    <SafeAreaView style={[s.safe, { backgroundColor: colors.bg }]} edges={['top']}>
      {/* Header */}
      <View style={[s.header, { borderBottomColor: colors.border }]}>
        <TouchableOpacity onPress={() => navigation.goBack()} activeOpacity={0.7} style={s.backBtn}>
          <Text style={{ color: ROSE, fontSize: 17 }}>‹ Back</Text>
        </TouchableOpacity>
        <Text style={[s.title, { color: colors.text }]}>{t('doctorReviews.title')}</Text>
        <TouchableOpacity onPress={() => setModalVisible(true)} activeOpacity={0.7} style={s.addBtn}>
          <Text style={{ color: ROSE, fontSize: 24, lineHeight: 28 }}>+</Text>
        </TouchableOpacity>
      </View>

      {loading ? (
        <View style={s.center}>
          <ActivityIndicator size="large" color={ROSE} />
        </View>
      ) : reviews.length === 0 ? (
        <View style={s.center}>
          <Text style={s.emptyEmoji}>🩺</Text>
          <Text style={[s.emptyText, { color: colors.text2 }]}>{t('doctorReviews.noReviews')}</Text>
        </View>
      ) : (
        <FlatList
          data={reviews}
          keyExtractor={(r) => r.id}
          renderItem={({ item }) => (
            <ReviewCard
              review={item}
              userId={user?.id}
              isAdmin={isAdmin}
              onDelete={handleDelete}
            />
          )}
          contentContainerStyle={s.list}
          refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} tintColor={ROSE} />}
        />
      )}

      {/* Add Review Modal */}
      <Modal visible={modalVisible} animationType="slide" presentationStyle="pageSheet" onRequestClose={closeModal}>
        <SafeAreaView style={[s.safe, { backgroundColor: colors.bg }]}>
          <View style={[s.modalHeader, { borderBottomColor: colors.border }]}>
            <TouchableOpacity onPress={closeModal} activeOpacity={0.7}>
              <Text style={{ color: ROSE, fontSize: 17 }}>✕</Text>
            </TouchableOpacity>
            <Text style={[s.title, { color: colors.text }]}>{t('doctorReviews.addReview')}</Text>
            <View style={{ width: 40 }} />
          </View>

          {submitSuccess ? (
            <View style={s.successWrap}>
              <Text style={s.successIcon}>🕐</Text>
              <Text style={[s.successTitle, { color: colors.text }]}>{t('common.pendingApproval')}</Text>
              <Text style={[s.successMsg, { color: colors.text2 }]}>{t('common.pendingApprovalMsg')}</Text>
              <TouchableOpacity style={s.doneBtn} onPress={closeModal} activeOpacity={0.8}>
                <Text style={s.doneBtnText}>{t('common.done')}</Text>
              </TouchableOpacity>
            </View>
          ) : (
            <ScrollView contentContainerStyle={s.formScroll} keyboardShouldPersistTaps="handled">
              <Text style={[s.label, { color: colors.text2 }]}>{t('doctorReviews.doctorName')}</Text>
              <TextInput style={[s.input, { backgroundColor: colors.input, borderColor: colors.inputBorder, color: colors.text }]}
                value={form.doctorName} onChangeText={f('doctorName')} placeholderTextColor={colors.text2} />

              <Text style={[s.label, { color: colors.text2 }]}>{t('doctorReviews.specialization')}</Text>
              <TextInput style={[s.input, { backgroundColor: colors.input, borderColor: colors.inputBorder, color: colors.text }]}
                value={form.specialization} onChangeText={f('specialization')} placeholderTextColor={colors.text2} />

              <Text style={[s.label, { color: colors.text2 }]}>{t('doctorReviews.clinicName')}</Text>
              <TextInput style={[s.input, { backgroundColor: colors.input, borderColor: colors.inputBorder, color: colors.text }]}
                value={form.clinicName} onChangeText={f('clinicName')} placeholderTextColor={colors.text2} />

              <Text style={[s.label, { color: colors.text2 }]}>{t('doctorReviews.city')}</Text>
              <TextInput style={[s.input, { backgroundColor: colors.input, borderColor: colors.inputBorder, color: colors.text }]}
                value={form.city} onChangeText={f('city')} placeholderTextColor={colors.text2} />

              <Text style={[s.label, { color: colors.text2 }]}>{t('doctorReviews.rating')}</Text>
              <View style={{ marginBottom: 16 }}>
                <StarRating value={form.rating} onChange={(v) => setForm((p) => ({ ...p, rating: v }))} />
              </View>

              <Text style={[s.label, { color: colors.text2 }]}>{t('doctorReviews.content')}</Text>
              <TextInput
                style={[s.input, s.textarea, { backgroundColor: colors.input, borderColor: colors.inputBorder, color: colors.text }]}
                value={form.content}
                onChangeText={f('content')}
                multiline
                numberOfLines={5}
                placeholderTextColor={colors.text2}
              />

              <Text style={[s.label, { color: colors.text2 }]}>{t('doctorReviews.superdocUrl')}</Text>
              <TextInput style={[s.input, { backgroundColor: colors.input, borderColor: colors.inputBorder, color: colors.text }]}
                value={form.superdocUrl} onChangeText={f('superdocUrl')} autoCapitalize="none" placeholderTextColor={colors.text2} />

              <View style={s.switchRow}>
                <Text style={[s.label, { color: colors.text2, marginBottom: 0 }]}>{t('doctorReviews.isAnonymous')}</Text>
                <Switch
                  value={form.isAnonymous}
                  onValueChange={(v) => setForm((p) => ({ ...p, isAnonymous: v }))}
                  trackColor={{ false: '#ccc', true: ROSE }}
                  thumbColor="#fff"
                />
              </View>

              <TouchableOpacity
                style={[s.submitBtn, submitting && { opacity: 0.6 }]}
                onPress={handleSubmit}
                disabled={submitting}
                activeOpacity={0.8}
              >
                {submitting ? (
                  <ActivityIndicator color="#fff" />
                ) : (
                  <Text style={s.submitBtnText}>{t('common.submit')}</Text>
                )}
              </TouchableOpacity>
            </ScrollView>
          )}
        </SafeAreaView>
      </Modal>
    </SafeAreaView>
  );
}

const s = StyleSheet.create({
  safe: { flex: 1 },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: 16,
    paddingVertical: 12,
    borderBottomWidth: StyleSheet.hairlineWidth,
  },
  backBtn: { width: 60 },
  addBtn: { width: 60, alignItems: 'flex-end' },
  title: { flex: 1, textAlign: 'center', fontSize: 18, fontWeight: '700' },
  list: { padding: 16 },
  center: { flex: 1, alignItems: 'center', justifyContent: 'center' },
  emptyEmoji: { fontSize: 48, marginBottom: 12 },
  emptyText: { fontSize: 16 },

  modalHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: 20,
    paddingVertical: 14,
    borderBottomWidth: StyleSheet.hairlineWidth,
  },
  formScroll: { padding: 20, paddingBottom: 40 },
  label: { fontSize: 13, fontWeight: '600', marginBottom: 6, textTransform: 'uppercase', letterSpacing: 0.4 },
  input: {
    height: 46,
    borderRadius: 10,
    borderWidth: 1,
    paddingHorizontal: 14,
    fontSize: 15,
    marginBottom: 16,
  },
  textarea: { height: 110, paddingTop: 12, textAlignVertical: 'top' },
  switchRow: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', marginBottom: 24 },
  submitBtn: {
    height: 52,
    borderRadius: 14,
    backgroundColor: ROSE,
    alignItems: 'center',
    justifyContent: 'center',
    marginTop: 8,
  },
  submitBtnText: { color: '#fff', fontSize: 16, fontWeight: '700' },

  successWrap: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: 32 },
  successIcon: { fontSize: 64, marginBottom: 20 },
  successTitle: { fontSize: 22, fontWeight: '700', marginBottom: 10, textAlign: 'center' },
  successMsg: { fontSize: 15, textAlign: 'center', lineHeight: 22, marginBottom: 32 },
  doneBtn: {
    height: 52,
    borderRadius: 14,
    backgroundColor: ROSE,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: 40,
  },
  doneBtnText: { color: '#fff', fontSize: 16, fontWeight: '700' },
});
