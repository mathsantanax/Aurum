import { useState } from "react";
import type { FormEvent } from "react";
import { Link } from "react-router-dom";
import { requestPasswordReset } from "../api/authApi";
import { getAuthErrorMessage } from "../utils/getAuthErrorMessage";

export function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [hasSubmitted, setHasSubmitted] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setErrorMessage(null);
    setIsSubmitting(true);
    try {
      await requestPasswordReset(email);
      setHasSubmitted(true);
    } catch (error) {
      setErrorMessage(
        getAuthErrorMessage(
          error,
          "Não foi possível enviar as instruções. Tente novamente em instantes.",
        ),
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="rounded-[28px] border border-white/70 bg-white p-6 text-slate-950 shadow-2xl shadow-black/20 sm:p-9">
      <div className="mb-7">
        <p className="text-xs font-bold uppercase tracking-[0.18em] text-violet-800">Recuperação de acesso</p>
        <h2 className="mt-3 text-2xl font-semibold tracking-tight sm:text-3xl">Vamos recuperar sua senha</h2>
        <p className="mt-2 text-sm leading-6 text-slate-600">
          Informe o e-mail da sua conta. Se ela existir, enviaremos os próximos passos.
        </p>
      </div>

      {hasSubmitted ? (
        <div role="status" className="rounded-2xl border border-emerald-200 bg-emerald-50 p-5 text-sm leading-6 text-emerald-900">
          <p className="font-semibold">Confira sua caixa de entrada</p>
          <p className="mt-1">Se houver uma conta associada a {email}, você receberá instruções para redefinir sua senha.</p>
        </div>
      ) : (
        <form onSubmit={handleSubmit} className="space-y-5" aria-busy={isSubmitting}>
          <div>
            <label htmlFor="forgot-email" className="mb-2 block text-sm font-semibold text-slate-800">E-mail</label>
            <input
              id="forgot-email"
              name="email"
              type="email"
              autoComplete="email"
              inputMode="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              required
              className="min-h-12 w-full rounded-xl border border-slate-300 bg-white px-4 text-base outline-none transition placeholder:text-slate-400 focus:border-violet-700 focus:ring-4 focus:ring-violet-100"
              placeholder="voce@email.com"
            />
          </div>
          {errorMessage && (
            <div role="alert" className="rounded-xl border border-rose-200 bg-rose-50 p-3 text-sm leading-5 text-rose-800">
              {errorMessage}
            </div>
          )}
          <button
            type="submit"
            disabled={isSubmitting}
            className="flex min-h-12 w-full items-center justify-center rounded-xl bg-violet-900 px-4 text-sm font-semibold text-white transition hover:bg-violet-800 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-800 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {isSubmitting ? "Enviando..." : "Enviar instruções"}
          </button>
        </form>
      )}

      <Link to="/login" className="mt-6 block text-center text-sm font-semibold text-violet-800 underline-offset-4 hover:underline">
        Voltar para o login
      </Link>
    </section>
  );
}