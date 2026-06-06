import { useTranslation } from 'react-i18next';
import type { Offer } from '@/types/offer';
import { OfferStatus } from '@/types/offer';
import { formatPrice } from '@/utils/currency';
import TabErrorState from './TabErrorState';

interface SentOffersTabProps {
  offers: Offer[];
  error: string | null;
  onRetry: () => void;
  onAcceptCounter: (offerId: string) => void;
  onDeclineCounter: (offerId: string) => void;
  onCancel: (offerId: string) => void;
}

function offerStatusLabel(t: (key: string) => string, status: number) {
  if (status === OfferStatus.Pending)   return { text: t('offer.status_pending'),   cls: 'bg-yellow-100 text-yellow-800' };
  if (status === OfferStatus.Accepted)  return { text: t('offer.status_accepted'),  cls: 'bg-green-100 text-green-800' };
  if (status === OfferStatus.Declined)  return { text: t('offer.status_declined'),  cls: 'bg-red-100 text-red-800' };
  if (status === OfferStatus.Countered) return { text: t('offer.status_countered'), cls: 'bg-blue-100 text-blue-800' };
  if (status === OfferStatus.Expired)   return { text: t('offer.status_expired'),   cls: 'bg-gray-100 text-gray-500' };
  return { text: t('offer.status_cancelled'), cls: 'bg-gray-100 text-gray-500' };
}

export default function SentOffersTab({ offers, error, onRetry, onAcceptCounter, onDeclineCounter, onCancel }: SentOffersTabProps) {
  const { t } = useTranslation();

  if (error) return <TabErrorState onRetry={onRetry} />;

  return (
    <div role="tabpanel" id="panel-sent-offers" aria-labelledby="tab-sent-offers">
      {offers.length === 0 ? (
        <p className="text-center py-20 text-gray-400">{t('dashboard.no_sent_offers')}</p>
      ) : (
        <div className="space-y-3">
          {offers.map((offer: Offer) => {
            const { text, cls } = offerStatusLabel(t, offer.status);
            return (
              <div key={offer.id} className="bg-white rounded-xl p-4 border border-lavender/30 flex items-center gap-4 hover:shadow-md transition-shadow duration-300">
                {offer.itemPhotoUrl ? (
                  <img src={offer.itemPhotoUrl} alt={offer.itemTitle ?? ''} className="w-14 h-14 rounded-lg object-cover flex-shrink-0" />
                ) : (
                  <div className="w-14 h-14 rounded-lg bg-lavender/20 flex-shrink-0" />
                )}
                <div className="flex-1 min-w-0">
                  <p className="font-medium text-primary truncate">{offer.itemTitle}</p>
                  <p className="text-sm text-gray-500">
                    {t('offer.offered')}: <span className="font-semibold text-mauve">{formatPrice(offer.offeredPrice)}</span>
                    {offer.counterPrice != null && (
                      <span className="ml-2 text-blue-600">{t('offer.counter_price')}: <span className="font-semibold">{formatPrice(offer.counterPrice)}</span></span>
                    )}
                  </p>
                  <p className="text-xs text-gray-400 mt-0.5">{new Date(offer.createdAt).toLocaleDateString()}</p>
                </div>
                <div className="flex items-center gap-2 flex-shrink-0">
                  <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${cls}`}>{text}</span>
                  {offer.status === OfferStatus.Countered && (
                    <>
                      <button onClick={() => onAcceptCounter(offer.id)} className="px-3 py-1.5 text-sm font-medium bg-green-500 text-white rounded-lg hover:bg-green-600 transition-colors">{t('offer.accept_counter')}</button>
                      <button onClick={() => onDeclineCounter(offer.id)} className="px-3 py-1.5 text-sm font-medium bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors">{t('offer.decline_counter')}</button>
                    </>
                  )}
                  {(offer.status === OfferStatus.Pending || offer.status === OfferStatus.Countered) && (
                    <button onClick={() => onCancel(offer.id)} className="px-3 py-1.5 text-sm font-medium border border-gray-300 text-gray-500 rounded-lg hover:bg-gray-50 transition-colors">{t('offer.cancel')}</button>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
