import legacy from '@vitejs/plugin-legacy'
import { defineConfig } from "vite";
import path from 'path';

export default defineConfig({
  build:
  {
    // rollupOptions: {
    //   output: {
    //     entryFileNames: `assets/[name].js`,
    //     chunkFileNames: `assets/[name].js`,
    //     assetFileNames: `assets/[name].[ext]`
    //   }
    // }    
    //sourcemap: true
  },
  plugins: [legacy({
    targets: ['ie >= 11'],
    additionalLegacyPolyfills: [
      '@webcomponents/webcomponentsjs',
      'core-js',
      'regenerator-runtime/runtime',
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