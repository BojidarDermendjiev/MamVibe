import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import DeliveryTypeSelector from './DeliveryTypeSelector'
import { DeliveryType } from '../../types/shipping'

describe('DeliveryTypeSelector', () => {
  it('renders 3 delivery type buttons', () => {
    render(<DeliveryTypeSelector value={DeliveryType.Office} onChange={vi.fn()} />)
    expect(screen.getAllByRole('button')).toHaveLength(3)
  })

  it('calls onChange with Address when Address clicked', async () => {
    const onChange = vi.fn()
    render(<DeliveryTypeSelector value={DeliveryType.Office} onChange={onChange} />)
    await userEvent.click(screen.getAllByRole('button')[1])
    expect(onChange).toHaveBeenCalledWith(DeliveryType.Address)
  })

  it('calls onChange with Locker when Locker clicked', async () => {
    const onChange = vi.fn()
    render(<DeliveryTypeSelector value={DeliveryType.Office} onChange={onChange} />)
    await userEvent.click(screen.getAllByRole('button')[2])
    expect(onChange).toHaveBeenCalledWith(DeliveryType.Locker)
  })

  it('calls onChange with Office when Office clicked', async () => {
    const onChange = vi.fn()
    render(<DeliveryTypeSelector value={DeliveryType.Address} onChange={onChange} />)
    await userEvent.click(screen.getAllByRole('button')[0])
    expect(onChange).toHaveBeenCalledWith(DeliveryType.Office)
  })
})
