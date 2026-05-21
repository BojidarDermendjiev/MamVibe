import { describe, it, expect } from 'vitest'
import { getCategoryImage } from './categoryImages'

describe('getCategoryImage', () => {
  it.each([
    ['clothes', '/categories/clothes.svg'],
    ['strollers', '/categories/strollers.svg'],
    ['car seats', '/categories/car-seats.svg'],
    ['toys', '/categories/toys.svg'],
    ['furniture', '/categories/furniture.svg'],
    ['other', '/categories/other.svg'],
  ])('maps "%s" to correct image', (category, expected) => {
    expect(getCategoryImage(category)).toBe(expected)
  })

  it('is case-insensitive', () => {
    expect(getCategoryImage('CLOTHES')).toBe('/categories/clothes.svg')
    expect(getCategoryImage('Toys')).toBe('/categories/toys.svg')
  })

  it('returns fallback for unknown category', () => {
    expect(getCategoryImage('electronics')).toBe('/categories/other.svg')
  })

  it('returns fallback for null', () => {
    expect(getCategoryImage(null)).toBe('/categories/other.svg')
  })

  it('returns fallback for undefined', () => {
    expect(getCategoryImage(undefined)).toBe('/categories/other.svg')
  })

  it('returns fallback for empty string', () => {
    expect(getCategoryImage('')).toBe('/categories/other.svg')
  })
})
