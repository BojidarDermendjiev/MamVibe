import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import toast from '@/utils/toast';
import { itemsApi } from '../api/itemsApi';
import { photosApi } from '../api/photosApi';
import { ListingType } from '../types/item';
import { useCategories } from '../hooks/useCategories';
import { useAuthStore } from '../store/authStore';
import Button from '../components/common/Button';
import Input from '../components/common/Input';
import PhotoUploader from '../components/items/PhotoUploader';
import IbanModal from '../components/common/IbanModal';

export default function CreateItemPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { categories } = useCategories();
  const { user } = useAuthStore();
  const [photos, setPhotos] = useState<File[]>([]);
  const [loading, setLoading] = useState(false);
  const [showIbanModal, setShowIbanModal] = useState(false);
  const [form, setForm] = useState<{
    title: string;
    description: string;
    categoryId: string;
    listingType: ListingType;
    price: string;
  }>({
    title: '',
    description: '',
    categoryId: '',
    listingType: ListingType.Donate,
    price: '',
  });

  const doSubmit = async () => {
    if (!form.categoryId) { toast.error('Please select a category'); return; }
    setLoading(true);
    try {
      const photoUrls: string[] = [];
      for (const file of photos) {
        const { data } = await photosApi.upload(file);
        photoUrls.push(data.url);
      }
      const { data: item } = await itemsApi.create({
        ...form,
        price: form.listingType === ListingType.Sell ? parseFloat(form.price) : null,
        photoUrls,
      });
      toast.success('Listing created!');
      navigate(`/items/${item.id}`);
    } catch (err: unknown) {
      const message = (err as { response?: { data?: { error?: string; message?: string } } })?.response?.data?.error
        || (err as { response?: { data?: { error?: string; message?: string } } })?.response?.data?.message
        || t('common.error');
      toast.error(message);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    // If selling and no IBAN saved, show modal first
    if (form.listingType === ListingType.Sell && !user?.iban) {
      setShowIbanModal(true);
      return;
    }
    await doSubmit();
  };

  return (
    <div className="max-w-2xl mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold text-primary mb-6">{t('items.create_title')}</h1>

      <form onSubmit={handleSubmit} className="space-y-5 bg-white rounded-xl p-6 border border-lavender/30">
        <Input
          label={t('items.title')}
          value={form.title}
          onChange={(e) => setForm({ ...form, title: e.target.value })}
          required
        />

        <div>
          <label className="block text-sm font-medium text-primary mb-1">{t('items.description')}</label>
          <textarea
            value={form.description}
            onChange={(e) => setForm({ ...form, description: e.target.value })}
            rows={4}
            required
            className="w-full px-4 py-2.5 rounded-lg border border-lavender bg-white text-gray-800 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-primary mb-1">{t('items.category')}</label>
          <select
            value={form.categoryId}
            onChange={(e) => setForm({ ...form, categoryId: e.target.value })}
            required
            className="w-full px-4 py-2.5 rounded-lg border border-lavender bg-white text-gray-800 focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <option value="">{t('items.all_categories')}</option>
            {categories.map((cat) => (
              <option key={cat.id} value={cat.id}>{cat.name}</option>
            ))}
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium text-primary mb-1">{t('items.listing_type')}</label>
          <div className="flex gap-3">
            <button
              type="button"
              onClick={() => setForm({ ...form, listingType: ListingType.Donate })}
              className={`flex-1 py-3 rounded-lg border-2 font-medium transition-all ${
                form.listingType === ListingType.Donate
                  ? 'border-green-500 bg-green-50 text-green-700'
                  : 'border-gray-200 text-gray-500 hover:border-gray-300'
              }`}
            >
              {t('items.donate')}
            </button>
            <button
              type="button"
              onClick={() => setForm({ ...form, listingType: ListingType.Sell })}
              className={`flex-1 py-3 rounded-lg border-2 font-medium transition-all ${
                form.listingType === ListingType.Sell
                  ? 'border-mauve bg-mauve/10 text-mauve'
                  : 'border-gray-200 text-gray-500 hover:border-gray-300'
              }`}
            >
              {t('items.sell')}
            </button>
          </div>
        </div>

        {form.listingType === ListingType.Sell && (
          <Input
            label={t('items.price')}
            type="number"
            min="0.01"
            step="0.01"
            value={form.price}
            onChange={(e) => setForm({ ...form, price: e.target.value })}
            required
          />
        )}

        <PhotoUploader photos={photos} onChange={setPhotos} />

        <Button type="submit" fullWidth isLoading={loading} size="lg">
          {t('items.submit')}
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
