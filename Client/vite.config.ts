import legacy from '@vitejs/plugin-legacy'
import { defineConfig } from "vite";
import path from 'path';
import { VitePWA } from 'vite-plugin-pwa'

export default defineConfig({
  build:
  {
    rollupOptions: {
      output: {
        manualChunks: {
          'babylon': [
            '@babylonjs/core',
          ]
        }
      }
    }
    //sourcemap: true
  },
  plugins: [
    VitePWA({
      includeAssets: [
        'assets/*',
        'img/*',
        'sounds/*'
      ],
      strategies: 'generateSW',
      registerType: 'autoUpdate',
      manifest: {
        "display": "fullscreen",
        "name": "DAUD.io",
        "short_name": "DAUD.io",
        "icons": [
          {
            "src": "/android-chrome-192x192.png",
            "sizes": "192x192",
            "type": "image/png"
          },
          {
            "src": "/android-chrome-512x512.png",
            "sizes": "512x512",
            "type": "image/png"
          }
        ],
        "theme_color": "#ffffff",
        "background_color": "#ffffff"
      }
    }),
    legacy({
      targets: ['ie >= 11'],
      additionalLegacyPolyfills: [
        '@webcomponents/webcomponentsjs',
        'core-js',
        'regenerator-runtime/runtime',
      ]
    })],
  server: {
    /*hmr: {
      host: "andy-desktop",
      port: 3501
    },*/
    proxy: {
      //"/api": "https://us.daud.io",
      "/api": "http://localhost:5100",
    },
  }
});