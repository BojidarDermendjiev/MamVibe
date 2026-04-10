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
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import { messagesApi } from '@/api/messagesApi';
import { useSignalR } from '@/contexts/SignalRContext';
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
    // Clear unread badge immediately
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
          <View style={styles.badge}>
            <Text style={styles.badgeText}>{item.unreadCount > 99 ? '99+' : item.unreadCount}</Text>
          </View>
        )}
      </View>

      <View style={styles.rowBody}>
        <View style={styles.rowTop}>
          <Text style={[styles.name, item.unreadCount > 0 && styles.nameBold]} numberOfLines={1}>
            {item.displayName}
          </Text>
          <Text style={styles.time}>{formatTime(item.lastMessageTime)}</Text>
        </View>
        <Text
          style={[styles.lastMsg, item.unreadCount > 0 && styles.lastMsgBold]}
          numberOfLines={1}
        >
          {item.lastMessage}
        </Text>
      </View>
    </TouchableOpacity>
  );

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <View style={styles.searchWrap}>
        <TextInput
          style={styles.searchInput}
          placeholder="Search conversations..."
          placeholderTextColor="#aaa"
          value={search}
          onChangeText={setSearch}
          clearButtonMode="while-editing"
        />
      </View>

      {loading ? (
        <View style={styles.center}>
          <ActivityIndicator size="large" color="#e91e8c" />
        </View>
      ) : filtered.length === 0 ? (
        <View style={styles.center}>
          <Text style={styles.emptyEmoji}>💬</Text>
          <Text style={styles.emptyText}>
            {search ? 'No results' : 'No conversations yet'}
          </Text>
        </View>
      ) : (
        <FlatList
          data={filtered}
          keyExtractor={(c) => c.userId}
          renderItem={renderItem}
          ItemSeparatorComponent={() => <View style={styles.separator} />}
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={() => { setRefreshing(true); load(); }}
              tintColor="#e91e8c"
            />
          }
        />
      )}
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: '#fff' },
  searchWrap: { padding: 12, paddingBottom: 4 },
  searchInput: {
    height: 40,
    backgroundColor: '#f5f5f5',
    borderRadius: 10,
    paddingHorizontal: 14,
    fontSize: 15,
    color: '#1a1a1a',
  },
  center: { flex: 1, alignItems: 'center', justifyContent: 'center' },
  emptyEmoji: { fontSize: 48, marginBottom: 12 },
  emptyText: { fontSize: 15, color: '#aaa' },
  row: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: 16, paddingVertical: 12 },
  avatarWrap: { position: 'relative', marginRight: 12 },
  avatar: { width: 50, height: 50, borderRadius: 25 },
  avatarFallback: { backgroundColor: '#e91e8c', alignItems: 'center', justifyContent: 'center' },
  avatarLetter: { color: '#fff', fontSize: 20, fontWeight: '700' },
  badge: {
    position: 'absolute',
    top: -2,
    right: -2,
    minWidth: 18,
    height: 18,
    borderRadius: 9,
    backgroundColor: '#e91e8c',
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: 4,
    borderWidth: 1.5,
    borderColor: '#fff',
  },
  badgeText: { color: '#fff', fontSize: 10, fontWeight: '700' },
  rowBody: { flex: 1 },
  rowTop: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: 3 },
  name: { fontSize: 15, color: '#1a1a1a', flexShrink: 1, marginRight: 8 },
  nameBold: { fontWeight: '700' },
  time: { fontSize: 12, color: '#aaa', flexShrink: 0 },
  lastMsg: { fontSize: 13, color: '#aaa' },
  lastMsgBold: { color: '#555', fontWeight: '500' },
  separator: { height: StyleSheet.hairlineWidth, backgroundColor: '#f0f0f0', marginLeft: 78 },
});
