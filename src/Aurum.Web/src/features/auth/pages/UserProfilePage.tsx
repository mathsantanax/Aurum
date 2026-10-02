import { useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { updateCurrentUser } from "../api/authApi";
import { useAuth } from "../hooks/useAuth";
import { getAuthErrorMessage } from "../utils/getAuthErrorMessage";

interface UserProfilePageProps {
  isCompletingProfile?: boolean;
}

export function UserProfilePage({
  isCompletingProfile = false,
}: UserProfilePageProps) {
  const navigate = useNavigate();
  const { user, refreshUser } = useAuth();
  const [fullName, setFullName] = useState(user?.fullName ?? "");
  const [phoneNumber, setPhoneNumber] = useState(user?.phoneNumber ?? "");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isSaved, setIsSaved] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setErrorMessage(null);
    setIsSaved(false);
    setIsSubmitting(true);

    try {
      await updateCurrentUser({
        fullName: fullName.trim(),
        phoneNumber: phoneNumber.trim(),
      });

      const updatedUser = await refreshUser();
      if (!updatedUser) {
        throw new Error("Não foi possível atualizar os dados da sua conta.");
      }

      setFullName(updatedUser.fullName ?? "");
      setPhoneNumber(updatedUser.phoneNumber ?? "");
      if (isCompletingProfile) {
        navigate("/dashboard", { replace: true });
      } else {
        setIsSaved(true);
      }
    } catch (error) {
      setErrorMessage(
        getAuthErrorMessage(
          error,
          "Não foi possível salvar seus dados. Confira as informações e tente novamente.",
        ),
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="flex min-h-screen items-center justify-center bg-[#f7f7fb] px-4 py-10 text-slate-950">
      <section className="w-full max-w-xl rounded-[28px] border border-white/70 bg-white p-6 shadow-2xl shadow-black/10 sm:p-9">
        <p className="text-xs font-bold uppercase tracking-[0.18em] text-violet-800">
          {isCompletingProfile ? "Só mais um passo" : "Sua conta"}
        </p>
        <h1 className="mt-3 text-2xl font-semibold tracking-tight sm:text-3xl">
          {isCompletingProfile ? "Finalize seu cadastro" : "Meus dados"}
        </h1>
        <p className="mt-2 text-sm leading-6 text-slate-600">
          {isCompletingProfile
            ? "Complete seu perfil para começar a usar o Aurum."
            : "Consulte e atualize as informações do seu perfil."}
        </p>

        <form onSubmit={handleSubmit} className="mt-7 space-y-5" aria-busy={isSubmitting}>
          <div>
            <label htmlFor="profile-full-name" className="mb-2 block text-sm font-semibold text-slate-800">
              Nome completo
            </label>
            <input
              id="profile-full-name"
              name="fullName"
              type="text"
              autoComplete="name"
              minLength={2}
              maxLength={250}
              value={fullName}
              onChange={(event) => setFullName(event.target.value)}
              required
              className="min-h-12 w-full rounded-xl border border-slate-300 bg-white px-4 text-base outline-none transition focus:border-violet-700 focus:ring-4 focus:ring-violet-100"
              placeholder="Seu nome completo"
            />
          </div>

          <div>
            <label htmlFor="profile-email" className="mb-2 block text-sm font-semibold text-slate-800">
              E-mail
            </label>
            <input
              id="profile-email"
              name="email"
              type="email"
              autoComplete="email"
              value={user?.email ?? ""}
              readOnly
              className="min-h-12 w-full rounded-xl border border-slate-200 bg-slate-50 px-4 text-base text-slate-600"
            />
            <p className="mt-2 text-xs text-slate-500">Este e-mail é usado para entrar na sua conta.</p>
          </div>

          <div>
            <label htmlFor="profile-phone" className="mb-2 block text-sm font-semibold text-slate-800">
              Telefone
            </label>
            <input
              id="profile-phone"
              name="phoneNumber"
              type="tel"
              autoComplete="tel"
              maxLength={32}
              value={phoneNumber}
              onChange={(event) => setPhoneNumber(event.target.value)}
              required
              className="min-h-12 w-full rounded-xl border border-slate-300 bg-white px-4 text-base outline-none transition focus:border-violet-700 focus:ring-4 focus:ring-violet-100"
              placeholder="(11) 99999-9999"
            />
          </div>

          {errorMessage && (
            <div role="alert" className="rounded-xl border border-rose-200 bg-rose-50 p-3 text-sm leading-5 text-rose-800">
              {errorMessage}
            </div>
          )}
          {isSaved && (
            <div role="status" className="rounded-xl border border-emerald-200 bg-emerald-50 p-3 text-sm leading-5 text-emerald-900">
              Seus dados foram atualizados.
            </div>
          )}

          <button
            type="submit"
            disabled={isSubmitting}
            className="flex min-h-12 w-full items-center justify-center rounded-xl bg-violet-900 px-4 text-sm font-semibold text-white transition hover:bg-violet-800 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-800 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {isSubmitting
              ? "Salvando..."
              : isCompletingProfile
                ? "Concluir cadastro"
                : "Salvar alterações"}
          </button>
        </form>

        {!isCompletingProfile && (
          <Link to="/dashboard" className="mt-5 block text-center text-sm font-semibold text-violet-800 underline-offset-4 hover:underline">
            Voltar à visão geral
          </Link>
        )}
      </section>
    </main>
  );
}
