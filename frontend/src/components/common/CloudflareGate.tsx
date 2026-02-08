import { useState, useEffect, useRef, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { turnstileApi } from "../../api/turnstileApi";
import LoadingSpinner from "./LoadingSpinner";

const TURNSTILE_SITE_KEY = "1x00000000000000000000AA";
const SESSION_KEY = "cf_verified";

declare global {
  interface Window {
    turnstile: {
      render: (
        container: string | HTMLElement,
        options: {
          sitekey: string;
          callback: (token: string) => void;
          "error-callback"?: () => void;
          theme?: "light" | "dark" | "auto";
        },
      ) => string;
      reset: (widgetId: string) => void;
      remove: (widgetId: string) => void;
    };
  }
}

interface CloudflareGateProps {
  children: React.ReactNode;
}

export default function CloudflareGate({ children }: CloudflareGateProps) {
  const { t } = useTranslation();
  const [verified, setVerified] = useState(() => {
    return sessionStorage.getItem(SESSION_KEY) === "true";
  });
  const [verifying, setVerifying] = useState(false);
  const [error, setError] = useState(false);
  const [scriptLoaded, setScriptLoaded] = useState(false);
  const widgetRef = useRef<HTMLDivElement>(null);
  const widgetIdRef = useRef<string | null>(null);

  // Load the Turnstile script and poll for window.turnstile readiness
  useEffect(() => {
    if (verified) return;

    // If already available, mark loaded immediately
    if (window.turnstile) {
      setScriptLoaded(true);
      return;
    }

    // Inject script tag if not already present
    const existingScript = document.querySelector(
      'script[src*="challenges.cloudflare.com/turnstile"]',
    );
    if (!existingScript) {
      const script = document.createElement("script");
      script.src =
        "https://challenges.cloudflare.com/turnstile/v0/api.js?render=explicit";
      script.async = true;
      document.head.appendChild(script);
    }

    // Poll for window.turnstile to become available
    // This avoids the global onload callback that React StrictMode breaks
    const interval = setInterval(() => {
      if (window.turnstile) {
        setScriptLoaded(true);
        clearInterval(interval);
      }
    }, 100);

    return () => clearInterval(interval);
  }, [verified]);

  const handleToken = useCallback(async (token: string) => {
    setVerifying(true);
    setError(false);
    try {
      const { data } = await turnstileApi.verify(token);
      if (data.verified) {
        sessionStorage.setItem(SESSION_KEY, "true");
        setVerified(true);
      } else {
        setError(true);
        if (widgetIdRef.current && window.turnstile) {
          window.turnstile.reset(widgetIdRef.current);
        }
      }
    } catch {
      setError(true);
      if (widgetIdRef.current && window.turnstile) {
        window.turnstile.reset(widgetIdRef.current);
      }
    } finally {
      setVerifying(false);
    }
  }, []);

  // Render the widget once the script is loaded
  useEffect(() => {
    if (verified || !scriptLoaded || !widgetRef.current || !window.turnstile)
      return;

    widgetIdRef.current = window.turnstile.render(widgetRef.current, {
      sitekey: TURNSTILE_SITE_KEY,
      callback: handleToken,
      "error-callback": () => setError(true),
      theme: "light",
    });

    return () => {
      if (widgetIdRef.current && window.turnstile) {
        window.turnstile.remove(widgetIdRef.current);
        widgetIdRef.current = null;
      }
    };
  }, [scriptLoaded, verified, handleToken]);

  if (verified) {
    return <>{children}</>;
  }

  return (
    <div className="min-h-screen bg-white flex items-center justify-center p-4">
      <div className="text-center">
        <div className="mb-8">
          <img src="/logo.png" alt="MomVibe" className="h-12 w-12 object-contain mx-auto" />
          <h1 className="text-3xl font-bold text-primary mt-2">MomVibe</h1>
        </div>

        <h2 className="text-xl font-semibold text-primary mb-2">
          {t("turnstile.title")}
        </h2>
        <p className="text-gray-500 mb-8 max-w-sm mx-auto">
          {t("turnstile.subtitle")}
        </p>

        <div className="flex justify-center mb-6">
          <div ref={widgetRef} />
        </div>

        {verifying && (
          <div className="flex items-center justify-center gap-2 text-primary">
            <LoadingSpinner size="sm" />
            <span className="text-sm">{t("turnstile.verifying")}</span>
          </div>
        )}

        {error && (
          <p className="text-red-500 text-sm mt-4">{t("turnstile.error")}</p>
        )}

        {!scriptLoaded && !error && <LoadingSpinner size="md" />}
      </div>
    </div>
  );
}
