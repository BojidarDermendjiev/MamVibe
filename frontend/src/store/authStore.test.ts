import { beforeEach, describe, expect, it } from 'vitest'
import { useAuthStore } from './authStore'
import type { User } from '../types/auth'

const mockUser: User = {
  id: '1',
  email: 'test@example.com',
  displayName: 'Test User',
  profileType: 0,
  avatarUrl: null,
  bio: null,
  phoneNumber: null,
  languagePreference: 'en',
  roles: ['User'],
  isBlocked: false,
  iban: null,
  isOnHoliday: false,
}

const resetState = {
  user: null,
  accessToken: null,
  isAuthenticated: false,
  isLoading: true,
}

beforeEach(() => {
  localStorage.clear()
  useAuthStore.setState(resetState)
})

describe('useAuthStore', () => {
  it('has correct initial state', () => {
    const s = useAuthStore.getState()
    expect(s.user).toBeNull()
    expect(s.accessToken).toBeNull()
    expect(s.isAuthenticated).toBe(false)
    expect(s.isLoading).toBe(true)
  })

  it('setAuth stores user, token and marks authenticated', () => {
    useAuthStore.getState().setAuth(mockUser, 'token123')
    const s = useAuthStore.getState()
    expect(s.user).toEqual(mockUser)
    expect(s.accessToken).toBe('token123')
    expect(s.isAuthenticated).toBe(true)
    expect(s.isLoading).toBe(false)
  })

  it('setUser updates user without touching auth or token', () => {
    useAuthStore.setState({ ...resetState, isAuthenticated: true, accessToken: 'tok' })
    const updated = { ...mockUser, displayName: 'Updated' }
    useAuthStore.getState().setUser(updated)
    const s = useAuthStore.getState()
    expect(s.user?.displayName).toBe('Updated')
    expect(s.isAuthenticated).toBe(true)
    expect(s.accessToken).toBe('tok')
  })

  it('logout clears all auth state', () => {
    useAuthStore.setState({ user: mockUser, accessToken: 'tok', isAuthenticated: true, isLoading: false })
    useAuthStore.getState().logout()
    const s = useAuthStore.getState()
    expect(s.user).toBeNull()
    expect(s.accessToken).toBeNull()
    expect(s.isAuthenticated).toBe(false)
    expect(s.isLoading).toBe(false)
  })

  it('setLoading toggles isLoading', () => {
    useAuthStore.getState().setLoading(false)
    expect(useAuthStore.getState().isLoading).toBe(false)
    useAuthStore.getState().setLoading(true)
    expect(useAuthStore.getState().isLoading).toBe(true)
  })

  it('persists user to localStorage but not accessToken', () => {
    useAuthStore.getState().setAuth(mockUser, 'secret-token')
    const stored = JSON.parse(localStorage.getItem('mamvibe-auth') ?? '{}')
    expect(stored.state.user).toEqual(mockUser)
    expect(stored.state.accessToken).toBeUndefined()
  })

  it('user with Admin role can be stored', () => {
    const adminUser = { ...mockUser, roles: ['User', 'Admin'] }
    useAuthStore.getState().setAuth(adminUser, 'admin-tok')
    expect(useAuthStore.getState().user?.roles).toContain('Admin')
  })

  it('sets isAuthenticated to true on rehydration when user is present in storage', async () => {
    localStorage.setItem('mamvibe-auth', JSON.stringify({ state: { user: mockUser }, version: 0 }))
    await useAuthStore.persist.rehydrate()
    expect(useAuthStore.getState().isAuthenticated).toBe(true)
  })
})
