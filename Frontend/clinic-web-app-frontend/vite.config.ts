import { fileURLToPath, URL } from 'node:url';
import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig(({ mode }) => {
  const localEnv = loadEnv(mode, process.cwd(), '');
  // Твой бэкенд BFF
  const target = localEnv.VITE_BFF_PROXY_URL || 'https://localhost:5001';

  return {
    plugins: [react()],
    resolve: {
      alias: {
        '@': fileURLToPath(new URL('./src', import.meta.url))
      }
    },
    server: {
      hmr: {
        protocol: 'ws',
        host: 'localhost',
        clientPort: 5173,
      },

      proxy: {
        // Проксируем авторизацию Duende
        '^/bff': {
          target,
          changeOrigin: true,
          secure: false
        },
        // Проксируем запросы к ProfilesAPI через BFF
        '^/api': {
          target,
          changeOrigin: true, // BFF сотрет куку и подставит Bearer JWT
          secure: false
        },
        // Колбэки Identity Server
        '^/signin-oidc': { target, secure: false },
        '^/signout-callback-oidc': { target, secure: false }
      }
    }
  };
});