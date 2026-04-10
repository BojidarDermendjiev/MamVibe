export const WalletStatus = {
  Active: 0,
  Frozen: 1,
  Suspended: 2,
  Closed: 3,
} as const;
export type WalletStatus = (typeof WalletStatus)[keyof typeof WalletStatus];

export const WalletTransactionType = {
  Credit: 0,
  Debit: 1,
} as const;
export type WalletTransactionType = (typeof WalletTransactionType)[keyof typeof WalletTransactionType];

export const WalletTransactionKind = {
  TopUp: 0,
  Transfer: 1,
  ItemPayment: 2,
  Withdrawal: 3,
  Refund: 4,
  Fee: 5,
} as const;
export type WalletTransactionKind = (typeof WalletTransactionKind)[keyof typeof WalletTransactionKind];

export const WalletTransactionStatus = {
  Pending: 0,
  Completed: 1,
  Failed: 2,
  Reversed: 3,
} as const;
export type WalletTransactionStatus = (typeof WalletTransactionStatus)[keyof typeof WalletTransactionStatus];

export interface WalletDto {
  id: string;
  userId: string;
  currency: string;
  status: WalletStatus;
  balance: number;
  createdAt: string;
}

export interface WalletTransactionDto {
  id: string;
  walletId: string;
  type: WalletTransactionType;
  kind: WalletTransactionKind;
  amount: number;
  balanceAfter: number;
  status: WalletTransactionStatus;
  reference: string | null;
  description: string | null;
  relatedTransactionId: string | null;
  paymentId: string | null;
  receiptUrl: string | null;
  createdAt: string;
}

export interface WalletTransferDto {
  id: string;
  senderWalletId: string;
  receiverWalletId: string;
  amount: number;
  currency: string;
  status: number;
  note: string | null;
  senderDisplayName: string;
  receiverDisplayName: string;
  createdAt: string;
}

export interface WalletTopUpResultDto {
  clientSecret: string;
  amount: number;
  currency: string;
}

export interface AdminWalletDto {
  id: string;
  userId: string;
  userEmail: string;
  userDisplayName: string;
  currency: string;
  status: WalletStatus;
  balance: number;
  transactionCount: number;
  createdAt: string;
}
