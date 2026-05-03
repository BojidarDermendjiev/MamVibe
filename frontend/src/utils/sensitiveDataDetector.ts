export type SensitiveDataType = 'phone' | 'email' | 'national-id' | 'iban' | 'card';

export interface SensitiveMatch {
  type: SensitiveDataType;
  labelKey: string;
}

const PATTERNS: { type: SensitiveDataType; labelKey: string; regex: RegExp }[] = [
  {
    type: 'phone',
    labelKey: 'privacy.type_phone',
    // Bulgarian mobile (087x / 088x / 089x / 098x / 099x) with optional spaces/dashes,
    // plus international +359 prefix variants.
    regex: /(\+?359[\s-]?|0)[89]\d[\s-]?\d{3}[\s-]?\d{3}/,
  },
  {
    type: 'email',
    labelKey: 'privacy.type_email',
    regex: /[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}/,
  },
  {
    type: 'national-id',
    labelKey: 'privacy.type_national_id',
    // Bulgarian EGN: 10 digits where chars 3-4 encode a valid month group.
    // Month ranges: 01-12 (born 1900-1999), 21-32 (born 2000-2099), 41-52 (born 1800-1899).
    // This avoids matching phone numbers (e.g. 0878… whose "month" digits fall outside ranges).
    regex: /\b\d{2}(0[1-9]|1[0-2]|2[1-9]|3[0-2]|4[1-9]|5[0-2])\d{6}\b/,
  },
  {
    type: 'iban',
    labelKey: 'privacy.type_iban',
    // Standard IBAN structure: 2-letter country code + 2 check digits + 4-letter bank code + digits.
    regex: /\b[A-Z]{2}\d{2}[A-Z]{4}\d{10,14}\b/,
  },
  {
    type: 'card',
    labelKey: 'privacy.type_card',
    // 16-digit card number, optionally formatted as 4 groups of 4.
    regex: /\b\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}\b/,
  },
];

export function detectSensitiveData(text: string): SensitiveMatch[] {
  return PATTERNS
    .filter(({ regex }) => regex.test(text))
    .map(({ type, labelKey }) => ({ type, labelKey }));
}
