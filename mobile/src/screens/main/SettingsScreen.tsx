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
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { RootStackParamList } from '@/navigation/types';
import { useAuthStore } from '@/store/authStore';
import { authApi } from '@/api/authApi';
import { useTheme } from '@/contexts/ThemeContext';

type Props = NativeStackScreenProps<RootStackParamList, 'Settings'>;

const PRIMARY = '#e91e8c';

export default function SettingsScreen({}: Props) {
  const { colors } = useTheme();
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
      Alert.alert('Validation', 'Display name is required.');
      return;
    }
    setProfileLoading(true);
    try {
      const { data } = await authApi.updateProfile(form);
      setUser(data);
      Alert.alert('Saved', 'Profile updated successfully.');
    } catch {
      Alert.alert('Error', 'Could not update profile.');
    } finally {
      setProfileLoading(false);
    }
  };

  const handleChangePassword = async () => {
    if (pw.newPassword.length < 8) {
      Alert.alert('Validation', 'New password must be at least 8 characters.'); return;
    }
    if (!/[A-Z]/.test(pw.newPassword)) {
      Alert.alert('Validation', 'Password must contain at least one uppercase letter.'); return;
    }
    if (!/[0-9]/.test(pw.newPassword)) {
      Alert.alert('Validation', 'Password must contain at least one digit.'); return;
    }
    if (pw.newPassword !== pw.confirmNewPassword) {
      Alert.alert('Validation', 'Passwords do not match.'); return;
    }
    setPwLoading(true);
    try {
      await authApi.changePassword(pw);
      Alert.alert('Done', 'Password changed successfully.');
      setPw({ currentPassword: '', newPassword: '', confirmNewPassword: '' });
    } catch (err: any) {
      Alert.alert('Error', err?.response?.data?.message ?? 'Could not change password.');
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

          {/* ── Profile section ── */}
          <View style={[s.card, { backgroundColor: colors.card, borderColor: colors.border }]}>
            <Text style={[s.cardTitle, { color: PRIMARY }]}>Profile</Text>

            <Text style={labelStyle}>Display Name</Text>
            <TextInput
              style={inputStyle}
              value={form.displayName}
              onChangeText={(v) => setForm({ ...form, displayName: v })}
              placeholder="Your name"
              placeholderTextColor={colors.text2}
            />

            <Text style={labelStyle}>Bio</Text>
            <TextInput
              style={[inputStyle, s.textArea]}
              value={form.bio}
              onChangeText={(v) => setForm({ ...form, bio: v })}
              placeholder="Tell families about yourself"
              placeholderTextColor={colors.text2}
              multiline
              numberOfLines={3}
            />

            <Text style={labelStyle}>IBAN</Text>
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
                : <Text style={s.saveBtnText}>Save Changes</Text>
              }
            </TouchableOpacity>
          </View>

          {/* ── Change password section ── */}
          <View style={[s.card, { backgroundColor: colors.card, borderColor: colors.border }]}>
            <Text style={[s.cardTitle, { color: PRIMARY }]}>Change Password</Text>

            <Text style={labelStyle}>Current Password</Text>
            <TextInput
              style={inputStyle}
              value={pw.currentPassword}
              onChangeText={(v) => setPw({ ...pw, currentPassword: v })}
              placeholder="••••••••"
              placeholderTextColor={colors.text2}
              secureTextEntry
            />

            <Text style={labelStyle}>New Password</Text>
            <TextInput
              style={inputStyle}
              value={pw.newPassword}
              onChangeText={(v) => setPw({ ...pw, newPassword: v })}
              placeholder="••••••••"
              placeholderTextColor={colors.text2}
              secureTextEntry
            />

            <Text style={labelStyle}>Confirm New Password</Text>
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
                : <Text style={s.saveBtnText}>Change Password</Text>
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
  saveBtn: { backgroundColor: '#e91e8c', borderRadius: 10, height: 48, alignItems: 'center', justifyContent: 'center', marginTop: 16 },
  saveBtnText: { color: '#fff', fontWeight: '700', fontSize: 15 },
});
