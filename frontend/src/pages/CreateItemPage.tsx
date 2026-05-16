import { useState, useMemo, useRef, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { usePageSEO } from "@/hooks/useSEO";
import toast from "@/utils/toast";
import { itemsApi } from "../api/itemsApi";
import { photosApi } from "../api/photosApi";
import { aiApi } from "../api/aiApi";
import { ListingType, AgeGroup, type PriceSuggestion, type Item } from "../types/item";
import { useCategories } from "../hooks/useCategories";
import { useAuthStore } from "../store/authStore";
import Button from "../components/common/Button";
import Input from "../components/common/Input";
import PhotoUploader from "../components/items/PhotoUploader";
import IbanModal from "../components/common/IbanModal";
import CategorySpecificSection from "../components/items/CategorySpecificSection";

function SubmissionResultModal({ item, onClose }: { item: Item; onClose: () => void }) {
  const navigate = useNavigate();

  const isLive = item.isActive;
  const isFlagged = item.aiModerationStatus === 3;

  const config = isLive
    ? {
        icon: "✅",
        iconBg: "bg-green-100 dark:bg-green-900/30",
        title: "Your listing is live!",
        body: "Great news — your item passed our review and is now visible in the marketplace for everyone to see.",
        note: null,
        primaryLabel: "View listing",
        primaryAction: () => navigate(`/items/${item.id}`),
        secondaryLabel: "Create another",
        secondaryAction: onClose,
      }
    : isFlagged
    ? {
        icon: "⚠️",
        iconBg: "bg-amber-100 dark:bg-amber-900/30",
        title: "Your listing needs closer review",
        body: "Our AI flagged this listing for a more detailed check by our moderation team before it can go live. This may take a little longer than usual.",
        note: item.aiModerationNotes ?? null,
        primaryLabel: "Go to Dashboard",
        primaryAction: () => navigate("/dashboard"),
        secondaryLabel: "Browse items",
        secondaryAction: () => navigate("/browse"),
      }
    : {
        icon: "🕐",
        iconBg: "bg-amber-100 dark:bg-amber-900/30",
        title: "Your listing is under review",
        body: "Your item has been submitted and is waiting for approval before it appears in the marketplace. This usually takes a short while.",
        note: null,
        primaryLabel: "Go to Dashboard",
        primaryAction: () => navigate("/dashboard"),
        secondaryLabel: "Browse items",
        secondaryAction: () => navigate("/browse"),
      };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4">
      <div className="bg-white dark:bg-[#2d2a42] rounded-2xl shadow-2xl w-full max-w-md p-7 space-y-5">
        {/* Icon */}
        <div className={`w-16 h-16 rounded-full ${config.iconBg} flex items-center justify-center text-3xl mx-auto`}>
          {config.icon}
        </div>

        {/* Text */}
        <div className="text-center space-y-2">
          <h2 className="text-xl font-bold text-[#364153] dark:text-white">{config.title}</h2>
          <p className="text-sm text-gray-600 dark:text-gray-300 leading-relaxed">{config.body}</p>
          {config.note && (
            <p className="mt-2 text-xs bg-amber-50 dark:bg-amber-900/20 text-amber-700 dark:text-amber-300 rounded-lg px-3 py-2 border border-amber-200 dark:border-amber-900/40 text-left">
              {config.note}
            </p>
          )}
        </div>

        {/* What happens next — only for pending items */}
        {!isLive && (
          <div className="bg-gray-50 dark:bg-white/5 rounded-xl p-4 space-y-2">
            <p className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wide">What happens next</p>
            <ul className="space-y-1.5 text-sm text-gray-600 dark:text-gray-300">
              <li className="flex items-start gap-2">
                <span className="text-primary mt-0.5">1.</span>
                Our team reviews your listing for quality and safety.
              </li>
              <li className="flex items-start gap-2">
                <span className="text-primary mt-0.5">2.</span>
                Once approved, it appears in Browse and your Dashboard.
              </li>
              <li className="flex items-start gap-2">
                <span className="text-primary mt-0.5">3.</span>
                You can track the status anytime from your Dashboard.
              </li>
            </ul>
          </div>
        )}

        {/* Actions */}
        <div className="flex flex-col gap-2 pt-1">
          <Button fullWidth onClick={config.primaryAction}>
            {config.primaryLabel}
          </Button>
          <button
            onClick={config.secondaryAction}
            className="w-full py-2 text-sm text-gray-500 hover:text-gray-700 dark:hover:text-gray-300 transition-colors"
          >
            {config.secondaryLabel}
          </button>
        </div>
      </div>
    </div>
  );
}

