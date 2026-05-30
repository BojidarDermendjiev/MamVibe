// Bulgaria's fixed EUR/BGN peg (since 1999)
const EUR_RATE = 1.95583;

export function bgnToEur(amountBGN: number): number {
  return amountBGN / EUR_RATE;
}

export function formatPrice(amountBGN: number | null | undefined): string {
  if (amountBGN == null) return '';
  return `€${bgnToEur(amountBGN).toFixed(2)}`;
}

export function formatEur(amount: number | null | undefined): string {
  if (amount == null) return '€0.00';
  return `€${amount.toFixed(2)}`;
}
