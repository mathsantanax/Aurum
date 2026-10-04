import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useMemo, useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate, useParams, useSearchParams } from "react-router-dom";
import { getAuthErrorMessage } from "../../auth/utils/getAuthErrorMessage";
import {
  addMember,
  deleteWalletspace,
  getFinancialSummary,
  getMembers,
  getSharedTransactions,
  getWalletspace,
  leaveWalletspace,
  removeMember,
  renameWalletspace,
  transferOwnership,
  updateMemberRole,
} from "../api/walletspacesApi";
import type {
  SharedFinancialTransaction,
  WalletspaceRole,
} from "../types/walletspace.types";

const currency = new Intl.NumberFormat("pt-BR", {
  style: "currency",
  currency: "BRL",
});

const today = new Date().toISOString().slice(0, 10);

function monthKey(date: Date) {
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}`;
}

function monthRange(key: string) {
  const [year, month] = key.split("-").map(Number);
  const last = new Date(year, month, 0).getDate();
  return { from: `${key}-01`, to: `${key}-${String(last).padStart(2, "0")}` };
}

function monthLabel(key: string) {
  const [year, month] = key.split("-").map(Number);
  const label = new Date(year, month - 1, 1).toLocaleDateString("pt-BR", {
    month: "long",
    year: "numeric",
  });
  return label.charAt(0).toUpperCase() + label.slice(1);
}

const tabs = [
  { id: "overview", label: "Resumo" },
  { id: "transactions", label: "Compartilhados" },
  { id: "reports", label: "Relatórios" },
  { id: "members", label: "Membros" },
  { id: "settings", label: "Configurações" },
] as const;
type TabId = (typeof tabs)[number]["id"];

const roleNames: Record<WalletspaceRole, string> = {
  1: "Proprietário",
  2: "Administrador",
  3: "Gestor",
  4: "Membro",
  5: "Visualizador",
};

function ErrorMessage({ message }: { message: string }) {
  return (
    <div role="alert" className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm leading-5 text-rose-800">
      {message}
    </div>
  );
}

function EmptyPanel({ title, description }: { title: string; description: string }) {
  return (
    <div className="rounded-2xl border border-dashed border-slate-300 bg-white p-8 text-center">
      <h3 className="font-semibold text-slate-950">{title}</h3>
      <p className="mx-auto mt-2 max-w-md text-sm leading-6 text-slate-600">{description}</p>
    </div>
  );
}

function sharedAmount(transaction: SharedFinancialTransaction) {
  return transaction.type === 1 ? transaction.amount : -transaction.amount;
}

export function WalletspaceDetailPage() {
  const { walletspaceId = "" } = useParams();
  const [searchParams, setSearchParams] = useSearchParams();
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const [month, setMonth] = useState(monthKey(new Date()));
  const [reportFrom, setReportFrom] = useState(`${month}-01`);
  const [reportTo, setReportTo] = useState(today);
  const [deleteConfirm, setDeleteConfirm] = useState("");
  const [actionError, setActionError] = useState<string | null>(null);
  const activeTab = useMemo<TabId>(() => {
    const value = searchParams.get("tab");
    return tabs.some((tab) => tab.id === value) ? (value as TabId) : "overview";
  }, [searchParams]);

  const spaceQuery = useQuery({
    queryKey: ["walletspace", walletspaceId],
    queryFn: () => getWalletspace(walletspaceId),
    enabled: Boolean(walletspaceId),
  });
  const monthBounds = monthRange(month);
  const transactionsQuery = useQuery({
    queryKey: ["walletspace", walletspaceId, "transactions", month],
    queryFn: () => getSharedTransactions(walletspaceId, monthBounds.from, monthBounds.to),
    enabled: Boolean(walletspaceId) && (activeTab === "transactions" || activeTab === "overview"),
  });
  const membersQuery = useQuery({
    queryKey: ["walletspace", walletspaceId, "members"],
    queryFn: () => getMembers(walletspaceId),
    enabled: Boolean(walletspaceId) && (activeTab === "members" || activeTab === "settings"),
  });
  const reportQuery = useQuery({
    queryKey: ["walletspace", walletspaceId, "report", reportFrom, reportTo],
    queryFn: () => getFinancialSummary(walletspaceId, reportFrom, reportTo),
    enabled: Boolean(walletspaceId) && activeTab === "reports" && Boolean(reportFrom) && Boolean(reportTo),
  });

  const invalidateMembers = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId, "members"] }),
      queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId] }),
      queryClient.invalidateQueries({ queryKey: ["walletspaces"] }),
    ]);
  };
  const addMemberMutation = useMutation({
    mutationFn: ({ email, role }: { email: string; role: WalletspaceRole }) =>
      addMember(walletspaceId, email, role),
    onSuccess: invalidateMembers,
  });
  const memberRoleMutation = useMutation({
    mutationFn: ({ userId, role }: { userId: string; role: WalletspaceRole }) =>
      updateMemberRole(walletspaceId, userId, role),
    onSuccess: invalidateMembers,
  });
  const removeMemberMutation = useMutation({
    mutationFn: (userId: string) => removeMember(walletspaceId, userId),
    onSuccess: invalidateMembers,
  });
  const renameMutation = useMutation({
    mutationFn: (name: string) => renameWalletspace(walletspaceId, name),
    onSuccess: invalidateMembers,
  });
  const deleteMutation = useMutation({
    mutationFn: () => deleteWalletspace(walletspaceId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["walletspaces"] });
      await queryClient.invalidateQueries({ queryKey: ["dashboard-summary"] });
      navigate("/walletspaces");
    },
  });
  const leaveMutation = useMutation({
    mutationFn: () => leaveWalletspace(walletspaceId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["walletspaces"] });
      await queryClient.invalidateQueries({ queryKey: ["dashboard-summary"] });
      navigate("/walletspaces");
    },
  });
  const transferMutation = useMutation({
    mutationFn: (userId: string) => transferOwnership(walletspaceId, userId),
    onSuccess: invalidateMembers,
  });

  function showTab(tab: TabId) {
    setActionError(null);
    setSearchParams(tab === "overview" ? {} : { tab });
  }

  async function performAction(action: () => Promise<unknown>) {
    setActionError(null);
    try {
      await action();
    } catch (error) {
      setActionError(getAuthErrorMessage(error, "Não foi possível concluir a operação."));
    }
  }

  function handleMemberSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    void performAction(() => addMemberMutation.mutateAsync({
      email: String(form.get("email") ?? "").trim(),
      role: Number(form.get("role")) as WalletspaceRole,
    }));
    event.currentTarget.reset();
  }

  function handleRename(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    void performAction(() => renameMutation.mutateAsync(String(form.get("name") ?? "").trim()));
  }

  function handleTransfer(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const userId = String(form.get("newOwner") ?? "");
    if (!userId || !window.confirm("Transferir a propriedade? Você passará a ser Administrador.")) return;
    void performAction(() => transferMutation.mutateAsync(userId));
  }

  if (spaceQuery.isPending) {
    return <div role="status" className="mx-auto max-w-5xl rounded-2xl border border-slate-200 bg-white p-8 text-center text-sm text-slate-600">Carregando Walletspace...</div>;
  }
  if (spaceQuery.isError || !spaceQuery.data) {
    return (
      <div className="mx-auto max-w-5xl">
        <Link to="/walletspaces" className="text-sm font-semibold text-violet-800 hover:underline">← Voltar aos espaços</Link>
        <div role="alert" className="mt-5 rounded-2xl border border-rose-200 bg-rose-50 p-5 text-sm text-rose-800">
          {getAuthErrorMessage(spaceQuery.error, "Não foi possível abrir este Walletspace.")}
        </div>
      </div>
    );
  }

  const space = spaceQuery.data;
  const transactions = transactionsQuery.data ?? [];
  const paid = transactions.filter((item) => item.status === 2);
  const income = paid.filter((item) => item.type === 1).reduce((total, item) => total + item.amount, 0);
  const expense = paid.filter((item) => item.type === 2).reduce((total, item) => total + item.amount, 0);
  const memberAdmin = space.myRole === 1 || space.myRole === 2;
  const isBusy = addMemberMutation.isPending || memberRoleMutation.isPending ||
    removeMemberMutation.isPending || renameMutation.isPending || deleteMutation.isPending ||
    leaveMutation.isPending || transferMutation.isPending;

  return (
    <div className="mx-auto max-w-5xl">
      <Link to="/walletspaces" className="inline-flex items-center gap-2 text-sm font-semibold text-slate-600 hover:text-violet-900">
        <span aria-hidden="true">←</span>
        Todos os espaços
      </Link>
      <div className="mt-5 flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
        <div className="min-w-0">
          <p className="text-xs font-bold uppercase tracking-[0.18em] text-violet-800">Walletspace colaborativo</p>
          <h1 className="mt-2 truncate text-3xl font-semibold tracking-tight text-slate-950 sm:text-4xl">{space.name}</h1>
          <p className="mt-2 text-sm text-slate-600">
            {roleNames[space.myRole]} · {space.memberCount} {space.memberCount === 1 ? "membro" : "membros"}
          </p>
        </div>
        <Link to="/finances" className="inline-flex min-h-10 items-center justify-center rounded-xl border border-slate-300 bg-white px-4 text-sm font-semibold text-slate-700 hover:bg-slate-50">
          Ir para minhas finanças
        </Link>
      </div>

      {actionError && <div className="mt-5"><ErrorMessage message={actionError} /></div>}

      <nav aria-label="Seções do Walletspace" className="mt-6 flex gap-1 overflow-x-auto rounded-2xl border border-slate-200 bg-white p-1.5">
        {tabs.map((tab) => (
          <button key={tab.id} type="button" onClick={() => showTab(tab.id)}
            aria-current={activeTab === tab.id ? "page" : undefined}
            className={`min-h-10 shrink-0 rounded-xl px-3 text-sm font-semibold transition ${activeTab === tab.id ? "bg-violet-900 text-white" : "text-slate-600 hover:bg-slate-100"}`}>
            {tab.label}
          </button>
        ))}
      </nav>

      {activeTab === "overview" && (
        <section className="mt-6">
          <div className="grid gap-4 sm:grid-cols-3">
            {[
              { label: `Receitas efetivadas · ${monthLabel(month)}`, value: currency.format(income) },
              { label: `Despesas efetivadas · ${monthLabel(month)}`, value: currency.format(expense) },
              { label: "Movimentações compartilhadas", value: String(transactions.length) },
            ].map((item) => (
              <article key={item.label} className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                <p className="text-sm font-medium text-slate-600">{item.label}</p>
                <p className="mt-3 text-2xl font-semibold tracking-tight text-slate-950">{item.value}</p>
              </article>
            ))}
          </div>
          <div className="mt-5 rounded-2xl border border-violet-200 bg-violet-50 p-5">
            <h2 className="font-semibold text-violet-950">Somente o que foi compartilhado</h2>
            <p className="mt-1 text-sm leading-6 text-violet-900">
              As contas, cartões e demais movimentações pessoais dos membros não aparecem neste espaço.
              Cada pessoa escolhe quais transações compartilhar.
            </p>
          </div>
        </section>
      )}

      {activeTab === "transactions" && (
        <section className="mt-6 space-y-4">
          <div className="flex flex-col justify-between gap-3 sm:flex-row sm:items-end">
            <div><h2 className="text-lg font-semibold text-slate-950">Movimentações compartilhadas</h2><p className="mt-1 text-sm text-slate-600">Visualização somente leitura, limitada a este Walletspace.</p></div>
            <label className="text-sm font-semibold text-slate-700">Mês
              <input type="month" value={month} onChange={(event) => setMonth(event.target.value)} className="ml-2 min-h-10 rounded-lg border border-slate-300 bg-white px-2 font-normal" />
            </label>
          </div>
          {transactionsQuery.isPending ? <p role="status" className="text-sm text-slate-500">Carregando transações...</p>
            : transactionsQuery.isError ? <ErrorMessage message={getAuthErrorMessage(transactionsQuery.error, "Não foi possível carregar as transações compartilhadas.")} />
              : transactions.length ? (
                <div className="divide-y divide-slate-100 overflow-hidden rounded-2xl border border-slate-200 bg-white">
                  {transactions.map((transaction) => (
                    <article key={transaction.id} className="flex flex-wrap items-center gap-3 p-4 sm:px-5">
                      <span aria-hidden="true" className={`flex size-10 items-center justify-center rounded-xl text-lg ${transaction.type === 1 ? "bg-emerald-50 text-emerald-800" : "bg-rose-50 text-rose-800"}`}>{transaction.type === 1 ? "+" : "−"}</span>
                      <div className="min-w-0 flex-1">
                        <h3 className="truncate text-sm font-semibold text-slate-950">{transaction.description}</h3>
                        <p className="mt-1 text-xs text-slate-500">{transaction.ownerDisplayName || "Membro"} · {new Date(`${transaction.transactionDate}T00:00:00`).toLocaleDateString("pt-BR")}{transaction.category ? ` · ${transaction.category}` : ""}</p>
                      </div>
                      <div className="text-right">
                        <p className={`text-sm font-semibold ${transaction.type === 1 ? "text-emerald-800" : "text-slate-900"}`}>{currency.format(sharedAmount(transaction))}</p>
                        <p className="mt-1 text-xs text-slate-500">{transaction.status === 2 ? "Efetivada" : "Pendente"}</p>
                      </div>
                    </article>
                  ))}
                </div>
              ) : <EmptyPanel title="Nenhuma transação compartilhada neste mês" description="As transações que os membros compartilharem com este espaço aparecerão aqui." />}
        </section>
      )}

      {activeTab === "reports" && (
        <section className="mt-6 space-y-4">
          <div><h2 className="text-lg font-semibold text-slate-950">Relatório do Walletspace</h2><p className="mt-1 text-sm text-slate-600">Os totais incluem apenas transações compartilhadas com este espaço.</p></div>
          <form onSubmit={(event) => { event.preventDefault(); void reportQuery.refetch(); }} className="flex flex-col gap-3 rounded-2xl border border-slate-200 bg-white p-4 sm:flex-row sm:items-end">
            <label className="flex-1 text-sm font-semibold text-slate-700">De<input type="date" value={reportFrom} onChange={(event) => setReportFrom(event.target.value)} className="mt-1.5 min-h-11 w-full rounded-xl border border-slate-300 px-3 font-normal" /></label>
            <label className="flex-1 text-sm font-semibold text-slate-700">Até<input type="date" value={reportTo} onChange={(event) => setReportTo(event.target.value)} className="mt-1.5 min-h-11 w-full rounded-xl border border-slate-300 px-3 font-normal" /></label>
            <button disabled={reportQuery.isFetching || reportFrom > reportTo} className="min-h-11 rounded-xl bg-violet-900 px-4 text-sm font-semibold text-white disabled:opacity-50">{reportQuery.isFetching ? "Atualizando..." : "Aplicar período"}</button>
          </form>
          {reportQuery.isError && <ErrorMessage message={getAuthErrorMessage(reportQuery.error, "Não foi possível gerar o relatório.")} />}
          {reportQuery.data && (
            <div className="grid gap-4 sm:grid-cols-3">
              <article className="rounded-2xl border border-slate-200 bg-white p-5"><p className="text-sm text-slate-600">Receitas efetivadas</p><p className="mt-2 text-xl font-semibold text-emerald-800">{currency.format(reportQuery.data.income)}</p></article>
              <article className="rounded-2xl border border-slate-200 bg-white p-5"><p className="text-sm text-slate-600">Despesas efetivadas</p><p className="mt-2 text-xl font-semibold text-rose-800">{currency.format(reportQuery.data.expense)}</p></article>
              <article className="rounded-2xl border border-slate-200 bg-white p-5"><p className="text-sm text-slate-600">Pendentes</p><p className="mt-2 text-xl font-semibold text-slate-950">{reportQuery.data.pendingCount}</p></article>
            </div>
          )}
        </section>
      )}

      {activeTab === "members" && (
        <section className="mt-6 space-y-5">
          <div><h2 className="text-lg font-semibold text-slate-950">Pessoas e permissões</h2><p className="mt-1 text-sm text-slate-600">Os papéis controlam o Walletspace, não concedem acesso às contas pessoais dos membros.</p></div>
          {memberAdmin && (
            <form onSubmit={handleMemberSubmit} className="grid gap-3 rounded-2xl border border-slate-200 bg-white p-4 sm:grid-cols-[1fr_13rem_auto] sm:items-end">
              <label className="text-sm font-semibold">E-mail da pessoa<input name="email" type="email" autoComplete="email" required placeholder="pessoa@email.com" className="mt-1.5 min-h-11 w-full rounded-xl border border-slate-300 px-3 font-normal" /></label>
              <label className="text-sm font-semibold">Permissão<select name="role" className="mt-1.5 min-h-11 w-full rounded-xl border border-slate-300 bg-white px-3 font-normal"><option value="5">Visualizador</option><option value="4">Membro</option><option value="3">Gestor</option>{space.myRole === 1 && <option value="2">Administrador</option>}</select></label>
              <button disabled={isBusy} className="min-h-11 rounded-xl bg-violet-900 px-4 text-sm font-semibold text-white disabled:opacity-50">Adicionar</button>
            </form>
          )}
          {membersQuery.isPending ? <p role="status" className="text-sm text-slate-500">Carregando membros...</p>
            : membersQuery.isError ? <ErrorMessage message={getAuthErrorMessage(membersQuery.error, "Não foi possível carregar os membros.")} />
              : membersQuery.data?.length ? (
                <div className="divide-y divide-slate-100 overflow-hidden rounded-2xl border border-slate-200 bg-white">
                  {membersQuery.data.map((member) => (
                    <article key={member.userId} className="flex flex-wrap items-center gap-3 p-4 sm:px-5">
                      <span className="flex size-10 shrink-0 items-center justify-center rounded-full bg-violet-100 text-xs font-bold text-violet-900">{(member.fullName || member.email).slice(0, 2).toUpperCase()}</span>
                      <div className="min-w-0 flex-1"><h3 className="truncate text-sm font-semibold text-slate-950">{member.fullName || member.email}</h3><p className="truncate text-xs text-slate-500">{member.email}</p></div>
                      {memberAdmin && member.role !== 1 && (space.myRole === 1 || member.role !== 2) ? (
                        <>
                          <label className="sr-only" htmlFor={`role-${member.userId}`}>Permissão de {member.email}</label>
                          <select id={`role-${member.userId}`} value={member.role} disabled={isBusy} onChange={(event) => { void performAction(() => memberRoleMutation.mutateAsync({ userId: member.userId, role: Number(event.target.value) as WalletspaceRole })); }} className="min-h-9 max-w-44 rounded-lg border border-slate-300 bg-white px-2 text-xs font-semibold text-slate-700">
                            {space.myRole === 1 && <option value="2">Administrador</option>}<option value="3">Gestor</option><option value="4">Membro</option><option value="5">Visualizador</option>
                          </select>
                          <button type="button" disabled={isBusy} onClick={() => { if (window.confirm(`Remover ${member.email} deste Walletspace?`)) void performAction(() => removeMemberMutation.mutateAsync(member.userId)); }} className="rounded-lg px-2 py-1 text-xs font-semibold text-slate-500 hover:bg-rose-50 hover:text-rose-700">Remover</button>
                        </>
                      ) : <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold text-slate-600">{roleNames[member.role]}</span>}
                    </article>
                  ))}
                </div>
              ) : <EmptyPanel title="Nenhum membro encontrado" description="As pessoas adicionadas a este espaço aparecerão aqui." />}
        </section>
      )}

      {activeTab === "settings" && (
        <section className="mt-6 space-y-5">
          <div><h2 className="text-lg font-semibold text-slate-950">Configurações do espaço</h2><p className="mt-1 text-sm text-slate-600">O espaço organiza compartilhamentos; os dados pessoais continuam com seus proprietários.</p></div>
          {memberAdmin && <form onSubmit={handleRename} className="rounded-2xl border border-slate-200 bg-white p-5">
            <h3 className="font-semibold text-slate-950">Renomear</h3>
            <div className="mt-3 flex flex-col gap-3 sm:flex-row"><label htmlFor="rename-space" className="sr-only">Nome do Walletspace</label><input id="rename-space" name="name" defaultValue={space.name} minLength={3} maxLength={100} required className="min-h-11 flex-1 rounded-xl border border-slate-300 px-3 text-sm" /><button disabled={isBusy} className="min-h-11 rounded-xl bg-violet-900 px-4 text-sm font-semibold text-white disabled:opacity-50">Salvar nome</button></div>
          </form>}
          {space.myRole === 1 && <form onSubmit={handleTransfer} className="rounded-2xl border border-slate-200 bg-white p-5">
            <h3 className="font-semibold text-slate-950">Transferir propriedade</h3><p className="mt-1 text-sm text-slate-600">O novo proprietário assume o controle total e você vira Administrador.</p>
            <div className="mt-3 flex flex-col gap-3 sm:flex-row"><label htmlFor="new-owner" className="sr-only">Novo proprietário</label><select id="new-owner" name="newOwner" required defaultValue="" className="min-h-11 flex-1 rounded-xl border border-slate-300 bg-white px-3 text-sm"><option value="" disabled>Selecione um membro</option>{(membersQuery.data ?? []).filter((member) => member.role !== 1).map((member) => <option key={member.userId} value={member.userId}>{member.fullName || member.email}</option>)}</select><button disabled={isBusy} className="min-h-11 rounded-xl border border-slate-300 px-4 text-sm font-semibold text-slate-800 hover:bg-violet-50 disabled:opacity-50">Transferir</button></div>
          </form>}
          {space.myRole !== 1 ? <div className="rounded-2xl border border-amber-200 bg-amber-50 p-5"><h3 className="font-semibold text-amber-950">Sair do espaço</h3><p className="mt-1 text-sm text-amber-900">Você perderá o acesso às transações compartilhadas com este Walletspace.</p><button type="button" disabled={isBusy} onClick={() => { if (window.confirm("Sair deste Walletspace?")) void performAction(() => leaveMutation.mutateAsync()); }} className="mt-3 min-h-11 rounded-xl bg-amber-700 px-4 text-sm font-semibold text-white disabled:opacity-50">Sair do espaço</button></div>
            : <div className="rounded-2xl border border-rose-200 bg-rose-50 p-5"><h3 className="font-semibold text-rose-950">Excluir Walletspace</h3><p className="mt-1 text-sm text-rose-900">A exclusão remove membros e compartilhamentos deste espaço, mas não apaga as contas nem as transações pessoais. Digite <strong>{space.name}</strong> para confirmar.</p><div className="mt-3 flex flex-col gap-3 sm:flex-row"><label htmlFor="delete-confirm" className="sr-only">Confirmar nome</label><input id="delete-confirm" value={deleteConfirm} onChange={(event) => setDeleteConfirm(event.target.value)} className="min-h-11 flex-1 rounded-xl border border-rose-300 bg-white px-3 text-sm" /><button type="button" disabled={isBusy || deleteConfirm !== space.name} onClick={() => void performAction(() => deleteMutation.mutateAsync())} className="min-h-11 rounded-xl bg-rose-700 px-4 text-sm font-semibold text-white disabled:opacity-50">Excluir espaço</button></div></div>}
        </section>
      )}
    </div>
  );
}
