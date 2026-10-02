import { useQuery } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { getAuthErrorMessage } from "../../auth/utils/getAuthErrorMessage";
import { getWalletspaces } from "../api/walletspacesApi";
import type { WalletspaceRole } from "../types/walletspace.types";

const roleNames: Record<WalletspaceRole, string> = {
  1: "Proprietário",
  2: "Administrador",
  3: "Gestor",
  4: "Membro",
  5: "Visualizador",
};

export function WalletspacesPage() {
  const spacesQuery = useQuery({
    queryKey: ["walletspaces"],
    queryFn: getWalletspaces,
  });

  return (
    <div className="mx-auto max-w-5xl">
      <div className="flex flex-col justify-between gap-5 sm:flex-row sm:items-end">
        <div>
          <p className="text-xs font-bold uppercase tracking-[0.18em] text-violet-800">Organize do seu jeito</p>
          <h1 className="mt-2 text-3xl font-semibold tracking-tight text-slate-950 sm:text-4xl">Espaços financeiros</h1>
          <p className="mt-2 text-sm leading-6 text-slate-600">Gerencie espaços pessoais ou compartilhados com sua equipe.</p>
        </div>
        <Link
          to="/walletspaces/new"
          className="inline-flex min-h-11 items-center justify-center gap-2 rounded-xl bg-violet-900 px-4 text-sm font-semibold text-white shadow-md shadow-violet-900/10 transition hover:bg-violet-800 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-800"
        >
          <span aria-hidden="true" className="text-lg leading-none">+</span>
          Novo espaço
        </Link>
      </div>

      {spacesQuery.isPending && (
        <div role="status" className="mt-8 rounded-2xl border border-slate-200 bg-white p-8 text-center text-sm text-slate-600">
          <span className="mx-auto mb-3 block size-5 animate-spin rounded-full border-2 border-violet-200 border-t-violet-800" />
          Carregando seus espaços...
        </div>
      )}

      {spacesQuery.isError && (
        <div role="alert" className="mt-8 rounded-2xl border border-rose-200 bg-rose-50 p-5 text-sm text-rose-800">
          <p>{getAuthErrorMessage(spacesQuery.error, "Não foi possível carregar os Walletspaces.")}</p>
          <button
            type="button"
            onClick={() => void spacesQuery.refetch()}
            className="mt-3 font-semibold underline underline-offset-4"
          >
            Tentar novamente
          </button>
        </div>
      )}

      {spacesQuery.isSuccess && spacesQuery.data.length === 0 && (
        <section className="mt-8 overflow-hidden rounded-3xl border border-slate-200 bg-white shadow-sm shadow-slate-200/40">
          <div className="grid items-center gap-8 p-6 sm:p-10 md:grid-cols-[1fr_auto]">
            <div className="max-w-xl">
              <span className="inline-flex items-center rounded-full bg-violet-50 px-3 py-1 text-xs font-semibold text-violet-800">Seu primeiro passo</span>
              <h2 className="mt-4 text-2xl font-semibold tracking-tight text-slate-950">Tudo começa com um espaço</h2>
              <p className="mt-3 text-sm leading-6 text-slate-600">
                Crie um espaço para separar objetivos, projetos ou contas compartilhadas.
              </p>
              <Link
                to="/walletspaces/new"
                className="mt-6 inline-flex min-h-11 items-center justify-center rounded-xl bg-violet-900 px-4 text-sm font-semibold text-white transition hover:bg-violet-800 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-800"
              >
                Criar meu primeiro espaço
              </Link>
            </div>
            <div aria-hidden="true" className="relative mx-auto flex size-44 items-center justify-center rounded-full bg-violet-50 sm:size-52">
              <div className="flex size-28 items-center justify-center rounded-[2rem] border border-violet-200 bg-white text-4xl text-violet-800 shadow-xl shadow-violet-900/10 sm:size-32">◇</div>
              <span className="absolute right-5 top-7 flex size-9 items-center justify-center rounded-xl bg-amber-200 text-lg text-amber-900">+</span>
            </div>
          </div>
        </section>
      )}

      {spacesQuery.data && spacesQuery.data.length > 0 && (
        <section aria-label="Seus Walletspaces" className="mt-8 grid gap-4 sm:grid-cols-2">
          {spacesQuery.data.map((space) => (
            <Link
              key={space.id}
              to={`/walletspaces/${space.id}`}
              className="group rounded-2xl border border-slate-200 bg-white p-5 shadow-sm shadow-slate-200/30 transition hover:-translate-y-0.5 hover:border-violet-200 hover:shadow-md focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-700 sm:p-6"
            >
              <div className="flex items-start justify-between gap-4">
                <span className="flex size-11 items-center justify-center rounded-2xl bg-violet-50 text-xl font-semibold text-violet-800">◇</span>
                <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold text-slate-600">
                  {roleNames[space.myRole]}
                </span>
              </div>
              <h2 className="mt-5 truncate text-lg font-semibold text-slate-950 group-hover:text-violet-900">{space.name}</h2>
              <div className="mt-2 flex items-center justify-between text-xs text-slate-500">
                <span>{space.memberCount} {space.memberCount === 1 ? "membro" : "membros"}</span>
                <span className="font-semibold text-violet-800">Abrir espaço <span aria-hidden="true">→</span></span>
              </div>
            </Link>
          ))}
        </section>
      )}
    </div>
  );
}
