import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { Toaster } from "react-hot-toast";
import "./i18n";
import "./index.css";
import App from "./App";
import CloudflareGate from "./components/common/CloudflareGate";

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
            // Custom renders (our beautiful toast cards) handle their own styling
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
