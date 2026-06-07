import { useEffect, useRef, useCallback } from 'react';

const SITE_KEY = import.meta.env.VITE_TURNSTILE_SITE_KEY ?? '1x00000000000000000000AA';

/**
 * Mounts an invisible Cloudflare Turnstile widget into a hidden div.
 * Call `execute()` just before form submission to get a one-time token silently.
 * Call `reset()` after each submission so the widget can issue a fresh token.
 */
export function useInvisibleTurnstile() {
  const containerRef = useRef<HTMLDivElement | null>(null);
  const widgetIdRef = useRef<string | null>(null);
  const resolveRef = useRef<((token: string) => void) | null>(null);

  useEffect(() => {
    const container = document.createElement('div');
    container.style.display = 'none';
    document.body.appendChild(container);
    containerRef.current = container;
    let intervalId: ReturnType<typeof setInterval> | null = null;

    const mount = () => {
      if (!window.turnstile || !containerRef.current || widgetIdRef.current) return;
      widgetIdRef.current = window.turnstile.render(containerRef.current, {
        sitekey: SITE_KEY,
        size: 'invisible',
        execution: 'execute',
        callback: (token: string) => {
          resolveRef.current?.(token);
          resolveRef.current = null;
        },
        'expired-callback': () => { resolveRef.current?.(''); resolveRef.current = null; },
        'error-callback': () => { resolveRef.current?.(''); resolveRef.current = null; },
      });
    };

    if (window.turnstile) {
      mount();
    } else {
      if (!document.querySelector('script[src*="challenges.cloudflare.com/turnstile"]')) {
        const script = document.createElement('script');
        script.src = 'https://challenges.cloudflare.com/turnstile/v0/api.js?render=explicit';
        script.async = true;
        document.head.appendChild(script);
      }
      intervalId = setInterval(() => {
        if (window.turnstile) { clearInterval(intervalId!); intervalId = null; mount(); }
      }, 100);
    }

    return () => {
      if (intervalId) clearInterval(intervalId);
      if (widgetIdRef.current && window.turnstile) {
        window.turnstile.remove(widgetIdRef.current);
        widgetIdRef.current = null;
      }
      container.remove();
      containerRef.current = null;
    };
  }, []);

  const execute = useCallback((): Promise<string> => {
    return new Promise((resolve) => {
      if (!window.turnstile || !widgetIdRef.current) { resolve(''); return; }
      resolveRef.current = resolve;
      window.turnstile.execute(widgetIdRef.current);
    });
  }, []);

  const reset = useCallback(() => {
    if (widgetIdRef.current && window.turnstile) {
      window.turnstile.reset(widgetIdRef.current);
    }
  }, []);

  return { execute, reset };
}
