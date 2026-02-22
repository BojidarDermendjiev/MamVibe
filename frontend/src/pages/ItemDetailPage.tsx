import { useEffect, useState, useRef } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { HiEye, HiPencil, HiTrash, HiChat } from 'react-icons/hi';
import toast from 'react-hot-toast';
import { itemsApi } from '../api/itemsApi';
import { type Item, ListingType } from '../types/item';
import { useAuthStore } from '../store/authStore';
import { useCartStore } from '../store/cartStore';
import { getCategoryImage } from '../utils/categoryImages';
import LikeButton from '../components/items/LikeButton';
import Avatar from '../components/common/Avatar';
import Button from '../components/common/Button';
import LoadingSpinner from '../components/common/LoadingSpinner';

export default function ItemDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const addItem = useCartStore((s) => s.addItem);
  const [item, setItem] = useState<Item | null>(null);
  const [loading, setLoading] = useState(true);
  const [activePhoto, setActivePhoto] = useState(0);
  const viewCounted = useRef(false);

  useEffect(() => {
    if (!id) return;
    const load = async () => {
      try {
        const { data } = await itemsApi.getById(id);
        setItem(data);
        if (!viewCounted.current) {
          viewCounted.current = true;
          itemsApi.incrementView(id).catch(() => {});
        }
      } catch {
        toast.error('Item not found');
        navigate('/browse');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [id, navigate]);

  const handleDelete = async () => {
    if (!item || !confirm(t('common.confirm') + '?')) return;
    try {
      await itemsApi.delete(item.id);
      toast.success('Item deleted');
      navigate('/dashboard');
    } catch {
      toast.error(t('common.error'));
    }
  };

  const handleLikeToggle = async () => {
    if (!item) return;
    try {
      await itemsApi.toggleLike(item.id);
      setItem({
        ...item,
        isLikedByCurrentUser: !item.isLikedByCurrentUser,
        likeCount: item.isLikedByCurrentUser ? item.likeCount - 1 : item.likeCount + 1,
      });
    } catch { /* ignore */ }
  };

  if (loading) return <LoadingSpinner size="lg" className="py-20" />;
  if (!item) return null;

  const isOwner = user?.id === item.userId;

  return (
    <div className="max-w-6xl mx-auto px-4 py-8 animate-fade-in">
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Photo gallery */}
        <div>
          <div className="aspect-square rounded-xl overflow-hidden bg-cream-dark mb-4">
            <img
              src={item.photos.length > 0 ? item.photos[activePhoto]?.url : getCategoryImage(item.categoryName)}
              alt={item.title}
              className="w-full h-full object-cover"
            />
          </div>
          {item.photos.length > 1 && (
            <div className="flex gap-2 overflow-x-auto">
              {item.photos.map((photo, i) => (
                <button
                  key={photo.id}
                  onClick={() => setActivePhoto(i)}
                  className={`w-16 h-16 rounded-lg overflow-hidden flex-shrink-0 border-2 transition-colors ${
                    i === activePhoto ? 'border-primary' : 'border-transparent'
                  }`}
                >
                  <img src={photo.url} alt="" className="w-full h-full object-cover" />
                </button>
              ))}
            </div>
          )}
        </div>

        {/* Details */}
        <div>
          <div className="flex items-start justify-between mb-2">
            <span className={`px-3 py-1 rounded-full text-sm font-medium text-white ${
              item.listingType === ListingType.Donate ? 'bg-green-500' : 'bg-mauve'
            }`}>
              {item.listingType === ListingType.Donate ? t('items.donate') : t('items.sell')}
            </span>
            {isOwner && (
              <div className="flex gap-2">
                <Link to={`/items/${item.id}/edit`}>
                  <Button size="sm" variant="ghost"><HiPencil className="h-4 w-4" /></Button>
                </Link>
                <Button size="sm" variant="danger" onClick={handleDelete}>
                  <HiTrash className="h-4 w-4" />
                </Button>
              </div>
            )}
          </div>

          <h1 className="text-3xl font-bold text-primary mb-2">{item.title}</h1>
          <p className="text-sm text-gray-500 mb-4">{item.categoryName}</p>

          <div className="text-3xl font-bold text-mauve mb-6">
            {item.listingType === ListingType.Donate ? t('items.free') : `$${item.price?.toFixed(2)}`}
          </div>

          <div className="flex items-center gap-4 mb-6 text-sm text-gray-500">
            <span className="flex items-center gap-1"><HiEye className="h-4 w-4" /> {item.viewCount} {t('items.views')}</span>
            <LikeButton
              itemId={item.id}
              likeCount={item.likeCount}
              isLiked={item.isLikedByCurrentUser}
              onToggle={handleLikeToggle}
            />
          </div>

          <div className="prose prose-sm text-gray-700 mb-8">
            <p className="whitespace-pre-wrap">{item.description}</p>
          </div>

          {/* Seller card */}
          <div className="bg-peach-light rounded-xl p-4 mb-6">
            <div className="flex items-center gap-3">
              <Avatar src={item.userAvatarUrl} size="md" />
              <div>
                <p className="font-medium text-primary">{item.userDisplayName}</p>
                <p className="text-xs text-gray-400">Listed {new Date(item.createdAt).toLocaleDateString()}</p>
              </div>
            </div>
          </div>

          {/* Actions */}
          {!isOwner && user && (
            <div className="flex gap-3">
              <Link
                to={`/chat/${item.userId}`}
                state={{ displayName: item.userDisplayName, avatarUrl: item.userAvatarUrl }}
                className="flex-1"
              >
                <Button fullWidth variant="secondary">
                  <HiChat className="h-5 w-5 mr-2" /> {t('items.contact_seller')}
                </Button>
              </Link>
              <Button
                fullWidth
                className={item.listingType === ListingType.Donate ? 'bg-green-500 hover:bg-green-600' : undefined}
                onClick={() => {
                  addItem({
                    id: item.id,
                    title: item.title,
                    price: item.price ?? 0,
                    imageUrl: item.photos[0]?.url,
                    listingType: item.listingType,
                    sellerId: item.userId,
                    categoryName: item.categoryName,
                  });
                  toast.success(t('cart.added'));
                  navigate('/browse');
                }}
              >
                {item.listingType === ListingType.Donate ? t('items.book_now') : t('items.buy_now')}
              </Button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
