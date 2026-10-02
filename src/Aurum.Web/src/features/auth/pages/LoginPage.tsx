import { useState } from "react";
import type { FormEvent } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";

import { useLogin } from "../hooks/useMutations";
import { useAuth } from "../hooks/useAuth";
import { getAuthErrorMessage } from "../utils/getAuthErrorMessage";

export function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();

  const loginMutation = useLogin();
  const { refreshUser, authError } = useAuth();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setErrorMessage(null);
    setIsSubmitting(true);

    try {
      await loginMutation.mutateAsync({
        email,
        password,
      });
      const currentUser = await refreshUser();
      if (!currentUser) {
        throw new Error("Não foi possível carregar os dados da sua conta.");
      }

      const from = (
        location.state as
          | { from?: { pathname?: string; search?: string; hash?: string } }
          | null
      )?.from;
      const destination =
        from?.pathname?.startsWith("/") && !from.pathname.startsWith("//")
          ? { pathname: from.pathname, search: from.search, hash: from.hash }
          : "/dashboard";
      navigate(destination, { replace: true });
    } catch (error) {
      setErrorMessage(
        getAuthErrorMessage(
          error,
          "Não foi possível entrar. Confira seus dados e tente novamente.",
          "E-mail ou senha incorretos. Confira seus dados e tente novamente.",
        ),
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="rounded-[28px] border border-white/70 bg-white p-6 text-slate-950 shadow-2xl shadow-black/20 sm:p-9">
      <div className="mb-8">
        <p className="text-xs font-bold uppercase tracking-[0.18em] text-violet-800">
          Acesse sua conta
        </p>
        <h2 className="mt-3 text-2xl font-semibold tracking-tight sm:text-3xl">
          Que bom ter você de volta
        </h2>
        <p className="mt-2 text-sm leading-6 text-slate-600">
          Entre para continuar cuidando das suas finanças.
        </p>
      </div>

      {authError && (
        <div role="alert" className="mb-5 rounded-xl border border-amber-200 bg-amber-50 p-3 text-sm leading-5 text-amber-900">
          {authError}
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-5" aria-busy={isSubmitting}>
        <div>
          <label htmlFor="login-email" className="mb-2 block text-sm font-semibold text-slate-800">
            E-mail
          </label>
          <input
            id="login-email"
            name="email"
            type="email"
            autoComplete="username"
            inputMode="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            required
            className="min-h-12 w-full rounded-xl border border-slate-300 bg-white px-4 text-base text-slate-950 outline-none transition placeholder:text-slate-400 hover:border-slate-400 focus:border-violet-700 focus:ring-4 focus:ring-violet-100"
            placeholder="voce@email.com"
          />
        </div>

        <div>
          <div className="mb-2 flex items-center justify-between gap-3">
            <label htmlFor="login-password" className="text-sm font-semibold text-slate-800">
              Senha
            </label>
            <Link
              to="/forgot-password"
              className="text-sm font-semibold text-violet-800 underline-offset-4 hover:text-violet-700 hover:underline focus-visible:rounded focus-visible:outline-2 focus-visible:outline-violet-700"
            >
              Esqueci a senha
            </Link>
          </div>
          <div className="relative">
            <input
              id="login-password"
              name="password"
              type={showPassword ? "text" : "password"}
              autoComplete="current-password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              required
              className="min-h-12 w-full rounded-xl border border-slate-300 bg-white px-4 pr-20 text-base text-slate-950 outline-none transition placeholder:text-slate-400 hover:border-slate-400 focus:border-violet-700 focus:ring-4 focus:ring-violet-100"
              placeholder="Sua senha"
            />
            <button
              type="button"
              onClick={() => setShowPassword((visible) => !visible)}
              aria-label={showPassword ? "Ocultar senha" : "Mostrar senha"}
              aria-pressed={showPassword}
              className="absolute inset-y-0 right-3 my-auto h-9 rounded-lg px-2 text-xs font-semibold text-violet-800 hover:bg-violet-50 focus-visible:outline-2 focus-visible:outline-violet-700"
            >
              {showPassword ? "Ocultar" : "Mostrar"}
            </button>
          </div>
        </div>

        {errorMessage && (
          <div role="alert" className="rounded-xl border border-rose-200 bg-rose-50 p-3 text-sm leading-5 text-rose-800">
            {errorMessage}
          </div>
        )}

        <button
          type="submit"
          disabled={isSubmitting || loginMutation.isPending}
          className="flex min-h-12 w-full items-center justify-center gap-2 rounded-xl bg-violet-900 px-4 text-sm font-semibold text-white shadow-lg shadow-violet-900/15 transition hover:bg-violet-800 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-800 disabled:cursor-not-allowed disabled:opacity-60"
        >
          {(isSubmitting || loginMutation.isPending) && (
            <span className="size-4 animate-spin rounded-full border-2 border-white/40 border-t-white" />
          )}
          {isSubmitting || loginMutation.isPending ? "Entrando..." : "Entrar na conta"}
        </button>
      </form>

      <div className="my-7 flex items-center gap-4">
        <div className="h-px flex-1 bg-slate-200" />
        <span className="text-xs font-medium text-slate-500">Ainda não tem uma conta?</span>
        <div className="h-px flex-1 bg-slate-200" />
      </div>
      <Link
        to="/register"
        className="flex min-h-12 w-full items-center justify-center rounded-xl border border-slate-300 px-4 text-sm font-semibold text-slate-800 transition hover:border-violet-400 hover:bg-violet-50 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-700"
      >
        Criar uma conta
      </Link>
      <p className="mt-6 text-center text-xs leading-5 text-slate-500">
        Sua sessão é protegida e mantida em um cookie seguro.
      </p>
    </section>
  );
}