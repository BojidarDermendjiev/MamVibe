import { useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import Input from '../common/Input';
import Button from '../common/Button';
import PaymentCard from './PaymentCard';

interface PaymentCardFormProps {
  onSubmit: () => void;
  isLoading?: boolean;
}

function formatCardNumber(value: string): string {
  const digits = value.replace(/\D/g, '').slice(0, 16);
  return digits.replace(/(.{4})/g, '$1 ').trim();
}

function formatExpiration(value: string): string {
  const digits = value.replace(/\D/g, '').slice(0, 4);
  if (digits.length >= 3) {
    return digits.slice(0, 2) + '/' + digits.slice(2);
  }
  return digits;
}

export default function PaymentCardForm({ onSubmit, isLoading = false }: PaymentCardFormProps) {
  const { t } = useTranslation();
  const [name, setName] = useState('');
  const [cardNumber, setCardNumber] = useState('');
  const [expiration, setExpiration] = useState('');
  const [securityCode, setSecurityCode] = useState('');
  const [isFlipped, setIsFlipped] = useState(false);

  const handleCardNumberChange = (value: string) => {
    setCardNumber(formatCardNumber(value));
  };

  const handleExpirationChange = (value: string) => {
    setExpiration(formatExpiration(value));
  };

  const handleSecurityCodeChange = (value: string) => {
    setSecurityCode(value.replace(/\D/g, '').slice(0, 4));
  };

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    onSubmit();
  };

  return (
    <div className="space-y-6">
      <PaymentCard
        name={name}
        cardNumber={cardNumber}
        expiration={expiration}
        securityCode={securityCode}
        isFlipped={isFlipped}
      />

      <form onSubmit={handleSubmit} className="space-y-4">
        <Input
          label={t('card.card_number')}
          placeholder="0000 0000 0000 0000"
          value={cardNumber}
          onChange={(e) => handleCardNumberChange(e.target.value)}
          maxLength={19}
          inputMode="numeric"
        />

        <Input
          label={t('card.name')}
          placeholder={t('card.cardholder_name')}
          value={name}
          onChange={(e) => setName(e.target.value)}
          maxLength={26}
        />

        <div className="grid grid-cols-2 gap-4">
          <Input
            label={t('card.expiration')}
            placeholder="MM/YY"
            value={expiration}
            onChange={(e) => handleExpirationChange(e.target.value)}
            maxLength={5}
            inputMode="numeric"
          />

          <Input
            label={t('card.cvv')}
            placeholder="***"
            value={securityCode}
            onChange={(e) => handleSecurityCodeChange(e.target.value)}
            onFocus={() => setIsFlipped(true)}
            onBlur={() => setIsFlipped(false)}
            maxLength={4}
            inputMode="numeric"
            type="password"
          />
        </div>

        <Button type="submit" fullWidth size="lg" isLoading={isLoading}>
          {t('card.pay_now')}
        </Button>
      </form>
    </div>
  );
}
