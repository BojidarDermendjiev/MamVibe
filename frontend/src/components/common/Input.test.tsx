import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import Input from './Input'

describe('Input', () => {
  it('renders without label or error', () => {
    render(<Input placeholder="Enter text" />)
    expect(screen.getByPlaceholderText('Enter text')).toBeInTheDocument()
  })

  it('renders label when provided', () => {
    render(<Input label="Email" />)
    expect(screen.getByText('Email')).toBeInTheDocument()
  })

  it('renders error message when provided', () => {
    render(<Input error="This field is required" />)
    expect(screen.getByText('This field is required')).toBeInTheDocument()
  })

  it('accepts user input', async () => {
    render(<Input placeholder="Type here" />)
    const input = screen.getByPlaceholderText('Type here')
    await userEvent.type(input, 'hello')
    expect(input).toHaveValue('hello')
  })

  it('does not show toggle button for non-password inputs', () => {
    render(<Input type="text" />)
    expect(screen.queryByRole('button')).toBeNull()
  })

  it('shows toggle button for password inputs', () => {
    render(<Input type="password" />)
    expect(screen.getByRole('button')).toBeInTheDocument()
  })

  it('toggles password visibility on button click', async () => {
    render(<Input type="password" placeholder="pw" />)
    const input = screen.getByPlaceholderText('pw')
    expect(input).toHaveAttribute('type', 'password')

    await userEvent.click(screen.getByRole('button'))
    expect(input).toHaveAttribute('type', 'text')

    await userEvent.click(screen.getByRole('button'))
    expect(input).toHaveAttribute('type', 'password')
  })

  it('forwards onChange handler', async () => {
    const handler = vi.fn()
    render(<Input placeholder="x" onChange={handler} />)
    await userEvent.type(screen.getByPlaceholderText('x'), 'a')
    expect(handler).toHaveBeenCalled()
  })
})
