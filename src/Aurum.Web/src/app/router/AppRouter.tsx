import { Navigate, Route, Routes } from "react-router-dom";

import { AuthLayout } from "../../layouts/AuthLayout";
import { AppLayout } from "../../layouts/AppLayout";
import { ProtectedRoute } from "./ProtectedRoute";

import { LoginPage } from "../../features/auth/pages/LoginPage";
import { RegisterPage } from "../../features/auth/pages/RegisterPage";
import { ForgotPasswordPage } from "../../features/auth/pages/ForgotPasswordPage";
import { ResetPasswordPage } from "../../features/auth/pages/ResetPasswordPage";
import { ConfirmEmailPage } from "../../features/auth/pages/ConfirmEmailPage";
import { UserProfilePage } from "../../features/auth/pages/UserProfilePage";

import { DashboardPage } from "../../features/dashboard/pages/DashboardPage";
import { WalletspacesPage } from "../../features/walletspaces/pages/WalletspacesPage";
import { WalletspaceDetailPage } from "../../features/walletspaces/pages/WalletspaceDetailPage";
import { CreateWalletspacePage } from "../../features/walletspaces/pages/CreateWalletspacePage";

export function AppRouter() {
  return (
    <Routes>
      <Route element={<AuthLayout />}>
        <Route
          path="/login"
          element={<LoginPage />}
        />

        <Route
          path="/register"
          element={<RegisterPage />}
        />

        <Route
          path="/forgot-password"
          element={<ForgotPasswordPage />}
        />

        <Route
          path="/reset-password"
          element={<ResetPasswordPage />}
        />

        <Route
          path="/confirm-email"
          element={<ConfirmEmailPage />}
        />
      </Route>

      <Route element={<ProtectedRoute />}>
        <Route
          path="/complete-profile"
          element={<UserProfilePage isCompletingProfile />}
        />
        <Route element={<AppLayout />}>
          <Route
            path="/dashboard"
            element={<DashboardPage />}
          />
          <Route
            path="/my-data"
            element={<UserProfilePage />}
          />

          <Route
            path="/walletspaces"
            element={<WalletspacesPage />}
          />

          <Route
            path="/walletspaces/new"
            element={<CreateWalletspacePage />}
          />
          <Route
            path="/walletspaces/:walletspaceId"
            element={<WalletspaceDetailPage />}
          />
        </Route>
      </Route>

      <Route
        path="/"
        element={
          <Navigate
            to="/dashboard"
            replace
          />
        }
      />

      <Route
        path="*"
        element={
          <Navigate
            to="/dashboard"
            replace
          />
        }
      />
    </Routes>
  );
}