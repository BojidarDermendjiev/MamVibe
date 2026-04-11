import { useCallback, useEffect, useRef, useState } from 'react';
import {
  View,
  Text,
  FlatList,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
  KeyboardAvoidingView,
  Platform,
  Image,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import { messagesApi } from '@/api/messagesApi';
import { useSignalR } from '@/contexts/SignalRContext';
import { useAuthStore } from '@/store/authStore';
import type { Message } from '@mamvibe/shared';
import type { ChatStackParamList } from '@/navigation/types';

type Props = NativeStackScreenProps<ChatStackParamList, 'Conversation'>;

function formatTime(ts: string) {
  return new Date(ts).toLocaleTimeString('en', { hour: '2-digit', minute: '2-digit', hour12: false });
}

function formatDateLabel(ts: string): string {
  const date = new Date(ts);
  const now = new Date();
  const isToday = date.toDateString() === now.toDateString();
  const yesterday = new Date(now);
  yesterday.setDate(yesterday.getDate() - 1);
  if (isToday) return 'Today';
  if (date.toDateString() === yesterday.toDateString()) return 'Yesterday';
  return date.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
}

export default function ConversationScreen({ route, navigation }: Props) {
  const { userId: peerId, displayName, avatarUrl } = route.params;
  const { user } = useAuthStore();
  const { sendMessage, sendTyping, markAsRead, onMessage, onTyping } = useSignalR();

  const [messages, setMessages] = useState<Message[]>([]);
  const [loading, setLoading] = useState(true);
  const [text, setText] = useState('');
  const [sending, setSending] = useState(false);
  const [typingVisible, setTypingVisible] = useState(false);
  const listRef = useRef<FlatList>(null);
  const typingTimer = useRef<ReturnType<typeof setTimeout>>(undefined);

  // Load message history
  useEffect(() => {
    messagesApi.getMessages(peerId)
      .then(({ data }) => {
        setMessages(data);
        scrollToBottom();
      })
      .catch(() => {})
      .finally(() => setLoading(false));

    messagesApi.markAsRead(peerId).catch(() => {});
    markAsRead(peerId).catch(() => {});
  }, [peerId]);

  // Update header with typing indicator
  useEffect(() => {
    navigation.setOptions({
      title: typingVisible ? `${displayName} is typing...` : displayName,
    });
  }, [typingVisible, displayName, navigation]);

  // SignalR: receive messages
  useEffect(() => {
    return onMessage((msg) => {
      const isRelevant = msg.senderId === peerId || msg.receiverId === peerId;
      if (!isRelevant) return;
      setMessages((prev) => {
        // deduplicate — hub might echo our own message back
        if (prev.some((m) => m.id === msg.id)) return prev;
        return [...prev, msg];
      });
      scrollToBottom();
      if (msg.senderId === peerId) {
        messagesApi.markAsRead(peerId).catch(() => {});
        markAsRead(peerId).catch(() => {});
      }
    });
  }, [onMessage, peerId, markAsRead]);

  // SignalR: typing indicator
  useEffect(() => {
    return onTyping((uid) => {
      if (uid !== peerId) return;
      setTypingVisible(true);
      clearTimeout(typingTimer.current);
      typingTimer.current = setTimeout(() => setTypingVisible(false), 2500);
    });
  }, [onTyping, peerId]);

  const scrollToBottom = useCallback(() => {
    setTimeout(() => listRef.current?.scrollToEnd({ animated: true }), 80);
  }, []);

  const handleSend = async () => {
    const content = text.trim();
    if (!content || sending) return;
    setText('');
    setSending(true);

    // Optimistic message
    const tempId = `temp-${Date.now()}`;
    const optimistic: Message = {
      id: tempId,
      content,
      senderId: user!.id,
      receiverId: peerId,
      timestamp: new Date().toISOString(),
      isRead: true,
      senderDisplayName: user?.displayName ?? '',
      senderAvatarUrl: user?.avatarUrl ?? null,
    };
    setMessages((prev) => [...prev, optimistic]);
    scrollToBottom();

    try {
      const real = await sendMessage(peerId, content);
      if (real) {
        setMessages((prev) => prev.map((m) => (m.id === tempId ? real : m)));
      }
    } catch {
      // Remove optimistic on failure
      setMessages((prev) => prev.filter((m) => m.id !== tempId));
      setText(content); // restore text so user can retry
    } finally {
      setSending(false);
    }
  };

  const handleTyping = () => {
    sendTyping(peerId).catch(() => {});
  };

  // Group messages by date for separators
  const renderMessages = () => {
    let lastDate = '';
    return messages.map((msg) => {
      const msgDate = new Date(msg.timestamp).toDateString();
      const showSep = msgDate !== lastDate;
      if (showSep) lastDate = msgDate;
      const isMine = msg.senderId === user?.id;
      const isTemp = msg.id.startsWith('temp-');

      return (
        <View key={msg.id}>
          {showSep && (
            <View style={styles.dateSep}>
              <View style={styles.dateLine} />
              <Text style={styles.dateLabel}>{formatDateLabel(msg.timestamp)}</Text>
              <View style={styles.dateLine} />
            </View>
          )}
          <View style={[styles.msgRow, isMine && styles.msgRowMine]}>
            {!isMine && (
              <View style={styles.peerAvatar}>
                {avatarUrl ? (
                  <Image source={{ uri: avatarUrl }} style={styles.peerAvatarImg} />
                ) : (
                  <Text style={styles.peerAvatarLetter}>{displayName.charAt(0).toUpperCase()}</Text>
                )}
              </View>
            )}
            <View style={[styles.bubble, isMine ? styles.bubbleMine : styles.bubblePeer]}>
              <Text style={[styles.bubbleText, isMine && styles.bubbleTextMine]}>
                {msg.content}
              </Text>
              <Text style={[styles.bubbleTime, isMine && styles.bubbleTimeMine]}>
                {isTemp ? '...' : formatTime(msg.timestamp)}
              </Text>
            </View>
          </View>
        </View>
      );
    });
  };

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      <KeyboardAvoidingView
        style={styles.kav}
        behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
        keyboardVerticalOffset={Platform.OS === 'ios' ? 90 : 0}
      >
        {loading ? (
          <View style={styles.center}>
            <ActivityIndicator size="large" color="#e91e8c" />
          </View>
        ) : (
          <FlatList
            ref={listRef}
            data={[{ key: 'messages' }]}
            keyExtractor={(i) => i.key}
            renderItem={() => <View style={styles.messageList}>{renderMessages()}</View>}
            onLayout={scrollToBottom}
            contentContainerStyle={styles.listContent}
          />
        )}

        {typingVisible && (
          <View style={styles.typingBanner}>
            <Text style={styles.typingText}>{displayName} is typing…</Text>
          </View>
        )}

        {/* Input bar */}
        <View style={styles.inputBar}>
          <TextInput
            style={styles.input}
            placeholder="Type a message..."
            placeholderTextColor="#aaa"
            value={text}
            onChangeText={setText}
            onKeyPress={handleTyping}
            multiline
            maxLength={1000}
            returnKeyType="default"
          />
          <TouchableOpacity
            style={[styles.sendBtn, (!text.trim() || sending) && styles.sendBtnDisabled]}
            onPress={handleSend}
            disabled={!text.trim() || sending}
          >
            {sending ? (
              <ActivityIndicator size="small" color="#fff" />
            ) : (
              <Text style={styles.sendIcon}>➤</Text>
            )}
          </TouchableOpacity>
        </View>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: '#fafafa' },
  kav: { flex: 1 },
  center: { flex: 1, alignItems: 'center', justifyContent: 'center' },
  listContent: { flexGrow: 1, justifyContent: 'flex-end' },
  messageList: { paddingHorizontal: 12, paddingVertical: 8 },

  dateSep: { flexDirection: 'row', alignItems: 'center', marginVertical: 16, paddingHorizontal: 8 },
  dateLine: { flex: 1, height: StyleSheet.hairlineWidth, backgroundColor: '#ddd' },
  dateLabel: { fontSize: 11, color: '#aaa', marginHorizontal: 10, fontWeight: '500' },

  msgRow: { flexDirection: 'row', alignItems: 'flex-end', marginBottom: 6 },
  msgRowMine: { flexDirection: 'row-reverse' },

  peerAvatar: {
    width: 30,
    height: 30,
    borderRadius: 15,
    backgroundColor: '#e91e8c',
    alignItems: 'center',
    justifyContent: 'center',
    marginRight: 6,
    overflow: 'hidden',
    flexShrink: 0,
  },
  peerAvatarImg: { width: 30, height: 30 },
  peerAvatarLetter: { color: '#fff', fontSize: 13, fontWeight: '700' },

  bubble: {
    maxWidth: '75%',
    paddingHorizontal: 12,
    paddingVertical: 8,
    borderRadius: 18,
  },
  bubblePeer: {
    backgroundColor: '#fff',
    borderBottomLeftRadius: 4,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 3,
    elevation: 1,
  },
  bubbleMine: {
    backgroundColor: '#e91e8c',
    borderBottomRightRadius: 4,
    marginLeft: 6,
  },
  bubbleText: { fontSize: 15, color: '#1a1a1a', lineHeight: 20 },
  bubbleTextMine: { color: '#fff' },
  bubbleTime: { fontSize: 10, color: '#aaa', marginTop: 4, textAlign: 'right' },
  bubbleTimeMine: { color: 'rgba(255,255,255,0.7)' },

  typingBanner: { paddingHorizontal: 16, paddingVertical: 4 },
  typingText: { fontSize: 12, color: '#e91e8c', fontStyle: 'italic' },

  inputBar: {
    flexDirection: 'row',
    alignItems: 'flex-end',
    paddingHorizontal: 12,
    paddingVertical: 8,
    backgroundColor: '#fff',
    borderTopWidth: StyleSheet.hairlineWidth,
    borderTopColor: '#eee',
    gap: 8,
  },
  input: {
    flex: 1,
    minHeight: 40,
    maxHeight: 120,
    backgroundColor: '#f5f5f5',
    borderRadius: 20,
    paddingHorizontal: 16,
    paddingVertical: 10,
    fontSize: 15,
    color: '#1a1a1a',
  },
  sendBtn: {
    width: 42,
    height: 42,
    borderRadius: 21,
    backgroundColor: '#e91e8c',
    alignItems: 'center',
    justifyContent: 'center',
  },
  sendBtnDisabled: { opacity: 0.4 },
  sendIcon: { color: '#fff', fontSize: 16, marginLeft: 2 },
});
