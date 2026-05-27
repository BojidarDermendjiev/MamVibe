import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter, Routes, Route } from 'react-router-dom'
import ProtectedRoute from './ProtectedRoute'
import { useAuthStore } from '../../store/authStore'
import type { User } from '../../types/auth'

vi.mock('../../store/authStore')

const mockedUseAuthStore = vi.mocked(useAuthStore)

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
  isOnHoliday: false,
}

function setup(authState: { isAuthenticated: boolean; isLoading: boolean; user: User | null }, requiredRole?: string) {
  mockedUseAuthStore.mockReturnValue(authState as ReturnType<typeof useAuthStore>)
  return render(
    <MemoryRouter initialEntries={['/protected']}>
      <Routes>
        <Route element={<ProtectedRoute requiredRole={requiredRole} />}>
          <Route path="/protected" element={<div>Protected Content</div>} />
        </Route>
        <Route path="/login" element={<div>Login Page</div>} />
        <Route path="/" element={<div>Home Page</div>} />
      </Routes>
    </MemoryRouter>
  )
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('ProtectedRoute', () => {
  it('shows spinner while loading', () => {
    setup({ isAuthenticated: false, isLoading: true, user: null })
    // LoadingSpinner renders a div with animate-spin, not an svg
    expect(document.querySelector('.animate-spin')).toBeInTheDocument()
    expect(screen.queryByText('Protected Content')).toBeNull()
  })

  it('redirects to /login when not authenticated', () => {
    setup({ isAuthenticated: false, isLoading: false, user: null })
    expect(screen.getByText('Login Page')).toBeInTheDocument()
    expect(screen.queryByText('Protected Content')).toBeNull()
  })

  it('renders protected content when authenticated', () => {
    setup({ isAuthenticated: true, isLoading: false, user: mockUser })
    expect(screen.getByText('Protected Content')).toBeInTheDocument()
  })

  it('redirects to / when user lacks required role', () => {
    setup({ isAuthenticated: true, isLoading: false, user: mockUser }, 'Admin')
    expect(screen.getByText('Home Page')).toBeInTheDocument()
    expect(screen.queryByText('Protected Content')).toBeNull()
  })

  it('renders content when user has required role', () => {
    const adminUser = { ...mockUser, roles: ['User', 'Admin'] }
    setup({ isAuthenticated: true, isLoading: false, user: adminUser }, 'Admin')
    expect(screen.getByText('Protected Content')).toBeInTheDocument()
  })

  it('renders content with no requiredRole check when not specified', () => {
    setup({ isAuthenticated: true, isLoading: false, user: mockUser })
    expect(screen.getByText('Protected Content')).toBeInTheDocument()
  })
})
