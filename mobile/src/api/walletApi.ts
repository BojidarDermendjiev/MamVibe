import axiosClient from './axiosClient';

export interface Wallet {
  balance: number;
}

export const walletApi = {
  getWallet: () =>
    axiosClient.get<Wallet>('/wallet'),

  payForItem: (itemId: string) =>
    axiosClient.post(`/wallet/pay/${itemId}`),
};
