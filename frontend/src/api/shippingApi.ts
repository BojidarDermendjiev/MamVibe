import axiosClient from './axiosClient';
import type {
  Shipment,
  CourierOffice,
  ShippingPriceResult,
  TrackingEvent,
  CalculateShippingRequest,
  CreateShipmentRequest,
  CourierProvider,
} from '../types/shipping';

export const shippingApi = {
  calculatePrice: (request: CalculateShippingRequest) =>
    axiosClient.post<ShippingPriceResult>('/shipping/calculate', request),

  createShipment: (request: CreateShipmentRequest) =>
    axiosClient.post<Shipment>('/shipping/create', request),

  getLabel: (shipmentId: string) =>
    axiosClient.get<Blob>(`/shipping/${shipmentId}/label`, { responseType: 'blob' }),

  trackShipment: (shipmentId: string) =>
    axiosClient.get<TrackingEvent[]>(`/shipping/${shipmentId}/track`),

  cancelShipment: (shipmentId: string) =>
    axiosClient.post(`/shipping/${shipmentId}/cancel`),

  getOffices: (provider: CourierProvider, city?: string) =>
    axiosClient.get<CourierOffice[]>('/shipping/offices', { params: { provider, city } }),

  getShipmentByPayment: (paymentId: string) =>
    axiosClient.get<Shipment>(`/shipping/payment/${paymentId}`),

  getMyShipments: () =>
    axiosClient.get<Shipment[]>('/shipping/my-shipments'),
};
