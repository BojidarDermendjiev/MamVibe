import axiosClient from './axiosClient';

export const photosApi = {
  upload: (uri: string, mime = 'image/jpeg') => {
    const formData = new FormData();
    formData.append('file', { uri, type: mime, name: 'photo.jpg' } as any);
    return axiosClient.post<{ url: string }>('/photos/upload', formData);
  },
};
