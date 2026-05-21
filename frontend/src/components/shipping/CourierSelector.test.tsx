import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import CourierSelector from './CourierSelector'
import { CourierProvider } from '../../types/shipping'

describe('CourierSelector', () => {
  it('renders 4 courier buttons', () => {
    render(<CourierSelector value={CourierProvider.Econt} onChange={vi.fn()} />)
    expect(screen.getAllByRole('button')).toHaveLength(4)
  })

  it('calls onChange with Speedy when Speedy button clicked', async () => {
    const onChange = vi.fn()
    render(<CourierSelector value={CourierProvider.Econt} onChange={onChange} />)
    await userEvent.click(screen.getAllByRole('button')[1])
    expect(onChange).toHaveBeenCalledWith(CourierProvider.Speedy)
  })

  it('calls onChange with BoxNow when BoxNow button clicked', async () => {
    const onChange = vi.fn()
    render(<CourierSelector value={CourierProvider.Econt} onChange={onChange} />)
    await userEvent.click(screen.getAllByRole('button')[2])
    expect(onChange).toHaveBeenCalledWith(CourierProvider.BoxNow)
  })

  it('calls onChange with Econt when Econt button clicked', async () => {
    const onChange = vi.fn()
    render(<CourierSelector value={CourierProvider.Speedy} onChange={onChange} />)
    await userEvent.click(screen.getAllByRole('button')[0])
    expect(onChange).toHaveBeenCalledWith(CourierProvider.Econt)
  })
})
