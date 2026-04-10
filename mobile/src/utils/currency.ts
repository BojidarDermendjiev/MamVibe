const EUR_RATE = 1.95583;

export function formatPrice(amountBGN: number | null | undefined): string {
  if (amountBGN == null) return '';
  const eur = amountBGN / EUR_RATE;
  return `€${eur.toFixed(2)} (${amountBGN.toFixed(2)} лв)`;
}

export function formatEur(amount: number | null | undefined): string {
  if (amount == null) return '€0.00';
  return `€${amount.toFixed(2)}`;
}
