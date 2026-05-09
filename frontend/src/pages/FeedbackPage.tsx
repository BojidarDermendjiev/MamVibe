import { useState, useEffect, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import toast from '@/utils/toast';
import { Paperclip, Mic, MicOff, CornerDownLeft, X } from 'lucide-react';
import { feedbackApi } from '../api/feedbackApi';
import { FeedbackCategory } from '../types/feedback';
import type { Feedback, FeedbackCategory as FeedbackCategoryType } from '../types/feedback';
import { useAuthStore } from '../store/authStore';
import StarRating from '../components/feedback/StarRating';

const categoryOptions: { value: FeedbackCategoryType; labelKey: string; icon: string }[] = [
  { value: FeedbackCategory.Praise,         labelKey: 'feedback.cat_praise',      icon: '❤️' },
  { value: FeedbackCategory.Improvement,    labelKey: 'feedback.cat_improvement',  icon: '💡' },
  { value: FeedbackCategory.FeatureRequest, labelKey: 'feedback.cat_feature',      icon: '🚀' },
  { value: FeedbackCategory.BugReport,      labelKey: 'feedback.cat_bug',          icon: '🐛' },
];

const categoryColor: Record<number, string> = {
  [FeedbackCategory.Praise]:         'bg-green-100 text-green-700 dark:bg-green-900/40 dark:text-green-300',
  [FeedbackCategory.Improvement]:    'bg-amber-100 text-amber-700 dark:bg-amber-900/40 dark:text-amber-300',
  [FeedbackCategory.FeatureRequest]: 'bg-blue-100 text-blue-700 dark:bg-blue-900/40 dark:text-blue-300',
  [FeedbackCategory.BugReport]:      'bg-red-100 text-red-700 dark:bg-red-900/40 dark:text-red-300',
};


export default function FeedbackPage() {
  const { t, i18n } = useTranslation();
  const { user } = useAuthStore();

  const [loading, setLoading] = useState(false);
  const [feedbacks, setFeedbacks] = useState<Feedback[]>([]);
  const [listLoading, setListLoading] = useState(true);
  const [attachedFile, setAttachedFile] = useState<File | null>(null);
  const [isRecording, setIsRecording] = useState(false);

  const fileInputRef = useRef<HTMLInputElement>(null);
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const recognitionRef = useRef<any>(null);

  const [form, setForm] = useState<{
    rating: number;
    category: FeedbackCategoryType;
    content: string;
    isContactable: boolean;
  }>({
    rating: 5,
    category: FeedbackCategory.Praise,
    content: '',
    isContactable: false,
  });

  // Stop recording if component unmounts mid-session
  useEffect(() => {
    return () => {
      recognitionRef.current?.stop();
    };
  }, []);

  const loadFeedbacks = async () => {
    setListLoading(true);
    try {
      const { data } = await feedbackApi.getAll(1, 20);
      setFeedbacks(data.items);
    } catch {
      // silently fail
    } finally {
      setListLoading(false);
    }
  };

  useEffect(() => {
    loadFeedbacks();
  }, []);

  // ── File attachment ──────────────────────────────────────────────────────────
  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0] ?? null;
    setAttachedFile(file);
    // Reset so the same file can be re-selected after removal
    e.target.value = '';
  };

  // ── Voice / Speech recognition ───────────────────────────────────────────────
  const handleMicClick = () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const SR = (window as any).SpeechRecognition ?? (window as any).webkitSpeechRecognition;

    if (!SR) {
      toast.error('Speech recognition is not supported in this browser. Try Chrome or Edge.');
      return;
    }

    // Second click → stop current session
    if (isRecording) {
      recognitionRef.current?.stop();
      return;
    }

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const recognition: any = new SR();
    recognition.continuous = true;
    recognition.interimResults = false;
    // Match the app's current language; fall back to English
    recognition.lang = i18n.language === 'bg' ? 'bg-BG' : 'en-US';

    recognition.onstart = () => setIsRecording(true);

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    recognition.onresult = (event: any) => {
      let transcript = '';
      for (let i = event.resultIndex; i < event.results.length; i++) {
        if (event.results[i].isFinal) {
          transcript += event.results[i][0].transcript;
        }
      }
      if (transcript) {
        setForm((prev) => ({
          ...prev,
          content:
            prev.content + (prev.content && !prev.content.endsWith(' ') ? ' ' : '') + transcript,
        }));
      }
    };

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    recognition.onerror = (event: any) => {
      if (event.error === 'not-allowed' || event.error === 'permission-denied') {
        toast.error('Microphone access denied. Please allow microphone permission and try again.');
      } else if (event.error !== 'aborted' && event.error !== 'no-speech') {
        toast.error(`Microphone error: ${event.error}`);
      }
      setIsRecording(false);
    };

    recognition.onend = () => setIsRecording(false);

    recognitionRef.current = recognition;
    recognition.start();
  };

  // ── Submit ───────────────────────────────────────────────────────────────────
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.content.trim()) {
      toast.error(t('feedback.content_required'));
      return;
    }
    setLoading(true);
    try {
      await feedbackApi.create(form);
      toast.success(t('feedback.submitted'));
      setForm({ rating: 5, category: FeedbackCategory.Praise, content: '', isContactable: false });
      setAttachedFile(null);
      loadFeedbacks();
    } catch {
      toast.error(t('feedback.submit_error'));
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await feedbackApi.delete(id);
      toast.success(t('feedback.deleted'));
      loadFeedbacks();
    } catch {
      toast.error(t('common.error'));
    }
  };

  const marqueeItems = feedbacks.length > 0 ? [...feedbacks, ...feedbacks] : [];

  return (
    <div className="animate-fade-in">
      {/* ── Header ── */}
      <section className="bg-white dark:bg-[#1e1b2e] py-12 px-4 text-center">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">{t('feedback.title')}</h1>
        <p className="text-gray-500 dark:text-gray-400 mt-2 max-w-lg mx-auto">{t('feedback.subtitle')}</p>
      </section>

      {/* ── Community Testimonials Marquee ── */}
      {!listLoading && feedbacks.length > 0 && (
        <section className="bg-[#e3b7ac] dark:bg-[#282440] py-12 px-0">
          <h2 className="text-xl font-semibold text-gray-900 dark:text-white text-center mb-8 px-4">
            {t('feedback.community')}
          </h2>
          <div className="group relative w-full overflow-hidden [--duration:45s]">
            <div className="flex w-max animate-marquee gap-4 group-hover:[animation-play-state:paused]">
              {marqueeItems.map((fb, i) => (
                <FeedbackMarqueeCard
                  key={`${fb.id}-${i}`}
                  feedback={fb}
                  canDelete={fb.userId === user?.id}
                  onDelete={handleDelete}
                  t={t}
                />
              ))}
            </div>
            <div className="pointer-events-none absolute inset-y-0 left-0 hidden w-32 bg-gradient-to-r from-[#e3b7ac] dark:from-[#282440] to-transparent sm:block" />
            <div className="pointer-events-none absolute inset-y-0 right-0 hidden w-32 bg-gradient-to-l from-[#e3b7ac] dark:from-[#282440] to-transparent sm:block" />
          </div>
        </section>
      )}

      {listLoading && (
        <section className="py-10 text-center text-gray-400 bg-[#e3b7ac] dark:bg-[#282440]">
          {t('common.loading')}
        </section>
      )}

      {!listLoading && feedbacks.length === 0 && (
        <section className="py-10 text-center bg-[#e3b7ac] dark:bg-[#282440]">
          <p className="text-gray-400">{t('feedback.no_feedback')}</p>
        </section>
      )}

      {/* ── Feedback Form ── */}
      <section className="bg-[#f9fafb] dark:bg-[#2d2a42] py-16 px-4">
        <div className="max-w-2xl mx-auto">
          <form
            onSubmit={handleSubmit}
            className="bg-white dark:bg-[#1e1b2e] rounded-2xl border border-lavender/30 dark:border-white/10 shadow-sm p-6 md:p-8 space-y-6"
          >
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white">{t('feedback.share')}</h2>

            {/* Star Rating */}
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                {t('feedback.rating_label')}
              </label>
              <StarRating value={form.rating} onChange={(rating) => setForm({ ...form, rating })} size="lg" />
            </div>

            {/* Category Selector */}
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                {t('feedback.category_label')}
              </label>
              <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
                {categoryOptions.map((opt) => (
                  <button
                    key={opt.value}
                    type="button"
                    onClick={() => setForm({ ...form, category: opt.value })}
                    className={`flex flex-col items-center gap-1.5 p-3 rounded-xl border-2 transition-all text-sm font-medium ${
                      form.category === opt.value
                        ? 'border-[#945c67] bg-[#945c67]/8 text-gray-900 dark:text-white dark:bg-[#945c67]/20'
                        : 'border-gray-200 dark:border-white/10 text-gray-500 dark:text-gray-400 hover:border-lavender dark:hover:border-white/20'
                    }`}
                  >
                    <span className="text-xl">{opt.icon}</span>
                    <span>{t(opt.labelKey)}</span>
                  </button>
                ))}
              </div>
            </div>

            {/* Content — chat-input style */}
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                {t('feedback.content_label')}
              </label>

              <div className="relative rounded-lg border border-lavender/50 bg-white dark:bg-[#2a2740] focus-within:ring-1 focus-within:ring-primary-dark/30 focus-within:border-primary-dark transition-shadow p-1">

                {/* Recording indicator */}
                {isRecording && (
                  <div className="flex items-center gap-2 px-3 py-1.5 mb-1 rounded-md bg-red-50 border border-red-100 text-xs text-red-500 font-medium">
                    <span className="relative flex h-2 w-2">
                      <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-red-400 opacity-75" />
                      <span className="relative inline-flex rounded-full h-2 w-2 bg-red-500" />
                    </span>
                    Listening… speak now. Click the mic again to stop.
                  </div>
                )}

                {/* Textarea */}
                <textarea
                  value={form.content}
                  onChange={(e) => setForm({ ...form, content: e.target.value })}
                  placeholder={t('feedback.content_placeholder')}
                  maxLength={2000}
                  className="min-h-[80px] max-h-48 w-full resize-none rounded-lg bg-white dark:bg-[#2a2740] dark:text-gray-200 dark:placeholder:text-gray-500 border-0 px-3 py-3 text-sm text-gray-700 placeholder:text-gray-400 focus:outline-none focus:ring-0 shadow-none"
                />

                {/* Attached file badge */}
                {attachedFile && (
                  <div className="flex items-center gap-1.5 mx-2 mb-1.5 px-2.5 py-1.5 rounded-lg bg-lavender/20 dark:bg-white/10 border border-lavender/40 dark:border-white/20 text-xs">
                    <Paperclip size={12} className="shrink-0 text-[#945c67] dark:text-[#c1c4e3]" />
                    <span className="truncate max-w-[200px] font-semibold text-gray-800 dark:text-gray-100">{attachedFile.name}</span>
                    <span className="text-gray-500 dark:text-gray-400 ml-0.5 shrink-0">
                      {(attachedFile.size / 1024).toFixed(0)} KB
                    </span>
                    <button
                      type="button"
                      onClick={() => setAttachedFile(null)}
                      className="ml-auto shrink-0 p-1 rounded-md text-gray-500 dark:text-gray-400 hover:text-red-500 dark:hover:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors"
                      title="Remove attachment"
                    >
                      <X size={13} />
                    </button>
                  </div>
                )}

                {/* Hidden file input */}
                <input
                  ref={fileInputRef}
                  type="file"
                  className="hidden"
                  accept="image/*,.pdf,.doc,.docx,.txt,.zip"
                  onChange={handleFileChange}
                />

                {/* Toolbar */}
                <div className="flex items-center px-2 pb-2 pt-0 gap-1">

                  {/* Paperclip */}
                  <button
                    type="button"
                    title="Attach file"
                    onClick={() => fileInputRef.current?.click()}
                    className={`p-1.5 rounded-md transition-colors ${
                      attachedFile
                        ? 'text-[#945c67] dark:text-[#c1c4e3] bg-lavender/30 dark:bg-white/10'
                        : 'text-gray-400 dark:text-gray-500 hover:text-gray-600 dark:hover:text-gray-300 hover:bg-gray-100 dark:hover:bg-white/10'
                    }`}
                  >
                    <Paperclip size={15} />
                    <span className="sr-only">Attach file</span>
                  </button>

                  {/* Mic */}
                  <button
                    type="button"
                    title={isRecording ? 'Stop recording' : 'Use microphone'}
                    onClick={handleMicClick}
                    className={`p-1.5 rounded-md transition-colors ${
                      isRecording
                        ? 'text-red-500 bg-red-50 dark:bg-red-900/20 animate-pulse'
                        : 'text-gray-400 dark:text-gray-500 hover:text-gray-600 dark:hover:text-gray-300 hover:bg-gray-100 dark:hover:bg-white/10'
                    }`}
                  >
                    {isRecording ? <MicOff size={15} /> : <Mic size={15} />}
                    <span className="sr-only">{isRecording ? 'Stop recording' : 'Use Microphone'}</span>
                  </button>

                  {/* Character count + Send */}
                  <div className="ml-auto flex items-center gap-2">
                    <span className="text-xs text-gray-400">{form.content.length}/2000</span>
                    <button
                      type="submit"
                      disabled={loading}
                      className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-md bg-[#945c67] text-white text-xs font-semibold hover:bg-[#7d4d57] transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      {t('feedback.submit')}
                      <CornerDownLeft size={13} />
                    </button>
                  </div>
                </div>
              </div>
            </div>

            {/* Contactable */}
            <label className="flex items-center gap-3 cursor-pointer">
              <input
                type="checkbox"
                checked={form.isContactable}
                onChange={(e) => setForm({ ...form, isContactable: e.target.checked })}
                className="w-4 h-4 rounded border-gray-300 text-primary-dark focus:ring-primary-dark"
              />
              <span className="text-sm text-gray-600">{t('feedback.contactable')}</span>
            </label>

          </form>
        </div>
      </section>
    </div>
  );
}

