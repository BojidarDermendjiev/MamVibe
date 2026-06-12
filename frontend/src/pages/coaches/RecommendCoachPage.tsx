import { useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { motion } from "framer-motion";
import axios from "axios";
import { ArrowLeft, ArrowRight, Loader2, ShieldCheck, Sparkles } from "lucide-react";
import { usePageSEO } from "@/hooks/useSEO";
import { businessApi } from "@/api/businessApi";
import { ActivityType } from "@/types/business";
import TurnstileWidget from "@/components/common/TurnstileWidget";

const ACTIVITY_OPTIONS: { value: ActivityType; key: string }[] = [
  { value: ActivityType.Swimming, key: "coaches.activityType.swimming" },
  { value: ActivityType.MartialArts, key: "coaches.activityType.martialArts" },
  { value: ActivityType.Music, key: "coaches.activityType.music" },
  { value: ActivityType.Dance, key: "coaches.activityType.dance" },
  { value: ActivityType.Gymnastics, key: "coaches.activityType.gymnastics" },
  { value: ActivityType.ArtAndCrafts, key: "coaches.activityType.artAndCrafts" },
  { value: ActivityType.EarlyDevelopment, key: "coaches.activityType.earlyDevelopment" },
  { value: ActivityType.LanguageClasses, key: "coaches.activityType.languageClasses" },
  { value: ActivityType.SportsTeam, key: "coaches.activityType.sportsTeam" },
  { value: ActivityType.Other, key: "coaches.activityType.other" },
];

interface FormState {
  businessName: string;
  contactEmail: string;
  contactPhone: string;
  activityType: ActivityType;
  city: string;
  notes: string;
  referralCode: string;
}

const EMPTY: FormState = {
  businessName: "",
  contactEmail: "",
  contactPhone: "",
  activityType: ActivityType.Swimming,
  city: "",
  notes: "",
  referralCode: "",
};

/**
 * Public form for anyone (parent, promoter, anonymous) to recommend a coach or venue.
 * Carries the optional ?ref=MAMA-XXXXXXXX promoter code through to the backend, which
 * attaches it to the new <c>CoachReferral</c> row so the promoter dashboard updates.
 */
export default function RecommendCoachPage() {
  const { t } = useTranslation();
  const [params] = useSearchParams();
  const refFromQuery = params.get("ref") ?? "";

  const [form, setForm] = useState<FormState>(() => ({
    ...EMPTY,
    referralCode: refFromQuery,
  }));
  const [turnstileToken, setTurnstileToken] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [turnstileResetKey, setTurnstileResetKey] = useState(0);

  usePageSEO({
    title: t("recommend.seoTitle") || "Recommend a coach or venue",
    description:
      t("recommend.seoDescription") ||
      "Help MamVibe families discover great coaches and family-friendly venues.",
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.businessName.trim() || !form.city.trim()) return;
    if (!form.contactEmail.trim() && !form.contactPhone.trim()) {
      setError(t("recommend.contactRequired"));
      return;
    }
    setError(null);
    setSubmitting(true);
    try {
      await businessApi.submitCoachReferral({
        businessName: form.businessName.trim(),
        contactEmail: form.contactEmail.trim() || undefined,
        contactPhone: form.contactPhone.trim() || undefined,
        activityType: form.activityType,
        city: form.city.trim(),
        notes: form.notes.trim() || undefined,
        referralCode: form.referralCode.trim() || undefined,
        turnstileToken: turnstileToken ?? undefined,
      });
      setSuccess(true);
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data) {
        const data = err.response.data as { error?: string; code?: string };
        if (data.code === "referral_duplicate") {
          setError(t("recommend.duplicateError"));
        } else {
          setError(data.error ?? t("recommend.error"));
        }
      } else {
        setError(t("recommend.error"));
      }
      setTurnstileResetKey((k) => k + 1);
      setTurnstileToken(null);
    } finally {
      setSubmitting(false);
    }
  };

  if (success) {
    return (
      <div className="max-w-xl mx-auto px-4 py-16 text-center">
        <motion.div
          initial={{ scale: 0.9, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          transition={{ duration: 0.35 }}
          className="inline-flex w-20 h-20 rounded-2xl bg-gradient-to-br from-emerald-100 to-emerald-200 dark:from-emerald-500/20 dark:to-emerald-500/30 items-center justify-center mb-5"
        >
          <ShieldCheck className="h-10 w-10 text-emerald-600" />
        </motion.div>
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
          {t("recommend.successHeading")}
        </h1>
        <p className="text-gray-500 dark:text-gray-400 mb-6">
          {t("recommend.successBody")}
        </p>
        <Link
          to="/coaches"
          className="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl bg-primary text-white text-sm font-semibold hover:bg-primary/90 transition-colors"
        >
          {t("recommend.successCta")} <ArrowRight size={14} />
        </Link>
      </div>
    );
  }

  return (
    <div className="max-w-2xl mx-auto px-4 py-10">
      <Link
        to="/coaches"
        className="inline-flex items-center gap-1.5 text-sm text-gray-500 dark:text-gray-400 hover:text-primary mb-5"
      >
        <ArrowLeft size={14} />
        {t("recommend.backToCoaches")}
      </Link>

      <motion.div
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.35 }}
      >
        <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-primary/10 text-primary text-xs font-semibold mb-3">
          <Sparkles size={13} />
          {t("recommend.tagline")}
        </div>
        <h1 className="text-2xl sm:text-3xl font-bold text-primary-dark dark:text-white mb-2">
          {t("recommend.heading")}
        </h1>
        <p className="text-sm text-gray-500 dark:text-gray-400 mb-6">
          {t("recommend.subtitle")}
        </p>
      </motion.div>

      <form
        onSubmit={handleSubmit}
        className="bg-white dark:bg-[#2d2a42] border border-lavender/30 dark:border-white/10 rounded-2xl shadow-sm p-6 space-y-4"
      >
        <Field
          label={t("recommend.businessNameLabel")}
          required
          value={form.businessName}
          maxLength={200}
          onChange={(v) => setForm((f) => ({ ...f, businessName: v }))}
          placeholder={t("recommend.businessNamePlaceholder")}
        />

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <Field
            label={t("recommend.emailLabel")}
            type="email"
            value={form.contactEmail}
            maxLength={254}
            onChange={(v) => setForm((f) => ({ ...f, contactEmail: v }))}
            placeholder="coach@example.com"
          />
          <Field
            label={t("recommend.phoneLabel")}
            value={form.contactPhone}
            maxLength={32}
            onChange={(v) => setForm((f) => ({ ...f, contactPhone: v }))}
            placeholder="+359 …"
          />
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
              {t("recommend.activityLabel")}
              <span className="text-red-500 ml-0.5">*</span>
            </label>
            <select
              value={form.activityType}
              onChange={(e) =>
                setForm((f) => ({ ...f, activityType: Number(e.target.value) as ActivityType }))
              }
              className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
            >
              {ACTIVITY_OPTIONS.map((o) => (
                <option key={o.value} value={o.value}>
                  {t(o.key)}
                </option>
              ))}
            </select>
          </div>
          <Field
            label={t("recommend.cityLabel")}
            required
            value={form.city}
            maxLength={100}
            onChange={(v) => setForm((f) => ({ ...f, city: v }))}
          />
        </div>

        <div>
          <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
            {t("recommend.notesLabel")}
          </label>
          <textarea
            rows={3}
            maxLength={2000}
            value={form.notes}
            onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))}
            placeholder={t("recommend.notesPlaceholder")}
            className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40 resize-none"
          />
        </div>

        <div>
          <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
            {t("recommend.referralLabel")}
          </label>
          <input
            type="text"
            value={form.referralCode}
            maxLength={16}
            onChange={(e) => setForm((f) => ({ ...f, referralCode: e.target.value.toUpperCase() }))}
            placeholder="MAMA-XXXXXXXX"
            className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40 font-mono uppercase tracking-wider"
          />
          <p className="text-[11px] text-gray-400 mt-1">{t("recommend.referralHint")}</p>
        </div>

        <div className="flex justify-center">
          <TurnstileWidget key={turnstileResetKey} onToken={setTurnstileToken} />
        </div>

        {error && (
          <p className="text-sm text-red-600 bg-red-50 dark:bg-red-500/10 rounded-lg px-3 py-2">
            {error}
          </p>
        )}

        <button
          type="submit"
          disabled={submitting}
          className="w-full inline-flex items-center justify-center gap-2 px-4 py-2.5 rounded-xl bg-primary text-white text-sm font-semibold hover:bg-primary/90 disabled:opacity-60 transition-colors"
        >
          {submitting ? (
            <Loader2 size={15} className="animate-spin" />
          ) : (
            <>
              {t("recommend.submitButton")} <ArrowRight size={14} />
            </>
          )}
        </button>
      </form>
    </div>
  );
}

interface FieldProps {
  label: string;
  value: string;
  onChange: (next: string) => void;
  type?: string;
  maxLength?: number;
  required?: boolean;
  placeholder?: string;
}
function Field({ label, value, onChange, type = "text", maxLength, required, placeholder }: FieldProps) {
  return (
    <div>
      <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
        {label}
        {required && <span className="text-red-500 ml-0.5">*</span>}
      </label>
      <input
        type={type}
        value={value}
        required={required}
        maxLength={maxLength}
        placeholder={placeholder}
        onChange={(e) => onChange(e.target.value)}
        className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
      />
    </div>
  );
}
