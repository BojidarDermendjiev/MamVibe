import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { useAuth } from './useAuth'
import { authApi } from '../api/authApi'
import { useAuthStore } from '../store/authStore'
import type { User } from '../types/auth'

vi.mock('../api/authApi', () => ({
  authApi: { refresh: vi.fn() },
}))

vi.mock('../store/authStore', () => ({
  useAuthStore: vi.fn(),
}))

const mockUser: User = {
  id: '1',
  email: 'test@example.com',
  displayName: 'Test',
  profileType: 0,
  avatarUrl: null,
  bio: null,
  phoneNumber: null,
  languagePreference: 'en',
  roles: ['User'],
  isBlocked: false,
  iban: null,
}

const mockSetAuth = vi.fn()
const mockLogout = vi.fn()

beforeEach(() => {
  vi.clearAllMocks()
  vi.mocked(useAuthStore).mockReturnValue({
    setAuth: mockSetAuth,
    logout: mockLogout,
    isLoading: true,
  } as ReturnType<typeof useAuthStore>)
})

describe('useAuth', () => {
  it('calls setAuth when refresh succeeds', async () => {
    vi.mocked(authApi.refresh).mockResolvedValue({
      data: { user: mockUser, accessToken: 'token123' },
    } as Awaited<ReturnType<typeof authApi.refresh>>)

    renderHook(() => useAuth())

    await waitFor(() => expect(mockSetAuth).toHaveBeenCalledWith(mockUser, 'token123'))
  })

  it('calls logout when refresh fails', async () => {
    vi.mocked(authApi.refresh).mockRejectedValue(new Error('Unauthorized'))

    renderHook(() => useAuth())

    await waitFor(() => expect(mockLogout).toHaveBeenCalledOnce())
  })

  it('does not update state after unmount', async () => {
    let resolveRefresh!: (v: unknown) => void
    vi.mocked(authApi.refresh).mockReturnValue(
      new Promise((res) => { resolveRefresh = res as unknown as (v: unknown) => void }) as ReturnType<typeof authApi.refresh>
    )

    const { unmount } = renderHook(() => useAuth())
    unmount()

    resolveRefresh({ data: { user: mockUser, accessToken: 'tok' } })
    await new Promise((r) => setTimeout(r, 0))

    expect(mockSetAuth).not.toHaveBeenCalled()
  })

  it('returns isLoading from the store', () => {
    vi.mocked(authApi.refresh).mockReturnValue(new Promise(() => {}) as ReturnType<typeof authApi.refresh>)
    const { result } = renderHook(() => useAuth())
    expect(result.current.isLoading).toBe(true)
  })

  it('does not call logout after unmount when refresh rejects', async () => {
    let rejectRefresh!: (e: unknown) => void
    vi.mocked(authApi.refresh).mockReturnValue(
      new Promise<never>((_, rej) => { rejectRefresh = rej }) as ReturnType<typeof authApi.refresh>
    )
    const { unmount } = renderHook(() => useAuth())
    unmount()
    rejectRefresh(new Error('Unauthorized'))
    await new Promise((r) => setTimeout(r, 0))
    expect(mockLogout).not.toHaveBeenCalled()
  })
})
