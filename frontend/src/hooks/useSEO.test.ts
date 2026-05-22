import { describe, it, expect, afterEach } from 'vitest'
import { renderHook } from '@testing-library/react'
import { buildTitle, usePageSEO } from './useSEO'

afterEach(() => {
  document.title = ''
  document.head.querySelectorAll('meta, link, script[data-seo-ld]').forEach((el) => el.remove())
})

describe('buildTitle', () => {
  it('appends site name', () => {
    expect(buildTitle('Browse Items')).toBe('Browse Items | MamVibe')
  })
})

describe('usePageSEO', () => {
  it('sets document.title', () => {
    renderHook(() => usePageSEO({ title: 'Browse Items', description: 'Find items' }))
    expect(document.title).toBe('Browse Items | MamVibe')
  })

  it('sets meta description', () => {
    renderHook(() => usePageSEO({ title: 'Test', description: 'My description' }))
    expect(document.querySelector('meta[name="description"]')?.getAttribute('content')).toBe('My description')
  })

  it('sets robots to noindex when index is false', () => {
    renderHook(() => usePageSEO({ title: 'Admin', description: 'Admin page', index: false }))
    expect(document.querySelector('meta[name="robots"]')?.getAttribute('content')).toBe('noindex, nofollow')
  })

  it('sets robots to index by default', () => {
    renderHook(() => usePageSEO({ title: 'Home', description: 'Home page' }))
    expect(document.querySelector('meta[name="robots"]')?.getAttribute('content')).toBe('index, follow')
  })

  it('sets og:title', () => {
    renderHook(() => usePageSEO({ title: 'Products', description: 'Product listing' }))
    expect(document.querySelector('meta[property="og:title"]')?.getAttribute('content')).toBe('Products | MamVibe')
  })

  it('sets og:type from prop', () => {
    renderHook(() => usePageSEO({ title: 'Item', description: 'Item detail', ogType: 'product' }))
    expect(document.querySelector('meta[property="og:type"]')?.getAttribute('content')).toBe('product')
  })

  it('injects JSON-LD structured data', () => {
    const structuredData = { '@type': 'Product', name: 'Baby Jacket' }
    renderHook(() => usePageSEO({ title: 'Item', description: 'Desc', structuredData }))
    const script = document.querySelector('script[data-seo-ld]')
    expect(script).toBeInTheDocument()
    expect(JSON.parse(script!.textContent!)).toEqual(structuredData)
  })

  it('removes structured data on unmount', () => {
    const { unmount } = renderHook(() =>
      usePageSEO({ title: 'Item', description: 'Desc', structuredData: { '@type': 'Product' } })
    )
    unmount()
    expect(document.querySelector('script[data-seo-ld]')).toBeNull()
  })

  it('uses provided canonical prop instead of window.location.href', () => {
    renderHook(() => usePageSEO({ title: 'Test', description: 'Desc', canonical: 'https://mamvibe.com/test-page' }))
    expect(document.querySelector('link[rel="canonical"]')?.getAttribute('href')).toBe('https://mamvibe.com/test-page')
  })

  it('updates existing meta tag content on rerender without duplicating', () => {
    const { rerender } = renderHook(
      ({ desc }: { desc: string }) => usePageSEO({ title: 'Test', description: desc }),
      { initialProps: { desc: 'First description' } }
    )
    rerender({ desc: 'Updated description' })
    const metas = document.querySelectorAll('meta[name="description"]')
    expect(metas).toHaveLength(1)
    expect(metas[0].getAttribute('content')).toBe('Updated description')
  })

  it('updates existing canonical link on rerender without duplicating', () => {
    const { rerender } = renderHook(
      ({ canonical }: { canonical: string }) => usePageSEO({ title: 'Test', description: 'Desc', canonical }),
      { initialProps: { canonical: 'https://mamvibe.com/v1' } }
    )
    rerender({ canonical: 'https://mamvibe.com/v2' })
    const links = document.querySelectorAll('link[rel="canonical"]')
    expect(links).toHaveLength(1)
    expect(links[0].getAttribute('href')).toBe('https://mamvibe.com/v2')
  })

  it('replaces existing structured data script on rerender', () => {
    const { rerender } = renderHook(
      ({ sd }: { sd: Record<string, unknown> }) => usePageSEO({ title: 'Test', description: 'Desc', structuredData: sd }),
      { initialProps: { sd: { '@type': 'Product', name: 'First' } } }
    )
    rerender({ sd: { '@type': 'Product', name: 'Second' } })
    const scripts = document.querySelectorAll('script[data-seo-ld]')
    expect(scripts).toHaveLength(1)
    expect(JSON.parse(scripts[0].textContent!)).toMatchObject({ name: 'Second' })
  })
})
