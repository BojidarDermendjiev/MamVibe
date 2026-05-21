import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import PhotoUploader from './PhotoUploader'

window.URL.createObjectURL = vi.fn(() => 'blob:mock-url')

function makeFile(name: string, type = 'image/jpeg') {
  return new File(['data'], name, { type })
}

const baseProps = {
  photos: [],
  onChange: vi.fn(),
}

beforeEach(() => baseProps.onChange.mockClear())

describe('PhotoUploader', () => {
  it('renders drop zone text', () => {
    render(<PhotoUploader {...baseProps} />)
    expect(screen.getByText('items.drag_drop')).toBeInTheDocument()
  })

  it('shows 0/5 count by default', () => {
    render(<PhotoUploader {...baseProps} />)
    expect(screen.getByText(/0\/5/)).toBeInTheDocument()
  })

  it('shows combined count of new and existing photos', () => {
    render(
      <PhotoUploader
        {...baseProps}
        photos={[makeFile('a.jpg')]}
        existingPhotos={[{ id: 'e1', url: '/e1.jpg' }]}
      />
    )
    expect(screen.getByText(/2\/5/)).toBeInTheDocument()
  })

  it('calls onChange with selected image files via input', async () => {
    render(<PhotoUploader {...baseProps} />)
    const input = document.querySelector('input[type="file"]') as HTMLInputElement
    const file = makeFile('photo.jpg')
    await userEvent.upload(input, file)
    expect(baseProps.onChange).toHaveBeenCalledWith([file])
  })

  it('filters out non-image files', async () => {
    render(<PhotoUploader {...baseProps} />)
    const input = document.querySelector('input[type="file"]') as HTMLInputElement
    const img = makeFile('photo.jpg', 'image/jpeg')
    const doc = makeFile('doc.pdf', 'application/pdf')
    await userEvent.upload(input, [img, doc])
    const uploaded = baseProps.onChange.mock.calls[0][0] as File[]
    expect(uploaded).toHaveLength(1)
    expect(uploaded[0].name).toBe('photo.jpg')
  })

  it('does not exceed maxPhotos when uploading multiple files', async () => {
    const onChange = vi.fn()
    render(<PhotoUploader maxPhotos={2} photos={[makeFile('a.jpg')]} onChange={onChange} />)
    const input = document.querySelector('input[type="file"]') as HTMLInputElement
    await userEvent.upload(input, [makeFile('b.jpg'), makeFile('c.jpg'), makeFile('d.jpg')])
    const uploaded = onChange.mock.calls[0][0] as File[]
    expect(uploaded).toHaveLength(2) // 1 existing + 1 new = 2 (max)
  })

  it('removes a new photo when its delete button is clicked', async () => {
    const onChange = vi.fn()
    render(<PhotoUploader photos={[makeFile('x.jpg')]} onChange={onChange} />)
    await userEvent.click(screen.getByRole('button'))
    expect(onChange).toHaveBeenCalledWith([])
  })

  it('shows existing photo thumbnails', () => {
    const { container } = render(
      <PhotoUploader {...baseProps} existingPhotos={[{ id: 'e1', url: '/existing.jpg' }]} />
    )
    expect(container.querySelector('img[src="/existing.jpg"]')).toBeInTheDocument()
  })

  it('calls onRemoveExisting with photo id when existing photo remove button clicked', async () => {
    const onRemoveExisting = vi.fn()
    render(
      <PhotoUploader
        {...baseProps}
        existingPhotos={[{ id: 'e1', url: '/e1.jpg' }]}
        onRemoveExisting={onRemoveExisting}
      />
    )
    await userEvent.click(screen.getByRole('button'))
    expect(onRemoveExisting).toHaveBeenCalledWith('e1')
  })

  it('disables drop zone when at max capacity', () => {
    const { container } = render(
      <PhotoUploader {...baseProps} maxPhotos={1} photos={[makeFile('a.jpg')]} />
    )
    expect(container.querySelector('.pointer-events-none')).toBeInTheDocument()
  })

  it('does not show remove button for existing photos when onRemoveExisting is not provided', () => {
    render(
      <PhotoUploader {...baseProps} existingPhotos={[{ id: 'e1', url: '/e1.jpg' }]} />
    )
    expect(screen.queryByRole('button')).toBeNull()
  })
})
