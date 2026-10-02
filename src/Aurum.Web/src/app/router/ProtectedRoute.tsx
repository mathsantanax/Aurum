import { Link, Navigate, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "../../features/auth/hooks/useAuth";

export function ProtectedRoute() {
  const { isAuthenticated, isLoading, authError, refreshUser } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-[#f7f7fb]">
        <div className="flex items-center gap-3 text-sm font-medium text-slate-600">
          <span className="size-5 animate-spin rounded-full border-2 border-violet-200 border-t-violet-700" />
          Verificando sua sessão...
        </div>
      </div>
    );
  }

  if (authError) {
    return (
      <main className="flex min-h-screen items-center justify-center bg-[#f7f7fb] px-5">
        <section className="w-full max-w-md rounded-3xl border border-slate-200 bg-white p-8 text-center shadow-xl shadow-slate-200/60">
          <div className="mx-auto mb-5 flex size-12 items-center justify-center rounded-2xl bg-amber-50 text-xl text-amber-700">
            !
          </div>
          <h1 className="text-xl font-semibold text-slate-950">
            Não foi possível conectar
          </h1>
          <p className="mt-2 text-sm leading-6 text-slate-600">{authError}</p>
          <button
            type="button"
            onClick={() => void refreshUser()}
            className="mt-6 inline-flex min-h-11 items-center justify-center rounded-xl bg-violet-800 px-5 text-sm font-semibold text-white transition hover:bg-violet-700 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-700"
          >
            Tentar novamente
          </button>
          <Link
            to="/login"
            className="mt-4 block text-sm font-semibold text-violet-800 hover:text-violet-700"
          >
            Ir para o login
          </Link>
        </section>
      </main>
    );
  }

  if (!isAuthenticated) {
    return (
      <Navigate
        to="/login"
        replace
        state={{ from: location }}
      />
    );
  }

  return <Outlet />;
}