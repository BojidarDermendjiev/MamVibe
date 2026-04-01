import axiosClient from './axiosClient';
import type { AiListingSuggestion, PriceSuggestion, PriceSuggestionRequest } from '../types/item';

export const aiApi = {
  suggestListing: (photo: File) => {
    const formData = new FormData();
    formData.append('photo', photo);
    return axiosClient.post<AiListingSuggestion>('/items/ai-suggest', formData);
  },

  suggestPrice: (request: PriceSuggestionRequest) =>
    axiosClient.post<PriceSuggestion>('/items/suggest-price', request),
};
