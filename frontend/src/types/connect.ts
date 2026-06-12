// Mirrors backend Domain.Enums.StripeConnectStatus.
export const StripeConnectStatus = {
  None: 0,
  Pending: 1,
  Verified: 2,
  Restricted: 3,
} as const;
export type StripeConnectStatus =
  (typeof StripeConnectStatus)[keyof typeof StripeConnectStatus];

export interface ConnectStatusDto {
  status: StripeConnectStatus;
  canReceivePayouts: boolean;
  hasAccount: boolean;
  statusUpdatedAt: string | null;
}

export interface ConnectOnboardingLinkDto {
  onboardingUrl: string;
}

export interface ConnectDashboardLinkDto {
  dashboardUrl: string;
}
