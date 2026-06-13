// const VITE_BFF_PROXY_URL = import.meta.env.VITE_BFF_PROXY_URL

// if (!VITE_BFF_PROXY_URL) {
//   console.warn('Warning: VITE_BFF_PROXY_URL is undefined. Fallback applied.')
// }

export const bffProfileService = {
  // Fetch the current user from the BFF session
  async getMyProfile() {
    const response = await fetch('/bff/profiles/my-profile', {
      headers: { 'X-CSRF': '1' },
    })

    if (response.ok) {
      return await response.json() // Array of { type, value } claim objects
    }

    return null // Not authenticated
  },
}
