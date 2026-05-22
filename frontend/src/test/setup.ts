import '@testing-library/jest-dom'
import React from 'react'

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => key,
    i18n: { language: 'en', changeLanguage: vi.fn() },
  }),
  initReactI18next: { type: '3rdParty', init: vi.fn() },
  Trans: ({ i18nKey }: { i18nKey: string }) => i18nKey,
}))

vi.mock('framer-motion', () => ({
  motion: {
    button: ({ children, onClick, 'aria-label': ariaLabel }: React.ButtonHTMLAttributes<HTMLButtonElement> & { 'aria-label'?: string }) => (
      React.createElement('button', { onClick, 'aria-label': ariaLabel }, children)
    ),
    div: ({ children, ...props }: React.HTMLAttributes<HTMLDivElement>) => (
      React.createElement('div', props, children)
    ),
  },
  AnimatePresence: ({ children }: { children: React.ReactNode }) => children,
}))

// Stub all lucide-react icons to avoid React context duplication in jsdom
vi.mock('lucide-react', () => {
  const stub = (name: string) => {
    const Comp = ({ className }: Record<string, unknown>) =>
      React.createElement('svg', { 'data-testid': name, className })
    Comp.displayName = name
    return Comp
  }
  const icons = [
    'ShieldAlert', 'Phone', 'Mail', 'CreditCard', 'Landmark', 'User',
    'ChevronUp', 'ChevronDown', 'Check', 'X', 'TriangleAlert', 'ThumbsUp',
    'Ruler', 'Footprints', 'Puzzle', 'ShieldCheck', 'Baby', 'Milk', 'Armchair',
    'Sun', 'Moon', 'Star', 'MapPin', 'Trash2', 'Heart',
    'Paperclip', 'Mic', 'MicOff', 'CornerDownLeft',
    'Lock', 'Eye', 'EyeOff', 'Search', 'Filter', 'Plus', 'Minus',
    'AlertTriangle', 'Info', 'Settings', 'Home', 'Menu', 'Bell',
    'Download', 'Upload', 'Edit', 'Trash', 'Package', 'Truck',
    'ArrowLeft', 'ArrowRight', 'ExternalLink', 'Send', 'MessageCircle',
    'Bot', 'MessageCircleQuestion', 'Loader2', 'Stethoscope',
    'PlusCircle', 'LayoutDashboard', 'MessageSquare', 'LogIn',
  ]
  const result: Record<string, unknown> = {}
  for (const name of icons) result[name] = stub(name)
  return result
})
