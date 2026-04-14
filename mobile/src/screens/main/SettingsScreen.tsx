import { useState } from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  ScrollView,
  StyleSheet,
  ActivityIndicator,
  Alert,
  KeyboardAvoidingView,
  Platform,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useTranslation } from 'react-i18next';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { RootStackParamList } from '@/navigation/types';
import { useAuthStore } from '@/store/authStore';
import { authApi } from '@/api/authApi';
import { useTheme } from '@/contexts/ThemeContext';
import { useLanguage } from '@/contexts/LanguageContext';

type Props = NativeStackScreenProps<RootStackParamList, 'Settings'>;

import { ROSE, SAGE } from '@/constants/palette';
const PRIMARY = ROSE;

export default function SettingsScreen({}: Props) {
  const { t } = useTranslation();
  const { colors } = useTheme();
  const { language, setLanguage } = useLanguage();
  const { user, setUser } = useAuthStore();

  const [form, setForm] = useState({
    displayName: user?.displayName ?? '',
    bio: user?.bio ?? '',
    iban: user?.iban ?? '',
  });
  const [profileLoading, setProfileLoading] = useState(false);

  const [pw, setPw] = useState({ currentPassword: '', newPassword: '', confirmNewPassword: '' });
  const [pwLoading, setPwLoading] = useState(false);

  const handleSaveProfile = async () => {
    if (!form.displayName.trim()) {
      Alert.alert(t('common.validation'), t('settings.displayNameRequired'));
      return;
    }
    setProfileLoading(true);
    try {
      const { data } = await authApi.updateProfile(form);
      setUser(data);
      Alert.alert(t('settings.saved'), t('settings.profileUpdated'));
    } catch {
      Alert.alert(t('common.error'), t('settings.profileError'));
    } finally {
      setProfileLoading(false);
    }
  };

  const handleChangePassword = async () => {
    if (pw.newPassword.length < 8) {
      Alert.alert(t('common.validation'), t('settings.passwordMin')); return;
    }
    if (!/[A-Z]/.test(pw.newPassword)) {
      Alert.alert(t('common.validation'), t('settings.passwordUppercase')); return;
    }
    if (!/[0-9]/.test(pw.newPassword)) {
      Alert.alert(t('common.validation'), t('settings.passwordDigit')); return;
    }
    if (pw.newPassword !== pw.confirmNewPassword) {
      Alert.alert(t('common.validation'), t('settings.passwordMismatch')); return;
    }
    setPwLoading(true);
    try {
      await authApi.changePassword(pw);
      Alert.alert(t('common.done'), t('settings.passwordChanged'));
      setPw({ currentPassword: '', newPassword: '', confirmNewPassword: '' });
    } catch (err: any) {
      Alert.alert(t('common.error'), err?.response?.data?.message ?? t('settings.passwordError'));
    } finally {
      setPwLoading(false);
    }
  };

  const inputStyle = [s.input, { backgroundColor: colors.input, borderColor: colors.inputBorder, color: colors.text }];
  const labelStyle = [s.label, { color: PRIMARY }];

  return (
    <SafeAreaView style={[s.safe, { backgroundColor: colors.bg }]}>
      <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : undefined} style={{ flex: 1 }}>
        <ScrollView contentContainerStyle={s.scroll} showsVerticalScrollIndicator={false}>

          {/* ── Language section ── */}
          <View style={[s.card, { backgroundColor: colors.card, borderColor: colors.border }]}>
            <Text style={[s.cardTitle, { color: PRIMARY }]}>{t('settings.language')}</Text>
            <View style={s.langRow}>
              {(['en', 'bg'] as const).map((lang) => (
                <TouchableOpacity
                  key={lang}
                  style={[s.langBtn, { borderColor: colors.border }, language === lang && s.langBtnActive]}
                  onPress={() => setLanguage(lang)}
                >
                  <Text style={[s.langTxt, { color: colors.text2 }, language === lang && s.langTxtActive]}>
                    {lang === 'en' ? '🇬🇧  English' : '🇧🇬  Български'}
                  </Text>
                </TouchableOpacity>
              ))}
            </View>
          </View>

          {/* ── Profile section ── */}
          <View style={[s.card, { backgroundColor: colors.card, borderColor: colors.border }]}>
            <Text style={[s.cardTitle, { color: PRIMARY }]}>{t('settings.profile')}</Text>

            <Text style={labelStyle}>{t('settings.displayName')}</Text>
            <TextInput
              style={inputStyle}
              value={form.displayName}
              onChangeText={(v) => setForm({ ...form, displayName: v })}
              placeholder={t('settings.displayName')}
              placeholderTextColor={colors.text2}
            />

            <Text style={labelStyle}>{t('settings.bio')}</Text>
            <TextInput
              style={[inputStyle, s.textArea]}
              value={form.bio}
              onChangeText={(v) => setForm({ ...form, bio: v })}
              placeholder={t('settings.bioPlaceholder')}
              placeholderTextColor={colors.text2}
              multiline
              numberOfLines={3}
            />

            <Text style={labelStyle}>{t('settings.iban')}</Text>
            <TextInput
              style={inputStyle}
              value={form.iban}
              onChangeText={(v) => setForm({ ...form, iban: v })}
              placeholder="BG80BNBG96611020345678"
              placeholderTextColor={colors.text2}
              autoCapitalize="characters"
            />

            <TouchableOpacity style={s.saveBtn} onPress={handleSaveProfile} disabled={profileLoading}>
              {profileLoading
                ? <ActivityIndicator color="#fff" />
                : <Text style={s.saveBtnText}>{t('settings.saveChanges')}</Text>
              }
            </TouchableOpacity>
          </View>

          {/* ── Change password section ── */}
          <View style={[s.card, { backgroundColor: colors.card, borderColor: colors.border }]}>
            <Text style={[s.cardTitle, { color: PRIMARY }]}>{t('settings.changePassword')}</Text>

            <Text style={labelStyle}>{t('settings.currentPassword')}</Text>
            <TextInput
              style={inputStyle}
              value={pw.currentPassword}
              onChangeText={(v) => setPw({ ...pw, currentPassword: v })}
              placeholder="••••••••"
              placeholderTextColor={colors.text2}
              secureTextEntry
            />

            <Text style={labelStyle}>{t('settings.newPassword')}</Text>
            <TextInput
              style={inputStyle}
              value={pw.newPassword}
              onChangeText={(v) => setPw({ ...pw, newPassword: v })}
              placeholder="••••••••"
              placeholderTextColor={colors.text2}
              secureTextEntry
            />

            <Text style={labelStyle}>{t('settings.confirmNewPassword')}</Text>
            <TextInput
              style={inputStyle}
              value={pw.confirmNewPassword}
              onChangeText={(v) => setPw({ ...pw, confirmNewPassword: v })}
              placeholder="••••••••"
              placeholderTextColor={colors.text2}
              secureTextEntry
            />

            <TouchableOpacity style={s.saveBtn} onPress={handleChangePassword} disabled={pwLoading}>
              {pwLoading
                ? <ActivityIndicator color="#fff" />
                : <Text style={s.saveBtnText}>{t('settings.changePassword')}</Text>
              }
            </TouchableOpacity>
          </View>

        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const s = StyleSheet.create({
  safe: { flex: 1 },
  scroll: { padding: 16, gap: 16, paddingBottom: 40 },
  card: { borderRadius: 16, padding: 20, borderWidth: 1, gap: 6 },
  cardTitle: { fontSize: 18, fontWeight: '700', marginBottom: 8 },
  label: { fontSize: 13, fontWeight: '600', marginTop: 8, marginBottom: 4 },
  input: { height: 46, borderRadius: 10, borderWidth: 1, paddingHorizontal: 14, fontSize: 15 },
  textArea: { height: 80, paddingTop: 12, textAlignVertical: 'top' },
  saveBtn: { backgroundColor: ROSE, borderRadius: 10, height: 48, alignItems: 'center', justifyContent: 'center', marginTop: 16 },
  saveBtnText: { color: '#fff', fontWeight: '700', fontSize: 15 },
  langRow: { flexDirection: 'row', gap: 10 },
  langBtn: { flex: 1, height: 44, borderRadius: 10, borderWidth: 1.5, alignItems: 'center', justifyContent: 'center' },
  langBtnActive: { backgroundColor: PRIMARY, borderColor: PRIMARY },
  langTxt: { fontSize: 14, fontWeight: '600' },
  langTxtActive: { color: '#fff' },
});
