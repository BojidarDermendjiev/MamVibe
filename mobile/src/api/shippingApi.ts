import axiosClient from './axiosClient';
import type {
  Shipment,
  TrackingEvent,
  CourierProvider,
  CourierOffice,
  ShippingPriceResult,
  CalculateShippingRequest,
} from '@mamvibe/shared';

export const shippingApi = {
  getMyShipments: () =>
    axiosClient.get<Shipment[]>('/shipping/my-shipments'),

  getShipmentByPayment: (paymentId: string) =>
    axiosClient.get<Shipment>(`/shipping/payment/${paymentId}`),

  trackShipment: (shipmentId: string) =>
    axiosClient.get<TrackingEvent[]>(`/shipping/${shipmentId}/track`),

  cancelShipment: (shipmentId: string) =>
    axiosClient.post(`/shipping/${shipmentId}/cancel`),

  getLabelUrl: (shipmentId: string) =>
    `${process.env.EXPO_PUBLIC_API_URL?.replace('/api', '')}/api/shipping/${shipmentId}/label`,

  getOffices: (provider: CourierProvider, city?: string) =>
    axiosClient.get<CourierOffice[]>('/shipping/offices', { params: { provider, city } }),

  calculatePrice: (request: CalculateShippingRequest) =>
    axiosClient.post<ShippingPriceResult>('/shipping/calculate', request),
};
