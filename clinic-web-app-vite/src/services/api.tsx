import axios from 'axios';

const API_BASE_URL = 
    process.env.REACT_APP_API_URL || 
    process.env.services__AuthorizationAPI__http__0 ||
    process.env.services__AuthorizationAPI__1; 

export const api = axios.create({
    baseURL: API_BASE_URL
})
