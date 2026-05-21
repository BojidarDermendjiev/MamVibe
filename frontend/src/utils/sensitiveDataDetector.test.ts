import { describe, it, expect } from 'vitest'
import { detectSensitiveData } from './sensitiveDataDetector'

describe('detectSensitiveData', () => {
  it('detects a Bulgarian mobile phone number', () => {
    const matches = detectSensitiveData('Call me at 0878123456 tomorrow')
    expect(matches).toContainEqual({ type: 'phone', labelKey: 'privacy.type_phone' })
  })

  it('detects a phone with international prefix (no inner spaces)', () => {
    const matches = detectSensitiveData('+359878123456')
    expect(matches).toContainEqual({ type: 'phone', labelKey: 'privacy.type_phone' })
  })

  it('detects an email address', () => {
    const matches = detectSensitiveData('Contact me at user@example.com')
    expect(matches).toContainEqual({ type: 'email', labelKey: 'privacy.type_email' })
  })

  it('detects a Bulgarian EGN (national ID)', () => {
    const matches = detectSensitiveData('My EGN is 9001011234')
    expect(matches).toContainEqual({ type: 'national-id', labelKey: 'privacy.type_national_id' })
  })

  it('detects a Bulgarian IBAN', () => {
    const matches = detectSensitiveData('IBAN: BG80BNBG96611020345678')
    expect(matches).toContainEqual({ type: 'iban', labelKey: 'privacy.type_iban' })
  })

  it('detects a card number with spaces', () => {
    const matches = detectSensitiveData('Card: 4111 1111 1111 1111')
    expect(matches).toContainEqual({ type: 'card', labelKey: 'privacy.type_card' })
  })

  it('returns empty array for clean text', () => {
    expect(detectSensitiveData('Hello, how are you?')).toEqual([])
  })

  it('returns multiple matches when several types present', () => {
    const matches = detectSensitiveData('Email user@test.com, phone 0878123456')
    expect(matches).toHaveLength(2)
  })
})
