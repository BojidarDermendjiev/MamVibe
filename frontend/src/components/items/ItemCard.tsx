import { Link } from 'react-router-dom';
import { HiEye } from 'react-icons/hi';
import { useTranslation } from 'react-i18next';
import { type Item, ListingType, ItemCondition } from '../../types/item';
import ConditionBadge from './ConditionBadge';
import { getCategoryImage } from '../../utils/categoryImages';
import { formatPrice } from '../../utils/currency';
import LikeButton from './LikeButton';

interface ItemCardProps {
  item: Item;
  onLikeToggle?: (id: string) => void;
  onRequireAuth?: () => void;
  showStatus?: boolean;
  onBump?: (id: string) => void;
}

export default function ItemCard({ item, onLikeToggle, onRequireAuth, showStatus, onBump }: ItemCardProps) {
  const { t } = useTranslation();
  const photo = item.photos[0];

  const isSold = showStatus && item.isSold;
  const isPending = showStatus && !item.isActive && !item.isSold;
  const isFlagged = isPending && item.aiModerationStatus === 3;
  const isReserved = item.isReserved;

  const now = Date.now();
  const bumpedMs = item.bumpedAt ? new Date(item.bumpedAt).getTime() : null;
  const isBumpActive = bumpedMs !== null && now - bumpedMs < 24 * 60 * 60 * 1000;
  const isOnCooldown = bumpedMs !== null && now - bumpedMs < 7 * 24 * 60 * 60 * 1000;
  const cooldownHoursLeft = isOnCooldown && bumpedMs !== null
    ? Math.ceil((bumpedMs + 7 * 24 * 60 * 60 * 1000 - now) / (60 * 60 * 1000))
    : 0;

  return (
    <div className={`bg-[#ffffff] dark:bg-[#2d2a42] rounded-2xl shadow-sm border hover-lift group animate-fade-in transition-all duration-300 ${
      isSold
        ? 'border-gray-300 dark:border-gray-600 opacity-75'
        : isPending && !isFlagged
        ? 'border-purple-400 dark:border-purple-500 shadow-purple-200 dark:shadow-purple-900/40 shadow-md'
        : isPending && isFlagged
        ? 'border-red-400 dark:border-red-600 shadow-red-200 dark:shadow-red-900/40 shadow-md'
        : isReserved
        ? 'border-amber-400 dark:border-amber-500 shadow-amber-100 dark:shadow-amber-900/30 shadow-md'
        : 'border-gray-100 dark:border-white/5 hover:shadow-md hover:-translate-y-0.5'
    }`}>
      <Link to={`/items/${item.id}`} className="block">
        {/* aspect-[4/3] container prevents CLS: the browser reserves space
            before the image loads, eliminating layout shift (Core Web Vitals). */}
        <div className="relative aspect-[4/3] overflow-hidden rounded-t-2xl bg-cream-dark">
          {photo ? (
            <img
              src={photo.url}
              alt={item.title}
              loading="lazy"
              decoding="async"
              width={400}
              height={300}
              className={`w-full h-full object-cover group-hover:scale-105 transition-transform duration-300 ${isPending || isSold ? 'opacity-60' : ''}`}
            />
          ) : (
            <img
              src={getCategoryImage(item.categoryName)}
              alt=""
              loading="lazy"
              decoding="async"
              width={400}
              height={300}
              className={`w-full h-full object-contain group-hover:scale-105 transition-transform duration-300 ${isPending || isSold ? 'opacity-60' : ''}`}
            />
          )}
          <span className={`absolute top-2 left-2 px-2 py-1 rounded-full text-xs font-medium text-white ${
            item.listingType === ListingType.Donate ? 'bg-green-500' : 'bg-mauve'
          }`}>
            {item.listingType === ListingType.Donate ? t('items.donate') : t('items.sell')}
          </span>

          {/* Sold overlay banner */}
          {isSold && (
            <div className="absolute bottom-0 left-0 right-0 py-2 flex items-center justify-center gap-2 bg-gradient-to-r from-gray-600/90 to-gray-700/90">
              <span className="text-white text-xs">✓</span>
              <span className="text-white text-xs font-semibold tracking-wide">
                {t('items.status_sold')}
              </span>
            </div>
          )}

          {/* Pending / flagged overlay banner */}
          {isPending && (
            <div className={`absolute bottom-0 left-0 right-0 py-2 flex items-center justify-center gap-2 ${
              isFlagged
                ? 'bg-gradient-to-r from-red-600/90 to-rose-600/90'
                : 'bg-gradient-to-r from-purple-600/90 to-violet-600/90'
            }`}>
              <span className={`text-white text-xs ${!isFlagged ? 'animate-pulse' : ''}`}>
                {isFlagged ? '⚠️' : '🕐'}
              </span>
              <span className="text-white text-xs font-semibold tracking-wide">
                {isFlagged ? t('items.status_flagged') : t('items.status_pending')}
              </span>
            </div>
          )}

          {/* Reserved overlay banner */}
          {!isPending && isReserved && (
            <div className="absolute bottom-0 left-0 right-0 py-2 flex items-center justify-center gap-2 bg-gradient-to-r from-amber-500/90 to-orange-500/90">
              <span className="text-white text-xs">🔒</span>
              <span className="text-white text-xs font-semibold tracking-wide">
                {t('items.status_reserved')}
              </span>
            </div>
          )}
        </div>
      </Link>
      {isSold && (
        <p className="text-xs text-center py-1.5 font-medium text-gray-500 dark:text-gray-400 bg-gray-50 dark:bg-gray-900/20">
          {t('items.status_sold_hint')}
        </p>
      )}
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
        <div className="flex items-center gap-1.5 mt-1 flex-wrap">
          <p className="text-sm text-text">{item.categoryName}</p>
          {item.condition !== ItemCondition.Unspecified && (
            <ConditionBadge condition={item.condition} size="sm" />
          )}
          {isBumpActive && (
            <span className="px-1.5 py-0.5 rounded-full bg-orange-100 text-orange-600 text-[10px] font-semibold uppercase tracking-wide">
              🚀 {t('items.bumped')}
            </span>
          )}
        </div>
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

        {onBump && (
          <div className="mt-2 pt-2 border-t border-gray-100 dark:border-white/5">
            {isBumpActive ? (
              <p className="text-xs text-center text-orange-500 font-medium">🚀 {t('items.bump_active')}</p>
            ) : isOnCooldown ? (
              <p className="text-xs text-center text-gray-400">{t('items.bump_cooldown', { hours: cooldownHoursLeft })}</p>
            ) : (
              <button
                onClick={e => { e.preventDefault(); onBump(item.id); }}
                className="w-full text-xs font-medium py-1.5 rounded-lg bg-orange-50 dark:bg-orange-900/20 text-orange-600 dark:text-orange-400 hover:bg-orange-100 dark:hover:bg-orange-900/40 transition-colors"
              >
                🚀 {t('items.bump')}
              </button>
            )}
          </div>
        )}
      </div>
    </div>
  );
}