export default function CreateItemPage() {
  const { t } = useTranslation();

  // Noindex: authenticated form page — not a public SEO target.
  usePageSEO({ title: "Create a Listing", description: "List your baby items for sale or donation on MamVibe.", index: false });
  const { categories } = useCategories();
  const { user } = useAuthStore();
  const [photos, setPhotos] = useState<File[]>([]);
  const [loading, setLoading] = useState(false);
  const [aiLoading, setAiLoading] = useState(false);
  const [priceLoading, setPriceLoading] = useState(false);
  const [priceSuggestion, setPriceSuggestion] = useState<PriceSuggestion | null>(null);
  const [showIbanModal, setShowIbanModal] = useState(false);
  const [createdItem, setCreatedItem] = useState<Item | null>(null);
  const aiInputRef = useRef<HTMLInputElement>(null);
  const [form, setForm] = useState<{
    title: string;
    description: string;
    categoryId: string;
    listingType: ListingType;
    ageGroup: AgeGroup | null;
    shoeSize: number | null;
    clothingSize: number | null;
    price: string;
  }>({
    title: "",
    description: "",
    categoryId: "",
    listingType: ListingType.Donate,
    ageGroup: null,
    shoeSize: null,
    clothingSize: null,
    price: "",
  });

  const selectedSlug = useMemo(
    () => categories.find((c) => c.id === form.categoryId)?.slug,
    [categories, form.categoryId]
  );

  const handlePriceSuggest = useCallback(async () => {
    if (!form.categoryId) return;
    setPriceLoading(true);
    try {
      const { data } = await aiApi.suggestPrice({
        categoryId: form.categoryId,
        title: form.title,
        description: form.description,
        ageGroup: form.ageGroup,
        clothingSize: form.clothingSize,
        shoeSize: form.shoeSize,
      });
      setPriceSuggestion(data);
    } catch {
      toast.error("Could not get a price suggestion. Please try again.");
    } finally {
      setPriceLoading(false);
    }
  }, [form.categoryId, form.title, form.description, form.ageGroup, form.clothingSize, form.shoeSize]);

  const handleAiFill = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setAiLoading(true);
    try {
      const { data: suggestion } = await aiApi.suggestListing(file);
      const matchedCategory = categories.find(
        (c) => c.slug === suggestion.categorySlug
      );
      setForm((prev) => ({
        ...prev,
        title: suggestion.title || prev.title,
        description: suggestion.description || prev.description,
        categoryId: matchedCategory?.id ?? prev.categoryId,
        listingType: suggestion.listingType,
        ageGroup: suggestion.ageGroup ?? prev.ageGroup,
        shoeSize: suggestion.shoeSize ?? prev.shoeSize,
        clothingSize: suggestion.clothingSize ?? prev.clothingSize,
        price:
          suggestion.suggestedPrice != null
            ? String(suggestion.suggestedPrice)
            : prev.price,
      }));
      // Add the analysed photo to the listing
      setPhotos((prev) => [file, ...prev]);
      toast.success("Form filled with AI suggestions — review before submitting!");
    } catch (err: unknown) {
      const detail =
        (err as { response?: { data?: { detail?: string; error?: string } } })
          ?.response?.data?.detail ||
        (err as { response?: { data?: { detail?: string; error?: string } } })
          ?.response?.data?.error;
      toast.error(detail ?? "Could not analyse the photo. Please fill the form manually.");
    } finally {
      setAiLoading(false);
      e.target.value = "";
    }
  };

  const doSubmit = async () => {
    if (!form.categoryId) {
      toast.error("Please select a category");
      return;
    }
    setLoading(true);
    try {
      const photoUrls: string[] = [];
      for (const file of photos) {
        const { data } = await photosApi.upload(file);
        photoUrls.push(data.url);
      }
      const { data: item } = await itemsApi.create({
        ...form,
        price:
          form.listingType === ListingType.Sell ? parseFloat(form.price) : null,
        ageGroup: form.ageGroup,
        shoeSize: form.shoeSize,
        clothingSize: form.clothingSize,
        photoUrls,
      });
      setCreatedItem(item);
    } catch (err: unknown) {
      const message =
        (err as { response?: { data?: { error?: string; message?: string } } })
          ?.response?.data?.error ||
        (err as { response?: { data?: { error?: string; message?: string } } })
          ?.response?.data?.message ||
        t("common.error");
      toast.error(message);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (form.listingType === ListingType.Sell && !user?.iban) {
      setShowIbanModal(true);
      return;
    }
    await doSubmit();
  };

  return (
    <div className="max-w-2xl mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold text-primary mb-6">
        {t("items.create_title")}
      </h1>

      {/* AI Listing Assistant */}
      <div className="mb-5 p-4 rounded-xl border border-primary-dark/30 bg-primary-dark/10 dark:bg-primary-dark/40 dark:border-primary-dark/60">
        <p className="text-base font-semibold text-primary-dark dark:text-white mb-1">
          ✨ AI Listing Assistant
        </p>
        <p className="text-sm text-primary-dark/70 dark:text-white/70 mb-3">
          Upload a photo and Claude AI will suggest the title, description,
          category, price, and size for you.
        </p>
        <input
          ref={aiInputRef}
          type="file"
          accept="image/jpeg,image/jpg,image/png,image/webp"
          className="hidden"
          onChange={handleAiFill}
        />
        <Button
          type="button"
          isLoading={aiLoading}
          onClick={() => aiInputRef.current?.click()}
          className="!bg-primary-dark hover:!bg-[#2d3a6e] focus:ring-primary-dark"
        >
          {aiLoading ? "Analysing photo…" : "✨ Fill with AI"}
        </Button>
      </div>

      <form
        onSubmit={handleSubmit}
        className="space-y-5 bg-white dark:bg-[#2d2a42] rounded-xl p-6 border border-lavender/30 dark:border-white/10"
      >
        <Input
          label={t("items.title")}
          value={form.title}
          onChange={(e) => setForm({ ...form, title: e.target.value })}
          required
        />

        <div>
          <label className="block text-sm font-medium text-primary mb-1">
            {t("items.description")}
          </label>
          <textarea
            value={form.description}
            onChange={(e) => setForm({ ...form, description: e.target.value })}
            rows={4}
            required
            className="w-full px-4 py-2.5 rounded-lg border border-lavender bg-white text-gray-800 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-primary mb-1">
            {t("items.category")}
          </label>
          <select
            value={form.categoryId}
            onChange={(e) => {
              setForm({ ...form, categoryId: e.target.value, ageGroup: null, shoeSize: null, clothingSize: null });
              setPriceSuggestion(null);
            }}
            required
            className="w-full px-4 py-2.5 rounded-lg border border-lavender bg-white text-gray-800 focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <option value="">{t("items.all_categories")}</option>
            {categories.map((cat) => (
              <option key={cat.id} value={cat.id}>
                {cat.name}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium text-primary mb-1">
            {t("items.listing_type")}
          </label>
          <div className="flex gap-3">
            <button
              type="button"
              onClick={() => {
                setForm({ ...form, listingType: ListingType.Donate });
                setPriceSuggestion(null);
              }}
              className={`flex-1 py-3 rounded-lg border-2 font-medium transition-all ${
                form.listingType === ListingType.Donate
                  ? "border-green-500 bg-green-50 text-green-700"
                  : "border-gray-200 text-gray-500 hover:border-gray-300"
              }`}
            >
              {t("items.donate")}
            </button>
            <button
              type="button"
              onClick={() => {
                setForm({ ...form, listingType: ListingType.Sell });
                setPriceSuggestion(null);
              }}
              className={`flex-1 py-3 rounded-lg border-2 font-medium transition-all ${
                form.listingType === ListingType.Sell
                  ? "border-mauve bg-mauve/10 text-mauve"
                  : "border-gray-200 text-gray-500 hover:border-gray-300"
              }`}
            >
              {t("items.sell")}
            </button>
          </div>
        </div>

        {form.listingType === ListingType.Sell && (
          <div className="space-y-2">
            <div className="flex items-end gap-2">
              <div className="flex-1">
                <Input
                  label={t("items.price")}
                  type="number"
                  min="0.01"
                  step="0.01"
                  value={form.price}
                  onChange={(e) => setForm({ ...form, price: e.target.value })}
                  required
                />
              </div>
              {form.categoryId && (
                <button
                  type="button"
                  onClick={handlePriceSuggest}
                  disabled={priceLoading}
                  className="mb-0.5 flex items-center gap-1.5 px-3 py-2.5 rounded-lg border border-lavender/60 bg-gradient-to-r from-purple-50 to-pink-50 text-sm font-medium text-gray-700 hover:border-primary/50 hover:bg-purple-50 transition-all disabled:opacity-60 whitespace-nowrap"
                >
                  {priceLoading ? (
                    <svg className="h-4 w-4 animate-spin text-primary" viewBox="0 0 24 24" fill="none">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8z" />
                    </svg>
                  ) : (
                    <span>✨</span>
                  )}
                  {priceLoading ? "Thinking…" : "Suggest price"}
                </button>
              )}
            </div>

            {priceSuggestion && priceSuggestion.suggestedPrice != null && (
              <div className="rounded-xl border border-purple-200 bg-gradient-to-r from-purple-50 to-pink-50 p-4">
                <div className="flex items-start justify-between gap-2">
                  <div className="flex-1">
                    <div className="flex items-baseline gap-2 flex-wrap">
                      <span className="text-xl font-bold text-primary">
                        {priceSuggestion.suggestedPrice} BGN
                      </span>
                      {priceSuggestion.low != null && priceSuggestion.high != null && (
                        <span className="text-sm text-gray-500">
                          range: {priceSuggestion.low}–{priceSuggestion.high} BGN
                        </span>
                      )}
                      {priceSuggestion.comparableCount > 0 && (
                        <span className="text-xs text-gray-400">
                          based on {priceSuggestion.comparableCount} similar listing{priceSuggestion.comparableCount !== 1 ? "s" : ""}
                        </span>
                      )}
                    </div>
                    {priceSuggestion.reason && (
                      <p className="mt-1 text-xs text-gray-600 leading-relaxed">{priceSuggestion.reason}</p>
                    )}
                  </div>
                  <div className="flex items-center gap-2 flex-shrink-0">
                    <button
                      type="button"
                      onClick={() => {
                        setForm((prev) => ({ ...prev, price: String(priceSuggestion.suggestedPrice) }));
                        setPriceSuggestion(null);
                      }}
                      className="px-3 py-1.5 rounded-lg bg-primary text-white text-xs font-medium hover:bg-primary/90 transition-colors"
                    >
                      Use this
                    </button>
                    <button
                      type="button"
                      onClick={() => setPriceSuggestion(null)}
                      className="text-gray-400 hover:text-gray-600 transition-colors"
                      aria-label="Dismiss"
                    >
                      ✕
                    </button>
                  </div>
                </div>
              </div>
            )}
          </div>
        )}

        <CategorySpecificSection
          categorySlug={selectedSlug}
          ageGroup={form.ageGroup}
          shoeSize={form.shoeSize}
          clothingSize={form.clothingSize}
          onAgeGroupChange={(v) => setForm({ ...form, ageGroup: v })}
          onShoeSizeChange={(v) => setForm({ ...form, shoeSize: v })}
          onClothingSizeChange={(v) => setForm({ ...form, clothingSize: v })}
        />

        <PhotoUploader photos={photos} onChange={setPhotos} />

        <Button type="submit" fullWidth isLoading={loading} size="lg">
          {t("items.submit")}
        </Button>
      </form>

      <IbanModal
        isOpen={showIbanModal}
        onClose={() => setShowIbanModal(false)}
        onSaved={() => {
          setShowIbanModal(false);
          doSubmit();
        }}
      />

      {createdItem && (
        <SubmissionResultModal
          item={createdItem}
          onClose={() => setCreatedItem(null)}
        />
      )}
    </div>
  );
}
