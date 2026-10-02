import { api, clearCsrfToken } from "../../../services/api";

import type {
  CurrentUser,
  LoginRequest,
  RegisterRequest,
} from "../types/auth.types";


export async function getCurrentUser(): Promise<CurrentUser> {
  const response = await api.get<CurrentUser>("/auth/me");

  return response.data;
}

export async function login(
  data: LoginRequest,
) {
  await api.post(
    "/auth/login",
    data,
    { params: { useCookies: true } },
  );
}

export async function register(
  data: RegisterRequest,
) {
  const response = await api.post(
    "/auth/register",
    data,
  );

  return response.data;
}

export async function logout() {
  await api.post("/auth/logout");
  clearCsrfToken();
}

export async function requestPasswordReset(email: string) {
  await api.post("/auth/forgotPassword", { email });
}

export async function resetPassword(
  email: string,
  resetCode: string,
  newPassword: string,
) {
  await api.post("/auth/resetPassword", {
    email,
    resetCode,
    newPassword,
  });
}

export async function confirmEmail(userId: string, code: string) {
  await api.get("/auth/confirmEmail", {
    params: { userId, code },
  });
}