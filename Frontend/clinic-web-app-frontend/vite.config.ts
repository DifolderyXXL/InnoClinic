import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'


const config = defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');

  return {
    plugins: [react()],
    server: {
      port: 3001,
      proxy: {
        '/bff': {
          target: env.VITE_BFF_PROXY_URL,
          changeOrigin: true,
          secure: false
        },
        '/api': {
          target: env.VITE_BFF_PROXY_URL,
          changeOrigin: true,
          secure: false
        }
      }
    }
  }
})



export default config