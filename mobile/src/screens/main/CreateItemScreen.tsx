import { useState, useCallback } from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  ScrollView,
  StyleSheet,
  ActivityIndicator,
  Alert,
  Image,
  KeyboardAvoidingView,
  Platform,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import * as ImagePicker from 'expo-image-picker';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { RootStackParamList } from '@/navigation/types';
import { itemsApi } from '@/api/itemsApi';
import { photosApi } from '@/api/photosApi';
import { aiApi } from '@/api/aiApi';
import { useTheme } from '@/contexts/ThemeContext';
import { useCategories } from '@/hooks/useCategories';
import { ListingType } from '@mamvibe/shared';
import type { PriceSuggestion } from '@mamvibe/shared';

type Props = NativeStackScreenProps<RootStackParamList, 'CreateItem'>;

const PRIMARY = '#e91e8c';

export default function CreateItemScreen({ navigation }: Props) {
  const { colors } = useTheme();
  const { categories } = useCategories();

  const [photos, setPhotos] = useState<{ uri: string; mime: string }[]>([]);
  const [form, setForm] = useState({
    title: '',
    description: '',
    categoryId: '',
    listingType: ListingType.Donate as ListingType,
    price: '',
  });
  const [aiLoading, setAiLoading] = useState(false);
  const [priceLoading, setPriceLoading] = useState(false);
  const [priceSuggestion, setPriceSuggestion] = useState<PriceSuggestion | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const pickPhoto = useCallback(async () => {
    const { status } = await ImagePicker.requestMediaLibraryPermissionsAsync();
    if (status !== 'granted') {
      Alert.alert('Permission needed', 'Please allow access to your photo library.');
      return;
    }
    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ImagePicker.MediaTypeOptions.Images,
      allowsMultipleSelection: true,
      selectionLimit: 5,
      quality: 0.8,
    });
    if (!result.canceled) {
      const picked = result.assets.map((a) => ({ uri: a.uri, mime: a.mimeType ?? 'image/jpeg' }));
      setPhotos((prev) => [...prev, ...picked].slice(0, 5));
    }
  }, []);

  const handleAiFill = useCallback(async () => {
    const { status } = await ImagePicker.requestMediaLibraryPermissionsAsync();
    if (status !== 'granted') {
      Alert.alert('Permission needed', 'Please allow access to your photo library.');
      return;
    }
    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ImagePicker.MediaTypeOptions.Images,
      allowsMultipleSelection: false,
      quality: 0.8,
    });
    if (result.canceled) return;

    const asset = result.assets[0];
    setAiLoading(true);
    try {
      const { data: suggestion } = await aiApi.suggestListing(asset.uri, asset.mimeType ?? 'image/jpeg');
      const matched = categories.find((c) => c.slug === suggestion.categorySlug);
      setForm((prev) => ({
        ...prev,
        title: suggestion.title || prev.title,
        description: suggestion.description || prev.description,
        categoryId: matched?.id ?? prev.categoryId,
        listingType: suggestion.listingType,
        price: suggestion.suggestedPrice != null ? String(suggestion.suggestedPrice) : prev.price,
      }));
      setPhotos((prev) => [{ uri: asset.uri, mime: asset.mimeType ?? 'image/jpeg' }, ...prev].slice(0, 5));
      Alert.alert('✨ AI filled the form', 'Review the suggestions before submitting.');
    } catch (err: any) {
      const msg = err?.response?.data?.detail ?? err?.response?.data?.error ?? 'Could not analyse the photo.';
      Alert.alert('Error', msg);
    } finally {
      setAiLoading(false);
    }
  }, [categories]);

  const handlePriceSuggest = useCallback(async () => {
    if (!form.categoryId) { Alert.alert('Select a category first'); return; }
    setPriceLoading(true);
    try {
      const { data } = await aiApi.suggestPrice({
        categoryId: form.categoryId,
        title: form.title,
        description: form.description,
      });
      setPriceSuggestion(data);
    } catch {
      Alert.alert('Error', 'Could not get a price suggestion.');
    } finally {
      setPriceLoading(false);
    }
  }, [form.categoryId, form.title, form.description]);

  const handleSubmit = async () => {
    if (!form.title.trim()) { Alert.alert('Validation', 'Title is required.'); return; }
    if (!form.categoryId) { Alert.alert('Validation', 'Please select a category.'); return; }
    if (form.listingType === ListingType.Sell && !form.price) {
      Alert.alert('Validation', 'Price is required for items for sale.');
      return;
    }
    setSubmitting(true);
    try {
      const photoUrls: string[] = [];
      for (const p of photos) {
        const { data } = await photosApi.upload(p.uri, p.mime);
        photoUrls.push(data.url);
      }
      await itemsApi.create({
        title: form.title,
        description: form.description,
        categoryId: form.categoryId,
        listingType: form.listingType,
        price: form.listingType === ListingType.Sell ? parseFloat(form.price) : null,
        photoUrls,
        ageGroup: null,
        shoeSize: null,
        clothingSize: null,
      });
      Alert.alert('Listed!', 'Your item has been published.', [
        { text: 'OK', onPress: () => navigation.goBack() },
      ]);
    } catch (err: any) {
      Alert.alert('Error', err?.response?.data?.error ?? 'Could not create listing.');
    } finally {
      setSubmitting(false);
    }
  };

  const input = [s.input, { backgroundColor: colors.input, borderColor: colors.inputBorder, color: colors.text }];
  const label = [s.label, { color: PRIMARY }];

  return (
    <SafeAreaView style={[s.safe, { backgroundColor: colors.bg }]} edges={['bottom']}>
      <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : undefined} style={{ flex: 1 }}>
        <ScrollView contentContainerStyle={s.scroll} showsVerticalScrollIndicator={false}>

          {/* AI Assistant banner */}
          <TouchableOpacity
            style={[s.aiBanner, { backgroundColor: colors.section, borderColor: colors.border }]}
            onPress={handleAiFill}
            disabled={aiLoading}
            activeOpacity={0.85}
          >
            {aiLoading ? (
              <ActivityIndicator color={PRIMARY} />
            ) : (
              <Text style={s.aiIcon}>✨</Text>
            )}
            <View style={{ flex: 1 }}>
              <Text style={[s.aiTitle, { color: colors.text }]}>
                {aiLoading ? 'Analysing photo…' : '✨ Fill with AI'}
              </Text>
              <Text style={[s.aiSub, { color: colors.text2 }]}>
                Pick a photo — Claude AI fills title, description, category & price
              </Text>
            </View>
          </TouchableOpacity>

          {/* Photos */}
          <Text style={label}>Photos</Text>
          <View style={s.photoRow}>
            {photos.map((p, i) => (
              <View key={i} style={s.photoThumb}>
                <Image source={{ uri: p.uri }} style={s.photoImg} />
                <TouchableOpacity
                  style={s.photoRemove}
                  onPress={() => setPhotos((prev) => prev.filter((_, j) => j !== i))}
                >
                  <Text style={{ color: '#fff', fontSize: 10, fontWeight: '700' }}>✕</Text>
                </TouchableOpacity>
              </View>
            ))}
            {photos.length < 5 && (
              <TouchableOpacity style={[s.photoAdd, { borderColor: colors.border }]} onPress={pickPhoto}>
                <Text style={{ fontSize: 24, color: colors.text2 }}>+</Text>
                <Text style={{ fontSize: 10, color: colors.text2 }}>Add Photo</Text>
              </TouchableOpacity>
            )}
          </View>

          {/* Listing type */}
          <Text style={label}>Type</Text>
          <View style={[s.typeRow, { borderColor: colors.border, backgroundColor: colors.section }]}>
            <TouchableOpacity
              style={[s.typeBtn, form.listingType === ListingType.Donate && s.typeBtnActive]}
              onPress={() => { setForm({ ...form, listingType: ListingType.Donate }); setPriceSuggestion(null); }}
            >
              <Text style={[s.typeTxt, { color: colors.text2 }, form.listingType === ListingType.Donate && s.typeTxtActive]}>🎁 Donate</Text>
            </TouchableOpacity>
            <TouchableOpacity
              style={[s.typeBtn, form.listingType === ListingType.Sell && s.typeBtnActive]}
              onPress={() => { setForm({ ...form, listingType: ListingType.Sell }); setPriceSuggestion(null); }}
            >
              <Text style={[s.typeTxt, { color: colors.text2 }, form.listingType === ListingType.Sell && s.typeTxtActive]}>🛒 For Sale</Text>
            </TouchableOpacity>
          </View>

          {/* Title */}
          <Text style={label}>Title *</Text>
          <TextInput
            style={input}
            value={form.title}
            onChangeText={(v) => setForm({ ...form, title: v })}
            placeholder="e.g. Baby Stroller, Clothes Set..."
            placeholderTextColor={colors.text2}
          />

          {/* Description */}
          <Text style={label}>Description</Text>
          <TextInput
            style={[input, s.textarea]}
            value={form.description}
            onChangeText={(v) => setForm({ ...form, description: v })}
            placeholder="Condition, size, brand..."
            placeholderTextColor={colors.text2}
            multiline
            numberOfLines={4}
            textAlignVertical="top"
          />

          {/* Category */}
          <Text style={label}>Category *</Text>
          <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={s.catRow}>
            {categories.map((cat) => (
              <TouchableOpacity
                key={cat.id}
                style={[s.catChip, { backgroundColor: colors.card, borderColor: colors.border }, form.categoryId === cat.id && s.catChipActive]}
                onPress={() => { setForm({ ...form, categoryId: cat.id }); setPriceSuggestion(null); }}
              >
                <Text style={[s.catTxt, { color: colors.text2 }, form.categoryId === cat.id && s.catTxtActive]}>
                  {cat.name}
                </Text>
              </TouchableOpacity>
            ))}
          </ScrollView>

          {/* Price (sell only) */}
          {form.listingType === ListingType.Sell && (
            <>
              <Text style={label}>Price (лв) *</Text>
              <View style={s.priceRow}>
                <TextInput
                  style={[input, { flex: 1 }]}
                  value={form.price}
                  onChangeText={(v) => setForm({ ...form, price: v })}
                  placeholder="0.00"
                  placeholderTextColor={colors.text2}
                  keyboardType="decimal-pad"
                />
                <TouchableOpacity
                  style={s.suggestBtn}
                  onPress={handlePriceSuggest}
                  disabled={priceLoading || !form.categoryId}
                >
                  {priceLoading
                    ? <ActivityIndicator size="small" color={PRIMARY} />
                    : <Text style={s.suggestBtnTxt}>✨ Suggest</Text>
                  }
                </TouchableOpacity>
              </View>

              {priceSuggestion?.suggestedPrice != null && (
                <View style={[s.suggCard, { backgroundColor: colors.section, borderColor: colors.border }]}>
                  <View style={{ flex: 1 }}>
                    <Text style={[s.suggPrice, { color: PRIMARY }]}>{priceSuggestion.suggestedPrice} лв</Text>
                    {priceSuggestion.reason ? (
                      <Text style={[s.suggReason, { color: colors.text2 }]}>{priceSuggestion.reason}</Text>
                    ) : null}
                  </View>
                  <TouchableOpacity
                    style={s.useBtn}
                    onPress={() => { setForm((f) => ({ ...f, price: String(priceSuggestion.suggestedPrice) })); setPriceSuggestion(null); }}
                  >
                    <Text style={s.useBtnTxt}>Use</Text>
                  </TouchableOpacity>
                  <TouchableOpacity onPress={() => setPriceSuggestion(null)} style={{ padding: 4 }}>
                    <Text style={{ color: colors.text2 }}>✕</Text>
                  </TouchableOpacity>
                </View>
              )}
            </>
          )}

          {/* Submit */}
          <TouchableOpacity
            style={[s.submitBtn, submitting && { opacity: 0.5 }]}
            onPress={handleSubmit}
            disabled={submitting}
          >
            {submitting
              ? <ActivityIndicator color="#fff" />
              : <Text style={s.submitTxt}>Publish Listing</Text>
            }
          </TouchableOpacity>

        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const s = StyleSheet.create({
  safe: { flex: 1 },
  scroll: { padding: 16, paddingBottom: 40, gap: 8 },

  aiBanner: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    borderRadius: 14,
    borderWidth: 1,
    padding: 14,
    marginBottom: 8,
  },
  aiIcon: { fontSize: 28 },
  aiTitle: { fontSize: 15, fontWeight: '700' },
  aiSub: { fontSize: 12, marginTop: 2 },

  photoRow: { flexDirection: 'row', gap: 8, flexWrap: 'wrap', marginBottom: 4 },
  photoThumb: { width: 72, height: 72, borderRadius: 10, overflow: 'hidden', position: 'relative' },
  photoImg: { width: '100%', height: '100%' },
  photoRemove: { position: 'absolute', top: 3, right: 3, backgroundColor: 'rgba(0,0,0,0.55)', borderRadius: 99, width: 18, height: 18, alignItems: 'center', justifyContent: 'center' },
  photoAdd: { width: 72, height: 72, borderRadius: 10, borderWidth: 1.5, borderStyle: 'dashed', alignItems: 'center', justifyContent: 'center' },

  label: { fontSize: 12, fontWeight: '700', textTransform: 'uppercase', letterSpacing: 0.8, marginTop: 6 },
  input: { height: 46, borderRadius: 10, borderWidth: 1, paddingHorizontal: 14, fontSize: 15 },
  textarea: { height: 90, paddingTop: 12 },

  typeRow: { flexDirection: 'row', borderRadius: 12, borderWidth: 1, overflow: 'hidden' },
  typeBtn: { flex: 1, paddingVertical: 12, alignItems: 'center' },
  typeBtnActive: { backgroundColor: PRIMARY },
  typeTxt: { fontSize: 14, fontWeight: '600' },
  typeTxtActive: { color: '#fff' },

  catRow: { gap: 8, paddingVertical: 4 },
  catChip: { paddingHorizontal: 14, paddingVertical: 7, borderRadius: 99, borderWidth: 1 },
  catChipActive: { backgroundColor: PRIMARY, borderColor: PRIMARY },
  catTxt: { fontSize: 13, fontWeight: '500' },
  catTxtActive: { color: '#fff', fontWeight: '600' },

  priceRow: { flexDirection: 'row', gap: 8, alignItems: 'center' },
  suggestBtn: { paddingHorizontal: 14, height: 46, borderRadius: 10, borderWidth: 1, borderColor: PRIMARY, alignItems: 'center', justifyContent: 'center' },
  suggestBtnTxt: { color: PRIMARY, fontSize: 13, fontWeight: '600' },

  suggCard: { flexDirection: 'row', alignItems: 'center', gap: 10, borderRadius: 12, borderWidth: 1, padding: 12 },
  suggPrice: { fontSize: 18, fontWeight: '800' },
  suggReason: { fontSize: 11, marginTop: 2 },
  useBtn: { backgroundColor: PRIMARY, borderRadius: 8, paddingHorizontal: 12, paddingVertical: 6 },
  useBtnTxt: { color: '#fff', fontSize: 13, fontWeight: '600' },

  submitBtn: { backgroundColor: PRIMARY, borderRadius: 14, height: 54, alignItems: 'center', justifyContent: 'center', marginTop: 8 },
  submitTxt: { color: '#fff', fontSize: 17, fontWeight: '700' },
});
