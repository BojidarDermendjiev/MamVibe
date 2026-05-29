import { describe, it, expect, beforeEach } from 'vitest'
import { render, screen, act } from '@testing-library/react'
import { ThemeProvider, useTheme } from './ThemeContext'

function ThemeConsumer() {
  const { theme, toggleTheme } = useTheme()
  return (
    <>
      <span data-testid="theme">{theme}</span>
      <button data-testid="toggle" onClick={toggleTheme}>toggle</button>
    </>
  )
}

beforeEach(() => {
  localStorage.clear()
  document.documentElement.classList.remove('dark')
})

describe('ThemeContext', () => {
  it('defaults to dark theme when localStorage is empty', () => {
    render(<ThemeProvider><ThemeConsumer /></ThemeProvider>)
    expect(screen.getByTestId('theme')).toHaveTextContent('dark')
  })

  it('adds dark class to html element on mount when theme is dark', () => {
    render(<ThemeProvider><ThemeConsumer /></ThemeProvider>)
    expect(document.documentElement).toHaveClass('dark')
  })

  it('persists dark to localStorage on mount', () => {
    render(<ThemeProvider><ThemeConsumer /></ThemeProvider>)
    expect(localStorage.getItem('theme')).toBe('dark')
  })

  it('toggles to light mode and removes dark class', () => {
    render(<ThemeProvider><ThemeConsumer /></ThemeProvider>)
    act(() => { screen.getByTestId('toggle').click() })
    expect(screen.getByTestId('theme')).toHaveTextContent('light')
    expect(document.documentElement).not.toHaveClass('dark')
    expect(localStorage.getItem('theme')).toBe('light')
  })

  it('restores persisted light theme from localStorage', () => {
    localStorage.setItem('theme', 'light')
    render(<ThemeProvider><ThemeConsumer /></ThemeProvider>)
    expect(screen.getByTestId('theme')).toHaveTextContent('light')
    expect(document.documentElement).not.toHaveClass('dark')
  })
})
