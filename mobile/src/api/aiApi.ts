import axiosClient from './axiosClient';
import type { AiListingSuggestion, PriceSuggestion, PriceSuggestionRequest } from '@mamvibe/shared';

export const aiApi = {
  suggestListing: (photoUri: string, mimeType = 'image/jpeg') => {
    const formData = new FormData();
    formData.append('photo', {
      uri: photoUri,
      type: mimeType,
      name: 'photo.jpg',
    } as any);
    return axiosClient.post<AiListingSuggestion>('/items/ai-suggest', formData);
  },

  suggestPrice: (request: PriceSuggestionRequest) =>
    axiosClient.post<PriceSuggestion>('/items/suggest-price', request),
};
