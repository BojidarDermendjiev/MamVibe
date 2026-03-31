import { useState, useMemo, useRef } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import toast from "@/utils/toast";
import { itemsApi } from "../api/itemsApi";
import { photosApi } from "../api/photosApi";
import { aiApi } from "../api/aiApi";
import { ListingType, AgeGroup } from "../types/item";
import { useCategories } from "../hooks/useCategories";
import { useAuthStore } from "../store/authStore";
import Button from "../components/common/Button";
import Input from "../components/common/Input";
import PhotoUploader from "../components/items/PhotoUploader";
import IbanModal from "../components/common/IbanModal";
import CategorySpecificSection from "../components/items/CategorySpecificSection";

export default function CreateItemPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { categories } = useCategories();
  const { user } = useAuthStore();
  const [photos, setPhotos] = useState<File[]>([]);
  const [loading, setLoading] = useState(false);
  const [aiLoading, setAiLoading] = useState(false);
  const [showIbanModal, setShowIbanModal] = useState(false);
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
    } catch {
      toast.error("Could not analyse the photo. Please fill the form manually.");
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
      toast.success("Listing created!");
      navigate(`/items/${item.id}`);
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
      <div className="mb-5 p-4 rounded-xl border border-lavender/40 bg-gradient-to-r from-purple-50 to-pink-50">
        <p className="text-sm font-medium text-gray-700 mb-1">
          ✨ AI Listing Assistant
        </p>
        <p className="text-xs text-gray-500 mb-3">
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
        >
          {aiLoading ? "Analysing photo…" : "✨ Fill with AI"}
        </Button>
      </div>

      <form
        onSubmit={handleSubmit}
        className="space-y-5 bg-white rounded-xl p-6 border border-lavender/30"
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
              onClick={() =>
                setForm({ ...form, listingType: ListingType.Donate })
              }
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
              onClick={() =>
                setForm({ ...form, listingType: ListingType.Sell })
              }
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
          <Input
            label={t("items.price")}
            type="number"
            min="0.01"
            step="0.01"
            value={form.price}
            onChange={(e) => setForm({ ...form, price: e.target.value })}
            required
          />
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
    </div>
  );
}
