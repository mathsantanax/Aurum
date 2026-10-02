import { useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useRegister } from "../hooks/useMutations";
import { getAuthErrorMessage } from "../utils/getAuthErrorMessage";

export function RegisterPage() {
  const navigate = useNavigate();
  const registerMutation = useRegister();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setErrorMessage(null);

    if (password !== confirmPassword) {
      setErrorMessage("As senhas não coincidem.");
      return;
    }

    setIsSubmitting(true);
    try {
      await registerMutation.mutateAsync({ email, password });
      navigate("/confirm-email", { replace: true, state: { email } });
    } catch (error) {
      setErrorMessage(
        getAuthErrorMessage(
          error,
          "Não foi possível criar sua conta. Confira os dados e tente novamente.",
        ),
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="rounded-[28px] border border-white/70 bg-white p-6 text-slate-950 shadow-2xl shadow-black/20 sm:p-9">
      <div className="mb-7">
        <p className="text-xs font-bold uppercase tracking-[0.18em] text-violet-800">Comece por aqui</p>
        <h2 className="mt-3 text-2xl font-semibold tracking-tight sm:text-3xl">Crie sua conta</h2>
        <p className="mt-2 text-sm leading-6 text-slate-600">
          Leva só um instante para começar a organizar sua vida financeira.
        </p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-4" aria-busy={isSubmitting}>
        <div>
          <label htmlFor="register-email" className="mb-2 block text-sm font-semibold text-slate-800">E-mail</label>
          <input
            id="register-email"
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

        <div>
          <label htmlFor="register-password" className="mb-2 block text-sm font-semibold text-slate-800">Senha</label>
          <input
            id="register-password"
            name="password"
            type="password"
            autoComplete="new-password"
            minLength={8}
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            required
            className="min-h-12 w-full rounded-xl border border-slate-300 bg-white px-4 text-base outline-none transition placeholder:text-slate-400 focus:border-violet-700 focus:ring-4 focus:ring-violet-100"
            placeholder="Mínimo de 8 caracteres"
          />
          <p className="mt-2 text-xs leading-5 text-slate-500">
            Use letras maiúsculas e minúsculas, número e caractere especial.
          </p>
        </div>

        <div>
          <label htmlFor="register-confirm-password" className="mb-2 block text-sm font-semibold text-slate-800">Confirme sua senha</label>
          <input
            id="register-confirm-password"
            name="confirmPassword"
            type="password"
            autoComplete="new-password"
            minLength={8}
            value={confirmPassword}
            onChange={(event) => setConfirmPassword(event.target.value)}
            required
            className="min-h-12 w-full rounded-xl border border-slate-300 bg-white px-4 text-base outline-none transition placeholder:text-slate-400 focus:border-violet-700 focus:ring-4 focus:ring-violet-100"
            placeholder="Digite a senha novamente"
          />
        </div>

        {errorMessage && (
          <div role="alert" className="rounded-xl border border-rose-200 bg-rose-50 p-3 text-sm leading-5 text-rose-800">
            {errorMessage}
          </div>
        )}
        <button
          type="submit"
          disabled={isSubmitting || registerMutation.isPending}
          className="mt-2 flex min-h-12 w-full items-center justify-center gap-2 rounded-xl bg-violet-900 px-4 text-sm font-semibold text-white transition hover:bg-violet-800 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-800 disabled:cursor-not-allowed disabled:opacity-60"
        >
          {isSubmitting || registerMutation.isPending ? "Criando sua conta..." : "Criar conta"}
        </button>
      </form>

      <p className="mt-6 text-center text-sm text-slate-600">
        Já tem uma conta?{" "}
        <Link
          to="/login"
          className="font-semibold text-violet-800 underline-offset-4 hover:underline"
        >
          Entrar
        </Link>
      </p>
    </section>
  );
}