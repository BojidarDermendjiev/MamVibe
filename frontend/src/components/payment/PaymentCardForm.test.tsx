import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import PaymentCardForm from './PaymentCardForm'

vi.mock('./PaymentCard', () => ({
  default: () => <div data-testid="payment-card" />,
}))

describe('PaymentCardForm', () => {
  const onSubmit = vi.fn()

  beforeEach(() => {
    onSubmit.mockClear()
  })

  it('renders all four form fields', () => {
    render(<PaymentCardForm onSubmit={onSubmit} />)
    expect(screen.getByPlaceholderText('0000 0000 0000 0000')).toBeInTheDocument()
    expect(screen.getByPlaceholderText('card.cardholder_name')).toBeInTheDocument()
    expect(screen.getByPlaceholderText('MM/YY')).toBeInTheDocument()
    expect(screen.getByPlaceholderText('***')).toBeInTheDocument()
  })

  it('renders the payment card preview', () => {
    render(<PaymentCardForm onSubmit={onSubmit} />)
    expect(screen.getByTestId('payment-card')).toBeInTheDocument()
  })

  it('formats card number digits into groups of four', () => {
    render(<PaymentCardForm onSubmit={onSubmit} />)
    const input = screen.getByPlaceholderText('0000 0000 0000 0000')
    fireEvent.change(input, { target: { value: '12345678' } })
    expect(input).toHaveValue('1234 5678')
  })

  it('strips non-digit characters from the card number', () => {
    render(<PaymentCardForm onSubmit={onSubmit} />)
    const input = screen.getByPlaceholderText('0000 0000 0000 0000')
    fireEvent.change(input, { target: { value: 'abcd1234' } })
    expect(input).toHaveValue('1234')
  })

  it('limits card number to 16 digits', () => {
    render(<PaymentCardForm onSubmit={onSubmit} />)
    const input = screen.getByPlaceholderText('0000 0000 0000 0000')
    fireEvent.change(input, { target: { value: '12345678901234567890' } })
    expect(input).toHaveValue('1234 5678 9012 3456')
  })

  it('adds a slash after the second expiration digit', () => {
    render(<PaymentCardForm onSubmit={onSubmit} />)
    const input = screen.getByPlaceholderText('MM/YY')
    fireEvent.change(input, { target: { value: '1225' } })
    expect(input).toHaveValue('12/25')
  })

  it('does not add a slash for two or fewer expiration digits', () => {
    render(<PaymentCardForm onSubmit={onSubmit} />)
    const input = screen.getByPlaceholderText('MM/YY')
    fireEvent.change(input, { target: { value: '12' } })
    expect(input).toHaveValue('12')
  })

  it('strips non-digit characters from the security code', () => {
    render(<PaymentCardForm onSubmit={onSubmit} />)
    const input = screen.getByPlaceholderText('***')
    fireEvent.change(input, { target: { value: 'abc123' } })
    expect(input).toHaveValue('123')
  })

  it('limits security code to four digits', () => {
    render(<PaymentCardForm onSubmit={onSubmit} />)
    const input = screen.getByPlaceholderText('***')
    fireEvent.change(input, { target: { value: '123456' } })
    expect(input).toHaveValue('1234')
  })

  it('calls onSubmit when the form is submitted', async () => {
    render(<PaymentCardForm onSubmit={onSubmit} />)
    await userEvent.click(screen.getByRole('button', { name: 'card.pay_now' }))
    expect(onSubmit).toHaveBeenCalledOnce()
  })

  it('renders custom submitLabel when provided', () => {
    render(<PaymentCardForm onSubmit={onSubmit} submitLabel="Pay $99" />)
    expect(screen.getByText('Pay $99')).toBeInTheDocument()
  })

  it('falls back to card.pay_now translation when no submitLabel', () => {
    render(<PaymentCardForm onSubmit={onSubmit} />)
    expect(screen.getByText('card.pay_now')).toBeInTheDocument()
  })

  it('shows a loading spinner when isLoading is true', () => {
    render(<PaymentCardForm onSubmit={onSubmit} isLoading />)
    expect(document.querySelector('.animate-spin')).toBeInTheDocument()
  })
})