// ── Marquee card ──────────────────────────────────────────────────────────────
function FeedbackMarqueeCard({
  feedback,
  canDelete,
  onDelete,
  t,
}: {
  feedback: Feedback;
  canDelete: boolean;
  onDelete: (id: string) => void;
  t: (key: string) => string;
}) {
  const cat = categoryColor[feedback.category] ?? 'bg-gray-100 text-gray-600';
  const date = new Date(feedback.createdAt).toLocaleDateString();

  return (
    <div className="flex flex-col rounded-xl border border-lavender/30 bg-white dark:bg-[#1a1825] p-5 max-w-[300px] min-w-[280px] transition-shadow duration-300 hover:shadow-md">
      <div className="flex items-center gap-3 mb-3">
        {feedback.userAvatarUrl ? (
          <img
            src={feedback.userAvatarUrl}
            alt=""
            className="w-10 h-10 rounded-full object-cover shrink-0"
          />
        ) : (
          <div className="w-10 h-10 rounded-full bg-lavender flex items-center justify-center text-white font-bold text-sm shrink-0">
            {feedback.userDisplayName?.charAt(0)?.toUpperCase() ?? '?'}
          </div>
        )}
        <div className="min-w-0">
          <p className="font-medium text-gray-900 dark:text-white text-sm truncate">
            {feedback.userDisplayName ?? t('feedback.anonymous')}
          </p>
          <p className="text-xs text-gray-400 dark:text-gray-500">{date}</p>
        </div>
        <span className={`ml-auto text-xs font-medium px-2 py-0.5 rounded-full shrink-0 ${cat}`}>
          {'★'.repeat(feedback.rating)}
        </span>
      </div>

      <p className="text-gray-600 text-sm leading-relaxed line-clamp-3 flex-1">
        {feedback.content}
      </p>

      {canDelete && (
        <button
          onClick={() => onDelete(feedback.id)}
          className="mt-3 text-xs text-gray-400 hover:text-red-500 transition-colors self-end"
        >
          {t('feedback.delete')}
        </button>
      )}
    </div>
  );
}
