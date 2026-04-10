import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import svgr from 'vite-plugin-svgr'
import { VitePWA } from 'vite-plugin-pwa'
import mkcert from 'vite-plugin-mkcert'

export default defineConfig({
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api/, ''),
      },
      '/hubs': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        ws: true,
      },
    },
  },
  plugins: [
    react(),
    tailwindcss(),
    svgr(),
    mkcert(),
    VitePWA({
      injectRegister: 'auto',
      devOptions: {
        enabled: true, // lets you test PWA in dev
      },
      manifest: {
        id: "/",
        name: 'Doppelkopf',
        short_name: 'Doko',
        description: 'A multiplayer card game',
        theme_color: '#ffffff',
        background_color: '#ffffff',
        display: 'standalone',
        start_url: '/',
        icons: [
          {
            src: '/icon192.png',
            sizes: '192x192',
            type: 'image/png'
          },
          {
            src: '/icon512.png',
            sizes: '512x512',
            type: 'image/png'
          }
        ],
        "screenshots": [
          {
            "src": "/screenshot.png",
            "sizes": "1920x1080",
            "type": "image/png",
            "form_factor": "wide",
            "label": "Application"
          },
          {
            "src": "/screenshot.png",
            "sizes": "1920x1080",
            "type": "image/png",
            "label": "Application"
          }
        ]        
      }
    }),
  ],
})