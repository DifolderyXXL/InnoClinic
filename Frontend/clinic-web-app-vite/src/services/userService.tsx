import { authorizationApi } from './api'
import type { LoginData, RegisterData } from '../types/auth'
import { catchResult } from '#/helpers/ServiceResult'

export const userService = {
  myInfo: async () => {
    return catchResult(authorizationApi.get('/auth/manage/info'))
  },
  login: async (loginData: LoginData, rememberMe: boolean) => {
    return catchResult(
      authorizationApi.post('/core/auth/login', loginData, {
        params: {
          useCookies: true,
          rememberMe: rememberMe,
        },
      }),
    )
  },

  register: async (registerData: RegisterData, rememberMe: boolean) => {
    return catchResult(
      authorizationApi.post('/core/auth/register', registerData, {
        params: {
          useCookies: true,
          rememberMe: rememberMe,
        },
      }),
    )
  },
  logout: async () => {
    return catchResult(authorizationApi.post('/core/auth/logout'))
  },
}
