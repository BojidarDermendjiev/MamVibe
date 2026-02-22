import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { itemsApi } from '../api/itemsApi';
import { photosApi } from '../api/photosApi';
import { ListingType } from '../types/item';
import { useCategories } from '../hooks/useCategories';
import Button from '../components/common/Button';
import Input from '../components/common/Input';
import PhotoUploader from '../components/items/PhotoUploader';
import LoadingSpinner from '../components/common/LoadingSpinner';

export default function EditItemPage() {
  const { id } = useParams<{ id: string }>();
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { categories } = useCategories();
  const [existingPhotos, setExistingPhotos] = useState<{ id: string; url: string }[]>([]);
  const [photosToDelete, setPhotosToDelete] = useState<{ id: string; url: string }[]>([]);
  const [newPhotos, setNewPhotos] = useState<File[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
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

  useEffect(() => {
    const load = async () => {
      try {
        const { data: item } = await itemsApi.getById(id!);
        setForm({
          title: item.title,
          description: item.description,
          categoryId: item.categoryId,
          listingType: item.listingType,
          price: item.price?.toString() || '',
        });
        setExistingPhotos(item.photos.map((p) => ({ id: p.id, url: p.url })));
      } catch {
        toast.error('Item not found');
        navigate('/dashboard');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [id, navigate]);

  const handleRemoveExisting = (photoId: string) => {
    const photo = existingPhotos.find((p) => p.id === photoId);
    if (!photo) return;
    setPhotosToDelete((prev) => [...prev, photo]);
    setExistingPhotos((prev) => prev.filter((p) => p.id !== photoId));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      // Delete photos marked for removal
      for (const photo of photosToDelete) {
        await photosApi.delete(photo.url);
      }
      // Upload new photos first to get URLs
      const newPhotoUrls: string[] = [];
      for (const file of newPhotos) {
        const { data } = await photosApi.upload(file);
        newPhotoUrls.push(data.url);
      }
      // Combine existing + new photo URLs
      const photoUrls = [
        ...existingPhotos.map((p) => p.url),
        ...newPhotoUrls,
      ];
      await itemsApi.update(id!, {
        ...form,
        price: form.listingType === ListingType.Sell ? parseFloat(form.price) : null,
        photoUrls,
      });
      toast.success('Item updated!');
      navigate(`/items/${id}`);
    } catch (err: unknown) {
      const message = (err as { response?: { data?: { error?: string; message?: string } } })?.response?.data?.error
        || (err as { response?: { data?: { error?: string; message?: string } } })?.response?.data?.message
        || t('common.error');
      toast.error(message);
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <LoadingSpinner size="lg" className="py-20" />;

  return (
    <div className="max-w-2xl mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold text-primary mb-6">{t('items.edit_title')}</h1>

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
                  : 'border-gray-200 text-gray-500'
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
                  : 'border-gray-200 text-gray-500'
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

        <PhotoUploader
          photos={newPhotos}
          existingPhotos={existingPhotos}
          onChange={setNewPhotos}
          onRemoveExisting={handleRemoveExisting}
        />

        <Button type="submit" fullWidth isLoading={saving} size="lg">
          {t('items.update')}
        </Button>
      </form>
    </div>
  );
}
