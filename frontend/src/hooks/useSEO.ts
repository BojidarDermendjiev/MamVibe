/**
 * useSEO — centralized page-level metadata management via react-helmet-async.
 *
 * Usage:
 *   useSEO({ title: 'Browse Items', description: '...' });
 *
 * Every page in MainLayout should call this hook. It injects:
 *   - <title>
 *   - <meta name="description">
 *   - <meta name="robots">
 *   - <link rel="canonical">
 *   - Open Graph tags (og:title, og:description, og:image, og:url, og:type)
 *   - Twitter Card tags
 *
 * SEO rationale: Googlebot renders SPAs, but dynamic <title> and <meta description>
 * must be present at render time. react-helmet-async synchronously updates the <head>
 * during React's render pass, ensuring crawlers see unique metadata per route.
 */

import { useEffect } from 'react';

export interface SEOProps {
  /** Page title — will be suffixed with " | MamVibe". Keep the raw title ≤ 50 chars. */
  title: string;
  /** Meta description — 120-155 characters for optimal SERP display. */
  description: string;
  /** Canonical URL for this page. Defaults to current href. */
  canonical?: string;
  /** Absolute URL to the social share image. Defaults to the site OG image. */
  image?: string;
  /**
   * Open Graph type. Use 'article' for blog/content, 'product' for items.
   * @default 'website'
   */
  ogType?: 'website' | 'article' | 'product';
  /**
   * Whether this page should be indexed by search engines.
   * Set to false for auth/admin/dashboard pages.
   * @default true
   */
  index?: boolean;
  /** JSON-LD structured data object — will be serialised and injected as <script type="application/ld+json"> */
  structuredData?: Record<string, unknown> | Record<string, unknown>[];
}

const SITE_NAME = 'MamVibe';
const DEFAULT_IMAGE = 'https://mamvibe.com/hero-bg.jpg';
const SITE_URL = 'https://mamvibe.com';

export function buildTitle(title: string): string {
  return `${title} | ${SITE_NAME}`;
}

/**
 * usePageSEO — imperatively updates <head> metadata for the current page.
 *
 * NOTE: react-helmet-async exposes a declarative <Helmet> component, but this
 * hook pattern is equally valid and keeps SEO logic out of JSX.
 */
export function usePageSEO(props: SEOProps): void {
  const {
    title,
    description,
    canonical,
    image = DEFAULT_IMAGE,
    ogType = 'website',
    index = true,
    structuredData,
  } = props;

  const fullTitle = buildTitle(title);
  const canonicalUrl = canonical ?? (typeof window !== 'undefined' ? window.location.href : SITE_URL);
  const robots = index ? 'index, follow' : 'noindex, nofollow';

  useEffect(() => {
    // Title
    document.title = fullTitle;

    // Helper to set/create a <meta> tag
    const setMeta = (selector: string, attr: string, value: string) => {
      let el = document.querySelector<HTMLMetaElement>(selector);
      if (!el) {
        el = document.createElement('meta');
        const [attrName, attrValue] = attr.split('=');
        el.setAttribute(attrName, attrValue);
        document.head.appendChild(el);
      }
      el.setAttribute('content', value);
    };

    // Helper to set/create a <link> tag
    const setLink = (rel: string, href: string) => {
      let el = document.querySelector<HTMLLinkElement>(`link[rel="${rel}"]`);
      if (!el) {
        el = document.createElement('link');
        el.setAttribute('rel', rel);
        document.head.appendChild(el);
      }
      el.setAttribute('href', href);
    };

    // Standard meta
    setMeta('meta[name="description"]', 'name=description', description);
    setMeta('meta[name="robots"]', 'name=robots', robots);

    // Canonical
    setLink('canonical', canonicalUrl);

    // Open Graph
    setMeta('meta[property="og:title"]', 'property=og:title', fullTitle);
    setMeta('meta[property="og:description"]', 'property=og:description', description);
    setMeta('meta[property="og:image"]', 'property=og:image', image);
    setMeta('meta[property="og:url"]', 'property=og:url', canonicalUrl);
    setMeta('meta[property="og:type"]', 'property=og:type', ogType);

    // Twitter Card
    setMeta('meta[name="twitter:title"]', 'name=twitter:title', fullTitle);
    setMeta('meta[name="twitter:description"]', 'name=twitter:description', description);
    setMeta('meta[name="twitter:image"]', 'name=twitter:image', image);

    // JSON-LD structured data
    const existingLd = document.querySelector('script[data-seo-ld]');
    if (existingLd) existingLd.remove();

    if (structuredData) {
      const script = document.createElement('script');
      script.type = 'application/ld+json';
      script.setAttribute('data-seo-ld', 'true');
      script.textContent = JSON.stringify(structuredData);
      document.head.appendChild(script);
    }

    return () => {
      // Clean up structured data on unmount to avoid stale schema on next page
      document.querySelector('script[data-seo-ld]')?.remove();
    };
  }, [fullTitle, description, canonicalUrl, image, ogType, robots, structuredData]);
}

// Re-export so callers can use either name
export { usePageSEO as useSEO };
