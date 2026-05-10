import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";
import path from "path";

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "");

  const API_TARGET = env.VITE_API_TARGET ?? "http://localhost:5038";
  const UPLOADS_TARGET = env.VITE_UPLOADS_TARGET ?? API_TARGET;
  const HUBS_TARGET = env.VITE_HUBS_TARGET ?? API_TARGET;

  return {
    plugins: [react(), tailwindcss()],
    build: {
      sourcemap: false,
      // Manual chunk splitting improves cache longevity:
      // vendor libs change infrequently → long-lived browser cache.
      // Page chunks change often → short cache with content-hash busting.
      rollupOptions: {
        output: {
          manualChunks(id: string) {
            // Core React runtime — almost never changes
            if (id.includes('node_modules/react/') || id.includes('node_modules/react-dom/')) {
              return 'vendor-react';
            }
            // React Router — changes infrequently
            if (id.includes('node_modules/react-router')) {
              return 'vendor-router';
            }
            // Framer Motion — large; isolate to avoid polluting main bundle
            if (id.includes('node_modules/framer-motion')) {
              return 'vendor-motion';
            }
            // SignalR — large; only needed post-login
            if (id.includes('node_modules/@microsoft/signalr')) {
              return 'vendor-signalr';
            }
            // Stripe — large; only needed on payment pages
            if (id.includes('node_modules/@stripe')) {
              return 'vendor-stripe';
            }
            // i18n
            if (id.includes('node_modules/i18next') || id.includes('node_modules/react-i18next')) {
              return 'vendor-i18n';
            }
          },
        },
      },
    },
    resolve: {
      alias: {
        "@": path.resolve(__dirname, "./src"),
      },
      dedupe: ["react", "react-dom", "react-dom/client"],
    },
    server: {
      headers: {
        "Cross-Origin-Opener-Policy": "same-origin-allow-popups",
        "Cross-Origin-Embedder-Policy": "unsafe-none",
        // Security headers for the Vite dev server
        "X-Content-Type-Options": "nosniff",
        "X-Frame-Options": "DENY",
        "Referrer-Policy": "strict-origin-when-cross-origin",
        "X-XSS-Protection": "0",
        "Permissions-Policy": "camera=(), microphone=(), geolocation=()",
      },
      proxy: {
        "/api": {
          target: API_TARGET,
          changeOrigin: true,
          secure: false,
        },
        "/hubs": {
          target: HUBS_TARGET,
          changeOrigin: true,
          secure: false,
          ws: true,
        },
        "/uploads": {
          target: UPLOADS_TARGET,
          changeOrigin: true,
          secure: false,
        },
      },
    },
  };
});
