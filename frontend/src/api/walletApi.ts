import axiosClient from './axiosClient';
import type {
  WalletDto,
  WalletTransactionDto,
  WalletTransferDto,
  WalletTopUpResultDto,
  AdminWalletDto,
  WalletStatus,
  WalletTransactionKind,
  WalletTransactionStatus,
  WalletTransactionType,
} from '../types/wallet';
import type { PagedResult } from '../types/item';

export const walletApi = {
  // User endpoints
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

  confirmDelivery: (paymentId: string) =>
    axiosClient.post<WalletTransactionDto>(`/wallet/confirm-delivery/${paymentId}`),

  // Admin endpoints
  admin: {
    getWallets: (page = 1, pageSize = 20, status?: WalletStatus) =>
      axiosClient.get<PagedResult<AdminWalletDto>>('/admin/wallets', {
        params: { page, pageSize, status },
      }),

    getWalletById: (id: string) =>
      axiosClient.get<AdminWalletDto>(`/admin/wallets/${id}`),

    freezeWallet: (id: string, reason: string) =>
      axiosClient.put(`/admin/wallets/${id}/freeze`, { reason }),

    unfreezeWallet: (id: string) =>
      axiosClient.put(`/admin/wallets/${id}/unfreeze`),

    getTransactions: (params: {
      userId?: string;
      walletId?: string;
      dateFrom?: string;
      dateTo?: string;
      kind?: WalletTransactionKind;
      status?: WalletTransactionStatus;
      type?: WalletTransactionType;
      minAmount?: number;
      maxAmount?: number;
      page?: number;
      pageSize?: number;
    }) =>
      axiosClient.get<PagedResult<WalletTransactionDto>>('/admin/wallet-transactions', {
        params,
      }),

    refundTransaction: (id: string, reason: string) =>
      axiosClient.post<WalletTransactionDto>(`/admin/wallet-transactions/${id}/refund`, { reason }),

    getPendingWithdrawals: (page = 1, pageSize = 20) =>
      axiosClient.get<PagedResult<WalletTransactionDto>>('/admin/withdrawals', {
        params: { page, pageSize },
      }),

    approveWithdrawal: (id: string) =>
      axiosClient.post(`/admin/withdrawals/${id}/approve`),

    rejectWithdrawal: (id: string, reason: string) =>
      axiosClient.post(`/admin/withdrawals/${id}/reject`, { reason }),
  },
};
