import { api } from './api';
import type { LoginData } from '../types/auth';

export const userService = {
    login: async (loginData: LoginData) => {
        const response = await api.post('/auth/login', loginData);
        return response.data;
    }
}