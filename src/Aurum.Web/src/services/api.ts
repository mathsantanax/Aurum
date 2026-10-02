import axios from "axios";

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? "https://localhost:8081/api",
  withCredentials: true,
  headers: {
    "Content-Type": "application/json",
  },
});

let csrfToken: string | null = null;
let csrfTokenRequest: Promise<string> | null = null;

declare module "axios" {
  interface AxiosRequestConfig {
    csrfRetry?: boolean;
  }
}

function getCsrfToken() {
  if (csrfToken) {
    return Promise.resolve(csrfToken);
  }

  if (!csrfTokenRequest) {
    csrfTokenRequest = api
      .get<{ requestToken: string }>("/auth/csrf")
      .then(({ data }) => {
        csrfToken = data.requestToken;
        return csrfToken;
      })
      .finally(() => {
        csrfTokenRequest = null;
      });
  }

  return csrfTokenRequest;
}

api.interceptors.request.use(async (config) => {
  const method = config.method?.toLowerCase() ?? "get";
  const isSafeMethod = ["get", "head", "options", "trace"].includes(method);
  const path = config.url?.split("?")[0].replace(/^\/+/, "").toLowerCase();
  const isPublicIdentityRequest =
    path === "auth/login" || path === "auth/register";

  if (isSafeMethod || isPublicIdentityRequest) {
    return config;
  }

  config.headers.set("X-CSRF-TOKEN", await getCsrfToken());
  return config;
});

api.interceptors.response.use(undefined, async (error: unknown) => {
  if (!axios.isAxiosError(error)) {
    return Promise.reject(error);
  }

  const config = error.config;
  const responseData: unknown = error.response?.data;
  const isCsrfFailure =
    responseData !== null &&
    typeof responseData === "object" &&
    "title" in responseData &&
    responseData.title === "A validação de segurança da requisição falhou.";

  if (
    error.response?.status !== 400 ||
    !config ||
    config.csrfRetry ||
    !isCsrfFailure
  ) {
    return Promise.reject(error);
  }

  csrfToken = null;
  config.csrfRetry = true;
  config.headers.set("X-CSRF-TOKEN", await getCsrfToken());
  return api.request(config);
});

export function clearCsrfToken() {
  csrfToken = null;
}