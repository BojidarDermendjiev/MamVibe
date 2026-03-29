import { Link } from 'react-router-dom';
import { HiEye } from 'react-icons/hi';
import { useTranslation } from 'react-i18next';
import { type Item, ListingType } from '../../types/item';
import { getCategoryImage } from '../../utils/categoryImages';
import { formatPrice } from '../../utils/currency';
import LikeButton from './LikeButton';

interface ItemCardProps {
  item: Item;
  onLikeToggle?: (id: string) => void;
  onRequireAuth?: () => void;
}

export default function ItemCard({ item, onLikeToggle, onRequireAuth }: ItemCardProps) {
  const { t } = useTranslation();
  const photo = item.photos[0];

  return (
    <div className="bg-white rounded-xl shadow-sm border border-lavender/30 hover-lift hover-glow group animate-fade-in">
      <Link to={`/items/${item.id}`} className="block">
        <div className="relative aspect-[4/3] overflow-hidden rounded-t-xl bg-cream-dark">
          <img
            src={photo ? photo.url : getCategoryImage(item.categoryName)}
            alt={item.title}
            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
          />
          <span className={`absolute top-2 left-2 px-2 py-1 rounded-full text-xs font-medium text-white ${
            item.listingType === ListingType.Donate ? 'bg-green-500' : 'bg-mauve'
          }`}>
            {item.listingType === ListingType.Donate ? t('items.donate') : t('items.sell')}
          </span>
        </div>
      </Link>
      <div className="p-3">
        <Link to={`/items/${item.id}`}>
          <h3 className="font-semibold text-primary truncate hover:text-mauve transition-colors">
            {item.title}
          </h3>
        </Link>
        <p className="text-sm text-text mt-0.5">{item.categoryName}</p>
        <div className="flex items-center justify-between mt-2">
          <span className="font-bold text-lg text-mauve">
            {item.listingType === ListingType.Donate
              ? t('items.free')
              : formatPrice(item.price)}
          </span>
          <div className="flex items-center gap-3 text-sm text-gray-400">
            <span className="flex items-center gap-1">
              <HiEye className="h-4 w-4" /> {item.viewCount}
            </span>
            <LikeButton
              itemId={item.id}
              likeCount={item.likeCount}
              isLiked={item.isLikedByCurrentUser}
              onToggle={onLikeToggle}
              onRequireAuth={onRequireAuth}
              size="sm"
            />
          </div>
        </div>
      </div>
    </div>
  );
}
