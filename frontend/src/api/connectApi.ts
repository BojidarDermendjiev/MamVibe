import axiosClient from "./axiosClient";
import type {
  ConnectStatusDto,
  ConnectOnboardingLinkDto,
  ConnectDashboardLinkDto,
} from "../types/connect";

/**
 * Stripe Connect Express onboarding API. The `status` endpoint serves the
 * cached local snapshot by default; pass `refresh=true` to force a
 * Stripe round-trip (used right after the user returns from onboarding).
 */
export const connectApi = {
  getStatus: (refresh = false) =>
    axiosClient.get<ConnectStatusDto>(`/connect/status${refresh ? "?refresh=true" : ""}`),

  startOnboarding: () =>
    axiosClient.post<ConnectOnboardingLinkDto>("/connect/onboard"),

  getDashboardLink: () =>
    axiosClient.post<ConnectDashboardLinkDto>("/connect/dashboard-link"),
};
