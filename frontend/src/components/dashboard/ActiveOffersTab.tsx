import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { Offer } from '@/types/offer';
import { OfferStatus } from '@/types/offer';
import { formatPrice } from '@/utils/currency';
import TabErrorState from './TabErrorState';

interface ActiveOffersTabProps {
  offers: Offer[];
  error: string | null;
  onRetry: () => void;
  onAccept: (offerId: string) => void;
  onDecline: (offerId: string) => void;
  onCounter: (offerId: string, price: number) => void;
}

function offerStatusLabel(t: (key: string) => string, status: number) {
  if (status === OfferStatus.Pending)   return { text: t('offer.status_pending'),   cls: 'bg-yellow-100 text-yellow-800' };
  if (status === OfferStatus.Accepted)  return { text: t('offer.status_accepted'),  cls: 'bg-green-100 text-green-800' };
  if (status === OfferStatus.Declined)  return { text: t('offer.status_declined'),  cls: 'bg-red-100 text-red-800' };
  if (status === OfferStatus.Countered) return { text: t('offer.status_countered'), cls: 'bg-blue-100 text-blue-800' };
  if (status === OfferStatus.Expired)   return { text: t('offer.status_expired'),   cls: 'bg-gray-100 text-gray-500' };
  return { text: t('offer.status_cancelled'), cls: 'bg-gray-100 text-gray-500' };
}

export default function ActiveOffersTab({ offers, error, onRetry, onAccept, onDecline, onCounter }: ActiveOffersTabProps) {
  const { t } = useTranslation();
  const [counteringOfferId, setCounteringOfferId] = useState<string | null>(null);
  const [counterPrice, setCounterPrice] = useState('');

  if (error) return <TabErrorState onRetry={onRetry} />;

  return (
    <div role="tabpanel" id="panel-received-offers" aria-labelledby="tab-received-offers">
      {offers.length === 0 ? (
        <p className="text-center py-20 text-gray-400">{t('dashboard.no_received_offers')}</p>
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
                    {t('offer.buyer')}: {offer.buyerDisplayName}
                    {' · '}{t('offer.offered')}: <span className="font-semibold text-mauve">{formatPrice(offer.offeredPrice)}</span>
                    {offer.itemPrice != null && (
                      <span className="text-gray-400 text-xs ml-1">({t('offer.list_price')}: {formatPrice(offer.itemPrice)})</span>
                    )}
                  </p>
                  <p className="text-xs text-gray-400 mt-0.5">{new Date(offer.createdAt).toLocaleDateString()}</p>
                </div>
                <div className="flex items-center gap-2 flex-shrink-0 flex-wrap justify-end">
                  <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${cls}`}>{text}</span>
                  {offer.status === OfferStatus.Pending && (
                    <>
                      <button onClick={() => onAccept(offer.id)} className="px-3 py-1.5 text-sm font-medium bg-green-500 text-white rounded-lg hover:bg-green-600 transition-colors">{t('offer.accept')}</button>
                      <button onClick={() => { setCounteringOfferId(offer.id); setCounterPrice(''); }} className="px-3 py-1.5 text-sm font-medium bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-colors">{t('offer.counter')}</button>
                      <button onClick={() => onDecline(offer.id)} className="px-3 py-1.5 text-sm font-medium bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors">{t('offer.decline')}</button>
                    </>
                  )}
                  {counteringOfferId === offer.id && (
                    <div className="flex items-center gap-2 mt-2 w-full">
                      <input
                        type="number" step="0.01" min="0.01"
                        value={counterPrice}
                        onChange={e => setCounterPrice(e.target.value)}
                        placeholder="0.00"
                        className="w-28 px-2 py-1.5 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                      />
                      <button
                        onClick={() => {
                          const parsed = parseFloat(counterPrice);
                          if (!isNaN(parsed) && parsed > 0) {
                            onCounter(offer.id, parsed);
                            setCounteringOfferId(null);
                            setCounterPrice('');
                          }
                        }}
                        className="px-3 py-1.5 text-sm font-medium bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-colors"
                      >
                        {t('common.send')}
                      </button>
                      <button onClick={() => setCounteringOfferId(null)} className="px-2 py-1.5 text-sm text-gray-400 hover:text-gray-600 transition-colors">{t('common.cancel')}</button>
                    </div>
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
