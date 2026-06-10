import { api } from './api'
import type { LoginData, RegisterData } from '../types/auth'
import { catchResult } from '#/helpers/ServiceResult'

export const userService = {
  login: async (loginData: LoginData) => {
    return catchResult(api.post('/auth/login', loginData))
  },

  register: async (registerData: RegisterData) => {
    return catchResult(api.post('/auth/register', registerData))
  },
}
