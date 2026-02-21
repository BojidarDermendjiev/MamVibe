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
    resolve: {
      alias: {
        "@": path.resolve(__dirname, "./src"),
      },
    },
    server: {
      headers: {
        "Cross-Origin-Opener-Policy": "same-origin-allow-popups",
        "Cross-Origin-Embedder-Policy": "unsafe-none",
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
