import legacy from '@vitejs/plugin-legacy'
import { defineConfig } from "vite";
import path from 'path';

export default defineConfig({
  assetsInclude: [
    'src/assets/**/*.png'
  ],
  build:
  {
    //sourcemap: true
  },
  plugins: [legacy({
    targets: ['ie >= 11'],
    additionalLegacyPolyfills: [
      'regenerator-runtime/runtime',
      '@webcomponents/webcomponentsjs',
      'core-js'
    ]
  })],
  server: {
    hmr: {
      host: "andy-desktop",
      port: 3501
    },
    proxy: {
      "/api": "https://daud.io",
    },
  }
});