import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { Toaster } from "react-hot-toast";
import * as Sentry from "@sentry/react";
import "./i18n";
import "./index.css";
import App from "./App";
import CloudflareGate from "./components/common/CloudflareGate";

const sentryDsn = import.meta.env.VITE_SENTRY_DSN as string | undefined;
if (sentryDsn) {
  Sentry.init({
    dsn: sentryDsn,
    environment: import.meta.env.MODE,
    tracesSampleRate: 0.1,
    replaysOnErrorSampleRate: 1.0,
    replaysSessionSampleRate: 0.05,
    integrations: [Sentry.replayIntegration()],
  });
}

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <BrowserRouter
      future={{ v7_startTransition: true, v7_relativeSplatPath: true }}
    >
      <CloudflareGate>
        <App />
        <Toaster
          position="top-right"
          containerStyle={{ top: 72, right: 16 }}
          toastOptions={{
            duration: 3500,
            style: {
              background: '#fff',
              color: '#3f4a7f',
              borderRadius: '12px',
              border: '1px solid #c1c4e3',
            },
            custom: {
              style: {
                background: 'transparent',
                boxShadow: 'none',
                padding: 0,
                maxWidth: 'none',
              },
            },
          }}
        />
      </CloudflareGate>
    </BrowserRouter>
  </StrictMode>,
);
