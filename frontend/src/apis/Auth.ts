import { httpService } from "../services/httpService";
import { type LoginData, LoginResponse, logoutResponse, type RegisterData } from "../types/Auth";
import { UserSchema } from "../types/User";
import { saveLoginData } from "../utils/Auth";

export async function login(loginData: LoginData) {
    const response = await httpService.post('login', LoginResponse, loginData);
    if (response) {
        saveLoginData(response);
    }
    return response;
}

export async function registerAccount(registerData: RegisterData) {
    const response = await httpService.post('register', UserSchema, registerData);
    return response;
}

//Not implemented on backend yet //TODO: implement on backend
export async function logout() {
    const response = await httpService.post('logout', logoutResponse);
    return response;
}

export async function refreshToken(refreshToken: string) {
    const response = await httpService.post('refresh', LoginResponse, { refreshToken });
    if (response) {
        saveLoginData(response);
    }
    return response;
}