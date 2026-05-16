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
  showStatus?: boolean;
}

export default function ItemCard({ item, onLikeToggle, onRequireAuth, showStatus }: ItemCardProps) {
  const { t } = useTranslation();
  const photo = item.photos[0];

  const isPending = showStatus && !item.isActive;
  const isFlagged = isPending && item.aiModerationStatus === 3;

  return (
    <div className={`bg-white rounded-xl shadow-sm border hover-lift group animate-fade-in transition-shadow duration-300 ${
      isPending && !isFlagged
        ? 'border-purple-400 dark:border-purple-500 shadow-purple-200 dark:shadow-purple-900/40 shadow-md'
        : isPending && isFlagged
        ? 'border-red-400 dark:border-red-600 shadow-red-200 dark:shadow-red-900/40 shadow-md'
        : 'border-lavender/30 hover-glow'
    }`}>
      <Link to={`/items/${item.id}`} className="block">
        {/* aspect-[4/3] container prevents CLS: the browser reserves space
            before the image loads, eliminating layout shift (Core Web Vitals). */}
        <div className="relative aspect-[4/3] overflow-hidden rounded-t-xl bg-cream-dark">
          {photo ? (
            <img
              src={photo.url}
              alt={item.title}
              loading="lazy"
              decoding="async"
              width={400}
              height={300}
              className={`w-full h-full object-cover group-hover:scale-105 transition-transform duration-300 ${isPending ? 'opacity-60' : ''}`}
            />
          ) : (
            <img
              src={getCategoryImage(item.categoryName)}
              alt=""
              loading="lazy"
              decoding="async"
              width={400}
              height={300}
              className={`w-full h-full object-contain group-hover:scale-105 transition-transform duration-300 ${isPending ? 'opacity-60' : ''}`}
            />
          )}
          <span className={`absolute top-2 left-2 px-2 py-1 rounded-full text-xs font-medium text-white ${
            item.listingType === ListingType.Donate ? 'bg-green-500' : 'bg-mauve'
          }`}>
            {item.listingType === ListingType.Donate ? t('items.donate') : t('items.sell')}
          </span>

          {/* Pending / flagged overlay banner */}
          {isPending && (
            <div className={`absolute bottom-0 left-0 right-0 px-3 py-2 flex items-center gap-2 ${
              isFlagged
                ? 'bg-gradient-to-r from-red-600/90 to-rose-600/90'
                : 'bg-gradient-to-r from-purple-600/90 to-violet-600/90'
            }`}>
              <span className={`text-white text-xs ${!isFlagged ? 'animate-pulse' : ''}`}>
                {isFlagged ? '⚠️' : '🕐'}
              </span>
              <span className="text-white text-xs font-semibold tracking-wide truncate">
                {isFlagged ? t('items.status_flagged') : t('items.status_pending')}
              </span>
            </div>
          )}
        </div>
      </Link>
      {isPending && (
        <p className={`text-xs text-center py-1.5 font-medium ${
          isFlagged
            ? 'text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-900/20'
            : 'text-purple-600 dark:text-purple-300 bg-purple-50 dark:bg-purple-900/20'
        }`}>
          {t('items.status_pending_hint')}
        </p>
      )}
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
