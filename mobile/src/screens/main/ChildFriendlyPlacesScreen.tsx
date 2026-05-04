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
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useTranslation } from 'react-i18next';
import { useFocusEffect } from '@react-navigation/native';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import { useAuthStore } from '@/store/authStore';
import { useTheme } from '@/contexts/ThemeContext';
import { ROSE } from '@/constants/palette';
import {
  childFriendlyPlacesApi,
  placeTypeLabel,
  PlaceType,
  type ChildFriendlyPlaceDto,
  type CreateChildFriendlyPlaceDto,
} from '@/api/childFriendlyPlacesApi';
import type { RootStackParamList } from '@/navigation/types';

type Props = NativeStackScreenProps<RootStackParamList, 'ChildFriendlyPlaces'>;

const PLACE_TYPES = Object.values(PlaceType).filter((v) => typeof v === 'number') as PlaceType[];

function PlaceCard({
  place,
  userId,
  isAdmin,
  onDelete,
}: {
  place: ChildFriendlyPlaceDto;
  userId: string | undefined;
  isAdmin: boolean;
  onDelete: (id: string) => void;
}) {
  const { colors } = useTheme();
  const { t } = useTranslation();
  const canDelete = isAdmin || place.userId === userId;
  const date = new Date(place.createdAt).toLocaleDateString();

  const ageRange =
    place.ageFromMonths != null && place.ageToMonths != null
      ? `${t('childFriendlyPlaces.ageRange')}: ${place.ageFromMonths}–${place.ageToMonths} mo`
      : null;

  return (
    <View style={[card.wrap, { backgroundColor: colors.card, borderColor: colors.border }]}>
      <View style={card.topRow}>
        <View style={{ flex: 1 }}>
          <Text style={[card.name, { color: colors.text }]}>{place.name}</Text>
          <Text style={[card.type, { color: ROSE }]}>{placeTypeLabel[place.placeType]}</Text>
        </View>
      </View>

      <Text style={[card.meta, { color: colors.text2 }]}>
        {[place.address, place.city].filter(Boolean).join(', ')}
      </Text>

      <Text style={[card.desc, { color: colors.text }]} numberOfLines={3}>{place.description}</Text>

      {ageRange ? <Text style={[card.age, { color: colors.text2 }]}>{ageRange}</Text> : null}
      {place.website ? (
        <Text style={[card.link, { color: ROSE }]} numberOfLines={1}>🔗 {place.website}</Text>
      ) : null}

      <View style={card.footer}>
        <Text style={[card.author, { color: colors.text2 }]}>
          {place.authorDisplayName ?? '—'}{'  ·  '}{date}
        </Text>
        {canDelete && (
          <TouchableOpacity onPress={() => onDelete(place.id)} activeOpacity={0.7}>
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
  name: { fontSize: 16, fontWeight: '700' },
  type: { fontSize: 13, marginTop: 2 },
  meta: { fontSize: 13, marginBottom: 8 },
  desc: { fontSize: 14, lineHeight: 20, marginBottom: 8 },
  age: { fontSize: 12, marginBottom: 4 },
  link: { fontSize: 12, marginBottom: 8 },
  footer: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between' },
  author: { fontSize: 12 },
  del: { fontSize: 20 },
});

const EMPTY_FORM: CreateChildFriendlyPlaceDto = {
  name: '',
  description: '',
  address: '',
  city: '',
  placeType: PlaceType.Park,
  ageFromMonths: undefined,
  ageToMonths: undefined,
  website: '',
};

export default function ChildFriendlyPlacesScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const { colors } = useTheme();
  const user = useAuthStore((s) => s.user);
  const isAdmin = user?.roles?.includes('Admin') ?? false;

  const [places, setPlaces] = useState<ChildFriendlyPlaceDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [modalVisible, setModalVisible] = useState(false);
  const [form, setForm] = useState<CreateChildFriendlyPlaceDto>(EMPTY_FORM);
  const [submitting, setSubmitting] = useState(false);
  const [submitSuccess, setSubmitSuccess] = useState(false);

  const load = useCallback(async () => {
    try {
      const data = await childFriendlyPlacesApi.getAll();
      setPlaces(data);
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
              await childFriendlyPlacesApi.adminDelete(id);
            } else {
              await childFriendlyPlacesApi.delete(id);
            }
            setPlaces((prev) => prev.filter((p) => p.id !== id));
          } catch {
            Alert.alert(t('common.error'), t('childFriendlyPlaces.deleteError'));
          }
        },
      },
    ]);
  };

  const handleSubmit = async () => {
    if (!form.name || !form.description || !form.city) {
      Alert.alert(t('common.validation'), 'Please fill all required fields.');
      return;
    }
    setSubmitting(true);
    try {
      await childFriendlyPlacesApi.create({
        ...form,
        address: form.address || undefined,
        website: form.website || undefined,
      });
      setSubmitSuccess(true);
    } catch {
      Alert.alert(t('common.error'), t('childFriendlyPlaces.submitError'));
    } finally {
      setSubmitting(false);
    }
  };

  const closeModal = () => {
    setModalVisible(false);
    setSubmitSuccess(false);
    setForm(EMPTY_FORM);
  };

  const f = (key: keyof CreateChildFriendlyPlaceDto) => (val: string) =>
    setForm((prev) => ({ ...prev, [key]: val }));

  return (
    <SafeAreaView style={[s.safe, { backgroundColor: colors.bg }]} edges={['top']}>
      {/* Header */}
      <View style={[s.header, { borderBottomColor: colors.border }]}>
        <TouchableOpacity onPress={() => navigation.goBack()} activeOpacity={0.7} style={s.backBtn}>
          <Text style={{ color: ROSE, fontSize: 17 }}>‹ Back</Text>
        </TouchableOpacity>
        <Text style={[s.title, { color: colors.text }]}>{t('childFriendlyPlaces.title')}</Text>
        <TouchableOpacity onPress={() => setModalVisible(true)} activeOpacity={0.7} style={s.addBtn}>
          <Text style={{ color: ROSE, fontSize: 24, lineHeight: 28 }}>+</Text>
        </TouchableOpacity>
      </View>

      {loading ? (
        <View style={s.center}>
          <ActivityIndicator size="large" color={ROSE} />
        </View>
      ) : places.length === 0 ? (
        <View style={s.center}>
          <Text style={s.emptyEmoji}>🌳</Text>
          <Text style={[s.emptyText, { color: colors.text2 }]}>{t('childFriendlyPlaces.noPlaces')}</Text>
        </View>
      ) : (
        <FlatList
          data={places}
          keyExtractor={(p) => p.id}
          renderItem={({ item }) => (
            <PlaceCard
              place={item}
              userId={user?.id}
              isAdmin={isAdmin}
              onDelete={handleDelete}
            />
          )}
          contentContainerStyle={s.list}
          refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} tintColor={ROSE} />}
        />
      )}

      {/* Add Place Modal */}
      <Modal visible={modalVisible} animationType="slide" presentationStyle="pageSheet" onRequestClose={closeModal}>
        <SafeAreaView style={[s.safe, { backgroundColor: colors.bg }]}>
          <View style={[s.modalHeader, { borderBottomColor: colors.border }]}>
            <TouchableOpacity onPress={closeModal} activeOpacity={0.7}>
              <Text style={{ color: ROSE, fontSize: 17 }}>✕</Text>
            </TouchableOpacity>
            <Text style={[s.title, { color: colors.text }]}>{t('childFriendlyPlaces.addPlace')}</Text>
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
              <Text style={[s.label, { color: colors.text2 }]}>{t('childFriendlyPlaces.name')}</Text>
              <TextInput style={[s.input, { backgroundColor: colors.input, borderColor: colors.inputBorder, color: colors.text }]}
                value={form.name} onChangeText={f('name')} placeholderTextColor={colors.text2} />

              <Text style={[s.label, { color: colors.text2 }]}>{t('childFriendlyPlaces.description')}</Text>
              <TextInput
                style={[s.input, s.textarea, { backgroundColor: colors.input, borderColor: colors.inputBorder, color: colors.text }]}
                value={form.description} onChangeText={f('description')} multiline numberOfLines={4}
                placeholderTextColor={colors.text2} />

              <Text style={[s.label, { color: colors.text2 }]}>{t('childFriendlyPlaces.city')}</Text>
              <TextInput style={[s.input, { backgroundColor: colors.input, borderColor: colors.inputBorder, color: colors.text }]}
                value={form.city} onChangeText={f('city')} placeholderTextColor={colors.text2} />

              <Text style={[s.label, { color: colors.text2 }]}>{t('childFriendlyPlaces.address')}</Text>
              <TextInput style={[s.input, { backgroundColor: colors.input, borderColor: colors.inputBorder, color: colors.text }]}
                value={form.address} onChangeText={f('address')} placeholderTextColor={colors.text2} />

              <Text style={[s.label, { color: colors.text2 }]}>{t('childFriendlyPlaces.placeType')}</Text>
              <ScrollView horizontal showsHorizontalScrollIndicator={false} style={{ marginBottom: 16 }}>
                <View style={{ flexDirection: 'row', gap: 8 }}>
                  {PLACE_TYPES.map((pt) => (
                    <TouchableOpacity
                      key={pt}
                      style={[s.typeChip, form.placeType === pt && s.typeChipActive]}
                      onPress={() => setForm((p) => ({ ...p, placeType: pt }))}
                      activeOpacity={0.7}
                    >
                      <Text style={[s.typeChipText, form.placeType === pt && { color: '#fff' }]}>
                        {placeTypeLabel[pt]}
                      </Text>
                    </TouchableOpacity>
                  ))}
                </View>
              </ScrollView>

              <View style={s.rowInputs}>
                <View style={{ flex: 1 }}>
                  <Text style={[s.label, { color: colors.text2 }]}>{t('childFriendlyPlaces.ageFrom')}</Text>
                  <TextInput
                    style={[s.input, { backgroundColor: colors.input, borderColor: colors.inputBorder, color: colors.text }]}
                    value={form.ageFromMonths != null ? String(form.ageFromMonths) : ''}
                    onChangeText={(v) => setForm((p) => ({ ...p, ageFromMonths: v ? parseInt(v, 10) : undefined }))}
                    keyboardType="numeric"
                    placeholderTextColor={colors.text2}
                  />
                </View>
                <View style={{ width: 12 }} />
                <View style={{ flex: 1 }}>
                  <Text style={[s.label, { color: colors.text2 }]}>{t('childFriendlyPlaces.ageTo')}</Text>
                  <TextInput
                    style={[s.input, { backgroundColor: colors.input, borderColor: colors.inputBorder, color: colors.text }]}
                    value={form.ageToMonths != null ? String(form.ageToMonths) : ''}
                    onChangeText={(v) => setForm((p) => ({ ...p, ageToMonths: v ? parseInt(v, 10) : undefined }))}
                    keyboardType="numeric"
                    placeholderTextColor={colors.text2}
                  />
                </View>
              </View>

              <Text style={[s.label, { color: colors.text2 }]}>{t('childFriendlyPlaces.website')}</Text>
              <TextInput style={[s.input, { backgroundColor: colors.input, borderColor: colors.inputBorder, color: colors.text }]}
                value={form.website} onChangeText={f('website')} autoCapitalize="none" placeholderTextColor={colors.text2} />

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
  textarea: { height: 100, paddingTop: 12, textAlignVertical: 'top' },
  rowInputs: { flexDirection: 'row' },

  typeChip: {
    paddingHorizontal: 12,
    paddingVertical: 8,
    borderRadius: 20,
    borderWidth: 1,
    borderColor: '#ccc',
    backgroundColor: 'transparent',
  },
  typeChipActive: { backgroundColor: ROSE, borderColor: ROSE },
  typeChipText: { fontSize: 13 },

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
