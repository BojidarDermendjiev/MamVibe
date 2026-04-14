import { View, Text, TouchableOpacity, StyleSheet, Image, Switch } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useTranslation } from 'react-i18next';
import type { CompositeScreenProps } from '@react-navigation/native';
import type { BottomTabScreenProps } from '@react-navigation/bottom-tabs';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import { useAuthStore } from '@/store/authStore';
import { tokenStorage } from '@/api/axiosClient';
import { useTheme } from '@/contexts/ThemeContext';
import type { MainTabParamList, RootStackParamList } from '@/navigation/types';

type Props = CompositeScreenProps<
  BottomTabScreenProps<MainTabParamList, 'Profile'>,
  NativeStackScreenProps<RootStackParamList>
>;

export default function ProfileScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const { user, logout } = useAuthStore();
  const { colors, isDark, toggleTheme } = useTheme();

  const handleLogout = async () => {
    await tokenStorage.clearTokens();
    logout();
  };

  const MENU = [
    { icon: '👜', label: t('profile.wallet'),    onPress: () => (navigation as any).navigate('Wallet') },
    { icon: '🗂️', label: t('profile.myOrders'), onPress: () => (navigation as any).navigate('Dashboard') },
    { icon: '📦', label: t('profile.myItems'),  onPress: () => (navigation as any).navigate('MyItems') },
    { icon: '⚙️', label: t('profile.settings'),  onPress: () => (navigation as any).navigate('Settings') },
  ];

  return (
    <SafeAreaView style={[s.safe, { backgroundColor: colors.bg }]}>
      {/* Header */}
      <View style={[s.header, { backgroundColor: colors.card, borderBottomColor: colors.border }]}>
        <View style={s.avatar}>
          {user?.avatarUrl ? (
            <Image source={{ uri: user.avatarUrl }} style={s.avatarImg} />
          ) : (
            <Text style={s.avatarLetter}>{user?.displayName?.charAt(0).toUpperCase()}</Text>
          )}
        </View>
        <Text style={[s.name, { color: colors.text }]}>{user?.displayName}</Text>
        <Text style={[s.email, { color: colors.text2 }]}>{user?.email}</Text>
      </View>

      {/* Dark mode toggle */}
      <View style={[s.menu, { backgroundColor: colors.card, borderColor: colors.border }]}>
        <View style={[s.menuRow, s.toggleRow, { borderBottomColor: colors.border }]}>
          <Text style={s.menuIcon}>{isDark ? '🌙' : '☀️'}</Text>
          <Text style={[s.menuLabel, { color: colors.text }]}>{isDark ? t('profile.darkMode') : t('profile.lightMode')}</Text>
          <Switch
            value={isDark}
            onValueChange={toggleTheme}
            trackColor={{ false: '#ccc', true: '#d4938f' }}
            thumbColor="#fff"
          />
        </View>

        {/* Nav items */}
        {MENU.map((item, i) => (
          <TouchableOpacity
            key={item.label}
            style={[s.menuRow, { borderBottomColor: colors.border, borderBottomWidth: i < MENU.length - 1 ? StyleSheet.hairlineWidth : 0 }]}
            onPress={item.onPress}
            activeOpacity={0.7}
          >
            <Text style={s.menuIcon}>{item.icon}</Text>
            <Text style={[s.menuLabel, { color: colors.text }]}>{item.label}</Text>
            <Text style={[s.menuChevron, { color: colors.text2 }]}>›</Text>
          </TouchableOpacity>
        ))}
      </View>

      <TouchableOpacity style={s.logoutBtn} onPress={handleLogout}>
        <Text style={s.logoutText}>{t('profile.signOut')}</Text>
      </TouchableOpacity>
    </SafeAreaView>
  );
}

const s = StyleSheet.create({
  safe: { flex: 1 },
  header: { alignItems: 'center', paddingVertical: 32, borderBottomWidth: StyleSheet.hairlineWidth },
  avatar: { width: 80, height: 80, borderRadius: 40, backgroundColor: '#d4938f', alignItems: 'center', justifyContent: 'center', overflow: 'hidden', marginBottom: 12 },
  avatarImg: { width: 80, height: 80 },
  avatarLetter: { color: '#fff', fontSize: 32, fontWeight: '700' },
  name: { fontSize: 20, fontWeight: '700' },
  email: { fontSize: 14, marginTop: 2 },
  menu: { marginTop: 16, borderTopWidth: StyleSheet.hairlineWidth, borderBottomWidth: StyleSheet.hairlineWidth },
  menuRow: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: 20, paddingVertical: 16, borderBottomWidth: StyleSheet.hairlineWidth },
  toggleRow: { borderBottomWidth: StyleSheet.hairlineWidth },
  menuIcon: { fontSize: 20, marginRight: 14 },
  menuLabel: { flex: 1, fontSize: 16 },
  menuChevron: { fontSize: 22 },
  logoutBtn: { margin: 24, height: 50, borderRadius: 12, borderWidth: 1.5, borderColor: '#d4938f', alignItems: 'center', justifyContent: 'center' },
  logoutText: { color: '#d4938f', fontSize: 16, fontWeight: '600' },
});
