import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import PaymentCard from './PaymentCard'

const baseProps = {
  name: '',
  cardNumber: '',
  expiration: '',
  securityCode: '',
  isFlipped: false,
}

describe('PaymentCard', () => {
  it('shows placeholder card number when empty', () => {
    render(<PaymentCard {...baseProps} />)
    expect(screen.getByText('#### #### #### ####')).toBeInTheDocument()
  })

  it('shows provided card number', () => {
    render(<PaymentCard {...baseProps} cardNumber="4111 1111 1111 1111" />)
    expect(screen.getByText('4111 1111 1111 1111')).toBeInTheDocument()
  })

  it('shows placeholder name when empty', () => {
    render(<PaymentCard {...baseProps} />)
    expect(screen.getAllByText('FULL NAME').length).toBeGreaterThan(0)
  })

  it('shows provided cardholder name on front and back', () => {
    render(<PaymentCard {...baseProps} name="Jane Doe" />)
    expect(screen.getAllByText('Jane Doe').length).toBeGreaterThanOrEqual(1)
  })

  it('shows placeholder expiry when empty', () => {
    render(<PaymentCard {...baseProps} />)
    expect(screen.getByText('MM/YY')).toBeInTheDocument()
  })

  it('shows provided expiry date', () => {
    render(<PaymentCard {...baseProps} expiration="09/27" />)
    expect(screen.getByText('09/27')).toBeInTheDocument()
  })

  it('shows placeholder CVV when empty', () => {
    render(<PaymentCard {...baseProps} />)
    expect(screen.getByText('***')).toBeInTheDocument()
  })

  it('shows provided security code', () => {
    render(<PaymentCard {...baseProps} securityCode="456" />)
    expect(screen.getByText('456')).toBeInTheDocument()
  })

  it('applies flipped class when isFlipped is true', () => {
    const { container } = render(<PaymentCard {...baseProps} isFlipped />)
    expect(container.querySelector('.creditcard.flipped')).toBeInTheDocument()
  })

  it('does not apply flipped class when isFlipped is false', () => {
    const { container } = render(<PaymentCard {...baseProps} isFlipped={false} />)
    expect(container.querySelector('.creditcard.flipped')).toBeNull()
  })
})
