import { describe, it, expect, vi, beforeEach } from 'vitest'
import axiosClient from './axiosClient'
import { photosApi } from './photosApi'

vi.mock('./axiosClient', () => ({
  default: { post: vi.fn(), delete: vi.fn() },
}))

const client = vi.mocked(axiosClient)

beforeEach(() => {
  client.post.mockClear()
  client.delete.mockClear()
})

describe('photosApi', () => {
  it('upload posts FormData with file to /photos/upload', () => {
    client.post.mockResolvedValue({ data: { url: '/uploads/photo.jpg' } } as never)
    const file = new File(['img'], 'photo.jpg', { type: 'image/jpeg' })
    photosApi.upload(file)
    expect(client.post).toHaveBeenCalledOnce()
    const [url, body] = client.post.mock.calls[0]
    expect(url).toBe('/photos/upload')
    expect(body).toBeInstanceOf(FormData)
    expect((body as FormData).get('file')).toBe(file)
  })

  it('delete calls DELETE /photos with url param', () => {
    client.delete.mockResolvedValue({} as never)
    photosApi.delete('/uploads/photo.jpg')
    expect(client.delete).toHaveBeenCalledWith('/photos', { params: { url: '/uploads/photo.jpg' } })
  })
})
