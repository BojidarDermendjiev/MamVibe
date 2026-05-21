import { describe, it, expect, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { ThemeProvider, useTheme } from './ThemeContext'

function ThemeConsumer() {
  const { theme, toggleTheme } = useTheme()
  return (
    <div>
      <span data-testid="theme">{theme}</span>
      <button onClick={toggleTheme}>toggle</button>
    </div>
  )
}

beforeEach(() => {
  localStorage.clear()
  document.documentElement.classList.remove('dark')
})

describe('ThemeContext', () => {
  it('defaults to light theme', () => {
    render(<ThemeProvider><ThemeConsumer /></ThemeProvider>)
    expect(screen.getByTestId('theme')).toHaveTextContent('light')
  })

  it('reads dark theme from localStorage', () => {
    localStorage.setItem('theme', 'dark')
    render(<ThemeProvider><ThemeConsumer /></ThemeProvider>)
    expect(screen.getByTestId('theme')).toHaveTextContent('dark')
  })

  it('toggles to dark on click', async () => {
    render(<ThemeProvider><ThemeConsumer /></ThemeProvider>)
    await userEvent.click(screen.getByText('toggle'))
    expect(screen.getByTestId('theme')).toHaveTextContent('dark')
  })

  it('toggles back to light on second click', async () => {
    render(<ThemeProvider><ThemeConsumer /></ThemeProvider>)
    await userEvent.click(screen.getByText('toggle'))
    await userEvent.click(screen.getByText('toggle'))
    expect(screen.getByTestId('theme')).toHaveTextContent('light')
  })

  it('adds dark class to html element when dark', async () => {
    render(<ThemeProvider><ThemeConsumer /></ThemeProvider>)
    await userEvent.click(screen.getByText('toggle'))
    expect(document.documentElement).toHaveClass('dark')
  })

  it('persists theme to localStorage', async () => {
    render(<ThemeProvider><ThemeConsumer /></ThemeProvider>)
    await userEvent.click(screen.getByText('toggle'))
    expect(localStorage.getItem('theme')).toBe('dark')
  })
})
