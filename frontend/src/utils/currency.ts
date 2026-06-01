// All monetary values across the platform are now denominated in EUR.
// Historical BGN values were one-shot converted via the BGN/EUR fixed peg (1.95583)
// in migration 20260531180113_ConvertPricesBgnToEur. New listings, bundles, offers,
// and Stripe charges are EUR end-to-end.

export function formatPrice(amount: number | null | undefined): string {
  if (amount == null) return '';
  return `€${amount.toFixed(2)}`;
}

export function formatEur(amount: number | null | undefined): string {
  if (amount == null) return '€0.00';
  return `€${amount.toFixed(2)}`;
}
