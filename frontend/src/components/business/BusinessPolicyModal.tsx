import { useEffect, useMemo, useState } from "react";
import { Sparkles, X, Check, Loader2 } from "lucide-react";
import { useTranslation } from "react-i18next";
import { businessApi } from "../../api/businessApi";
import type { BusinessPolicyDto } from "../../types/business";

interface BusinessPolicyModalProps {
  isOpen: boolean;
  language?: string;
  onAccept: (policy: BusinessPolicyDto) => void;
  onClose: () => void;
}

/**
 * Lavender-themed policy acceptance modal used by the business signup wizard.
 * Renders the markdown body with a small inline parser (headers, bold, lists) — we
 * deliberately avoid pulling in react-markdown for this single use case.
 */
export default function BusinessPolicyModal({
  isOpen,
  language,
  onAccept,
  onClose,
}: BusinessPolicyModalProps) {
  const { t, i18n } = useTranslation();
  const [policy, setPolicy] = useState<BusinessPolicyDto | null>(null);
  const [agreed, setAgreed] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const effectiveLanguage = language ?? i18n.language ?? "en";

  useEffect(() => {
    if (!isOpen) return;
    let cancelled = false;
    const load = async () => {
      if (cancelled) return;
      setLoading(true);
      setError(null);
      try {
        const p = await businessApi.getCurrentPolicy(effectiveLanguage);
        if (!cancelled) setPolicy(p);
      } catch {
        if (!cancelled) setError(t("business.policy.loadError"));
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    void load();
    return () => {
      cancelled = true;
    };
  }, [isOpen, effectiveLanguage, t]);

  useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = "hidden";
    } else {
      document.body.style.overflow = "";
    }
    return () => {
      document.body.style.overflow = "";
    };
  }, [isOpen]);

  const rendered = useMemo(
    () => (policy ? renderPolicyMarkdown(policy.bodyMarkdown) : null),
    [policy],
  );

  if (!isOpen) return null;

  return (
    <div
      className="fixed inset-0 z-[70] overflow-y-auto"
      role="dialog"
      aria-modal="true"
      aria-labelledby="business-policy-title"
    >
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black/50 backdrop-blur-sm"
        onClick={onClose}
        aria-hidden="true"
      />
      {/* min-h-full guarantees the flex container fills the scroll viewport so items-center works */}
      <div className="relative flex min-h-full items-center justify-center p-4">
      <div className="relative w-full max-w-2xl flex flex-col bg-white dark:bg-[#2d2a42] rounded-2xl shadow-2xl border border-lavender/30 dark:border-white/10" style={{maxHeight: 'calc(100vh - 2rem)'}}>
        {/* Header */}
        <div className="flex items-start justify-between gap-4 p-6 border-b border-lavender/20 dark:border-white/10">
          <div className="flex items-start gap-3">
            <span className="flex-shrink-0 w-11 h-11 rounded-2xl bg-gradient-to-br from-primary/20 to-mauve/20 flex items-center justify-center">
              <Sparkles className="h-5 w-5 text-primary" aria-hidden="true" />
            </span>
            <div>
              <h2
                id="business-policy-title"
                className="text-lg font-bold text-gray-900 dark:text-white"
              >
                {policy?.title ?? t("business.policy.heading")}
              </h2>
              <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                {policy
                  ? t("business.policy.version", {
                      v: policy.version,
                      lang: policy.language.toUpperCase(),
                    })
                  : t("business.policy.subtitle")}
              </p>
            </div>
          </div>
          <button
            onClick={onClose}
            aria-label={t("common.cancel") || "Close"}
            className="p-1.5 rounded-lg hover:bg-lavender/20 dark:hover:bg-white/10 transition-colors text-gray-500"
          >
            <X size={18} />
          </button>
        </div>

        {/* Body */}
        <div className="flex-1 overflow-y-auto p-6">
          {loading && (
            <div className="flex items-center justify-center py-10 text-gray-400">
              <Loader2 className="h-5 w-5 animate-spin" />
            </div>
          )}
          {error && (
            <div className="rounded-xl bg-red-50 dark:bg-red-500/10 border border-red-200 dark:border-red-500/20 px-4 py-3 text-sm text-red-700 dark:text-red-300">
              {error}
            </div>
          )}
          {!loading && !error && policy && (
            <div className="prose prose-sm max-w-none text-gray-700 dark:text-gray-200 leading-relaxed">
              {rendered}
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="border-t border-lavender/20 dark:border-white/10 p-6 bg-cream-dark/40 dark:bg-white/[0.02] rounded-b-2xl">
          <label className="flex items-start gap-3 cursor-pointer select-none">
            <span
              className={`mt-0.5 inline-flex h-5 w-5 flex-shrink-0 items-center justify-center rounded-md border transition-colors ${
                agreed
                  ? "bg-primary border-primary text-white"
                  : "bg-white dark:bg-white/5 border-gray-300 dark:border-white/20"
              }`}
            >
              {agreed && <Check size={14} strokeWidth={3} />}
            </span>
            <span className="text-sm text-gray-700 dark:text-gray-200">
              {t("business.policy.consent")}
            </span>
            <input
              type="checkbox"
              checked={agreed}
              onChange={(e) => setAgreed(e.target.checked)}
              className="sr-only"
              aria-label={t("business.policy.consent") || "I agree"}
            />
          </label>

          <div className="mt-5 flex flex-col-reverse sm:flex-row gap-3 justify-end">
            <button
              type="button"
              onClick={onClose}
              className="px-5 py-2.5 rounded-xl border border-gray-200 dark:border-white/10 text-sm font-medium text-gray-600 dark:text-gray-300 hover:bg-white dark:hover:bg-white/5 transition-colors"
            >
              {t("common.cancel")}
            </button>
            <button
              type="button"
              disabled={!agreed || !policy}
              onClick={() => policy && onAccept(policy)}
              className="px-5 py-2.5 rounded-xl bg-primary text-white text-sm font-semibold shadow-sm hover:bg-primary/90 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              {t("business.policy.acceptButton")}
            </button>
          </div>
        </div>
      </div>
      </div>
    </div>
  );
}


// Minimal markdown renderer — supports `## headings`, `**bold**`, numbered lists,
// and blank-line paragraphs. Sufficient for the seeded policy body; deliberately not
// a general-purpose parser.
function renderPolicyMarkdown(body: string): React.ReactNode {
  const lines = body.split("\n");
  const blocks: React.ReactNode[] = [];
  let listItems: string[] | null = null;
  let paragraph: string[] | null = null;

  const flushParagraph = () => {
    if (paragraph && paragraph.length > 0) {
      blocks.push(
        <p key={`p-${blocks.length}`} className="mb-3">
          {renderInline(paragraph.join(" "))}
        </p>,
      );
    }
    paragraph = null;
  };
  const flushList = () => {
    if (listItems && listItems.length > 0) {
      blocks.push(
        <ol
          key={`ol-${blocks.length}`}
          className="list-decimal pl-5 space-y-1.5 mb-3 marker:text-primary"
        >
          {listItems.map((item, i) => (
            <li key={i}>{renderInline(item)}</li>
          ))}
        </ol>,
      );
    }
    listItems = null;
  };

  for (const raw of lines) {
    const line = raw.trimEnd();
    if (line.length === 0) {
      flushParagraph();
      flushList();
      continue;
    }

    if (line.startsWith("## ")) {
      flushParagraph();
      flushList();
      blocks.push(
        <h3
          key={`h-${blocks.length}`}
          className="text-base font-semibold text-primary mb-2 mt-1"
        >
          {renderInline(line.slice(3))}
        </h3>,
      );
      continue;
    }

    const listMatch = /^(\d+)\.\s+(.*)$/.exec(line);
    if (listMatch) {
      flushParagraph();
      if (listItems == null) listItems = [];
      listItems.push(listMatch[2]);
      continue;
    }

    flushList();
    if (paragraph == null) paragraph = [];
    paragraph.push(line);
  }
  flushParagraph();
  flushList();
  return blocks;
}

function renderInline(text: string): React.ReactNode {
  // Split on **bold** segments; preserve order, no nesting.
  const parts = text.split(/(\*\*[^*]+\*\*)/g);
  return parts.map((part, i) => {
    if (part.startsWith("**") && part.endsWith("**")) {
      return (
        <strong key={i} className="font-semibold text-gray-900 dark:text-white">
          {part.slice(2, -2)}
        </strong>
      );
    }
    return <span key={i}>{part}</span>;
  });
}
