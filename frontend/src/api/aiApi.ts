import axiosClient from './axiosClient';
import type { AiListingSuggestion } from '../types/item';

export const aiApi = {
  suggestListing: (photo: File) => {
    const formData = new FormData();
    formData.append('photo', photo);
    return axiosClient.post<AiListingSuggestion>('/items/ai-suggest', formData);
  },
};
