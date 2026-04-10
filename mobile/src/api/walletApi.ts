import axiosClient from './axiosClient';
import type {
  WalletDto,
  WalletTransactionDto,
  WalletTransferDto,
  WalletTopUpResultDto,
} from '@mamvibe/shared';
import type { PagedResult } from '@mamvibe/shared';

export const walletApi = {
  getWallet: () =>
    axiosClient.get<WalletDto>('/wallet'),

  getTransactions: (page = 1, pageSize = 20) =>
    axiosClient.get<PagedResult<WalletTransactionDto>>('/wallet/transactions', {
      params: { page, pageSize },
    }),

  createTopUp: (amount: number) =>
    axiosClient.post<WalletTopUpResultDto>('/wallet/topup', { amount }),

  transfer: (receiverEmail: string, amount: number, note?: string) =>
    axiosClient.post<WalletTransferDto>('/wallet/transfer', { receiverEmail, amount, note }),

  withdraw: (amount: number) =>
    axiosClient.post<WalletTransactionDto>('/wallet/withdraw', { amount }),

  payForItem: (itemId: string) =>
    axiosClient.post<WalletTransactionDto>(`/wallet/pay/${itemId}`),
};
