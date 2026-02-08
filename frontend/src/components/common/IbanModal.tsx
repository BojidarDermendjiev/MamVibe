import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import axiosClient from '../../api/axiosClient';
import { useAuthStore } from '../../store/authStore';
import Modal from './Modal';
import Input from './Input';
import Button from './Button';

interface IbanModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSaved: () => void;
}

const IBAN_REGEX = /^[A-Z]{2}[A-Z0-9]{13,32}$/;

export default function IbanModal({ isOpen, onClose, onSaved }: IbanModalProps) {
  const { t } = useTranslation();
  const { setUser } = useAuthStore();
  const [iban, setIban] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const validate = (value: string) => {
    const cleaned = value.replace(/\s/g, '').toUpperCase();
    if (!cleaned) return false;
    if (cleaned.length < 15 || cleaned.length > 34) return false;
    return IBAN_REGEX.test(cleaned);
  };

  const handleSave = async () => {
    const cleaned = iban.replace(/\s/g, '').toUpperCase();
    if (!validate(cleaned)) {
      setError(t('payment.iban_invalid', 'Invalid IBAN format'));
      return;
    }
    setLoading(true);
    try {
      const { data } = await axiosClient.put('/users/profile', { iban: cleaned });
      setUser(data);
      toast.success(t('profile.save'));
      onSaved();
    } catch {
      toast.error(t('common.error'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={t('payment.iban_title')}>
      <div className="space-y-4">
        <p className="text-sm text-gray-600">{t('payment.iban_desc')}</p>
        <Input
          label={t('payment.iban_label')}
          value={iban}
          onChange={(e) => {
            setIban(e.target.value.toUpperCase());
            setError('');
          }}
          error={error}
          placeholder="BG80BNBG96611020345678"
        />
        <Button fullWidth isLoading={loading} onClick={handleSave}>
          {t('payment.iban_save')}
        </Button>
      </div>
    </Modal>
  );
}
