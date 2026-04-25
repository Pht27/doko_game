import { defineConfig, loadEnv } from 'vite'
import { fileURLToPath, URL } from 'node:url'
import { readFileSync } from 'node:fs'
import { resolve } from 'node:path'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import svgr from 'vite-plugin-svgr'
import { VitePWA } from 'vite-plugin-pwa'
import mkcert from 'vite-plugin-mkcert'

const pkg = JSON.parse(readFileSync('./package.json', 'utf-8')) as { version: string }
const releaseNotesMd = readFileSync(resolve(__dirname, '../../RELEASENOTES.md'), 'utf-8')

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  return {
  define: {
    __APP_VERSION__: JSON.stringify(pkg.version),
    __RELEASE_NOTES__: JSON.stringify(releaseNotesMd),
  },
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
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
        name: env.VITE_APP_NAME ?? 'Doppelkopf',
        short_name: env.VITE_APP_SHORT_NAME ?? 'Doko',
        description: 'A multiplayer card game',
        theme_color: '#ffffff',
        background_color: '#ffffff',
        display: 'fullscreen',
        orientation: 'any',
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
  }
})