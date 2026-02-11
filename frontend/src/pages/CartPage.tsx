import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { HiTrash, HiShoppingCart } from 'react-icons/hi';
import { useCartStore } from '../store/cartStore';

export default function CartPage() {
  const { t } = useTranslation();
  const { items, removeItem, clearCart } = useCartStore();

  const total = items.reduce((sum, item) => sum + (item.listingType === 1 ? item.price : 0), 0);

  return (
    <div className="max-w-3xl mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold text-primary mb-6 flex items-center gap-2">
        <HiShoppingCart className="h-8 w-8" />
        {t('cart.title')}
      </h1>

      {items.length === 0 ? (
        <div className="text-center py-16">
          <HiShoppingCart className="h-16 w-16 text-gray-300 mx-auto mb-4" />
          <p className="text-gray-500 text-lg mb-4">{t('cart.empty')}</p>
          <Link
            to="/browse"
            className="inline-block px-6 py-3 bg-primary text-white rounded-lg hover:bg-primary-dark transition-colors"
          >
            {t('nav.browse')}
          </Link>
        </div>
      ) : (
        <>
          <div className="space-y-4">
            {items.map((item) => (
              <div
                key={item.id}
                className="flex items-center gap-4 bg-white rounded-xl shadow-sm border border-lavender/30 p-4"
              >
                {item.imageUrl ? (
                  <img
                    src={item.imageUrl}
                    alt={item.title}
                    className="h-20 w-20 rounded-lg object-cover"
                  />
                ) : (
                  <div className="h-20 w-20 rounded-lg bg-cream-dark flex items-center justify-center text-gray-400">
                    <HiShoppingCart className="h-8 w-8" />
                  </div>
                )}
                <div className="flex-1 min-w-0">
                  <Link
                    to={`/items/${item.id}`}
                    className="text-lg font-semibold text-text hover:text-primary transition-colors truncate block"
                  >
                    {item.title}
                  </Link>
                  <p className="text-primary font-bold">
                    {item.listingType === 0 ? t('items.free') : `${item.price.toFixed(2)} лв.`}
                  </p>
                </div>
                <button
                  onClick={() => removeItem(item.id)}
                  className="p-2 text-red-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                  title={t('common.delete')}
                >
                  <HiTrash className="h-5 w-5" />
                </button>
              </div>
            ))}
          </div>

          <div className="mt-6 bg-white rounded-xl shadow-sm border border-lavender/30 p-6">
            <div className="flex justify-between items-center mb-4">
              <span className="text-lg font-medium text-gray-700">{t('cart.total')}</span>
              <span className="text-2xl font-bold text-primary">
                {total.toFixed(2)} лв.
              </span>
            </div>
            <div className="flex gap-3">
              <button
                onClick={clearCart}
                className="px-4 py-2 text-sm font-medium text-red-500 border border-red-300 rounded-lg hover:bg-red-50 transition-colors"
              >
                {t('cart.clear')}
              </button>
              <Link
                to="/checkout"
                className="flex-1 text-center px-4 py-2 text-sm font-medium bg-primary text-white rounded-lg hover:bg-primary-dark transition-colors"
              >
                {t('cart.proceed_checkout')}
              </Link>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
