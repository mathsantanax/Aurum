import { useState } from "react";
import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { useAuth } from "../features/auth/hooks/useAuth";
import { useLogout } from "../features/auth/hooks/useMutations";
import { getAuthErrorMessage } from "../features/auth/utils/getAuthErrorMessage";
import { ThemeSelect } from "../components/ThemeSelect";

const navigation = [
  {
    label: "Visão geral",
    to: "/dashboard",
    marker: "01",
  },
  {
    label: "Espaços financeiros",
    to: "/walletspaces",
    marker: "02",
  },
  {
    label: "Minhas finanças",
    to: "/finances",
    marker: "03",
  },
  {
    label: "Meus dados",
    to: "/my-data",
    marker: "04",
  },
];

export function AppLayout() {
  const navigate = useNavigate();
  const { user, clearUser } = useAuth();
  const logoutMutation = useLogout();
  const [logoutError, setLogoutError] = useState<string | null>(null);
  const accountName = user?.fullName?.trim() || user?.email || "Minha conta";
  const initials = accountName
    .split(/\s+/)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join("");

  async function handleLogout() {
    setLogoutError(null);
    try {
      await logoutMutation.mutateAsync();
      clearUser();
      navigate("/login", { replace: true });
    } catch (error) {
      setLogoutError(
        getAuthErrorMessage(
          error,
          "Não foi possível encerrar a sessão. Tente novamente.",
        ),
      );
    }
  }

  return (
    <div className="min-h-screen bg-[#f7f7fb] text-slate-900">
      <header className="sticky top-0 z-30 border-b border-slate-200/80 bg-white/90 backdrop-blur-xl">
        <div className="mx-auto flex h-[4.5rem] max-w-screen-2xl items-center justify-between gap-4 px-4 sm:px-7 lg:px-10">
          <NavLink to="/dashboard" className="inline-flex shrink-0 items-center gap-3" aria-label="Aurum, visão geral">
            <span className="flex size-9 items-center justify-center rounded-xl bg-violet-900 text-sm font-black text-amber-300 shadow-sm">A</span>
            <span className="text-lg font-semibold tracking-tight text-slate-950">Aurum</span>
          </NavLink>
          <div className="flex items-center gap-3">
            <ThemeSelect />
            <div className="hidden min-w-0 items-center gap-3 sm:flex">
              <span className="flex size-9 shrink-0 items-center justify-center rounded-full bg-violet-100 text-xs font-bold text-violet-900">
                {initials}
              </span>
              <div className="min-w-0">
                <p className="max-w-48 truncate text-sm font-semibold text-slate-800">{accountName}</p>
                <p className="text-xs text-slate-500">Sua conta</p>
              </div>
            </div>
            <button
              type="button"
              onClick={() => void handleLogout()}
              disabled={logoutMutation.isPending}
              className="inline-flex min-h-10 items-center justify-center rounded-xl border border-slate-300 px-3 text-sm font-semibold text-slate-700 transition hover:border-slate-400 hover:bg-slate-50 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-700 disabled:cursor-wait disabled:opacity-60 sm:px-4"
            >
              {logoutMutation.isPending ? "Saindo..." : "Sair"}
            </button>
          </div>
        </div>
      </header>

      <div className="mx-auto flex min-h-[calc(100vh-4.5rem)] max-w-screen-2xl">
        <aside className="hidden w-64 shrink-0 border-r border-slate-200/80 bg-white px-4 py-7 lg:block">
          <p className="px-3 text-[11px] font-bold uppercase tracking-[0.18em] text-slate-400">Seu espaço</p>
          <nav aria-label="Navegação principal" className="mt-4 space-y-1.5">
            {navigation.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                className={({ isActive }) =>
                  [
                    "flex min-h-11 items-center gap-3 rounded-xl px-3 text-sm font-semibold transition",
                    isActive
                      ? "bg-violet-50 text-violet-900"
                      : "text-slate-600 hover:bg-slate-50 hover:text-slate-950",
                  ].join(" ")
                }
              >
                <span className="flex size-7 items-center justify-center rounded-lg bg-white text-[10px] font-bold text-slate-500 shadow-sm">{item.marker}</span>
                <span>{item.label}</span>
              </NavLink>
            ))}
          </nav>
          <div className="mt-10 rounded-2xl border border-amber-200/70 bg-amber-50/70 p-4">
            <p className="text-sm font-semibold text-slate-900">Um passo de cada vez</p>
            <p className="mt-1 text-xs leading-5 text-slate-600">Organize seus espaços e acompanhe o que importa para você.</p>
          </div>
        </aside>

        <main className="min-w-0 flex-1 px-4 pb-28 pt-7 sm:px-7 sm:pt-9 lg:px-10 lg:pb-10">
          {logoutError && (
            <div role="alert" className="mx-auto mb-5 max-w-5xl rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-800">
              {logoutError}
            </div>
          )}
          <Outlet />
        </main>
      </div>
      <nav aria-label="Navegação principal" className="fixed inset-x-0 bottom-0 z-30 border-t border-slate-200 bg-white/95 px-3 pb-[max(0.75rem,env(safe-area-inset-bottom))] pt-2 backdrop-blur-xl lg:hidden">
        <div className="mx-auto grid max-w-lg grid-cols-4 gap-2">
          {navigation.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                [
                  "flex min-h-12 items-center justify-center gap-2 rounded-xl px-3 text-xs font-semibold transition",
                  isActive ? "bg-violet-50 text-violet-900" : "text-slate-600 hover:bg-slate-50",
                ].join(" ")
              }
            >
              <span className="text-[10px] font-bold opacity-60">{item.marker}</span>
              {item.label}
            </NavLink>
          ))}
        </div>
      </nav>
    </div>
  );
}