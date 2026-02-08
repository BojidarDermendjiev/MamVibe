import axiosClient from './axiosClient';

export const photosApi = {
  upload: (file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    return axiosClient.post<{ url: string }>('/photos/upload', formData);
  },

  delete: (url: string) =>
    axiosClient.delete('/photos', { params: { url } }),
};
