import { describe, it, expect } from 'vitest'
import { formatPrice, formatEur } from './currency'

describe('formatPrice', () => {
  it('converts BGN to EUR with fixed peg', () => {
    expect(formatPrice(19.56)).toBe('€10.00')
  })

  it('returns empty string for null', () => {
    expect(formatPrice(null)).toBe('')
  })

  it('returns empty string for undefined', () => {
    expect(formatPrice(undefined)).toBe('')
  })

  it('handles zero', () => {
    expect(formatPrice(0)).toBe('€0.00')
  })
})

describe('formatEur', () => {
  it('formats a positive amount', () => {
    expect(formatEur(10)).toBe('€10.00')
  })

  it('returns €0.00 for null', () => {
    expect(formatEur(null)).toBe('€0.00')
  })

  it('returns €0.00 for undefined', () => {
    expect(formatEur(undefined)).toBe('€0.00')
  })

  it('formats two decimal places', () => {
    expect(formatEur(3.5)).toBe('€3.50')
  })
})
