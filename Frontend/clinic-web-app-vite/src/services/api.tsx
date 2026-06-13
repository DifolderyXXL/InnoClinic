import axios from 'axios'

const AUTHORIZATION_API_URL = import.meta.env.VITE_AUTHORIZATION_API_URL
const PROFILES_API_URL = import.meta.env.VITE_PROFILES_API_URL

if (!AUTHORIZATION_API_URL) {
  console.warn('Warning: AUTHORIZATION_API_URL is undefined. Fallback applied.')
}

if (!PROFILES_API_URL) {
  console.warn('Warning: PROFILES_API_URL is undefined. Fallback applied.')
}

export const authorizationApi = axios.create({
  baseURL: AUTHORIZATION_API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true,
})

export const profilesApi = axios.create({
  baseURL: PROFILES_API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true,
})
