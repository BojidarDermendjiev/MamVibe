import { describe, it, expect, vi, beforeEach } from 'vitest'
import { authApi } from './authApi'
import axiosClient from './axiosClient'

vi.mock('./axiosClient', () => ({
  default: { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() },
}))

const client = vi.mocked(axiosClient)

beforeEach(() => {
  client.get.mockClear()
  client.post.mockClear()
})

describe('authApi', () => {
  it('login posts to /auth/login', () => {
    authApi.login({ email: 'a@b.com', password: 'pw' })
    expect(client.post).toHaveBeenCalledWith('/auth/login', { email: 'a@b.com', password: 'pw' })
  })

  it('register posts to /auth/register', () => {
    authApi.register({ email: 'a@b.com', password: 'pw', confirmPassword: 'pw', displayName: 'A', profileType: 0 })
    expect(client.post).toHaveBeenCalledWith('/auth/register', expect.objectContaining({ email: 'a@b.com' }))
  })

  it('refresh posts to /auth/refresh', () => {
    authApi.refresh()
    expect(client.post).toHaveBeenCalledWith('/auth/refresh', null)
  })

  it('revoke posts to /auth/revoke', () => {
    authApi.revoke()
    expect(client.post).toHaveBeenCalledWith('/auth/revoke')
  })

  it('me gets /auth/me', () => {
    authApi.me()
    expect(client.get).toHaveBeenCalledWith('/auth/me')
  })

  it('forgotPassword posts to /auth/forgot-password', () => {
    authApi.forgotPassword({ email: 'a@b.com' })
    expect(client.post).toHaveBeenCalledWith('/auth/forgot-password', { email: 'a@b.com' })
  })

  it('changePassword posts to /auth/change-password', () => {
    authApi.changePassword({ currentPassword: 'old', newPassword: 'new', confirmNewPassword: 'new' })
    expect(client.post).toHaveBeenCalledWith('/auth/change-password', expect.any(Object))
  })
})
