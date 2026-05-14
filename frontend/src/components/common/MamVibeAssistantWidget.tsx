import { useState, useRef, useEffect, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { X, Send, Bot, MessageCircleQuestion, Loader2 } from "lucide-react";
import { assistantApi, type ChatMessage } from "../../api/assistantApi";

const SUGGESTED: Record<string, string[]> = {
  en: [
    "How do I sell an item?",
    "How does shipping work?",
    "Where can I find doctor reviews?",
    "How do I pay for an item?",
  ],
  bg: [
    "Как да продам продукт?",
    "Как работи доставката?",
    "Къде намирам отзиви за лекари?",
    "Как да платя за продукт?",
  ],
};

const WELCOME: Record<string, string> = {
  en: "Hi! 👋 I'm the MamVibe Assistant. I can help you navigate the platform — buying, selling, shipping, doctor reviews, and more.\n\nWhat would you like to know?",
  bg: "Здравей! 👋 Аз съм MamVibe Асистентът. Мога да те помогна с платформата — купуване, продаване, доставки, отзиви за лекари и още.\n\nС какво мога да помогна?",
};

const PLACEHOLDER: Record<string, string> = {
  en: "Ask about MamVibe…",
  bg: "Питай за MamVibe…",
};

const SUBTITLE: Record<string, string> = {
  en: "Ask me anything about the platform",
  bg: "Питай ме всичко за платформата",
};

const QUICK_LABEL: Record<string, string> = {
  en: "Quick questions",
  bg: "Бързи въпроси",
};

const ERROR_MSG: Record<string, string> = {
  en: "Sorry, I couldn't connect right now. Please try again in a moment.",
  bg: "Съжалявам, не мога да се свържа в момента. Моля, опитай отново.",
};

export default function MamVibeAssistantWidget() {
  const { i18n } = useTranslation();
  const lang = i18n.language === "bg" ? "bg" : "en";

  const makeWelcome = (l: string): ChatMessage => ({
    role: "assistant",
    content: WELCOME[l] ?? WELCOME.en,
  });

  const [open, setOpen] = useState(false);
  const [messages, setMessages] = useState<ChatMessage[]>([makeWelcome(lang)]);
  const [input, setInput] = useState("");
  const [loading, setLoading] = useState(false);
  const prevLang = useRef(lang);
  const bottomRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  // Reset welcome message when language changes
  useEffect(() => {
    if (prevLang.current !== lang) {
      prevLang.current = lang;
      setMessages([makeWelcome(lang)]);
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [lang]);

  useEffect(() => {
    if (open) {
      setTimeout(() => {
        bottomRef.current?.scrollIntoView({ behavior: "smooth" });
        inputRef.current?.focus();
      }, 80);
    }
  }, [open]);

  useEffect(() => {
    if (open) bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages, open]);

  const send = useCallback(
    async (text: string) => {
      const trimmed = text.trim();
      if (!trimmed || loading) return;

      const userMsg: ChatMessage = { role: "user", content: trimmed };
      setMessages((prev) => [...prev, userMsg]);
      setInput("");
      setLoading(true);

      try {
        const history = messages.slice(1); // exclude welcome message
        const { data } = await assistantApi.chat(trimmed, history, lang);
        setMessages((prev) => [...prev, { role: "assistant", content: data.reply }]);
      } catch {
        setMessages((prev) => [
          ...prev,
          { role: "assistant", content: ERROR_MSG[lang] ?? ERROR_MSG.en },
        ]);
      } finally {
        setLoading(false);
      }
    },
    [loading, messages, lang]
  );

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    send(input);
  };

  // ScrollToTop sits at right-16 on mobile, right-24 on desktop — no overlap.
  // Assistant button: right-4 mobile, right-6 desktop.
  // ScrollToTop sits at bottom-[5.5rem]/bottom-6 (mobile/desktop).
  // Assistant button is one step above it:
  //   mobile: bottom-[9rem]  (ScrollToTop h-10=40px + 88px + 8px gap ≈ 144px)
  //   desktop: bottom-[4.5rem] (ScrollToTop h-10=40px + 24px + 8px gap ≈ 72px)
  const BUTTON_CLASS =
    "fixed z-50 w-14 h-14 rounded-full shadow-xl transition-all duration-200 " +
    "flex items-center justify-center " +
    "bg-primary text-white hover:bg-primary/90 hover:scale-105 active:scale-95 " +
    "bottom-[9rem] right-4 md:bottom-[4.5rem] md:right-6";

  const PANEL_CLASS =
    "fixed z-50 flex flex-col overflow-hidden " +
    "bg-white dark:bg-[#2d2a42] " +
    "rounded-2xl shadow-2xl border border-gray-100 dark:border-white/10 " +
    "w-[calc(100vw-2rem)] max-w-[360px] " +
    "bottom-[13.5rem] right-4 " +
    "md:bottom-[8.5rem] md:right-6";

  const suggested = SUGGESTED[lang] ?? SUGGESTED.en;

  return (
    <>
      {/* ── Chat panel ── */}
      {open && (
        <div className={PANEL_CLASS} style={{ height: "min(500px, calc(100dvh - 12rem))" }}>
          {/* Header */}
          <div className="flex items-center gap-3 px-4 py-3 bg-primary text-white flex-shrink-0">
            <div className="w-8 h-8 rounded-full bg-white/20 flex items-center justify-center flex-shrink-0">
              <Bot size={18} />
            </div>
            <div className="flex-1 min-w-0">
              <p className="font-semibold text-sm leading-none">MamVibe Assistant</p>
              <p className="text-[11px] text-white/70 mt-0.5">{SUBTITLE[lang]}</p>
            </div>
            <button
              onClick={() => setOpen(false)}
              aria-label="Close assistant"
              className="p-1 rounded-full hover:bg-white/20 transition-colors flex-shrink-0"
            >
              <X size={16} />
            </button>
          </div>

          {/* Messages */}
          <div className="flex-1 overflow-y-auto px-4 py-3 space-y-3">
            {messages.map((msg, i) => (
              <div key={i} className={`flex ${msg.role === "user" ? "justify-end" : "justify-start"}`}>
                {msg.role === "assistant" && (
                  <div className="w-6 h-6 rounded-full bg-primary/10 flex items-center justify-center flex-shrink-0 mr-2 mt-0.5">
                    <Bot size={13} className="text-primary" />
                  </div>
                )}
                <div
                  className={`max-w-[80%] px-3 py-2 rounded-2xl text-sm leading-relaxed whitespace-pre-wrap break-words ${
                    msg.role === "user"
                      ? "bg-primary text-white rounded-br-sm"
                      : "bg-gray-100 dark:bg-white/10 text-gray-800 dark:text-gray-100 rounded-bl-sm"
                  }`}
                >
                  {msg.content}
                </div>
              </div>
            ))}

            {loading && (
              <div className="flex justify-start">
                <div className="w-6 h-6 rounded-full bg-primary/10 flex items-center justify-center flex-shrink-0 mr-2 mt-0.5">
                  <Bot size={13} className="text-primary" />
                </div>
                <div className="bg-gray-100 dark:bg-white/10 rounded-2xl rounded-bl-sm px-4 py-3">
                  <Loader2 size={15} className="text-primary animate-spin" />
                </div>
              </div>
            )}

            {messages.length === 1 && !loading && (
              <div className="space-y-2 pt-1">
                <p className="text-[11px] text-gray-400 dark:text-gray-500 font-medium px-1">
                  {QUICK_LABEL[lang]}
                </p>
                {suggested.map((q) => (
                  <button
                    key={q}
                    onClick={() => send(q)}
                    className="w-full text-left text-xs px-3 py-2 rounded-xl
                               border border-primary/20 text-primary
                               hover:bg-primary/5 dark:hover:bg-primary/10
                               transition-colors"
                  >
                    {q}
                  </button>
                ))}
              </div>
            )}

            <div ref={bottomRef} />
          </div>

          {/* Input */}
          <form
            onSubmit={handleSubmit}
            className="flex items-center gap-2 px-3 py-2.5 border-t border-gray-100 dark:border-white/10 flex-shrink-0"
          >
            <input
              ref={inputRef}
              value={input}
              onChange={(e) => setInput(e.target.value)}
              placeholder={PLACEHOLDER[lang]}
              maxLength={600}
              disabled={loading}
              className="flex-1 text-sm px-3 py-2 rounded-xl
                         border border-gray-200 dark:border-white/10
                         bg-gray-50 dark:bg-white/5
                         text-gray-800 dark:text-gray-100
                         placeholder-gray-400 dark:placeholder-gray-600
                         focus:outline-none focus:ring-2 focus:ring-primary/30
                         disabled:opacity-50"
            />
            <button
              type="submit"
              disabled={!input.trim() || loading}
              aria-label="Send message"
              className="w-9 h-9 rounded-xl bg-primary text-white flex items-center justify-center
                         hover:bg-primary/90 disabled:opacity-40 transition-colors flex-shrink-0"
            >
              <Send size={15} />
            </button>
          </form>
        </div>
      )}

      {/* ── Floating trigger button ── */}
      <button
        onClick={() => setOpen((v) => !v)}
        aria-label={open ? "Close MamVibe Assistant" : "Open MamVibe Assistant"}
        className={BUTTON_CLASS}
      >
        {open ? <X size={22} /> : <MessageCircleQuestion size={24} />}
      </button>
    </>
  );
}
