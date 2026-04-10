import { View, Text, TouchableOpacity, StyleSheet, Image } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import type { CompositeScreenProps } from '@react-navigation/native';
import type { BottomTabScreenProps } from '@react-navigation/bottom-tabs';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import { useAuthStore } from '@/store/authStore';
import { tokenStorage } from '@/api/axiosClient';
import type { MainTabParamList, RootStackParamList } from '@/navigation/types';

type Props = CompositeScreenProps<
  BottomTabScreenProps<MainTabParamList, 'Profile'>,
  NativeStackScreenProps<RootStackParamList>
>;

export default function ProfileScreen({ navigation }: Props) {
  const { user, logout } = useAuthStore();

  const handleLogout = async () => {
    await tokenStorage.clearTokens();
    logout();
  };

  const MENU = [
    { icon: '👜', label: 'Wallet', onPress: () => (navigation as any).navigate('Wallet') },
    { icon: '🗂️', label: 'My Orders', onPress: () => (navigation as any).navigate('Dashboard') },
    { icon: '📦', label: 'My Items', onPress: () => {} },
    { icon: '⚙️', label: 'Settings', onPress: () => {} },
  ];

  return (
    <SafeAreaView style={styles.safe}>
      {/* Header */}
      <View style={styles.header}>
        <View style={styles.avatar}>
          {user?.avatarUrl ? (
            <Image source={{ uri: user.avatarUrl }} style={styles.avatarImg} />
          ) : (
            <Text style={styles.avatarLetter}>{user?.displayName?.charAt(0).toUpperCase()}</Text>
          )}
        </View>
        <Text style={styles.name}>{user?.displayName}</Text>
        <Text style={styles.email}>{user?.email}</Text>
      </View>

      {/* Menu */}
      <View style={styles.menu}>
        {MENU.map((item) => (
          <TouchableOpacity key={item.label} style={styles.menuRow} onPress={item.onPress} activeOpacity={0.7}>
            <Text style={styles.menuIcon}>{item.icon}</Text>
            <Text style={styles.menuLabel}>{item.label}</Text>
            <Text style={styles.menuChevron}>›</Text>
          </TouchableOpacity>
        ))}
      </View>

      <TouchableOpacity style={styles.logoutBtn} onPress={handleLogout}>
        <Text style={styles.logoutText}>Sign Out</Text>
      </TouchableOpacity>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: '#fafafa' },
  header: { alignItems: 'center', paddingVertical: 32, backgroundColor: '#fff', borderBottomWidth: StyleSheet.hairlineWidth, borderBottomColor: '#f0f0f0' },
  avatar: { width: 80, height: 80, borderRadius: 40, backgroundColor: '#e91e8c', alignItems: 'center', justifyContent: 'center', overflow: 'hidden', marginBottom: 12 },
  avatarImg: { width: 80, height: 80 },
  avatarLetter: { color: '#fff', fontSize: 32, fontWeight: '700' },
  name: { fontSize: 20, fontWeight: '700', color: '#1a1a1a' },
  email: { fontSize: 14, color: '#888', marginTop: 2 },
  menu: { marginTop: 16, backgroundColor: '#fff', borderTopWidth: StyleSheet.hairlineWidth, borderBottomWidth: StyleSheet.hairlineWidth, borderColor: '#f0f0f0' },
  menuRow: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: 20, paddingVertical: 16, borderBottomWidth: StyleSheet.hairlineWidth, borderBottomColor: '#f5f5f5' },
  menuIcon: { fontSize: 20, marginRight: 14 },
  menuLabel: { flex: 1, fontSize: 16, color: '#1a1a1a' },
  menuChevron: { fontSize: 22, color: '#ccc' },
  logoutBtn: { margin: 24, height: 50, borderRadius: 12, borderWidth: 1.5, borderColor: '#e91e8c', alignItems: 'center', justifyContent: 'center' },
  logoutText: { color: '#e91e8c', fontSize: 16, fontWeight: '600' },
});
