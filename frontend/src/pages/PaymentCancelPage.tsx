import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { HiXCircle } from 'react-icons/hi';
import Button from '../components/common/Button';

export default function PaymentCancelPage() {
  const { t } = useTranslation();

  return (
    <div className="min-h-[60vh] flex flex-col items-center justify-center text-center px-4">
      <HiXCircle className="h-20 w-20 text-red-400 mb-4" />
      <h1 className="text-3xl font-bold text-primary mb-2">{t('payment.cancel_title')}</h1>
      <p className="text-gray-500 mb-8 max-w-md">{t('payment.cancel_msg')}</p>
      <div className="flex gap-3">
        <Link to="/browse"><Button>{t('nav.browse')}</Button></Link>
      </div>
    </div>
  );
}
