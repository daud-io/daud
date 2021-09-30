import { defineConfig } from "vite";
import { minifyHtml } from "vite-plugin-html";
import path from 'path';

export default defineConfig({
  assetsInclude: [
    'src/assets/**/*.png'
  ],
  plugins: [minifyHtml()],
  build: {
    rollupOptions: {
      input: {
        main: path.resolve(__dirname, 'index.html')
      }
    },
    lib: {
      formats: ['es'],
      entry: path.resolve(__dirname, 'src/boot.ts'),
      name: 'daud',
      fileName: (format) => `daud-boot.${format}.js`
    }
  },
  server: {
    proxy: {
      "/api": "https://daud.io",
    },
  }
});