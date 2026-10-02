import { useCallback, useEffect, useState } from "react";
import type { ReactNode } from "react";
import axios from "axios";
import { getCurrentUser } from "../../features/auth/api/authApi";
import { AuthContext } from "./authContext";
import type { CurrentUser } from "../../features/auth/types/auth.types";

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<CurrentUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [authError, setAuthError] = useState<string | null>(null);

  const refreshUser = useCallback(async () => {
    try {
      const currentUser = await getCurrentUser();
      setUser(currentUser);
      setAuthError(null);
      return currentUser;
    } catch (error) {
      setUser(null);
      if (axios.isAxiosError(error) && error.response?.status === 401) {
        setAuthError(null);
        return null;
      }

      setAuthError(
        "Não foi possível validar sua sessão. Confira sua conexão com a API e tente novamente.",
      );
      return null;
    }
  }, []);

  const clearUser = useCallback(() => {
    setUser(null);
    setAuthError(null);
  }, []);

  useEffect(() => {
    async function loadUser() {
      await refreshUser();
      setIsLoading(false);
    }

    void loadUser().catch(() => {
      setAuthError(
        "Não foi possível validar sua sessão. Confira sua conexão com a API e tente novamente.",
      );
      setIsLoading(false);
    });
  }, [refreshUser]);

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: user !== null,
        isLoading,
        authError,
        refreshUser,
        clearUser,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}