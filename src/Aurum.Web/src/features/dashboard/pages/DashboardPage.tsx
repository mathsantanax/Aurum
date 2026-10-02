import { useQuery } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { getAuthErrorMessage } from "../../auth/utils/getAuthErrorMessage";
import { getDashboardSummary } from "../../walletspaces/api/walletspacesApi";

const currency = new Intl.NumberFormat("pt-BR", {
  style: "currency",
  currency: "BRL",
});

export function DashboardPage() {
  const summaryQuery = useQuery({
    queryKey: ["dashboard-summary"],
    queryFn: getDashboardSummary,
  });
  const summary = summaryQuery.data;

  return (
    <div className="mx-auto max-w-5xl">
      <div className="flex flex-col justify-between gap-5 sm:flex-row sm:items-end">
        <div>
          <p className="text-xs font-bold uppercase tracking-[0.18em] text-violet-800">Seu panorama</p>
          <h1 className="mt-2 text-3xl font-semibold tracking-tight text-slate-950 sm:text-4xl">Visão geral</h1>
          <p className="mt-2 max-w-xl text-sm leading-6 text-slate-600">
            Acompanhe seus espaços e tenha mais clareza sobre suas finanças.
          </p>
        </div>
        <Link
          to="/walletspaces/new"
          className="inline-flex min-h-11 items-center justify-center rounded-xl bg-violet-900 px-4 text-sm font-semibold text-white shadow-md shadow-violet-900/10 transition hover:bg-violet-800 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-800"
        >
          Criar espaço financeiro
        </Link>
      </div>

      {summaryQuery.isPending && (
        <div role="status" className="mt-8 rounded-2xl border border-slate-200 bg-white p-8 text-center text-sm text-slate-600">
          <span className="mx-auto mb-3 block size-5 animate-spin rounded-full border-2 border-violet-200 border-t-violet-800" />
          Atualizando seu panorama...
        </div>
      )}
      {summaryQuery.isError && (
        <div role="alert" className="mt-8 rounded-2xl border border-rose-200 bg-rose-50 p-5 text-sm text-rose-800">
          <p>{getAuthErrorMessage(summaryQuery.error, "Não foi possível carregar o resumo financeiro.")}</p>
          <button type="button" onClick={() => void summaryQuery.refetch()} className="mt-3 font-semibold underline underline-offset-4">
            Tentar novamente
          </button>
        </div>
      )}

      {summary && (
        <>
          <section className="mt-8 grid gap-4 sm:grid-cols-3" aria-label="Resumo financeiro">
            {[
              { label: "Saldo em contas", value: currency.format(summary.accountBalance), marker: "R$", tone: "bg-violet-50 text-violet-800" },
              { label: "Receitas do mês", value: currency.format(summary.incomeThisMonth), marker: "+", tone: "bg-emerald-50 text-emerald-800" },
              { label: "Despesas do mês", value: currency.format(summary.expenseThisMonth), marker: "−", tone: "bg-rose-50 text-rose-800" },
            ].map((item) => (
              <article key={item.label} className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm shadow-slate-200/30">
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <p className="text-sm font-medium text-slate-600">{item.label}</p>
                    <p className="mt-4 text-2xl font-semibold tracking-tight text-slate-950">{item.value}</p>
                  </div>
                  <span className={`flex size-9 items-center justify-center rounded-xl text-xs font-bold ${item.tone}`}>{item.marker}</span>
                </div>
                <p className="mt-3 text-xs text-slate-500">Lançamentos efetivados neste mês</p>
              </article>
            ))}
          </section>
          <section className="mt-8">
            <div className="flex items-center justify-between gap-4">
              <div>
                <h2 className="text-lg font-semibold text-slate-950">Seus Walletspaces</h2>
                <p className="mt-1 text-sm text-slate-600">
                  {summary.walletspaceCount} {summary.walletspaceCount === 1 ? "espaço financeiro" : "espaços financeiros"}
                </p>
              </div>
              <Link to="/walletspaces" className="text-sm font-semibold text-violet-800 hover:underline">
                Ver todos
              </Link>
            </div>
            {summary.walletspaces.length ? (
              <div className="mt-4 grid gap-4 sm:grid-cols-2">
                {summary.walletspaces.slice(0, 4).map((space) => (
                  <Link
                    key={space.id}
                    to={`/walletspaces/${space.id}`}
                    className="flex items-center gap-4 rounded-2xl border border-slate-200 bg-white p-5 transition hover:border-violet-200 hover:shadow-sm focus-visible:outline-2 focus-visible:outline-violet-700"
                  >
                    <span className="flex size-11 items-center justify-center rounded-2xl bg-violet-50 text-xl text-violet-800">◇</span>
                    <span className="min-w-0 flex-1">
                      <span className="block truncate text-sm font-semibold text-slate-950">{space.name}</span>
                      <span className="mt-1 block text-xs text-slate-500">{space.memberCount} {space.memberCount === 1 ? "membro" : "membros"}</span>
                    </span>
                    <span aria-hidden="true" className="text-violet-800">→</span>
                  </Link>
                ))}
              </div>
            ) : (
              <div className="mt-4 rounded-2xl border border-dashed border-slate-300 bg-white p-7 text-center">
                <h3 className="font-semibold text-slate-950">Seu resumo começa aqui</h3>
                <p className="mt-2 text-sm text-slate-600">Crie seu primeiro Walletspace para organizar contas, cartões e lançamentos.</p>
                <Link to="/walletspaces/new" className="mt-4 inline-flex min-h-10 items-center rounded-xl bg-violet-900 px-4 text-sm font-semibold text-white hover:bg-violet-800">
                  Criar Walletspace
                </Link>
              </div>
            )}
          </section>
        </>
      )}
    </div>
  );
}
