import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useLocation, useSearchParams } from "react-router-dom";
import { confirmEmail, resendConfirmationEmail } from "../api/authApi";
import { getAuthErrorMessage } from "../utils/getAuthErrorMessage";

export function ConfirmEmailPage() {
  const [searchParams] = useSearchParams();
  const location = useLocation();
  const email =
    searchParams.get("email") ??
    (location.state as { email?: string } | null)?.email;
  const userId = searchParams.get("userId");
  const code = searchParams.get("code");
  const changedEmail = searchParams.get("changedEmail") ?? undefined;
  const [status, setStatus] = useState<"checking" | "confirmed" | "pending" | "error">(
    userId && code ? "checking" : "pending",
  );
  const [isResending, setIsResending] = useState(false);
  const [resendMessage, setResendMessage] = useState<string | null>(null);
  const [resendError, setResendError] = useState<string | null>(null);

  useEffect(() => {
    if (!userId || !code) {
      return;
    }

    let isCurrent = true;
    void confirmEmail(userId, code, changedEmail)
      .then(() => {
        if (isCurrent) setStatus("confirmed");
      })
      .catch(() => {
        if (isCurrent) setStatus("error");
      });
    return () => {
      isCurrent = false;
    };
  }, [changedEmail, code, userId]);

  async function handleResend() {
    if (!email) {
      return;
    }

    setIsResending(true);
    setResendMessage(null);
    setResendError(null);
    try {
      await resendConfirmationEmail(email);
      setResendMessage("Se a conta ainda precisar de confirmação, enviaremos um novo link.");
    } catch (error) {
      setResendError(
        getAuthErrorMessage(error, "Não foi possível enviar um novo link. Tente novamente."),
      );
    } finally {
      setIsResending(false);
    }
  }

  return (
    <section className="rounded-[28px] border border-white/70 bg-white p-6 text-center text-slate-950 shadow-2xl shadow-black/20 sm:p-9">
      <div className={`mx-auto mb-5 flex size-14 items-center justify-center rounded-2xl text-xl font-bold ${
        status === "error" ? "bg-rose-50 text-rose-700" : "bg-emerald-50 text-emerald-700"
      }`}>
        {status === "checking" ? <span className="size-5 animate-spin rounded-full border-2 border-emerald-200 border-t-emerald-700" /> : status === "error" ? "!" : "✓"}
      </div>
      <p className="text-xs font-bold uppercase tracking-[0.18em] text-violet-800">Ativação da conta</p>
      <h2 className="mt-3 text-2xl font-semibold tracking-tight">
        {status === "confirmed" ? "E-mail confirmado" : status === "error" ? "Não foi possível confirmar" : "Confirme seu e-mail"}
      </h2>
      <p className="mt-3 text-sm leading-6 text-slate-600">
        {status === "checking"
          ? "Estamos validando seu link de confirmação."
          : status === "confirmed"
            ? "Seu endereço foi confirmado com sucesso. Entre para começar a usar a Aurum."
            : status === "error"
              ? "Não conseguimos validar este link. Ele pode ter expirado ou já ter sido utilizado. Solicite um novo link abaixo."
              : `Enviamos um link de confirmação${email ? ` para ${email}` : ""}. Abra-o para ativar sua conta.`}
      </p>
      {resendMessage && (
        <p role="status" className="mt-4 rounded-xl border border-emerald-200 bg-emerald-50 p-3 text-sm text-emerald-900">
          {resendMessage}
        </p>
      )}
      {resendError && (
        <p role="alert" className="mt-4 rounded-xl border border-rose-200 bg-rose-50 p-3 text-sm text-rose-800">
          {resendError}
        </p>
      )}
      {status !== "confirmed" && email && (
        <button
          type="button"
          onClick={() => void handleResend()}
          disabled={isResending}
          className="mt-5 inline-flex min-h-11 w-full items-center justify-center rounded-xl border border-slate-300 px-4 text-sm font-semibold text-slate-700 transition hover:bg-slate-50 disabled:cursor-wait disabled:opacity-60"
        >
          {isResending ? "Enviando novo link..." : "Reenviar e-mail de confirmação"}
        </button>
      )}
      <Link
        to="/login"
        state={email ? { email } : undefined}
        className="mt-7 inline-flex min-h-12 items-center justify-center rounded-xl bg-violet-900 px-5 text-sm font-semibold text-white transition hover:bg-violet-800 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-800"
      >
        Ir para o login
      </Link>
    </section>
  );
}