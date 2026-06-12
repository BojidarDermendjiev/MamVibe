import { useState } from "react";
import { useTranslation } from "react-i18next";
import { AlertTriangle, Loader2, X } from "lucide-react";
import axios from "axios";
import { businessApi } from "@/api/businessApi";
import {
  ListingReportReason,
  type BusinessErrorEnvelope,
} from "@/types/business";

interface ReportListingModalProps {
  isOpen: boolean;
  listingId: string;
  onClose: () => void;
  onSuccess?: () => void;
}

const REASON_OPTIONS: { value: ListingReportReason; key: string }[] = [
  { value: ListingReportReason.Spam, key: "business.report.reasons.spam" },
  { value: ListingReportReason.Scam, key: "business.report.reasons.scam" },
  { value: ListingReportReason.Harassment, key: "business.report.reasons.harassment" },
  { value: ListingReportReason.FakeListing, key: "business.report.reasons.fakeListing" },
  { value: ListingReportReason.Inappropriate, key: "business.report.reasons.inappropriate" },
  { value: ListingReportReason.Other, key: "business.report.reasons.other" },
];

/**
 * Lightweight report dialog. Reuses the existing reports pipeline server-side
 * (duplicate-pending guard, threshold-signal emission). Description requires
 * 10–2000 chars to match the backend validation.
 */
export default function ReportListingModal({
  isOpen,
  listingId,
  onClose,
  onSuccess,
}: ReportListingModalProps) {
  const { t } = useTranslation();
  const [reason, setReason] = useState<ListingReportReason>(ListingReportReason.Spam);
  const [description, setDescription] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (description.trim().length < 10) {
      setError(t("business.report.tooShort"));
      return;
    }
    setSubmitting(true);
    setError(null);
    try {
      await businessApi.reportListing(listingId, {
        reason,
        description: description.trim(),
      });
      setSuccess(true);
      onSuccess?.();
      setTimeout(() => {
        setSuccess(false);
        setDescription("");
        onClose();
      }, 1500);
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data) {
        const env = err.response.data as BusinessErrorEnvelope;
        setError(env.error || t("business.report.error"));
      } else {
        setError(t("business.report.error"));
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center p-4"
      role="dialog"
      aria-modal="true"
    >
      <div
        className="absolute inset-0 bg-black/50 backdrop-blur-sm"
        onClick={onClose}
        aria-hidden="true"
      />
      <div className="relative w-full max-w-md bg-white dark:bg-[#2d2a42] rounded-2xl shadow-2xl border border-red-200/40 dark:border-white/10">
        <div className="flex items-start justify-between gap-3 p-5 border-b border-gray-100 dark:border-white/10">
          <div className="flex items-start gap-3">
            <span className="flex-shrink-0 w-10 h-10 rounded-2xl bg-red-50 dark:bg-red-500/15 flex items-center justify-center">
              <AlertTriangle className="h-5 w-5 text-red-600 dark:text-red-400" />
            </span>
            <div>
              <h2 className="text-base font-bold text-gray-900 dark:text-white">
                {t("business.report.heading")}
              </h2>
              <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                {t("business.report.subtitle")}
              </p>
            </div>
          </div>
          <button
            type="button"
            onClick={onClose}
            aria-label={t("common.cancel") || "Close"}
            className="p-1.5 rounded-lg hover:bg-gray-100 dark:hover:bg-white/10 transition-colors text-gray-500"
          >
            <X size={16} />
          </button>
        </div>

        {success ? (
          <div className="p-8 text-center">
            <div className="w-12 h-12 rounded-full bg-green-100 dark:bg-green-500/20 flex items-center justify-center mx-auto mb-3">
              <span className="text-2xl">✓</span>
            </div>
            <p className="font-semibold text-gray-900 dark:text-white">
              {t("business.report.successTitle")}
            </p>
            <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
              {t("business.report.successBody")}
            </p>
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="p-5 space-y-4">
            <div>
              <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
                {t("business.report.reasonLabel")}
              </label>
              <select
                value={reason}
                onChange={(e) => setReason(Number(e.target.value) as ListingReportReason)}
                className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-red-500/40"
              >
                {REASON_OPTIONS.map((o) => (
                  <option key={o.value} value={o.value}>
                    {t(o.key)}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
                {t("business.report.detailsLabel")}
                <span className="text-red-500 ml-0.5">*</span>
              </label>
              <textarea
                required
                rows={4}
                minLength={10}
                maxLength={2000}
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder={t("business.report.detailsPlaceholder")}
                className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-red-500/40 resize-none"
              />
              <p className="text-[11px] text-gray-400 mt-1">
                {description.trim().length}/2000
              </p>
            </div>

            {error && (
              <p className="text-sm text-red-600 bg-red-50 dark:bg-red-500/10 rounded-lg px-3 py-2">
                {error}
              </p>
            )}

            <div className="flex gap-3 pt-1">
              <button
                type="button"
                onClick={onClose}
                className="flex-1 px-4 py-2.5 rounded-xl border border-gray-200 dark:border-white/10 text-sm font-medium text-gray-600 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-white/5 transition-colors"
              >
                {t("common.cancel")}
              </button>
              <button
                type="submit"
                disabled={submitting}
                className="flex-1 inline-flex items-center justify-center gap-2 px-4 py-2.5 rounded-xl bg-red-600 text-white text-sm font-semibold hover:bg-red-700 disabled:opacity-60 transition-colors"
              >
                {submitting ? (
                  <Loader2 className="h-4 w-4 animate-spin" />
                ) : (
                  t("business.report.submitButton")
                )}
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
}
