import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import IbanModal from './IbanModal'

vi.mock('../../api/axiosClient', () => ({
  default: { put: vi.fn() },
}))
vi.mock('../../store/authStore', () => ({
  useAuthStore: () => ({ setUser: vi.fn() }),
}))
vi.mock('@/utils/toast', () => ({
  default: { success: vi.fn(), error: vi.fn() },
}))

import axiosClient from '../../api/axiosClient'
const mockPut = vi.mocked(axiosClient.put)

const baseProps = {
  isOpen: true,
  onClose: vi.fn(),
  onSaved: vi.fn(),
}

beforeEach(() => {
  mockPut.mockClear()
  baseProps.onClose.mockClear()
  baseProps.onSaved.mockClear()
})

describe('IbanModal', () => {
  it('renders when isOpen is true', () => {
    render(<IbanModal {...baseProps} />)
    expect(screen.getByText('payment.iban_title')).toBeInTheDocument()
  })

  it('renders nothing when isOpen is false', () => {
    const { container } = render(<IbanModal {...baseProps} isOpen={false} />)
    expect(container.querySelector('[role="dialog"]')).toBeNull()
  })

  it('shows validation error for invalid IBAN', async () => {
    render(<IbanModal {...baseProps} />)
    await userEvent.type(screen.getByPlaceholderText('BG80BNBG96611020345678'), 'INVALID')
    await userEvent.click(screen.getByText('payment.iban_save'))
    expect(screen.getByText('payment.iban_invalid')).toBeInTheDocument()
    expect(mockPut).not.toHaveBeenCalled()
  })

  it('calls API with cleaned IBAN on valid input', async () => {
    mockPut.mockResolvedValue({ data: { iban: 'BG80BNBG96611020345678' } } as never)
    render(<IbanModal {...baseProps} />)
    await userEvent.type(screen.getByPlaceholderText('BG80BNBG96611020345678'), 'BG80BNBG96611020345678')
    await userEvent.click(screen.getByText('payment.iban_save'))
    await waitFor(() => expect(mockPut).toHaveBeenCalledWith('/users/profile', { iban: 'BG80BNBG96611020345678' }))
  })

  it('calls onSaved after successful save', async () => {
    mockPut.mockResolvedValue({ data: {} } as never)
    render(<IbanModal {...baseProps} />)
    await userEvent.type(screen.getByPlaceholderText('BG80BNBG96611020345678'), 'BG80BNBG96611020345678')
    await userEvent.click(screen.getByText('payment.iban_save'))
    await waitFor(() => expect(baseProps.onSaved).toHaveBeenCalledOnce())
  })

  it('converts input to uppercase', async () => {
    render(<IbanModal {...baseProps} />)
    const input = screen.getByPlaceholderText('BG80BNBG96611020345678')
    await userEvent.type(input, 'bg80bnbg96611020345678')
    expect((input as HTMLInputElement).value).toBe('BG80BNBG96611020345678')
  })

  it('shows error toast when API call fails', async () => {
    const toast = await import('@/utils/toast')
    mockPut.mockRejectedValue(new Error('Network error'))
    render(<IbanModal {...baseProps} />)
    await userEvent.type(screen.getByPlaceholderText('BG80BNBG96611020345678'), 'BG80BNBG96611020345678')
    await userEvent.click(screen.getByText('payment.iban_save'))
    await waitFor(() => expect(vi.mocked(toast.default.error)).toHaveBeenCalledWith('common.error'))
  })
})
