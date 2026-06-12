import { useMemo, useState, type FormEvent } from 'react';
import { loadStripe, type Stripe, type StripeCardNumberElementOptions } from '@stripe/stripe-js';
import {
  Elements,
  CardNumberElement,
  CardExpiryElement,
  CardCvcElement,
  useStripe,
  useElements,
} from '@stripe/react-stripe-js';
import { useTranslation } from 'react-i18next';
import Button from '../common/Button';
import PaymentCard from './PaymentCard';
import { useTheme } from '../../contexts/ThemeContext';

const publishableKey = import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY as string | undefined;

// Stripe.js must be loaded once per app lifetime — `loadStripe` caches internally
// but holding the Promise at module scope guarantees a single load even across
// remounts of <Elements>, which avoids the "Please use the same instance" warning.
const stripePromise: Promise<Stripe | null> = publishableKey
  ? loadStripe(publishableKey)
  : Promise.resolve(null);

interface StripeCardFormProps {
  clientSecret: string;
  onSuccess: () => void;
  onCancel: () => void;
  submitLabel?: string;
}

export default function StripeCardForm({
  clientSecret,
  onSuccess,
  onCancel,
  submitLabel,
}: StripeCardFormProps) {
  const { theme } = useTheme();

  if (!publishableKey) {
    return (
      <div className="rounded-xl border border-red-200 bg-red-50 p-4 text-sm text-red-700">
        Stripe publishable key is not configured. Set VITE_STRIPE_PUBLISHABLE_KEY in your .env.local.
      </div>
    );
  }

  // The clientSecret in options is required by Elements when using individual
  // Card* elements with a PaymentIntent; appearance is intentionally minimal —
  // each element gets its own iframe `style`, set below in InnerForm.
  return (
    <Elements
      stripe={stripePromise}
      options={{
        clientSecret,
        appearance: { theme: theme === 'dark' ? 'night' : 'stripe' },
      }}
    >
      <InnerForm
        clientSecret={clientSecret}
        onSuccess={onSuccess}
        onCancel={onCancel}
        submitLabel={submitLabel}
      />
    </Elements>
  );
}

interface InnerFormProps {
  clientSecret: string;
  onSuccess: () => void;
  onCancel: () => void;
  submitLabel?: string;
}

function InnerForm({ clientSecret, onSuccess, onCancel, submitLabel }: InnerFormProps) {
  const { t } = useTranslation();
  const { theme } = useTheme();
  const stripe = useStripe();
  const elements = useElements();

  // The only field we control is the cardholder name — the others live inside
  // Stripe's iframes (PCI scope stays out of our DOM). isFlipped flips the
  // card preview when the CVV field is focused, exactly like the old design.
  const [name, setName] = useState('');
  const [isFlipped, setIsFlipped] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Stripe element style — typography only; the visual box (border, bg, padding)
  // is the wrapping <div className="stripe-input-wrap"> which Tailwind controls.
  const elementStyle: StripeCardNumberElementOptions['style'] = useMemo(() => ({
    base: {
      fontSize: '16px',
      fontFamily: 'system-ui, -apple-system, sans-serif',
      color: theme === 'dark' ? '#f3f4f6' : '#1f2937',
      '::placeholder': { color: theme === 'dark' ? '#6b7280' : '#9ca3af' },
      iconColor: theme === 'dark' ? '#9ca3af' : '#6b7280',
    },
    invalid: { color: '#ef4444', iconColor: '#ef4444' },
  }), [theme]);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!stripe || !elements) return;

    const cardNumber = elements.getElement(CardNumberElement);
    if (!cardNumber) return;

    setSubmitting(true);
    setError(null);

    // confirmCardPayment handles 3DS internally when the issuer requires it —
    // Stripe.js shows the challenge modal and resolves once authentication
    // completes. We don't need to handle that branch explicitly here.
    const { error: confirmError, paymentIntent } = await stripe.confirmCardPayment(
      clientSecret,
      {
        payment_method: {
          card: cardNumber,
          billing_details: { name: name.trim() || undefined },
        },
      },
    );

    if (confirmError) {
      setError(confirmError.message ?? t('common.error'));
      setSubmitting(false);
      return;
    }

    if (paymentIntent?.status === 'succeeded' || paymentIntent?.status === 'processing') {
      onSuccess();
      return;
    }

    setSubmitting(false);
  };

  // Shared wrapper classes for each Stripe element so they look like the app's
  // own inputs (rose-tinted border, focus ring, dark-mode aware).
  const inputWrap =
    'w-full rounded-xl border border-lavender/40 bg-white dark:bg-gray-800 dark:border-gray-700 px-4 py-3 transition-shadow focus-within:border-primary focus-within:ring-2 focus-within:ring-primary/20';

  return (
    <form onSubmit={handleSubmit} className="space-y-10">
      <PaymentCard
        name={name}
        cardNumber=""
        expiration=""
        securityCode=""
        isFlipped={isFlipped}
      />

      <div className="space-y-4 pt-4">
        <div>
          <label className="block text-sm font-medium text-primary dark:text-gray-200 mb-1">
            {t('card.card_number')}
          </label>
          <div className={inputWrap}>
            <CardNumberElement
              options={{
                style: elementStyle,
                showIcon: true,
                placeholder: '0000 0000 0000 0000',
              }}
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-primary dark:text-gray-200 mb-1">
            {t('card.name')}
          </label>
          <input
            type="text"
            value={name}
            onChange={(e) => setName(e.target.value.slice(0, 26))}
            placeholder={t('card.cardholder_name')}
            autoComplete="cc-name"
            className="w-full rounded-xl border border-lavender/40 bg-white dark:bg-gray-800 dark:border-gray-700 px-4 py-3 text-gray-900 dark:text-gray-100 placeholder:text-gray-400 dark:placeholder:text-gray-500 focus:outline-none focus:border-primary focus:ring-2 focus:ring-primary/20"
          />
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-primary dark:text-gray-200 mb-1">
              {t('card.expiration')}
            </label>
            <div className={inputWrap}>
              <CardExpiryElement options={{ style: elementStyle, placeholder: 'MM/YY' }} />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-primary dark:text-gray-200 mb-1">
              {t('card.cvv')}
            </label>
            <div className={inputWrap}>
              <CardCvcElement
                options={{ style: elementStyle, placeholder: '***' }}
                onFocus={() => setIsFlipped(true)}
                onBlur={() => setIsFlipped(false)}
              />
            </div>
          </div>
        </div>

        {error && (
          <div className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">
            {error}
          </div>
        )}

        <div className="flex flex-col-reverse gap-2 sm:flex-row pt-2">
          <Button
            type="button"
            variant="secondary"
            fullWidth
            onClick={onCancel}
            disabled={submitting}
          >
            {t('common.cancel')}
          </Button>
          <Button
            type="submit"
            fullWidth
            size="lg"
            isLoading={submitting}
            disabled={!stripe || !elements}
          >
            {submitLabel ?? t('card.pay_now')}
          </Button>
        </div>
      </div>
    </form>
  );
}
