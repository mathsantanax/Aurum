import { Link, Navigate, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "../features/auth/hooks/useAuth";
import { ThemeSelect } from "../components/ThemeSelect";

export function AuthLayout() {
  const { isAuthenticated, isLoading } = useAuth();
  const location = useLocation();
  const from = (
    location.state as
      | { from?: { pathname?: string; search?: string; hash?: string } }
      | null
  )?.from;
  const returnTo =
    from?.pathname?.startsWith("/") && !from.pathname.startsWith("//")
      ? { pathname: from.pathname, search: from.search, hash: from.hash }
      : "/dashboard";

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-[#f7f7fb] text-sm font-medium text-slate-700">
          <span className="mr-3 size-5 animate-spin rounded-full border-2 border-slate-300 border-t-violet-700" />
        Preparando seu espaço...
      </div>
    );
  }

  if (isAuthenticated) {
    return <Navigate to={returnTo} replace />;
  }

  return (
    <main className="relative isolate min-h-screen overflow-hidden bg-[#f7f7fb] text-slate-900">
      <div className="absolute right-5 top-5 z-20 sm:right-8 sm:top-8">
        <ThemeSelect />
      </div>
      <div aria-hidden="true" className="pointer-events-none absolute inset-0 -z-10 overflow-hidden">
        <div className="absolute -left-40 -top-48 size-[34rem] rounded-full bg-violet-700/10 blur-[120px]" />
        <div className="absolute -bottom-64 right-[-8rem] size-[36rem] rounded-full bg-amber-400/10 blur-[140px]" />
      </div>
      <div className="mx-auto grid min-h-screen max-w-7xl items-center gap-12 px-5 py-20 sm:px-8 lg:grid-cols-[1.05fr_0.95fr] lg:gap-20 lg:px-12">
        <section className="mx-auto w-full max-w-xl lg:py-12">
          <Link to="/login" className="inline-flex items-center gap-3" aria-label="Aurum, entrar na conta">
            <span className="flex size-11 items-center justify-center rounded-2xl bg-amber-300 text-lg font-black text-[#17131d] shadow-lg shadow-amber-400/15">
              A
            </span>
            <span className="text-xl font-semibold tracking-tight">Aurum</span>
          </Link>
          <div className="mt-10 sm:mt-14">
            <p className="text-xs font-bold uppercase tracking-[0.24em] text-violet-800">
              Clareza para suas escolhas
            </p>
            <h1 className="mt-5 max-w-lg text-4xl font-semibold leading-tight tracking-tight text-slate-950 sm:text-5xl">
              Seu dinheiro, com mais intenção.
            </h1>
            <p className="mt-5 max-w-lg text-base leading-7 text-slate-600">
              Reúna seus espaços financeiros e acompanhe sua vida financeira em um só lugar.
            </p>
          </div>
          <div className="mt-9 grid gap-3 text-sm text-slate-600 sm:grid-cols-2">
            <div className="flex items-center gap-3 rounded-2xl border border-slate-200 bg-white/70 p-4">
              <span className="flex size-8 shrink-0 items-center justify-center rounded-xl bg-violet-50 text-violet-800">01</span>
              Organização simples
            </div>
            <div className="flex items-center gap-3 rounded-2xl border border-slate-200 bg-white/70 p-4">
              <span className="flex size-8 shrink-0 items-center justify-center rounded-xl bg-violet-50 text-violet-800">02</span>
              Acesso protegido
            </div>
          </div>
          <p className="mt-12 hidden text-xs text-slate-500 lg:block">
            Um espaço mais claro para cuidar do que importa.
          </p>
        </section>
        <section className="mx-auto w-full max-w-md lg:py-12">
          <Outlet />
        </section>
      </div>
    </main>
  );
}