import { describe, it, expect, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import { ThemeProvider, useTheme } from './ThemeContext'

function ThemeConsumer() {
  const { theme } = useTheme()
  return <span data-testid="theme">{theme}</span>
}

beforeEach(() => {
  localStorage.clear()
  document.documentElement.classList.remove('dark')
})

describe('ThemeContext', () => {
  it('always reports dark theme', () => {
    render(<ThemeProvider><ThemeConsumer /></ThemeProvider>)
    expect(screen.getByTestId('theme')).toHaveTextContent('dark')
  })

  it('adds dark class to html element on mount', () => {
    render(<ThemeProvider><ThemeConsumer /></ThemeProvider>)
    expect(document.documentElement).toHaveClass('dark')
  })

  it('persists dark to localStorage on mount', () => {
    render(<ThemeProvider><ThemeConsumer /></ThemeProvider>)
    expect(localStorage.getItem('theme')).toBe('dark')
  })
})
