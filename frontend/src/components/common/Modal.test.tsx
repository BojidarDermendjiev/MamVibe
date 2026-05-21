import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import Modal from './Modal'

const noop = vi.fn()

describe('Modal', () => {
  it('renders nothing when closed', () => {
    const { container } = render(<Modal isOpen={false} onClose={noop}>Content</Modal>)
    expect(container.firstChild).toBeNull()
  })

  it('renders children when open', () => {
    render(<Modal isOpen onClose={noop}>Hello Modal</Modal>)
    expect(screen.getByText('Hello Modal')).toBeInTheDocument()
  })

  it('renders title when provided', () => {
    render(<Modal isOpen onClose={noop} title="My Title">Body</Modal>)
    expect(screen.getByText('My Title')).toBeInTheDocument()
  })

  it('calls onClose when close button is clicked', async () => {
    const onClose = vi.fn()
    render(<Modal isOpen onClose={onClose}>Body</Modal>)
    await userEvent.click(screen.getByRole('button', { name: 'Close dialog' }))
    expect(onClose).toHaveBeenCalledOnce()
  })

  it('calls onClose when backdrop is clicked', async () => {
    const onClose = vi.fn()
    render(<Modal isOpen onClose={onClose}>Body</Modal>)
    const dialog = screen.getByRole('dialog')
    const backdrop = dialog.querySelector('.absolute.inset-0') as HTMLElement
    await userEvent.click(backdrop)
    expect(onClose).toHaveBeenCalledOnce()
  })

  it('calls onClose when Escape key is pressed', () => {
    const onClose = vi.fn()
    render(<Modal isOpen onClose={onClose}>Body</Modal>)
    fireEvent.keyDown(document, { key: 'Escape' })
    expect(onClose).toHaveBeenCalledOnce()
  })

  it('does not call onClose on Escape when closed', () => {
    const onClose = vi.fn()
    render(<Modal isOpen={false} onClose={onClose}>Body</Modal>)
    fireEvent.keyDown(document, { key: 'Escape' })
    expect(onClose).not.toHaveBeenCalled()
  })

  it('sets body overflow hidden when open', () => {
    render(<Modal isOpen onClose={noop}>Body</Modal>)
    expect(document.body.style.overflow).toBe('hidden')
  })

  it('has role dialog and aria-modal', () => {
    render(<Modal isOpen onClose={noop}>Body</Modal>)
    const dialog = screen.getByRole('dialog')
    expect(dialog).toHaveAttribute('aria-modal', 'true')
  })
})
