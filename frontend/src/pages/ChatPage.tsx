import { useCallback, useEffect, useState, useRef } from 'react';
import { useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { HiPaperAirplane, HiSearch } from 'react-icons/hi';
import { format, isToday, isYesterday } from 'date-fns';
import { messagesApi } from '../api/messagesApi';
import { useSignalR } from '../hooks/useSignalR';
import { useNotification } from '../contexts/NotificationContext';
import { useAuthStore } from '../store/authStore';
import type { Message, Conversation } from '../types/message';
import Avatar from '../components/common/Avatar';
import LoadingSpinner from '../components/common/LoadingSpinner';

function groupConversationsByDate(conversations: Conversation[]) {
  const today: Conversation[] = [];
  const yesterday: Conversation[] = [];
  const older: Conversation[] = [];

  for (const conv of conversations) {
    const date = new Date(conv.lastMessageTime);
    if (isToday(date)) today.push(conv);
    else if (isYesterday(date)) yesterday.push(conv);
    else older.push(conv);
  }

  return { today, yesterday, older };
}

function formatMessageTime(timestamp: string) {
  const date = new Date(timestamp);
  if (isToday(date)) return format(date, 'HH:mm');
  if (isYesterday(date)) return 'Yesterday';
  return format(date, 'dd MMM');
}

export default function ChatPage() {
  const { userId } = useParams<{ userId?: string }>();
  const { t } = useTranslation();
  const { user } = useAuthStore();
  const { sendMessage, sendTyping, onMessage, onTyping } = useSignalR();
  const { markConversationRead, setActiveChatUserId } = useNotification();
  const [conversations, setConversations] = useState<Conversation[]>([]);
  const [messages, setMessages] = useState<Message[]>([]);
  const [activeChat, setActiveChat] = useState<string | null>(userId || null);
  const [newMessage, setNewMessage] = useState('');
  const [loading, setLoading] = useState(true);
  const [typingUser, setTypingUser] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const typingTimeout = useRef<ReturnType<typeof setTimeout> | undefined>(undefined);

  const scrollToBottom = useCallback(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, []);

  // Sync active chat to NotificationContext so it knows not to increment badge
  useEffect(() => {
    setActiveChatUserId(activeChat);
    return () => setActiveChatUserId(null);
  }, [activeChat, setActiveChatUserId]);

  useEffect(() => {
    messagesApi.getConversations().then((res) => {
      setConversations(res.data);
      setLoading(false);
    }).catch(() => setLoading(false));
  }, []);

  useEffect(() => {
    const unsub1 = onMessage((msg) => {
      const isFromActiveChat = msg.senderId === activeChat;
      const isSentToActiveChat = msg.receiverId === activeChat;

      if (isFromActiveChat || isSentToActiveChat) {
        setMessages((prev) => [...prev, msg]);
        setTimeout(scrollToBottom, 100);
      }

      // If the message is from the user we're currently chatting with,
      // mark it as read immediately — no unread increment
      if (isFromActiveChat) {
        messagesApi.markAsRead(activeChat).catch(() => {});
      }

      setConversations((prev) => {
        const existing = prev.find((c) => c.userId === msg.senderId || c.userId === msg.receiverId);
        if (existing) {
          return prev.map((c) => {
            if (c.userId !== msg.senderId && c.userId !== msg.receiverId) return c;
            // Only increment unread if this message is NOT from the active chat
            const shouldIncrementUnread = c.userId === msg.senderId && msg.senderId !== activeChat;
            return {
              ...c,
              lastMessage: msg.content,
              lastMessageTime: msg.timestamp,
              unreadCount: shouldIncrementUnread ? c.unreadCount + 1 : c.unreadCount,
            };
          });
        }
        return prev;
      });
    });

    const unsub2 = onTyping((uid) => {
      if (uid === activeChat) {
        setTypingUser(uid);
        clearTimeout(typingTimeout.current);
        typingTimeout.current = setTimeout(() => setTypingUser(null), 2000);
      }
    });

    return () => { unsub1(); unsub2(); };
  }, [onMessage, onTyping, activeChat, user?.id, scrollToBottom]);

  useEffect(() => {
    if (!activeChat) return;
    const loadMessages = async () => {
      try {
        const { data } = await messagesApi.getMessages(activeChat);
        setMessages(data);
        setTimeout(scrollToBottom, 100);
        await messagesApi.markAsRead(activeChat);
        markConversationRead(activeChat);
        setConversations((prev) =>
          prev.map((c) => c.userId === activeChat ? { ...c, unreadCount: 0 } : c)
        );
      } catch { /* ignore */ }
    };
    loadMessages();
  }, [activeChat, markConversationRead, scrollToBottom]);

  const handleSend = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newMessage.trim() || !activeChat) return;
    try {
      const msg = await sendMessage(activeChat, newMessage.trim());
      setNewMessage('');
      if (msg) {
        setMessages((prev) => [...prev, msg]);
        setConversations((prev) =>
          prev.map((c) =>
            c.userId === activeChat
              ? { ...c, lastMessage: msg.content, lastMessageTime: msg.timestamp }
              : c
          )
        );
        setTimeout(scrollToBottom, 100);
      }
    } catch { /* ignore */ }
  };

  const handleTyping = () => {
    if (activeChat) {
      sendTyping(activeChat).catch(() => {});
    }
  };

  const activeConversation = conversations.find((c) => c.userId === activeChat);

  const filteredConversations = searchQuery
    ? conversations.filter((c) =>
        c.displayName.toLowerCase().includes(searchQuery.toLowerCase())
      )
    : conversations;

  const grouped = groupConversationsByDate(filteredConversations);

  if (loading) return <LoadingSpinner size="lg" className="py-20" />;

  const renderConversationItem = (conv: Conversation) => (
    <button
      key={conv.userId}
      onClick={() => setActiveChat(conv.userId)}
      className={`w-full flex items-center gap-3 px-4 py-3 transition-all duration-200 text-left rounded-lg mx-2 ${
        activeChat === conv.userId
          ? 'bg-primary/8 border-l-3 border-primary'
          : 'hover:bg-peach/50'
      }`}
      style={{ width: 'calc(100% - 16px)' }}
    >
      <div className="relative flex-shrink-0">
        <Avatar src={conv.avatarUrl} size="md" />
        {conv.unreadCount > 0 && (
          <span className="absolute -top-0.5 -right-0.5 bg-primary text-white text-[10px] font-bold rounded-full h-4 min-w-4 flex items-center justify-center px-1">
            {conv.unreadCount}
          </span>
        )}
      </div>
      <div className="flex-1 min-w-0">
        <div className="flex items-center justify-between mb-0.5">
          <p className={`text-sm truncate ${conv.unreadCount > 0 ? 'font-semibold text-primary-dark' : 'font-medium text-gray-700'}`}>
            {conv.displayName}
          </p>
          <span className="text-[11px] text-gray-400 ml-2 flex-shrink-0">
            {formatMessageTime(conv.lastMessageTime)}
          </span>
        </div>
        <p className={`text-xs truncate ${conv.unreadCount > 0 ? 'text-gray-600 font-medium' : 'text-gray-400'}`}>
          {conv.lastMessage}
        </p>
      </div>
    </button>
  );

  const renderDateGroup = (label: string, items: Conversation[]) => {
    if (items.length === 0) return null;
    return (
      <div key={label}>
        <p className="text-[11px] font-semibold text-gray-400 uppercase tracking-wider px-5 pt-4 pb-1.5">
          {label}
        </p>
        <div className="space-y-0.5">{items.map(renderConversationItem)}</div>
      </div>
    );
  };

  // Group messages by date for separators
  const renderMessages = () => {
    let lastDate = '';
    return messages.map((msg) => {
      const isMine = msg.senderId === user?.id;
      const msgDate = format(new Date(msg.timestamp), 'dd MMM yyyy');
      const showDateSep = msgDate !== lastDate;
      lastDate = msgDate;

      return (
        <div key={msg.id}>
          {showDateSep && (
            <div className="flex items-center gap-3 my-5">
              <div className="flex-1 h-px bg-lavender/30" />
              <span className="text-[11px] text-gray-400 font-medium">
                {isToday(new Date(msg.timestamp))
                  ? 'Today'
                  : isYesterday(new Date(msg.timestamp))
                  ? 'Yesterday'
                  : msgDate}
              </span>
              <div className="flex-1 h-px bg-lavender/30" />
            </div>
          )}
          <div className={`flex gap-2.5 mb-4 ${isMine ? 'flex-row-reverse' : 'flex-row'}`}>
            <div className="flex-shrink-0 mt-1">
              <Avatar
                src={isMine ? user?.avatarUrl : activeConversation?.avatarUrl}
                profileType={isMine ? user?.profileType : undefined}
                size="sm"
              />
            </div>
            <div className={`max-w-[65%] ${isMine ? 'items-end' : 'items-start'} flex flex-col`}>
              <div className={`flex items-center gap-2 mb-1 ${isMine ? 'flex-row-reverse' : ''}`}>
                <span className="text-xs font-semibold text-gray-600">
                  {isMine ? t('chat.you') : (msg.senderDisplayName || activeConversation?.displayName)}
                </span>
                <span className="text-[10px] text-gray-400">
                  {format(new Date(msg.timestamp), 'HH:mm')}
                </span>
              </div>
              <div
                className={`px-4 py-2.5 text-sm leading-relaxed ${
                  isMine
                    ? 'bg-primary text-white rounded-2xl rounded-tr-md'
                    : 'bg-peach/60 text-gray-800 rounded-2xl rounded-tl-md'
                }`}
              >
                {msg.content}
              </div>
            </div>
          </div>
        </div>
      );
    });
  };

  return (
    <div className="max-w-6xl mx-auto px-4 py-4">
      <div className="bg-white rounded-2xl border border-lavender/20 shadow-sm flex h-[calc(100vh-8rem)] overflow-hidden">
        {/* Left sidebar — Inbox */}
        <div className="w-80 border-r border-lavender/20 flex flex-col bg-white">
          {/* Inbox header */}
          <div className="px-5 pt-5 pb-3">
            <h2 className="text-lg font-bold text-primary-dark">{t('chat.title')}</h2>
          </div>

          {/* Search */}
          <div className="px-4 pb-3">
            <div className="relative">
              <HiSearch className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
              <input
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                placeholder={t('common.search')}
                className="w-full pl-9 pr-3 py-2 rounded-lg border border-lavender/40 bg-peach/30 text-sm text-gray-700 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-primary/30 focus:border-primary/30 transition-all"
              />
            </div>
          </div>

          {/* Conversation list */}
          <div className="flex-1 overflow-y-auto">
            {filteredConversations.length === 0 ? (
              <p className="text-center text-gray-400 text-sm py-10">{t('chat.no_conversations')}</p>
            ) : (
              <>
                {renderDateGroup('Today', grouped.today)}
                {renderDateGroup('Yesterday', grouped.yesterday)}
                {renderDateGroup('Earlier', grouped.older)}
              </>
            )}
          </div>
        </div>

        {/* Right side — Messages */}
        <div className="flex-1 flex flex-col bg-peach/20">
          {activeChat ? (
            <>
              {/* Chat header */}
              <div className="px-6 py-4 bg-white border-b border-lavender/20 flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <Avatar src={activeConversation?.avatarUrl} size="md" />
                  <div>
                    <p className="font-semibold text-primary-dark">
                      {activeConversation?.displayName || 'User'}
                    </p>
                    {typingUser ? (
                      <p className="text-xs text-primary animate-pulse">{t('chat.typing')}</p>
                    ) : (
                      <p className="text-xs text-gray-400">{t('chat.online')}</p>
                    )}
                  </div>
                </div>
              </div>

              {/* Messages */}
              <div className="flex-1 overflow-y-auto px-6 py-4">
                {renderMessages()}
                <div ref={messagesEndRef} />
              </div>

              {/* Message input */}
              <div className="px-6 py-4 bg-white border-t border-lavender/20">
                <form onSubmit={handleSend} className="flex items-center gap-3">
                  <input
                    value={newMessage}
                    onChange={(e) => setNewMessage(e.target.value)}
                    onKeyDown={handleTyping}
                    placeholder={t('chat.type_message')}
                    className="flex-1 px-4 py-3 rounded-xl border border-lavender/40 bg-peach/20 text-sm text-gray-700 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-primary/30 focus:border-primary/30 transition-all"
                  />
                  <button
                    type="submit"
                    disabled={!newMessage.trim()}
                    className="flex items-center gap-2 px-5 py-3 bg-primary text-white text-sm font-medium rounded-xl hover:bg-primary-dark disabled:opacity-40 transition-all duration-200 shadow-sm hover:shadow-md"
                  >
                    {t('chat.send')}
                    <HiPaperAirplane className="h-4 w-4 rotate-90" />
                  </button>
                </form>
              </div>
            </>
          ) : (
            <div className="flex-1 flex flex-col items-center justify-center text-gray-400 gap-3">
              <div className="h-16 w-16 rounded-full bg-lavender/20 flex items-center justify-center">
                <HiPaperAirplane className="h-7 w-7 text-primary/40 rotate-90" />
              </div>
              <p className="text-sm">{t('chat.select_conversation')}</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
