import { useEffect, useRef, useCallback } from 'react';

const SITE_KEY = import.meta.env.VITE_TURNSTILE_SITE_KEY ?? '1x00000000000000000000AA';

// The bundled Turnstile type omits invisible-mode options — extend via any.
// eslint-disable-next-line @typescript-eslint/no-explicit-any
type TurnstileAny = any;

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
      const ts: TurnstileAny = window.turnstile;
      if (!ts || !containerRef.current || widgetIdRef.current) return;
      widgetIdRef.current = ts.render(containerRef.current, {
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
      const ts: TurnstileAny = window.turnstile;
      if (widgetIdRef.current && ts) {
        ts.remove(widgetIdRef.current);
        widgetIdRef.current = null;
      }
      container.remove();
      containerRef.current = null;
    };
  }, []);

  const execute = useCallback((): Promise<string> => {
    return new Promise((resolve) => {
      const ts: TurnstileAny = window.turnstile;
      if (!ts || !widgetIdRef.current) { resolve(''); return; }
      resolveRef.current = resolve;
      ts.execute(widgetIdRef.current);
    });
  }, []);

  const reset = useCallback(() => {
    const ts: TurnstileAny = window.turnstile;
    if (widgetIdRef.current && ts) ts.reset(widgetIdRef.current);
  }, []);

  return { execute, reset };
}
