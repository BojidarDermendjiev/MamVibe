import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import ProfileTypeSelector from './ProfileTypeSelector'
import { ProfileType } from '../../types/auth'

describe('ProfileTypeSelector', () => {
  it('renders 3 option buttons', () => {
    render(<ProfileTypeSelector value={ProfileType.Female} onChange={vi.fn()} />)
    expect(screen.getAllByRole('button')).toHaveLength(3)
  })

  it('calls onChange with Female when Female option clicked', async () => {
    const onChange = vi.fn()
    render(<ProfileTypeSelector value={ProfileType.Male} onChange={onChange} />)
    const buttons = screen.getAllByRole('button')
    await userEvent.click(buttons[0])
    expect(onChange).toHaveBeenCalledWith(ProfileType.Female)
  })

  it('calls onChange with Male when Male option clicked', async () => {
    const onChange = vi.fn()
    render(<ProfileTypeSelector value={ProfileType.Female} onChange={onChange} />)
    await userEvent.click(screen.getAllByRole('button')[1])
    expect(onChange).toHaveBeenCalledWith(ProfileType.Male)
  })

  it('calls onChange with Family when Family option clicked', async () => {
    const onChange = vi.fn()
    render(<ProfileTypeSelector value={ProfileType.Female} onChange={onChange} />)
    await userEvent.click(screen.getAllByRole('button')[2])
    expect(onChange).toHaveBeenCalledWith(ProfileType.Family)
  })
})
