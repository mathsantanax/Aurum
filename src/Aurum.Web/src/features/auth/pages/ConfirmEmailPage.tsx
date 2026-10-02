import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useLocation, useSearchParams } from "react-router-dom";
import { confirmEmail } from "../api/authApi";

export function ConfirmEmailPage() {
  const [searchParams] = useSearchParams();
  const location = useLocation();
  const email = (location.state as { email?: string } | null)?.email;
  const userId = searchParams.get("userId");
  const code = searchParams.get("code");
  const [status, setStatus] = useState<"checking" | "confirmed" | "pending" | "error">(
    userId && code ? "checking" : "pending",
  );

  useEffect(() => {
    if (!userId || !code) {
      return;
    }

    let isCurrent = true;
    void confirmEmail(userId, code)
      .then(() => {
        if (isCurrent) setStatus("confirmed");
      })
      .catch(() => {
        if (isCurrent) setStatus("error");
      });
    return () => {
      isCurrent = false;
    };
  }, [code, userId]);

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
            ? "Sua conta está pronta. Entre para continuar."
            : status === "error"
              ? "O link pode ter expirado ou já ter sido utilizado. Solicite uma nova confirmação."
              : `Verifique sua caixa de entrada${email ? ` em ${email}` : ""} e abra o link que enviamos para ativar sua conta.`}
      </p>
      <Link
        to={status === "error" ? "/register" : "/login"}
        className="mt-7 inline-flex min-h-12 items-center justify-center rounded-xl bg-violet-900 px-5 text-sm font-semibold text-white transition hover:bg-violet-800 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-800"
      >
        {status === "error" ? "Criar uma nova conta" : "Ir para o login"}
      </Link>
    </section>
  );
}