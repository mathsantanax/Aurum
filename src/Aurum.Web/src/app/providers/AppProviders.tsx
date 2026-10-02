import {
  QueryClient,
  QueryClientProvider,
} from "@tanstack/react-query";

import type { ReactNode } from "react";
import { AuthProvider } from "./AuthProviders";

interface AppProvidersProps {
  children: ReactNode;
}

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

export function AppProviders({
  children,
}: AppProvidersProps) {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>{children}</AuthProvider>
    </QueryClientProvider>
  );
}