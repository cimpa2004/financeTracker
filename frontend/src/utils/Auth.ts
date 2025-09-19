import type { LoginResponseData } from '../types/Auth';
import type {User} from "../types/User";

export function saveLoginData(data: LoginResponseData) {
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('accessTokenExpires', data.accessTokenExpires);
    localStorage.setItem('refreshToken', data.refreshToken);
    localStorage.setItem('refreshTokenExpires', data.refreshTokenExpires);
    localStorage.setItem('user', JSON.stringify(data.user));
}

export function getStoredAuthData(): { token: string; refreshToken: string; user: User} | null {
    const token = localStorage.getItem('accessToken');
    const refreshToken = localStorage.getItem('refreshToken');
    const user = localStorage.getItem('user');

    if (!token || !refreshToken || !user) {
        return null;
    }

    return {
        token,
        refreshToken,
        user: JSON.parse(user),
    };
}
