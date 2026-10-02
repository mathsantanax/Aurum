import axios from "axios";

export function getAuthErrorMessage(
  error: unknown,
  fallback: string,
  unauthorizedMessage?: string,
) {
  if (!axios.isAxiosError(error)) {
    return fallback;
  }

  const status = error.response?.status;
  if (status === 401) {
    return unauthorizedMessage ?? fallback;
  }
  if (status === 403) {
    return "Esta ação não está disponível para sua conta no momento.";
  }
  if (status === 429) {
    return "Muitas tentativas em pouco tempo. Aguarde um instante e tente novamente.";
  }
  if (!error.response) {
    return "Não foi possível conectar à API. Verifique sua conexão e tente novamente.";
  }

  const data: unknown = error.response.data;
  if (typeof data === "string" && data.trim()) {
    return data;
  }
  if (data && typeof data === "object") {
    const details = data as Record<string, unknown>;
    for (const key of ["detail", "message", "title"]) {
      if (typeof details[key] === "string") {
        return details[key];
      }
    }
    if (details.errors && typeof details.errors === "object") {
      return "Verifique os campos informados e tente novamente.";
    }
  }

  return fallback;
}
