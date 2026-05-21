import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import LanguageSwitcher from './LanguageSwitcher'

const mockChangeLanguage = vi.fn()

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (k: string) => k,
    i18n: { language: 'en', changeLanguage: mockChangeLanguage },
  }),
}))

beforeEach(() => {
  mockChangeLanguage.mockClear()
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
    expect(mockChangeLanguage).toHaveBeenCalledWith('bg')
  })

  it('persists selected language to localStorage', async () => {
    render(<LanguageSwitcher />)
    await userEvent.click(screen.getByRole('button'))
    expect(localStorage.getItem('language')).toBe('bg')
  })
})
