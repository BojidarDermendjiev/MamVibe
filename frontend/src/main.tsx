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
          toastOptions={{
            duration: 3000,
            style: {
              background: "#fff",
              color: "#3f4a7f",
              borderRadius: "12px",
              border: "1px solid #c1c4e3",
            },
          }}
        />
      </CloudflareGate>
    </BrowserRouter>
  </StrictMode>,
);
