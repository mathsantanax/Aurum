import { useEffect, useState } from "react";
import type { FormEvent } from "react";
import { Link, useLocation, useNavigate, useSearchParams } from "react-router-dom";
import { resetPassword } from "../api/authApi";
import { getAuthErrorMessage } from "../utils/getAuthErrorMessage";

export function ResetPasswordPage() {
  const [searchParams] = useSearchParams();
  const location = useLocation();
  const navigate = useNavigate();
  const [email, setEmail] = useState(searchParams.get("email") ?? "");
  const [resetCode] = useState(
    searchParams.get("resetCode") ?? searchParams.get("code") ?? "",
  );
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isComplete, setIsComplete] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  useEffect(() => {
    if (searchParams.has("resetCode") || searchParams.has("code")) {
      navigate(
        {
          pathname: location.pathname,
          search: email ? `?email=${encodeURIComponent(email)}` : "",
        },
        { replace: true, state: location.state },
      );
    }
  }, [email, location.pathname, location.state, navigate, searchParams]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setErrorMessage(null);
    if (password !== confirmPassword) {
      setErrorMessage("As senhas não coincidem.");
      return;
    }
    setIsSubmitting(true);
    try {
      await resetPassword(email, resetCode, password);
      setIsComplete(true);
    } catch (error) {
      setErrorMessage(
        getAuthErrorMessage(
          error,
          "Não foi possível alterar sua senha. Solicite um novo link e tente novamente.",
        ),
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="rounded-[28px] border border-white/70 bg-white p-6 text-slate-950 shadow-2xl shadow-black/20 sm:p-9">
      <div className="mb-7">
        <p className="text-xs font-bold uppercase tracking-[0.18em] text-violet-800">Quase lá</p>
        <h2 className="mt-3 text-2xl font-semibold tracking-tight sm:text-3xl">Defina uma nova senha</h2>
        <p className="mt-2 text-sm leading-6 text-slate-600">Escolha uma senha forte para proteger sua conta.</p>
      </div>

      {isComplete ? (
        <div role="status" className="rounded-2xl border border-emerald-200 bg-emerald-50 p-5 text-sm leading-6 text-emerald-900">
          Sua senha foi alterada. Agora você já pode entrar na sua conta.
        </div>
      ) : (
        <>
        {!resetCode && !isComplete && (
          <div role="alert" className="mb-5 rounded-xl border border-amber-200 bg-amber-50 p-3 text-sm leading-5 text-amber-900">
            Este link de recuperação está incompleto. Solicite um novo e-mail para redefinir sua senha.
          </div>
        )}
        <form onSubmit={handleSubmit} className="space-y-4" aria-busy={isSubmitting}>
          <div>
            <label htmlFor="reset-email" className="mb-2 block text-sm font-semibold text-slate-800">E-mail da conta</label>
            <input
              id="reset-email"
              name="email"
              type="email"
              autoComplete="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              required
              className="min-h-12 w-full rounded-xl border border-slate-300 px-4 outline-none transition focus:border-violet-700 focus:ring-4 focus:ring-violet-100"
            />
          </div>
          <div>
            <label htmlFor="reset-password" className="mb-2 block text-sm font-semibold text-slate-800">Nova senha</label>
            <input
              id="reset-password"
              name="newPassword"
              type="password"
              autoComplete="new-password"
              minLength={8}
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              required
              className="min-h-12 w-full rounded-xl border border-slate-300 px-4 outline-none transition focus:border-violet-700 focus:ring-4 focus:ring-violet-100"
              placeholder="Mínimo de 8 caracteres"
            />
          </div>
          <div>
            <label htmlFor="reset-confirm-password" className="mb-2 block text-sm font-semibold text-slate-800">Confirme a nova senha</label>
            <input
              id="reset-confirm-password"
              name="confirmPassword"
              type="password"
              autoComplete="new-password"
              minLength={8}
              value={confirmPassword}
              onChange={(event) => setConfirmPassword(event.target.value)}
              required
              className="min-h-12 w-full rounded-xl border border-slate-300 px-4 outline-none transition focus:border-violet-700 focus:ring-4 focus:ring-violet-100"
              placeholder="Digite novamente"
            />
          </div>
          {errorMessage && (
            <div role="alert" className="rounded-xl border border-rose-200 bg-rose-50 p-3 text-sm leading-5 text-rose-800">{errorMessage}</div>
          )}
          <button
            type="submit"
            disabled={isSubmitting || !resetCode}
            className="flex min-h-12 w-full items-center justify-center rounded-xl bg-violet-900 px-4 text-sm font-semibold text-white transition hover:bg-violet-800 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-800 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {isSubmitting ? "Atualizando..." : "Alterar senha"}
          </button>
        </form>
        </>
      )}
      <Link to="/login" className="mt-6 block text-center text-sm font-semibold text-violet-800 underline-offset-4 hover:underline">
        Voltar para o login
      </Link>
    </section>
  );
}