import type { LoginResponseData } from '../types/Auth';

export function saveLoginData(data: LoginResponseData) {
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('accessTokenExpires', data.accessTokenExpires);
    localStorage.setItem('refreshToken', data.refreshToken);
    localStorage.setItem('refreshTokenExpires', data.refreshTokenExpires);
    localStorage.setItem('user', JSON.stringify(data.user));
}