import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor, act } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import MamVibeAssistantWidget from './MamVibeAssistantWidget'
import { assistantApi } from '../../api/assistantApi'

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ i18n: { language: 'en' } }),
}))

vi.mock('../../api/assistantApi', () => ({
  assistantApi: { chat: vi.fn() },
}))

const mockChat = vi.mocked(assistantApi.chat)

beforeEach(() => {
  mockChat.mockClear()
  Element.prototype.scrollIntoView = vi.fn()
})

describe('MamVibeAssistantWidget', () => {
  it('renders the floating trigger button in closed state', () => {
    render(<MamVibeAssistantWidget />)
    expect(screen.getByLabelText('Open MamVibe Assistant')).toBeInTheDocument()
  })

  it('opens the chat panel when the trigger button is clicked', async () => {
    render(<MamVibeAssistantWidget />)
    await userEvent.click(screen.getByLabelText('Open MamVibe Assistant'))
    expect(screen.getByText('MamVibe Assistant')).toBeInTheDocument()
  })

  it('shows the welcome message when the panel opens', async () => {
    render(<MamVibeAssistantWidget />)
    await userEvent.click(screen.getByLabelText('Open MamVibe Assistant'))
    expect(screen.getByText(/Hi! 👋/)).toBeInTheDocument()
  })

  it('shows suggested questions when only the welcome message is present', async () => {
    render(<MamVibeAssistantWidget />)
    await userEvent.click(screen.getByLabelText('Open MamVibe Assistant'))
    expect(screen.getByText('How do I sell an item?')).toBeInTheDocument()
  })

  it('closes the panel when the close button is clicked', async () => {
    render(<MamVibeAssistantWidget />)
    await userEvent.click(screen.getByLabelText('Open MamVibe Assistant'))
    await userEvent.click(screen.getByLabelText('Close assistant'))
    expect(screen.queryByText('MamVibe Assistant')).not.toBeInTheDocument()
  })

  it('send button is disabled when input is empty', async () => {
    render(<MamVibeAssistantWidget />)
    await userEvent.click(screen.getByLabelText('Open MamVibe Assistant'))
    expect(screen.getByLabelText('Send message')).toBeDisabled()
  })

  it('send button is disabled when input is only whitespace', async () => {
    render(<MamVibeAssistantWidget />)
    await userEvent.click(screen.getByLabelText('Open MamVibe Assistant'))
    await userEvent.type(screen.getByPlaceholderText('Ask about MamVibe…'), '   ')
    expect(screen.getByLabelText('Send message')).toBeDisabled()
  })

  it('sends a message and displays the assistant reply', async () => {
    mockChat.mockResolvedValue({ data: { reply: 'Here is how to sell!' } } as never)
    render(<MamVibeAssistantWidget />)
    await userEvent.click(screen.getByLabelText('Open MamVibe Assistant'))
    await userEvent.type(screen.getByPlaceholderText('Ask about MamVibe…'), 'How do I sell?')
    await userEvent.click(screen.getByLabelText('Send message'))
    await waitFor(() => expect(screen.getByText('Here is how to sell!')).toBeInTheDocument())
  })

  it('displays error message when the API call fails', async () => {
    mockChat.mockRejectedValue(new Error('Network error'))
    render(<MamVibeAssistantWidget />)
    await userEvent.click(screen.getByLabelText('Open MamVibe Assistant'))
    await userEvent.type(screen.getByPlaceholderText('Ask about MamVibe…'), 'Help?')
    await userEvent.click(screen.getByLabelText('Send message'))
    await waitFor(() => expect(screen.getByText(/couldn't connect/)).toBeInTheDocument())
  })

  it('sends a suggested question when clicked', async () => {
    mockChat.mockResolvedValue({ data: { reply: 'To sell, go to...' } } as never)
    render(<MamVibeAssistantWidget />)
    await userEvent.click(screen.getByLabelText('Open MamVibe Assistant'))
    await userEvent.click(screen.getByText('How do I sell an item?'))
    expect(mockChat).toHaveBeenCalledWith('How do I sell an item?', [], 'en')
  })

  it('shows a loading indicator while the API call is in progress', async () => {
    let resolve!: (v: unknown) => void
    mockChat.mockReturnValue(new Promise((r) => { resolve = r }) as never)
    render(<MamVibeAssistantWidget />)
    await userEvent.click(screen.getByLabelText('Open MamVibe Assistant'))
    await userEvent.type(screen.getByPlaceholderText('Ask about MamVibe…'), 'Hello?')
    await userEvent.click(screen.getByLabelText('Send message'))
    expect(document.querySelector('.animate-spin')).toBeInTheDocument()
    await act(async () => { resolve({ data: { reply: 'Hi!' } }) })
  })

  it('clears the input after sending a message', async () => {
    mockChat.mockResolvedValue({ data: { reply: 'Done' } } as never)
    render(<MamVibeAssistantWidget />)
    await userEvent.click(screen.getByLabelText('Open MamVibe Assistant'))
    const input = screen.getByPlaceholderText('Ask about MamVibe…')
    await userEvent.type(input, 'Test message')
    await userEvent.click(screen.getByLabelText('Send message'))
    expect(input).toHaveValue('')
  })

  it('adds user message to the chat immediately on send', async () => {
    mockChat.mockReturnValue(new Promise(() => {}) as never)
    render(<MamVibeAssistantWidget />)
    await userEvent.click(screen.getByLabelText('Open MamVibe Assistant'))
    await userEvent.type(screen.getByPlaceholderText('Ask about MamVibe…'), 'My question')
    await userEvent.click(screen.getByLabelText('Send message'))
    expect(screen.getByText('My question')).toBeInTheDocument()
  })

  it('hides suggested questions after first message is sent', async () => {
    mockChat.mockResolvedValue({ data: { reply: 'Answer' } } as never)
    render(<MamVibeAssistantWidget />)
    await userEvent.click(screen.getByLabelText('Open MamVibe Assistant'))
    await userEvent.type(screen.getByPlaceholderText('Ask about MamVibe…'), 'Question')
    await userEvent.click(screen.getByLabelText('Send message'))
    await waitFor(() => expect(screen.queryByText('How do I sell an item?')).not.toBeInTheDocument())
  })
})
