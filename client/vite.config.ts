import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      "/api": {
        target: "http://actor-system:8080",
        changeOrigin: true,
      },
      "/swagger": {
      
        target: "http://actor-system:8080",
        changeOrigin: true,
      },
      "/ws": {
        target: "http://asteroids_signalr:8081",
        changeOrigin: true,
        ws: true,
      }
    },
  },
});
