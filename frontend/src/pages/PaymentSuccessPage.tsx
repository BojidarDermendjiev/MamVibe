import { Link, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { HiCheckCircle } from 'react-icons/hi';
import Button from '../components/common/Button';

export default function PaymentSuccessPage() {
  const { t } = useTranslation();
  const [searchParams] = useSearchParams();
  // session_id starting with "test_" means Stripe is not configured — show a test-mode banner
  const isTestMode = (searchParams.get('session_id') ?? '').startsWith('test_');

  return (
    <div className="min-h-[60vh] flex flex-col items-center justify-center text-center px-4">
      {isTestMode && (
        <div className="mb-6 px-4 py-2 rounded-lg bg-amber-100 border border-amber-300 text-amber-800 text-sm font-medium">
          ⚠️ Test mode — no real payment was processed.
        </div>
      )}
      <HiCheckCircle className="h-20 w-20 text-green-500 mb-4" />
      <h1 className="text-3xl font-bold text-primary mb-2">{t('payment.success_title')}</h1>
      <p className="text-gray-500 mb-8 max-w-md">{t('payment.success_msg')}</p>
      <div className="flex gap-3">
        <Link to="/dashboard"><Button variant="secondary">{t('nav.dashboard')}</Button></Link>
        <Link to="/browse"><Button>{t('nav.browse')}</Button></Link>
      </div>
    </div>
  );
}
