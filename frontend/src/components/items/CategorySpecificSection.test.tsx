import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import CategorySpecificSection from './CategorySpecificSection'
import { AgeGroup } from '../../types/item'

const noop = vi.fn()

const defaultProps = {
  ageGroup: null,
  shoeSize: null,
  clothingSize: null,
  onAgeGroupChange: noop,
  onShoeSizeChange: noop,
  onClothingSizeChange: noop,
}

describe('CategorySpecificSection', () => {
  it('renders nothing for unknown slug', () => {
    const { container } = render(
      <CategorySpecificSection {...defaultProps} categorySlug="unknown" />
    )
    expect(container.firstChild).toBeNull()
  })

  it('renders nothing when categorySlug is undefined', () => {
    const { container } = render(
      <CategorySpecificSection {...defaultProps} categorySlug={undefined} />
    )
    expect(container.firstChild).toBeNull()
  })

  it('renders clothing size grid for clothing slug', () => {
    render(<CategorySpecificSection {...defaultProps} categorySlug="clothing" />)
    expect(screen.getByText('Find the perfect size for your little one')).toBeInTheDocument()
    expect(screen.getByText(/Baby/)).toBeInTheDocument()
  })

  it('calls onClothingSizeChange when a size button is clicked', async () => {
    const onChange = vi.fn()
    render(
      <CategorySpecificSection
        {...defaultProps}
        categorySlug="clothing"
        onClothingSizeChange={onChange}
      />
    )
    await userEvent.click(screen.getByText('50'))
    expect(onChange).toHaveBeenCalledWith(50)
  })

  it('clears clothing size when same size is clicked again', async () => {
    const onChange = vi.fn()
    render(
      <CategorySpecificSection
        {...defaultProps}
        categorySlug="clothing"
        clothingSize={50}
        onClothingSizeChange={onChange}
      />
    )
    await userEvent.click(screen.getByText('50'))
    expect(onChange).toHaveBeenCalledWith(null)
  })

  it('shows Clear size button when clothingSize is set', () => {
    render(
      <CategorySpecificSection {...defaultProps} categorySlug="clothing" clothingSize={62} />
    )
    expect(screen.getByText(/Clear size/)).toBeInTheDocument()
  })

  it('renders shoe size grid for shoes slug', () => {
    render(<CategorySpecificSection {...defaultProps} categorySlug="shoes" />)
    expect(screen.getByText('Shoe Size (EU)')).toBeInTheDocument()
  })

  it('calls onShoeSizeChange when a shoe size is clicked', async () => {
    const onChange = vi.fn()
    render(
      <CategorySpecificSection {...defaultProps} categorySlug="shoes" onShoeSizeChange={onChange} />
    )
    await userEvent.click(screen.getByText('16'))
    expect(onChange).toHaveBeenCalledWith(16)
  })

  it('renders age grid for toys slug', () => {
    render(<CategorySpecificSection {...defaultProps} categorySlug="toys" />)
    expect(screen.getByText('Recommended Age')).toBeInTheDocument()
  })

  it('calls onAgeGroupChange for toys', async () => {
    const onChange = vi.fn()
    render(
      <CategorySpecificSection {...defaultProps} categorySlug="toys" onAgeGroupChange={onChange} />
    )
    await userEvent.click(screen.getByText('Newborn'))
    expect(onChange).toHaveBeenCalledWith(AgeGroup.Newborn)
  })

  it('renders car-seats section', () => {
    render(<CategorySpecificSection {...defaultProps} categorySlug="car-seats" />)
    expect(screen.getByText('Car Seat Group')).toBeInTheDocument()
  })

  it('renders strollers section', () => {
    render(<CategorySpecificSection {...defaultProps} categorySlug="strollers" />)
    expect(screen.getByText('Suitable From')).toBeInTheDocument()
  })

  it('renders furniture section', () => {
    render(<CategorySpecificSection {...defaultProps} categorySlug="furniture" />)
    expect(screen.getByText('Suitable Age')).toBeInTheDocument()
  })

  it('renders feeding section', () => {
    render(<CategorySpecificSection {...defaultProps} categorySlug="feeding" />)
    expect(screen.getAllByText('Suitable Age').length).toBeGreaterThan(0)
  })

  it('shows Clear selection when an age group is selected', () => {
    render(
      <CategorySpecificSection
        {...defaultProps}
        categorySlug="toys"
        ageGroup={AgeGroup.Toddler}
      />
    )
    expect(screen.getByText(/Clear selection/)).toBeInTheDocument()
  })

  it('clears age group when Clear selection is clicked', async () => {
    const onChange = vi.fn()
    render(
      <CategorySpecificSection
        {...defaultProps}
        categorySlug="toys"
        ageGroup={AgeGroup.Toddler}
        onAgeGroupChange={onChange}
      />
    )
    await userEvent.click(screen.getByText(/Clear selection/))
    expect(onChange).toHaveBeenCalledWith(null)
  })
})
