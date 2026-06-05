import { useEffect, useRef, useCallback } from 'react';

const SITE_KEY = import.meta.env.VITE_TURNSTILE_SITE_KEY ?? '1x00000000000000000000AA';

interface TurnstileWidgetProps {
  onToken: (token: string) => void;
  onExpire?: () => void;
  theme?: 'light' | 'dark' | 'auto';
}

/**
 * Renders an inline Cloudflare Turnstile challenge and calls `onToken` with
 * the one-time token once the user passes. The token is consumed server-side
 * on form submission. After submission the caller should call `reset()` on
 * this widget (or unmount/remount) to obtain a fresh token for the next attempt.
 */
export default function TurnstileWidget({ onToken, onExpire, theme = 'light' }: TurnstileWidgetProps) {
  const containerRef = useRef<HTMLDivElement>(null);
  const widgetIdRef = useRef<string | null>(null);

  const mount = useCallback(() => {
    if (!containerRef.current || !window.turnstile) return;
    if (widgetIdRef.current) return; // already mounted

    widgetIdRef.current = window.turnstile.render(containerRef.current, {
      sitekey: SITE_KEY,
      callback: onToken,
      'expired-callback': onExpire,
      'error-callback': onExpire,
      theme,
    });
  }, [onToken, onExpire, theme]);

  useEffect(() => {
    if (window.turnstile) {
      mount();
      return;
    }

    const existing = document.querySelector('script[src*="challenges.cloudflare.com/turnstile"]');
    if (!existing) {
      const script = document.createElement('script');
      script.src = 'https://challenges.cloudflare.com/turnstile/v0/api.js?render=explicit';
      script.async = true;
      document.head.appendChild(script);
    }

    const interval = setInterval(() => {
      if (window.turnstile) {
        clearInterval(interval);
        mount();
      }
    }, 100);

    return () => clearInterval(interval);
  }, [mount]);

  useEffect(() => {
    return () => {
      if (widgetIdRef.current && window.turnstile) {
        window.turnstile.remove(widgetIdRef.current);
        widgetIdRef.current = null;
      }
    };
  }, []);

  return <div ref={containerRef} />;
}
