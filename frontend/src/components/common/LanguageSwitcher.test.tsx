import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import LanguageSwitcher from './LanguageSwitcher'

const mockI18n = vi.hoisted(() => ({ language: 'en' as string, changeLanguage: vi.fn() }))

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (k: string) => k,
    i18n: mockI18n,
  }),
}))

beforeEach(() => {
  mockI18n.changeLanguage.mockClear()
  mockI18n.language = 'en'
  localStorage.clear()
})

describe('LanguageSwitcher', () => {
  it('shows BG button when language is en', () => {
    render(<LanguageSwitcher />)
    expect(screen.getByRole('button', { name: 'BG' })).toBeInTheDocument()
  })

  it('calls changeLanguage with bg when current is en', async () => {
    render(<LanguageSwitcher />)
    await userEvent.click(screen.getByRole('button'))
    expect(mockI18n.changeLanguage).toHaveBeenCalledWith('bg')
  })

  it('persists bg to localStorage when switching from en', async () => {
    render(<LanguageSwitcher />)
    await userEvent.click(screen.getByRole('button'))
    expect(localStorage.getItem('language')).toBe('bg')
  })

  it('shows EN button when language is bg', () => {
    mockI18n.language = 'bg'
    render(<LanguageSwitcher />)
    expect(screen.getByRole('button', { name: 'EN' })).toBeInTheDocument()
  })

  it('calls changeLanguage with en when current is bg', async () => {
    mockI18n.language = 'bg'
    render(<LanguageSwitcher />)
    await userEvent.click(screen.getByRole('button'))
    expect(mockI18n.changeLanguage).toHaveBeenCalledWith('en')
  })

  it('persists en to localStorage when switching from bg', async () => {
    mockI18n.language = 'bg'
    render(<LanguageSwitcher />)
    await userEvent.click(screen.getByRole('button'))
    expect(localStorage.getItem('language')).toBe('en')
  })
})
