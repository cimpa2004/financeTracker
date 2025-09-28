import { httpService } from "../services/httpService";
import { type LoginData, LoginResponse, logoutResponse, type RegisterData } from "../types/Auth";
import { UserSchema, type User } from "../types/User";
import { saveLoginData } from "../utils/Auth";

export async function login(loginData: LoginData, setAuthData: (token: string, refreshToken: string, user: User) => void) {
    const response = await httpService.post('login', LoginResponse, loginData);
    if (response) {
        saveLoginData(response);
        setAuthData(response.accessToken, response.refreshToken, response.user);
    }
    return response;
}

export async function registerAccount(registerData: RegisterData) {
    const response = await httpService.post('register', UserSchema, registerData);
    return response;
}

export async function logout() {
    const response = await httpService.post('logout', logoutResponse);
    return response;
}

export async function refreshToken(refreshToken: string) {
    const response = await httpService.post('auth/refresh', LoginResponse, { refreshToken });
    if (response) {
        saveLoginData(response);
    }
    return response;
}