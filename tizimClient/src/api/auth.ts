import { api } from "./axios";
import { type AccessTokenResponse, type RegisterResponse } from "@/types";

export const authApi = {
  login: (identifier: string, password: string) =>
    api.post<AccessTokenResponse>("/auth/login", { identifier, password }).then((r) => r.data),

  register: (email: string, password: string, organizationName: string) =>
    api.post<RegisterResponse>("/auth/register", { email, password, organizationName }).then((r) => r.data),

  logout: () => api.post("/auth/logout"),

  refresh: () => api.post<AccessTokenResponse>("/auth/refresh").then((r) => r.data),

  telegramLogin: (initData: string) =>
    api.post<AccessTokenResponse>("/auth/telegram", { initData }).then((r) => r.data),

  googleLogin: (idToken: string) =>
    api.post<AccessTokenResponse>("/auth/google", { idToken }).then((r) => r.data),
};
