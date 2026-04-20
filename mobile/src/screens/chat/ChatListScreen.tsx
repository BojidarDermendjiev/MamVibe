import { useCallback, useEffect, useState } from 'react';
import {
  View,
  Text,
  FlatList,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
  TextInput,
  Image,
  RefreshControl,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useTranslation } from 'react-i18next';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import { messagesApi } from '@/api/messagesApi';
import { useSignalR } from '@/contexts/SignalRContext';
import { useTheme } from '@/contexts/ThemeContext';
import type { Conversation } from '@mamvibe/shared';
import type { ChatStackParamList } from '@/navigation/types';

type Props = NativeStackScreenProps<ChatStackParamList, 'ChatList'>;

function formatTime(timestamp: string): string {
  const date = new Date(timestamp);
  const now = new Date();
  const isToday = date.toDateString() === now.toDateString();
  const yesterday = new Date(now);
  yesterday.setDate(yesterday.getDate() - 1);
  const isYesterday = date.toDateString() === yesterday.toDateString();

  if (isToday) return date.toLocaleTimeString('en', { hour: '2-digit', minute: '2-digit', hour12: false });
  if (isYesterday) return 'Yesterday';
  return date.toLocaleDateString('en-GB', { day: '2-digit', month: 'short' });
}

export default function ChatListScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const { colors } = useTheme();
  const [conversations, setConversations] = useState<Conversation[]>([]);
  const [filtered, setFiltered] = useState<Conversation[]>([]);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const { onMessage } = useSignalR();

  const load = useCallback(async () => {
    try {
      const { data } = await messagesApi.getConversations();
      setConversations(data);
      setFiltered(data);
    } catch {
      // silent — list stays as-is
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  // Keep conversation list fresh when new messages arrive via SignalR
  useEffect(() => {
    return onMessage((msg) => {
      setConversations((prev) => {
        const existing = prev.find((c) => c.userId === msg.senderId);
        if (existing) {
          return prev.map((c) =>
            c.userId === msg.senderId
              ? { ...c, lastMessage: msg.content, lastMessageTime: msg.timestamp, unreadCount: c.unreadCount + 1 }
              : c,
          );
        }
        return [
          { userId: msg.senderId, displayName: msg.senderDisplayName, avatarUrl: msg.senderAvatarUrl, lastMessage: msg.content, lastMessageTime: msg.timestamp, unreadCount: 1 },
          ...prev,
        ];
      });
    });
  }, [onMessage]);

  useEffect(() => {
    if (!search.trim()) { setFiltered(conversations); return; }
    setFiltered(conversations.filter((c) => c.displayName.toLowerCase().includes(search.toLowerCase())));
  }, [search, conversations]);

  const handleOpen = (conv: Conversation) => {
    setConversations((prev) =>
      prev.map((c) => (c.userId === conv.userId ? { ...c, unreadCount: 0 } : c)),
    );
    navigation.navigate('Conversation', {
      userId: conv.userId,
      displayName: conv.displayName,
      avatarUrl: conv.avatarUrl,
    });
  };

  const renderItem = ({ item }: { item: Conversation }) => (
    <TouchableOpacity style={styles.row} onPress={() => handleOpen(item)} activeOpacity={0.7}>
      <View style={styles.avatarWrap}>
        {item.avatarUrl ? (
          <Image source={{ uri: item.avatarUrl }} style={styles.avatar} />
        ) : (
          <View style={[styles.avatar, styles.avatarFallback]}>
            <Text style={styles.avatarLetter}>{item.displayName.charAt(0).toUpperCase()}</Text>
          </View>
        )}
        {item.unreadCount > 0 && (
          <View style={[styles.badge, { borderColor: colors.bg }]}>
            <Text style={styles.badgeText}>{item.unreadCount > 99 ? '99+' : item.unreadCount}</Text>
          </View>
        )}
      </View>

      <View style={styles.rowBody}>
        <View style={styles.rowTop}>
          <Text style={[styles.name, { color: colors.text }, item.unreadCount > 0 && styles.nameBold]} numberOfLines={1}>
            {item.displayName}
          </Text>
          <Text style={[styles.time, { color: colors.text2 }]}>{formatTime(item.lastMessageTime)}</Text>
        </View>
        <Text
          style={[styles.lastMsg, { color: item.unreadCount > 0 ? colors.text : colors.text2 }, item.unreadCount > 0 && styles.lastMsgBold]}
          numberOfLines={1}
        >
          {item.lastMessage}
        </Text>
      </View>
    </TouchableOpacity>
  );

  return (
    <SafeAreaView style={[styles.safe, { backgroundColor: colors.bg }]} edges={['bottom']}>
      <View style={styles.searchWrap}>
        <TextInput
          style={[styles.searchInput, { backgroundColor: colors.input, color: colors.text, borderColor: colors.inputBorder }]}
          placeholder={t('browse.searchPlaceholder')}
          placeholderTextColor={colors.text2}
          value={search}
          onChangeText={setSearch}
          clearButtonMode="while-editing"
        />
      </View>

      {loading ? (
        <View style={styles.center}>
          <ActivityIndicator size="large" color="#d4938f" />
        </View>
      ) : filtered.length === 0 ? (
        <View style={styles.center}>
          <Text style={styles.emptyEmoji}>💬</Text>
          <Text style={[styles.emptyText, { color: colors.text2 }]}>
            {search ? t('browse.noItems') : t('chat.noConversations')}
          </Text>
        </View>
      ) : (
        <FlatList
          data={filtered}
          keyExtractor={(c) => c.userId}
          renderItem={renderItem}
          ItemSeparatorComponent={() => <View style={[styles.separator, { backgroundColor: colors.border }]} />}
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={() => { setRefreshing(true); load(); }}
              tintColor="#d4938f"
            />
          }
        />
      )}
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1 },
  searchWrap: { padding: 12, paddingBottom: 4 },
  searchInput: {
    height: 40,
    borderRadius: 10,
    paddingHorizontal: 14,
    fontSize: 15,
    borderWidth: 1,
  },
  center: { flex: 1, alignItems: 'center', justifyContent: 'center' },
  emptyEmoji: { fontSize: 48, marginBottom: 12 },
  emptyText: { fontSize: 15 },
  row: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: 16, paddingVertical: 12 },
  avatarWrap: { position: 'relative', marginRight: 12 },
  avatar: { width: 50, height: 50, borderRadius: 25 },
  avatarFallback: { backgroundColor: '#d4938f', alignItems: 'center', justifyContent: 'center' },
  avatarLetter: { color: '#fff', fontSize: 20, fontWeight: '700' },
  badge: {
    position: 'absolute',
    top: -2,
    right: -2,
    minWidth: 18,
    height: 18,
    borderRadius: 9,
    backgroundColor: '#d4938f',
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: 4,
    borderWidth: 1.5,
  },
  badgeText: { color: '#fff', fontSize: 10, fontWeight: '700' },
  rowBody: { flex: 1 },
  rowTop: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: 3 },
  name: { fontSize: 15, flexShrink: 1, marginRight: 8 },
  nameBold: { fontWeight: '700' },
  time: { fontSize: 12, flexShrink: 0 },
  lastMsg: { fontSize: 13 },
  lastMsgBold: { fontWeight: '500' },
  separator: { height: StyleSheet.hairlineWidth, marginLeft: 78 },
});
