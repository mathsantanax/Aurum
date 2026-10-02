import { createContext } from "react";
import type { CurrentUser } from "../../features/auth/types/auth.types";

export interface AuthContextData {
  user: CurrentUser | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  authError: string | null;
  refreshUser: () => Promise<CurrentUser | null>;
  clearUser: () => void;
}

export const AuthContext =
  createContext<AuthContextData | undefined>(undefined);