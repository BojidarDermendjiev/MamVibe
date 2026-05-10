import { useEffect, useState, useRef, useMemo } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { usePageSEO } from '@/hooks/useSEO';
import { HiEye, HiPencil, HiTrash, HiChat } from 'react-icons/hi';
import toast from '@/utils/toast';
import { itemsApi } from '../api/itemsApi';
import { purchaseRequestsApi } from '../api/purchaseRequestsApi';
import { userRatingsApi } from '../api/userRatingsApi';
import { type Item, ListingType } from '../types/item';
import type { UserRatingSummary } from '../types/userRating';
import { useAuthStore } from '../store/authStore';
import { getCategoryImage } from '../utils/categoryImages';
import { formatPrice } from '../utils/currency';
import LikeButton from '../components/items/LikeButton';
import Avatar from '../components/common/Avatar';
import Button from '../components/common/Button';
import LoadingSpinner from '../components/common/LoadingSpinner';
import StarRating from '../components/common/StarRating';
import NekorektenWarningModal from '../components/common/NekorektenWarningModal';

export default function ItemDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const [item, setItem] = useState<Item | null>(null);
  const [loading, setLoading] = useState(true);
  const [activePhoto, setActivePhoto] = useState(0);
  const [requestPending, setRequestPending] = useState(false);
  const [requestSent, setRequestSent] = useState(false);
  const [showNekorektenWarning, setShowNekorektenWarning] = useState(false);
  const [isSellerReported, setIsSellerReported] = useState(false);
  const [sellerRating, setSellerRating] = useState<UserRatingSummary | null>(null);
  const nekorektenReportUrl = 'https://nekorekten.com/';
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
        itemsApi.checkSeller(id)
          .then(({ data: check }) => setIsSellerReported(check.hasReports))
          .catch(() => {});
        userRatingsApi.getSummary(data.userId)
          .then(({ data: summary }) => setSellerRating(summary))
          .catch(() => {});
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

  const handlePurchaseClick = () => {
    if (isSellerReported) {
      setShowNekorektenWarning(true);
    } else {
      handleRequestPurchase();
    }
  };

  const handleRequestPurchase = async () => {
    if (!item) return;
    setShowNekorektenWarning(false);
    setRequestPending(true);
    try {
      await purchaseRequestsApi.create(item.id);
      setRequestSent(true);
      toast.success('Request sent to seller!');
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { error?: string } } })?.response?.data?.error ??
        'Could not send request. The item may already be reserved.';
      toast.error(msg);
    } finally {
      setRequestPending(false);
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

  // SEO: build Product + BreadcrumbList structured data from the loaded item.
  // Product schema enables Google rich results (price, availability) in SERPs.
  // BreadcrumbList enables the breadcrumb trail displayed under the title in SERPs.
  // eslint-disable-next-line react-hooks/rules-of-hooks
  const productSchema = useMemo(() => {
    if (!item) return undefined;
    const itemUrl = `https://mamvibe.com/items/${item.id}`;
    const firstPhoto = item.photos?.[0]?.url;

    const product = {
      "@context": "https://schema.org",
      "@type": "Product",
      name: item.title,
      description: item.description,
      ...(firstPhoto ? { image: firstPhoto } : {}),
      url: itemUrl,
      offers: {
        "@type": "Offer",
        priceCurrency: "BGN",
        price: item.price ?? 0,
        availability: item.isActive
          ? "https://schema.org/InStock"
          : "https://schema.org/SoldOut",
        url: itemUrl,
        seller: {
          "@type": "Person",
          name: item.userDisplayName,
        },
      },
      ...(sellerRating && sellerRating.count > 0
        ? {
            aggregateRating: {
              "@type": "AggregateRating",
              ratingValue: sellerRating.average,
              reviewCount: sellerRating.count,
              bestRating: 5,
              worstRating: 1,
            },
          }
        : {}),
    };

    const breadcrumb = {
      "@context": "https://schema.org",
      "@type": "BreadcrumbList",
      itemListElement: [
        {
          "@type": "ListItem",
          position: 1,
          name: "Home",
          item: "https://mamvibe.com/",
        },
        {
          "@type": "ListItem",
          position: 2,
          name: "Browse",
          item: "https://mamvibe.com/browse",
        },
        ...(item.categoryName
          ? [
              {
                "@type": "ListItem",
                position: 3,
                name: item.categoryName,
                item: `https://mamvibe.com/browse?category=${encodeURIComponent(item.categoryName)}`,
              },
              {
                "@type": "ListItem",
                position: 4,
                name: item.title,
                item: itemUrl,
              },
            ]
          : [
              {
                "@type": "ListItem",
                position: 3,
                name: item.title,
                item: itemUrl,
              },
            ]),
      ],
    };

    return [product, breadcrumb];
  }, [item, sellerRating]);

  // eslint-disable-next-line react-hooks/rules-of-hooks
  usePageSEO({
    title: item ? `${item.title} — ${item.categoryName}` : "Item Detail",
    description: item
      ? `${item.title} for ${item.price ? `${item.price} BGN` : "free"} on MamVibe. ${item.description?.slice(0, 100) ?? ""}`
      : "View this item on MamVibe.",
    canonical: item ? `https://mamvibe.com/items/${item.id}` : undefined,
    image: item?.photos?.[0]?.url,
    ogType: "product",
    structuredData: productSchema,
  });

  if (loading) return <LoadingSpinner size="lg" className="py-20" />;
  if (!item) return null;

  const isOwner = user?.id === item.userId;

  return (
    <div className="max-w-6xl mx-auto px-4 py-8 animate-fade-in">
      {/* Breadcrumb — improves navigation UX and enables BreadcrumbList rich result in SERPs */}
      <nav aria-label="Breadcrumb" className="mb-4">
        <ol className="flex items-center gap-1.5 text-sm text-gray-400 flex-wrap">
          <li>
            <Link to="/" className="hover:text-primary transition-colors">Home</Link>
          </li>
          <li aria-hidden="true">/</li>
          <li>
            <Link to="/browse" className="hover:text-primary transition-colors">Browse</Link>
          </li>
          {item.categoryName && (
            <>
              <li aria-hidden="true">/</li>
              <li>
                <Link
                  to={`/browse?category=${encodeURIComponent(item.categoryName)}`}
                  className="hover:text-primary transition-colors"
                >
                  {item.categoryName}
                </Link>
              </li>
            </>
          )}
          <li aria-hidden="true">/</li>
          <li className="text-gray-600 dark:text-gray-300 truncate max-w-[200px]" aria-current="page">
            {item.title}
          </li>
        </ol>
      </nav>

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
            {item.listingType === ListingType.Donate ? t('items.free') : formatPrice(item.price)}
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

          <section aria-labelledby="item-description-heading" className="mb-8">
            {/* h2 establishes heading hierarchy: h1 (item title) → h2 (description section) */}
            <h2 id="item-description-heading" className="text-base font-semibold text-gray-700 dark:text-gray-300 mb-2">
              Description
            </h2>
            <div className="prose prose-sm text-gray-700 dark:text-gray-300">
              <p className="whitespace-pre-wrap">{item.description}</p>
            </div>
          </section>

          {/* Seller card */}
          <div className={`rounded-xl p-4 mb-6 ${isSellerReported ? 'bg-red-50 border border-red-200' : 'bg-peach-light'}`}>
            <div className="flex items-center gap-3">
              <Avatar src={item.userAvatarUrl} size="md" />
              <div className="flex-1 min-w-0">
                <p className="font-medium text-primary">{item.userDisplayName}</p>
                <div className="flex items-center gap-1.5 mt-0.5">
                  <StarRating value={Math.round(sellerRating?.average ?? 0)} readonly size="sm" />
                  <span className="text-xs text-gray-500">
                    {sellerRating && sellerRating.count > 0
                      ? `${sellerRating.average?.toFixed(1)} (${sellerRating.count})`
                      : t('rating.no_ratings')}
                  </span>
                </div>
              </div>
              {isSellerReported && (
                <a
                  href={nekorektenReportUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="flex items-center gap-1 px-2.5 py-1 rounded-full bg-red-100 text-red-600 text-xs font-semibold border border-red-300 hover:bg-red-200 transition-colors flex-shrink-0"
                >
                  ⚠️ {t('nekorekten.badge')}
                </a>
              )}
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

              {/* Item is available — show Request Purchase button */}
              {item.isActive && (
                <Button
                  fullWidth
                  disabled={requestSent || requestPending}
                  className={item.listingType === ListingType.Donate ? 'bg-green-500 hover:bg-green-600' : undefined}
                  onClick={handlePurchaseClick}
                >
                  {requestSent
                    ? 'Pending Approval'
                    : requestPending
                    ? 'Sending…'
                    : item.listingType === ListingType.Donate
                    ? 'Request Booking'
                    : 'Request Purchase'}
                </Button>
              )}

              {/* Item is reserved by someone else */}
              {!item.isActive && (
                <div className="flex-1 flex items-center justify-center bg-gray-100 rounded-xl px-4 py-2 text-sm font-medium text-gray-500">
                  Not Available
                </div>
              )}
            </div>
          )}
        </div>
      </div>
      <NekorektenWarningModal
        isOpen={showNekorektenWarning}
        onClose={() => setShowNekorektenWarning(false)}
        onConfirm={handleRequestPurchase}
        sellerName={item.userDisplayName}
        reportUrl={nekorektenReportUrl}
      />
    </div>
  );
}
