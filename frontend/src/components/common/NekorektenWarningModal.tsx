import { HiExternalLink, HiExclamation } from 'react-icons/hi';
import { useTranslation } from 'react-i18next';
import Modal from './Modal';

interface NekorektenWarningModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  sellerName: string;
  reportUrl: string;
}

export default function NekorektenWarningModal({
  isOpen,
  onClose,
  onConfirm,
  sellerName,
  reportUrl,
}: NekorektenWarningModalProps) {
  const { t } = useTranslation();

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={t('nekorekten.warning_title')}>
      <div className="space-y-4">
        {/* Warning icon + header */}
        <div className="flex items-center gap-3 bg-red-50 border border-red-200 rounded-lg p-4">
          <span className="flex-shrink-0 flex items-center justify-center w-10 h-10 rounded-full bg-red-100">
            <HiExclamation className="h-6 w-6 text-red-600" />
          </span>
          <div>
            <p className="font-semibold text-red-700 text-sm">{t('nekorekten.seller_reported')}</p>
            <p className="text-red-600 text-sm font-medium">{sellerName}</p>
          </div>
        </div>

        {/* Description */}
        <p className="text-sm text-gray-600 leading-relaxed">
          {t('nekorekten.warning_desc')}
        </p>

        {/* Link to nekorekten.com */}
        <a
          href={reportUrl}
          target="_blank"
          rel="noopener noreferrer"
          className="flex items-center gap-2 w-full justify-center py-2.5 px-4 rounded-lg border border-red-300 text-red-600 text-sm font-medium hover:bg-red-50 transition-colors"
        >
          <HiExternalLink className="h-4 w-4" />
          {t('nekorekten.check_profile')}
        </a>

        {/* Actions */}
        <div className="flex gap-3 pt-1">
          <button
            onClick={onClose}
            className="flex-1 py-2.5 px-4 rounded-lg border border-gray-300 text-gray-700 text-sm font-medium hover:bg-gray-50 transition-colors"
          >
            {t('common.cancel')}
          </button>
          <button
            onClick={onConfirm}
            className="flex-1 py-2.5 px-4 rounded-lg bg-red-600 text-white text-sm font-medium hover:bg-red-700 transition-colors"
          >
            {t('nekorekten.continue_anyway')}
          </button>
        </div>
      </div>
    </Modal>
  );
}
