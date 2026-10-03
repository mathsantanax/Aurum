import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useMemo, useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate, useParams, useSearchParams } from "react-router-dom";
import { getAuthErrorMessage } from "../../auth/utils/getAuthErrorMessage";
import {
  addMember,
  createAccount,
  createCard,
  createTransaction,
  deleteAccount,
  deleteCard,
  deleteTransaction,
  deleteWalletspace,
  leaveWalletspace,
  transferOwnership,
  getAccounts,
  getCards,
  getFinancialSummary,
  getMembers,
  getTransactions,
  getWalletspace,
  removeMember,
  renameWalletspace,
  updateAccount,
  updateCard,
  updateMemberRole,
  updateTransaction,
} from "../api/walletspacesApi";
import type {
  FinancialAccountType,
  FinancialAccount,
  CreditCard,
  FinancialTransaction,
  FinancialTransactionRecurrence,
  FinancialTransactionStatus,
  TransactionSeriesScope,
  FinancialTransactionType,
  SaveFinancialTransaction,
  WalletspaceRole,
} from "../types/walletspace.types";

const currency = new Intl.NumberFormat("pt-BR", {
  style: "currency",
  currency: "BRL",
});

const today = new Date().toISOString().slice(0, 10);
const monthStart = `${today.slice(0, 7)}-01`;

function monthKey(date: Date) {
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}`;
}

function monthRange(key: string) {
  const [year, month] = key.split("-").map(Number);
  const last = new Date(year, month, 0).getDate();
  return { from: `${key}-01`, to: `${key}-${String(last).padStart(2, "0")}` };
}

function shiftMonth(key: string, delta: number) {
  const [year, month] = key.split("-").map(Number);
  return monthKey(new Date(year, month - 1 + delta, 1));
}

function monthLabel(key: string) {
  const [year, month] = key.split("-").map(Number);
  const label = new Date(year, month - 1, 1).toLocaleDateString("pt-BR", { month: "long", year: "numeric" });
  return label.charAt(0).toUpperCase() + label.slice(1);
}

function askSeriesScope(action: string): TransactionSeriesScope | null {
  const answer = window.prompt(
    `Este lançamento faz parte de uma série (fixa/parcelada).\n${action}:\n1 = somente este\n2 = este e os seguintes\n3 = toda a série\n(vazio cancela)`,
    "1",
  );
  if (answer === "1") return "single";
  if (answer === "2") return "future";
  if (answer === "3") return "all";
  return null;
}

const tabs = [
  { id: "overview", label: "Resumo" },
  { id: "accounts", label: "Contas" },
  { id: "cards", label: "Cartões" },
  { id: "transactions", label: "Lançamentos" },
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
const accountTypeNames: Record<FinancialAccountType, string> = {
  1: "Conta corrente",
  2: "Poupança",
  3: "Dinheiro",
  4: "Investimentos",
  5: "Outra",
};

function ErrorMessage({ message }: { message: string }) {
  return (
    <div role="alert" className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm leading-5 text-rose-800">
      {message}
    </div>
  );
}

function EmptyPanel({
  title,
  description,
}: {
  title: string;
  description: string;
}) {
  return (
    <div className="rounded-2xl border border-dashed border-slate-300 bg-white p-8 text-center">
      <div aria-hidden="true" className="mx-auto flex size-12 items-center justify-center rounded-2xl bg-violet-50 text-xl text-violet-800">◇</div>
      <h3 className="mt-4 font-semibold text-slate-950">{title}</h3>
      <p className="mx-auto mt-2 max-w-md text-sm leading-6 text-slate-600">{description}</p>
    </div>
  );
}

export function WalletspaceDetailPage() {
  const { walletspaceId = "" } = useParams();
  const [searchParams, setSearchParams] = useSearchParams();
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const [month, setMonth] = useState(monthKey(new Date()));
  const [recurrence, setRecurrence] = useState<FinancialTransactionRecurrence>(0);
  const [occurrences, setOccurrences] = useState(12);
  const [installmentAmount, setInstallmentAmount] = useState(0);
  const [amountIsPerInstallment, setAmountIsPerInstallment] = useState(false);
  const [deleteConfirm, setDeleteConfirm] = useState("");
  const [formOpen, setFormOpen] = useState(false);
  const [editingAccount, setEditingAccount] = useState<FinancialAccount | null>(null);
  const [editingCard, setEditingCard] = useState<CreditCard | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [reportFrom, setReportFrom] = useState(monthStart);
  const [reportTo, setReportTo] = useState(today);
  const [transactionResourceKind, setTransactionResourceKind] =
    useState<"account" | "card">("account");
  const activeTab = useMemo<TabId>(() => {
    const value = searchParams.get("tab");
    return tabs.some((tab) => tab.id === value) ? (value as TabId) : "overview";
  }, [searchParams]);

  const spaceQuery = useQuery({
    queryKey: ["walletspace", walletspaceId],
    queryFn: () => getWalletspace(walletspaceId),
    enabled: Boolean(walletspaceId),
  });
  const accountsQuery = useQuery({
    queryKey: ["walletspace", walletspaceId, "accounts"],
    queryFn: () => getAccounts(walletspaceId),
    enabled: Boolean(walletspaceId) && (activeTab === "accounts" || activeTab === "transactions" || activeTab === "overview"),
  });
  const cardsQuery = useQuery({
    queryKey: ["walletspace", walletspaceId, "cards"],
    queryFn: () => getCards(walletspaceId),
    enabled: Boolean(walletspaceId) && (activeTab === "cards" || activeTab === "transactions" || activeTab === "overview"),
  });
  const transactionsQuery = useQuery({
    queryKey: ["walletspace", walletspaceId, "transactions", month],
    queryFn: () => getTransactions(walletspaceId, monthRange(month).from, monthRange(month).to),
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
  const writeAccess = Boolean(spaceQuery.data && spaceQuery.data.myRole !== 5);
  const manageAccess = Boolean(spaceQuery.data && [1, 2, 3].includes(spaceQuery.data.myRole));
  const memberAdmin = Boolean(spaceQuery.data && [1, 2].includes(spaceQuery.data.myRole));

  const accountMutation = useMutation({
    mutationFn: async ({ account, id }: { account: Parameters<typeof createAccount>[1]; id?: string }) => {
      if (id) {
        await updateAccount(walletspaceId, id, account);
        return;
      }
      await createAccount(walletspaceId, account);
    },
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId, "accounts"] }),
        queryClient.invalidateQueries({ queryKey: ["dashboard-summary"] }),
      ]);
    },
  });
  const cardMutation = useMutation({
    mutationFn: async ({ card, id }: { card: Parameters<typeof createCard>[1]; id?: string }) => {
      if (id) {
        await updateCard(walletspaceId, id, card);
        return;
      }
      await createCard(walletspaceId, card);
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId, "cards"] }),
  });
  const createTransactionMutation = useMutation({
    mutationFn: (data: SaveFinancialTransaction) => createTransaction(walletspaceId, data),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId, "transactions"] }),
        queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId, "accounts"] }),
        queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId, "cards"] }),
        queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId, "report"] }),
        queryClient.invalidateQueries({ queryKey: ["dashboard-summary"] }),
      ]);
    },
  });
  const transactionMutation = useMutation({
    mutationFn: ({ transaction, data, scope }: { transaction: FinancialTransaction; data: SaveFinancialTransaction; scope?: TransactionSeriesScope }) =>
      updateTransaction(walletspaceId, transaction.id, data, scope),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId, "transactions"] }),
        queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId, "accounts"] }),
        queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId, "cards"] }),
        queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId, "report"] }),
        queryClient.invalidateQueries({ queryKey: ["dashboard-summary"] }),
      ]);
    },
  });
  const deleteTransactionMutation = useMutation({
    mutationFn: ({ id, scope }: { id: string; scope?: TransactionSeriesScope }) => deleteTransaction(walletspaceId, id, scope),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId, "transactions"] }),
        queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId, "accounts"] }),
        queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId, "cards"] }),
        queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId, "report"] }),
        queryClient.invalidateQueries({ queryKey: ["dashboard-summary"] }),
      ]);
    },
  });
  const addMemberMutation = useMutation({
    mutationFn: ({ email, role }: { email: string; role: WalletspaceRole }) => addMember(walletspaceId, email, role),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId, "members"] }),
        queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId] }),
        queryClient.invalidateQueries({ queryKey: ["walletspaces"] }),
      ]);
    },
  });
  const memberRoleMutation = useMutation({
    mutationFn: ({ userId, role }: { userId: string; role: WalletspaceRole }) =>
      updateMemberRole(walletspaceId, userId, role),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId, "members"] }),
  });
  const removeMemberMutation = useMutation({
    mutationFn: (userId: string) => removeMember(walletspaceId, userId),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId, "members"] }),
        queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId] }),
        queryClient.invalidateQueries({ queryKey: ["walletspaces"] }),
      ]);
    },
  });
  const renameMutation = useMutation({
    mutationFn: (name: string) => renameWalletspace(walletspaceId, name),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId] }),
        queryClient.invalidateQueries({ queryKey: ["walletspaces"] }),
      ]);
    },
  });

  const deleteSpaceMutation = useMutation({
    mutationFn: () => deleteWalletspace(walletspaceId),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["walletspaces"] }),
        queryClient.invalidateQueries({ queryKey: ["dashboard-summary"] }),
      ]);
      navigate("/walletspaces");
    },
  });
  const leaveSpaceMutation = useMutation({
    mutationFn: () => leaveWalletspace(walletspaceId),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["walletspaces"] }),
        queryClient.invalidateQueries({ queryKey: ["dashboard-summary"] }),
      ]);
      navigate("/walletspaces");
    },
  });
  const transferMutation = useMutation({
    mutationFn: (userId: string) => transferOwnership(walletspaceId, userId),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId] }),
        queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId, "members"] }),
        queryClient.invalidateQueries({ queryKey: ["walletspaces"] }),
      ]);
    },
  });

  function showTab(tab: TabId) {
    setRecurrence(0);
    setFormOpen(false);
    setEditingAccount(null);
    setEditingCard(null);
    setActionError(null);
    setSearchParams(tab === "overview" ? {} : { tab });
  }

  async function performAction(action: () => Promise<unknown>) {
    setActionError(null);
    try {
      await action();
      setFormOpen(false);
      setEditingAccount(null);
      setEditingCard(null);
    } catch (error) {
      setActionError(getAuthErrorMessage(error, "Não foi possível concluir a operação."));
    }
  }

  async function handleAccountSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    await performAction(() => accountMutation.mutateAsync({
      id: editingAccount?.id,
      account: {
        name: String(form.get("name") ?? "").trim(),
        type: Number(form.get("type")) as FinancialAccountType,
        openingBalance: Number(form.get("openingBalance")),
        institution: String(form.get("institution") ?? "").trim() || undefined,
      },
    }));
  }

  async function handleCardSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    await performAction(() => cardMutation.mutateAsync({
      id: editingCard?.id,
      card: {
        name: String(form.get("name") ?? "").trim(),
        lastFourDigits: String(form.get("lastFourDigits") ?? ""),
        creditLimit: Number(form.get("creditLimit")),
        closingDay: Number(form.get("closingDay")),
        dueDay: Number(form.get("dueDay")),
      },
    }));
  }

  async function handleTransactionSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const resourceKind = String(form.get("resourceKind"));
    const transactionType = Number(form.get("type")) as FinancialTransactionType;
    await performAction(() => createTransactionMutation.mutateAsync({
      financialAccountId: resourceKind === "account" ? String(form.get("resourceId")) : null,
      creditCardId: resourceKind === "card" ? String(form.get("resourceId")) : null,
      description: String(form.get("description") ?? "").trim(),
      category: String(form.get("category") ?? "").trim() || undefined,
      amount: Number(form.get("amount")),
      type: transactionType,
      status: Number(form.get("status")) as FinancialTransactionStatus,
      transactionDate: String(form.get("transactionDate")),
      dueDate: String(form.get("dueDate") ?? "") || null,
      recurrence,
      occurrences: recurrence === 0 ? undefined : occurrences,
      amountIsPerInstallment: recurrence === 2 ? amountIsPerInstallment : false,
    }));
    setRecurrence(0);
  }

  async function handleTransfer(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const userId = String(form.get("newOwner") ?? "");
    if (!userId) return;
    if (!window.confirm("Transferir a propriedade? Você passará a ser Administrador.")) return;
    await performAction(() => transferMutation.mutateAsync(userId));
  }

  async function handleMemberSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    await performAction(() => addMemberMutation.mutateAsync({
      email: String(form.get("email") ?? "").trim(),
      role: Number(form.get("role")) as WalletspaceRole,
    }));
  }

  async function handleRename(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    await performAction(() => renameMutation.mutateAsync(String(form.get("name") ?? "").trim()));
  }

  function toggleTransactionStatus(transaction: FinancialTransaction) {
    const status: FinancialTransactionStatus = transaction.status === 1 ? 2 : 1;
    const data: SaveFinancialTransaction = {
      financialAccountId: transaction.financialAccountId,
      creditCardId: transaction.creditCardId,
      description: transaction.description,
      category: transaction.category ?? undefined,
      amount: transaction.amount,
      type: transaction.type,
      status,
      transactionDate: transaction.transactionDate,
      dueDate: transaction.dueDate,
    };
    void performAction(() => transactionMutation.mutateAsync({ transaction, data, scope: "single" }));
  }

  function removeTransaction(transaction: FinancialTransaction) {
    let scope: TransactionSeriesScope | null = "single";
    if (transaction.seriesId) {
      scope = askSeriesScope("Remover");
    } else if (!window.confirm("Remover este lançamento?")) {
      scope = null;
    }
    if (!scope) return;
    const chosen = scope;
    void performAction(() => deleteTransactionMutation.mutateAsync({ id: transaction.id, scope: chosen }));
  }

  function deleteSpaceResource(kind: "account" | "card", id: string) {
    const confirmed = window.confirm(
      kind === "account"
        ? "Remover esta conta? Contas com lançamentos não podem ser removidas."
        : "Remover este cartão? Cartões com lançamentos não podem ser removidos.",
    );
    if (!confirmed) return;
    void performAction(async () => {
      if (kind === "account") {
        await deleteAccount(walletspaceId, id);
        await queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId, "accounts"] });
      } else {
        await deleteCard(walletspaceId, id);
        await queryClient.invalidateQueries({ queryKey: ["walletspace", walletspaceId, "cards"] });
      }
    });
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
  const paidTransactions = transactions.filter((transaction) => transaction.status === 2);
  const income = paidTransactions
    .filter((transaction) => transaction.type === 1)
    .reduce((total, transaction) => total + transaction.amount, 0);
  const expense = paidTransactions
    .filter((transaction) => transaction.type === 2)
    .reduce((total, transaction) => total + transaction.amount, 0);
  const isBusy =
    accountMutation.isPending ||
    cardMutation.isPending ||
    createTransactionMutation.isPending ||
    transactionMutation.isPending ||
    deleteTransactionMutation.isPending ||
    addMemberMutation.isPending ||
    memberRoleMutation.isPending ||
    removeMemberMutation.isPending ||
    renameMutation.isPending ||
    deleteSpaceMutation.isPending ||
    leaveSpaceMutation.isPending ||
    transferMutation.isPending;

  return (
    <div className="mx-auto max-w-5xl">
      <Link to="/walletspaces" className="inline-flex items-center gap-2 text-sm font-semibold text-slate-600 hover:text-violet-900">
        <span aria-hidden="true">←</span>
        Todos os espaços
      </Link>
      <div className="mt-5 flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
        <div className="min-w-0">
          <p className="text-xs font-bold uppercase tracking-[0.18em] text-violet-800">Walletspace</p>
          <h1 className="mt-2 truncate text-3xl font-semibold tracking-tight text-slate-950 sm:text-4xl">{space.name}</h1>
          <p className="mt-2 text-sm text-slate-600">
            {roleNames[space.myRole]} · {space.memberCount} {space.memberCount === 1 ? "membro" : "membros"}
          </p>
        </div>
        {memberAdmin && (
          <button
            type="button"
            onClick={() => showTab("settings")}
            className="inline-flex min-h-11 items-center justify-center rounded-xl border border-slate-300 bg-white px-4 text-sm font-semibold text-slate-700 transition hover:border-violet-300 hover:bg-violet-50 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-700"
          >
            Configurações do espaço
          </button>
        )}
      </div>

      <nav aria-label="Seções do Walletspace" className="mt-7 flex gap-1 overflow-x-auto border-b border-slate-200 pb-px">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            type="button"
            onClick={() => showTab(tab.id)}
            aria-current={activeTab === tab.id ? "page" : undefined}
            className={`min-h-11 shrink-0 border-b-2 px-3 text-sm font-semibold transition ${
              activeTab === tab.id
                ? "border-violet-800 text-violet-900"
                : "border-transparent text-slate-500 hover:border-slate-300 hover:text-slate-800"
            }`}
          >
            {tab.label}
          </button>
        ))}
      </nav>

      {actionError && <div className="mt-5"><ErrorMessage message={actionError} /></div>}

      {activeTab === "overview" && (
        <>
          <section className="mt-6 grid gap-4 sm:grid-cols-3" aria-label="Resumo do Walletspace">
            {[
              { label: "Saldo em contas", value: currency.format((accountsQuery.data ?? []).reduce((total, account) => total + account.currentBalance, 0)) },
              { label: `Receitas efetivadas · ${monthLabel(month)}`, value: currency.format(income) },
              { label: `Despesas efetivadas · ${monthLabel(month)}`, value: currency.format(expense) },
            ].map((item) => (
              <article key={item.label} className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                <p className="text-sm font-medium text-slate-600">{item.label}</p>
                <p className="mt-4 text-2xl font-semibold text-slate-950">{item.value}</p>
              </article>
            ))}
          </section>
          <section className="mt-6 grid gap-4 sm:grid-cols-3">
            {[
              { tab: "accounts" as const, title: "Contas", count: accountsQuery.data?.length ?? 0, hint: "Bancos, dinheiro e investimentos" },
              { tab: "cards" as const, title: "Cartões", count: cardsQuery.data?.length ?? 0, hint: "Limites e faturas" },
              { tab: "transactions" as const, title: "Lançamentos", count: transactions.length, hint: "Receitas e despesas" },
            ].map((item) => (
              <button key={item.tab} type="button" onClick={() => showTab(item.tab)} className="rounded-2xl border border-slate-200 bg-white p-5 text-left transition hover:border-violet-200 hover:shadow-sm">
                <p className="text-sm font-semibold text-slate-950">{item.title}</p>
                <p className="mt-3 text-2xl font-semibold text-violet-900">{item.count}</p>
                <p className="mt-1 text-xs text-slate-500">{item.hint}</p>
              </button>
            ))}
          </section>
        </>
      )}

      {activeTab === "accounts" && (
        <section className="mt-6 space-y-5">
          <div className="flex items-center justify-between gap-4">
            <div><h2 className="text-lg font-semibold text-slate-950">Contas e carteiras</h2><p className="mt-1 text-sm text-slate-600">Gerencie contas bancárias, dinheiro e investimentos.</p></div>
            {manageAccess && <button type="button" onClick={() => { setActionError(null); setEditingAccount(null); setFormOpen((open) => !open); }} className="min-h-10 rounded-xl bg-violet-900 px-3 text-sm font-semibold text-white hover:bg-violet-800">{formOpen ? "Cancelar" : "+ Nova conta"}</button>}
          </div>
          {formOpen && manageAccess && (
            <form key={editingAccount?.id ?? "new-account"} onSubmit={handleAccountSubmit} className="grid gap-4 rounded-2xl border border-slate-200 bg-white p-5 sm:grid-cols-2">
              <div><label htmlFor="account-name" className="mb-1.5 block text-sm font-semibold">Nome da conta</label><input id="account-name" name="name" required maxLength={100} defaultValue={editingAccount?.name ?? ""} placeholder="Ex.: Conta principal" className="min-h-11 w-full rounded-xl border border-slate-300 px-3 text-sm outline-none focus:border-violet-700 focus:ring-4 focus:ring-violet-100" /></div>
              <div><label htmlFor="account-type" className="mb-1.5 block text-sm font-semibold">Tipo</label><select id="account-type" name="type" defaultValue={editingAccount?.type ?? 1} className="min-h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm"><option value="1">Conta corrente</option><option value="2">Poupança</option><option value="3">Dinheiro</option><option value="4">Investimentos</option><option value="5">Outra</option></select></div>
              <div><label htmlFor="account-bank" className="mb-1.5 block text-sm font-semibold">Instituição <span className="font-normal text-slate-500">(opcional)</span></label><input id="account-bank" name="institution" maxLength={100} defaultValue={editingAccount?.institution ?? ""} placeholder="Banco ou instituição" className="min-h-11 w-full rounded-xl border border-slate-300 px-3 text-sm outline-none focus:border-violet-700 focus:ring-4 focus:ring-violet-100" /></div>
              <div><label htmlFor="account-opening" className="mb-1.5 block text-sm font-semibold">Saldo inicial</label><input id="account-opening" name="openingBalance" type="number" step="0.01" defaultValue={editingAccount?.openingBalance ?? 0} required className="min-h-11 w-full rounded-xl border border-slate-300 px-3 text-sm outline-none focus:border-violet-700 focus:ring-4 focus:ring-violet-100" /></div>
              <button disabled={isBusy} className="min-h-11 rounded-xl bg-violet-900 px-4 text-sm font-semibold text-white disabled:opacity-50 sm:col-span-2 sm:justify-self-end">{isBusy ? "Salvando..." : editingAccount ? "Salvar alterações" : "Salvar conta"}</button>
            </form>
          )}
          {accountsQuery.isPending ? <p className="text-sm text-slate-500">Carregando contas...</p> : accountsQuery.isError ? <ErrorMessage message={getAuthErrorMessage(accountsQuery.error, "Não foi possível carregar as contas.")} /> : accountsQuery.data?.length ? (
            <div className="overflow-hidden rounded-2xl border border-slate-200 bg-white">
              <div className="divide-y divide-slate-100">
                {accountsQuery.data.map((account) => (
                  <article key={account.id} className="flex flex-wrap items-center justify-between gap-4 p-4 sm:px-5">
                    <div className="flex min-w-0 items-center gap-3"><span className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-violet-50 text-violet-800">◈</span><div className="min-w-0"><h3 className="truncate text-sm font-semibold text-slate-950">{account.name}</h3><p className="mt-1 text-xs text-slate-500">{account.institution ? `${account.institution} · ` : ""}{accountTypeNames[account.type]}</p></div></div>
                    <div className="ml-auto text-right"><p className="text-sm font-semibold text-slate-950">{currency.format(account.currentBalance)}</p><p className="mt-1 text-xs text-slate-500">Saldo atual</p></div>
                    {manageAccess && <div className="flex gap-1"><button type="button" onClick={() => { setEditingAccount(account); setFormOpen(true); setActionError(null); }} className="rounded-lg px-2 py-1 text-xs font-semibold text-violet-800 hover:bg-violet-50">Editar</button><button type="button" aria-label={`Remover conta ${account.name}`} onClick={() => deleteSpaceResource("account", account.id)} className="rounded-lg px-2 py-1 text-xs font-semibold text-slate-500 hover:bg-rose-50 hover:text-rose-700">Remover</button></div>}
                  </article>
                ))}
              </div>
            </div>
          ) : <EmptyPanel title="Nenhuma conta cadastrada" description="Adicione uma conta para acompanhar saldo inicial e movimentações." />}
        </section>
      )}

      {activeTab === "cards" && (
        <section className="mt-6 space-y-5">
          <div className="flex items-center justify-between gap-4">
            <div><h2 className="text-lg font-semibold text-slate-950">Cartões</h2><p className="mt-1 text-sm text-slate-600">Acompanhe seus limites e despesas pendentes.</p></div>
            {manageAccess && <button type="button" onClick={() => { setActionError(null); setEditingCard(null); setFormOpen((open) => !open); }} className="min-h-10 rounded-xl bg-violet-900 px-3 text-sm font-semibold text-white hover:bg-violet-800">{formOpen ? "Cancelar" : "+ Novo cartão"}</button>}
          </div>
          {formOpen && manageAccess && (
            <form key={editingCard?.id ?? "new-card"} onSubmit={handleCardSubmit} className="grid gap-4 rounded-2xl border border-slate-200 bg-white p-5 sm:grid-cols-2">
              <div><label htmlFor="card-name" className="mb-1.5 block text-sm font-semibold">Nome do cartão</label><input id="card-name" name="name" required maxLength={100} defaultValue={editingCard?.name ?? ""} placeholder="Ex.: Cartão pessoal" className="min-h-11 w-full rounded-xl border border-slate-300 px-3 text-sm" /></div>
              <div><label htmlFor="card-digits" className="mb-1.5 block text-sm font-semibold">Últimos 4 dígitos</label><input id="card-digits" name="lastFourDigits" required pattern="[0-9]{4}" maxLength={4} inputMode="numeric" defaultValue={editingCard?.lastFourDigits ?? ""} placeholder="1234" className="min-h-11 w-full rounded-xl border border-slate-300 px-3 text-sm" /></div>
              <div><label htmlFor="card-limit" className="mb-1.5 block text-sm font-semibold">Limite</label><input id="card-limit" name="creditLimit" type="number" min="0" step="0.01" defaultValue={editingCard?.creditLimit ?? 0} required className="min-h-11 w-full rounded-xl border border-slate-300 px-3 text-sm" /></div>
              <div className="grid grid-cols-2 gap-3"><div><label htmlFor="card-closing" className="mb-1.5 block text-sm font-semibold">Fechamento</label><input id="card-closing" name="closingDay" type="number" min="1" max="31" defaultValue={editingCard?.closingDay ?? 10} required className="min-h-11 w-full rounded-xl border border-slate-300 px-3 text-sm" /></div><div><label htmlFor="card-due" className="mb-1.5 block text-sm font-semibold">Vencimento</label><input id="card-due" name="dueDay" type="number" min="1" max="31" defaultValue={editingCard?.dueDay ?? 17} required className="min-h-11 w-full rounded-xl border border-slate-300 px-3 text-sm" /></div></div>
              <button disabled={isBusy} className="min-h-11 rounded-xl bg-violet-900 px-4 text-sm font-semibold text-white disabled:opacity-50 sm:col-span-2 sm:justify-self-end">{isBusy ? "Salvando..." : editingCard ? "Salvar alterações" : "Salvar cartão"}</button>
            </form>
          )}
          {cardsQuery.isPending ? <p className="text-sm text-slate-500">Carregando cartões...</p> : cardsQuery.isError ? <ErrorMessage message={getAuthErrorMessage(cardsQuery.error, "Não foi possível carregar os cartões.")} /> : cardsQuery.data?.length ? (
            <div className="grid gap-4 sm:grid-cols-2">
              {cardsQuery.data.map((card) => (
                <article key={card.id} className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                  <div className="flex items-start justify-between"><span className="flex size-10 items-center justify-center rounded-xl bg-amber-50 text-amber-800">▰</span><span className="text-xs font-medium text-slate-500">•••• {card.lastFourDigits}</span></div>
                  <h3 className="mt-4 font-semibold text-slate-950">{card.name}</h3>
                  <p className="mt-1 text-xs text-slate-500">Fecha dia {card.closingDay} · vence dia {card.dueDay}</p>
                  <div className="mt-5 flex justify-between border-t border-slate-100 pt-4 text-sm"><div><p className="text-xs text-slate-500">Em aberto</p><p className="mt-1 font-semibold text-slate-950">{currency.format(card.outstanding)}</p></div><div className="text-right"><p className="text-xs text-slate-500">Limite</p><p className="mt-1 font-semibold text-slate-950">{currency.format(card.creditLimit)}</p></div></div>
                  {manageAccess && <div className="mt-4 flex gap-3"><button type="button" onClick={() => { setEditingCard(card); setFormOpen(true); setActionError(null); }} className="text-xs font-semibold text-violet-800 hover:underline">Editar cartão</button><button type="button" onClick={() => deleteSpaceResource("card", card.id)} className="text-xs font-semibold text-slate-500 hover:text-rose-700">Remover cartão</button></div>}
                </article>
              ))}
            </div>
          ) : <EmptyPanel title="Nenhum cartão cadastrado" description="Cadastre um cartão para lançar despesas e acompanhar o total pendente." />}
        </section>
      )}

      {activeTab === "transactions" && (
        <section className="mt-6 space-y-5">
          <div className="flex items-center justify-between gap-4">
            <div><h2 className="text-lg font-semibold text-slate-950">Lançamentos</h2><p className="mt-1 text-sm text-slate-600">Registre receitas e despesas das contas e cartões.</p></div>
            {writeAccess && <button type="button" onClick={() => { setActionError(null); setFormOpen((open) => !open); }} className="min-h-10 rounded-xl bg-violet-900 px-3 text-sm font-semibold text-white hover:bg-violet-800">{formOpen ? "Cancelar" : "+ Novo lançamento"}</button>}
          </div>
          {formOpen && writeAccess && (
            <form onSubmit={handleTransactionSubmit} className="grid gap-4 rounded-2xl border border-slate-200 bg-white p-5 sm:grid-cols-2">
              <div><label htmlFor="transaction-description" className="mb-1.5 block text-sm font-semibold">Descrição</label><input id="transaction-description" name="description" required maxLength={200} placeholder="Ex.: Mercado, salário" className="min-h-11 w-full rounded-xl border border-slate-300 px-3 text-sm" /></div>
              <div><label htmlFor="transaction-category" className="mb-1.5 block text-sm font-semibold">Categoria</label><input id="transaction-category" name="category" maxLength={100} placeholder="Ex.: Alimentação" className="min-h-11 w-full rounded-xl border border-slate-300 px-3 text-sm" /></div>
              <div><label htmlFor="transaction-type" className="mb-1.5 block text-sm font-semibold">Tipo</label><select id="transaction-type" name="type" className="min-h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm"><option value="2">Despesa</option><option value="1">Receita</option></select></div>
              <div><label htmlFor="transaction-status" className="mb-1.5 block text-sm font-semibold">Situação</label><select id="transaction-status" name="status" className="min-h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm"><option value="1">Pendente</option><option value="2">Efetivada</option></select></div>
              <div><label htmlFor="transaction-amount" className="mb-1.5 block text-sm font-semibold">Valor</label><input id="transaction-amount" name="amount" type="number" min="0.01" step="0.01" required onChange={(event) => setInstallmentAmount(Number(event.target.value) || 0)} className="min-h-11 w-full rounded-xl border border-slate-300 px-3 text-sm" /></div>
              <div><label htmlFor="transaction-date" className="mb-1.5 block text-sm font-semibold">Data</label><input id="transaction-date" name="transactionDate" type="date" defaultValue={today} required className="min-h-11 w-full rounded-xl border border-slate-300 px-3 text-sm" /></div>
              <div><label htmlFor="transaction-resource-kind" className="mb-1.5 block text-sm font-semibold">Lançar em</label><select id="transaction-resource-kind" name="resourceKind" value={transactionResourceKind} onChange={(event) => setTransactionResourceKind(event.target.value as "account" | "card")} className="min-h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm"><option value="account">Conta</option><option value="card" disabled={cardsQuery.data?.length === 0}>Cartão</option></select></div>
              <div><label htmlFor="transaction-resource-id" className="mb-1.5 block text-sm font-semibold">{transactionResourceKind === "account" ? "Conta" : "Cartão"}</label><select id="transaction-resource-id" name="resourceId" required className="min-h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm">{transactionResourceKind === "account" ? (accountsQuery.data ?? []).map((account) => <option key={account.id} value={account.id}>{account.name}</option>) : (cardsQuery.data ?? []).map((card) => <option key={card.id} value={card.id}>{card.name} · •••• {card.lastFourDigits}</option>)}</select></div>
              <div><label htmlFor="transaction-due" className="mb-1.5 block text-sm font-semibold">Vencimento <span className="font-normal text-slate-500">(opcional)</span></label><input id="transaction-due" name="dueDate" type="date" className="min-h-11 w-full rounded-xl border border-slate-300 px-3 text-sm" /></div>
              <div><label htmlFor="transaction-recurrence" className="mb-1.5 block text-sm font-semibold">Repetição</label><select id="transaction-recurrence" value={recurrence} onChange={(event) => setRecurrence(Number(event.target.value) as FinancialTransactionRecurrence)} className="min-h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm"><option value={0}>Lançamento único</option><option value={1}>Conta fixa mensal (aluguel, assinatura...)</option><option value={2}>{transactionResourceKind === "card" ? "Parcelado no cartão" : "Parcelado (imóvel, veículo, financiamento...)"}</option></select></div>
              {recurrence !== 0 && (
                <div><label htmlFor="transaction-occurrences" className="mb-1.5 block text-sm font-semibold">{recurrence === 1 ? "Quantidade de meses" : "Número de parcelas"}</label><input id="transaction-occurrences" type="number" min={2} max={120} value={occurrences} onChange={(event) => setOccurrences(Math.min(120, Math.max(2, Number(event.target.value) || 2)))} className="min-h-11 w-full rounded-xl border border-slate-300 px-3 text-sm" /></div>
              )}
              {recurrence === 2 && (
                <label className="flex items-center gap-2 text-sm text-slate-700 sm:col-span-2"><input type="checkbox" checked={amountIsPerInstallment} onChange={(event) => setAmountIsPerInstallment(event.target.checked)} className="size-4" />O valor informado é o de cada parcela (desmarcado = valor total a dividir)</label>
              )}
              {recurrence !== 0 && (
                <p className="rounded-xl bg-violet-50 px-3 py-2 text-xs leading-5 text-violet-900 sm:col-span-2">
                  {recurrence === 1
                    ? `Serão criados ${occurrences} lançamentos mensais com o mesmo valor, a partir da data informada. Os próximos ficam pendentes até serem efetivados.`
                    : `Serão criadas ${occurrences} parcelas${installmentAmount > 0 ? ` de ${currency.format(amountIsPerInstallment ? installmentAmount : installmentAmount / occurrences)} (total ${currency.format(amountIsPerInstallment ? installmentAmount * occurrences : installmentAmount)})` : ""}.${transactionResourceKind === "card" ? " O vencimento de cada parcela segue o fechamento e vencimento do cartão." : ""}`}
                </p>
              )}
              <p className="text-xs leading-5 text-slate-500 sm:col-span-2">Para despesas no cartão, escolha “Cartão” no campo “Lançar em”. O cartão aceita apenas despesas.</p>
              <button disabled={isBusy || (!accountsQuery.data?.length && !cardsQuery.data?.length)} className="min-h-11 rounded-xl bg-violet-900 px-4 text-sm font-semibold text-white disabled:opacity-50 sm:col-span-2 sm:justify-self-end">{isBusy ? "Salvando..." : "Salvar lançamento"}</button>
            </form>
          )}
          <div className="flex items-center justify-between gap-3 rounded-2xl border border-slate-200 bg-white p-3">
            <button type="button" aria-label="Mês anterior" onClick={() => setMonth((current) => shiftMonth(current, -1))} className="min-h-10 rounded-xl px-3 text-sm font-semibold text-slate-700 hover:bg-violet-50">←</button>
            <div className="text-center"><p className="text-sm font-semibold text-slate-950">{monthLabel(month)}</p><p className="text-xs text-slate-500">Receitas {currency.format(transactions.filter((t) => t.type === 1).reduce((a, t) => a + t.amount, 0))} · Despesas {currency.format(transactions.filter((t) => t.type === 2).reduce((a, t) => a + t.amount, 0))} (previstas)</p></div>
            <button type="button" aria-label="Próximo mês" onClick={() => setMonth((current) => shiftMonth(current, 1))} className="min-h-10 rounded-xl px-3 text-sm font-semibold text-slate-700 hover:bg-violet-50">→</button>
          </div>
          {transactionsQuery.isPending ? <p className="text-sm text-slate-500">Carregando lançamentos...</p> : transactionsQuery.isError ? <ErrorMessage message={getAuthErrorMessage(transactionsQuery.error, "Não foi possível carregar os lançamentos.")} /> : transactions.length ? (
            <div className="overflow-hidden rounded-2xl border border-slate-200 bg-white">
              <div className="divide-y divide-slate-100">
                {transactions.map((transaction) => {
                  const linkedAccount = accountsQuery.data?.find((account) => account.id === transaction.financialAccountId);
                  const linkedCard = cardsQuery.data?.find((card) => card.id === transaction.creditCardId);
                  return (
                    <article key={transaction.id} className="flex flex-wrap items-center gap-3 p-4 sm:px-5">
                      <span className={`flex size-9 shrink-0 items-center justify-center rounded-xl text-sm font-bold ${transaction.type === 1 ? "bg-emerald-50 text-emerald-800" : "bg-rose-50 text-rose-800"}`}>{transaction.type === 1 ? "+" : "−"}</span>
                      <div className="min-w-0 flex-1"><h3 className="truncate text-sm font-semibold text-slate-950">{transaction.description}{transaction.recurrence === 1 && <span className="ml-2 rounded-full bg-sky-50 px-2 py-0.5 text-[10px] font-semibold text-sky-800">Fixa</span>}{transaction.recurrence === 2 && <span className="ml-2 rounded-full bg-violet-50 px-2 py-0.5 text-[10px] font-semibold text-violet-800">Parcela {transaction.installmentNumber}/{transaction.installmentCount}</span>}</h3><p className="mt-1 text-xs text-slate-500">{transaction.category ?? "Sem categoria"} · {linkedAccount?.name ?? linkedCard?.name ?? "Conta removida"} · {new Date(`${transaction.transactionDate}T12:00:00`).toLocaleDateString("pt-BR")}</p></div>
                      <div className="text-right"><p className={`text-sm font-semibold ${transaction.type === 1 ? "text-emerald-800" : "text-slate-950"}`}>{transaction.type === 1 ? "+" : "−"}{currency.format(transaction.amount)}</p><span className={`mt-1 inline-flex rounded-full px-2 py-0.5 text-[10px] font-semibold ${transaction.status === 2 ? "bg-emerald-50 text-emerald-800" : "bg-amber-50 text-amber-800"}`}>{transaction.status === 2 ? "Efetivada" : "Pendente"}</span></div>
                      {writeAccess && <div className="flex items-center gap-1"><button type="button" onClick={() => toggleTransactionStatus(transaction)} className="rounded-lg px-2 py-1 text-xs font-semibold text-violet-800 hover:bg-violet-50">{transaction.status === 2 ? "Reabrir" : "Efetivar"}</button><button type="button" aria-label={`Remover lançamento ${transaction.description}`} onClick={() => removeTransaction(transaction)} className="rounded-lg px-2 py-1 text-xs font-semibold text-slate-500 hover:bg-rose-50 hover:text-rose-700">Remover</button></div>}
                    </article>
                  );
                })}
              </div>
            </div>
          ) : <EmptyPanel title="Nenhum lançamento neste mês" description="Adicione uma conta ou cartão e registre receitas, despesas, contas fixas ou parcelas." />}
        </section>
      )}

      {activeTab === "reports" && (
        <section className="mt-6 space-y-5">
          <div><h2 className="text-lg font-semibold text-slate-950">Relatório financeiro</h2><p className="mt-1 text-sm text-slate-600">Compare receitas e despesas efetivadas por período.</p></div>
          <form onSubmit={(event) => { event.preventDefault(); void reportQuery.refetch(); }} className="flex flex-col gap-3 rounded-2xl border border-slate-200 bg-white p-4 sm:flex-row sm:items-end">
            <div className="flex-1"><label htmlFor="report-from" className="mb-1.5 block text-sm font-semibold">De</label><input id="report-from" type="date" value={reportFrom} onChange={(event) => setReportFrom(event.target.value)} required className="min-h-11 w-full rounded-xl border border-slate-300 px-3 text-sm" /></div>
            <div className="flex-1"><label htmlFor="report-to" className="mb-1.5 block text-sm font-semibold">Até</label><input id="report-to" type="date" value={reportTo} onChange={(event) => setReportTo(event.target.value)} required className="min-h-11 w-full rounded-xl border border-slate-300 px-3 text-sm" /></div>
            <button disabled={reportQuery.isFetching || reportFrom > reportTo} className="min-h-11 rounded-xl bg-violet-900 px-4 text-sm font-semibold text-white disabled:opacity-50">{reportQuery.isFetching ? "Atualizando..." : "Aplicar período"}</button>
          </form>
          {reportFrom > reportTo && <ErrorMessage message="A data inicial precisa ser anterior à data final." />}
          {reportQuery.isError && <ErrorMessage message={getAuthErrorMessage(reportQuery.error, "Não foi possível gerar o relatório.")} />}
          {reportQuery.data && (
            <>
              <div className="grid gap-4 sm:grid-cols-3">
                {[{ label: "Receitas efetivadas", value: reportQuery.data.income, tone: "text-emerald-800" }, { label: "Despesas efetivadas", value: reportQuery.data.expense, tone: "text-rose-800" }, { label: "Resultado do período", value: reportQuery.data.net, tone: reportQuery.data.net >= 0 ? "text-violet-900" : "text-rose-800" }].map((item) => (
                  <article key={item.label} className="rounded-2xl border border-slate-200 bg-white p-5"><p className="text-sm text-slate-600">{item.label}</p><p className={`mt-3 text-2xl font-semibold ${item.tone}`}>{currency.format(item.value)}</p></article>
                ))}
              </div>
              <div className="rounded-2xl border border-slate-200 bg-white p-5">
                <div className="flex items-center justify-between"><h3 className="font-semibold text-slate-950">Categorias</h3><span className="text-xs text-slate-500">{reportQuery.data.pendingCount} pendentes no período</span></div>
                {reportQuery.data.categories.length ? <div className="mt-4 divide-y divide-slate-100">{reportQuery.data.categories.map((category) => <div key={`${category.type}-${category.category}`} className="flex justify-between gap-4 py-3 text-sm"><span className="text-slate-700">{category.category} <span className="text-xs text-slate-500">· {category.type === 1 ? "Receita" : "Despesa"}</span></span><span className="font-semibold text-slate-950">{currency.format(category.total)}</span></div>)}</div> : <p className="mt-4 text-sm text-slate-500">Nenhum lançamento efetivado neste período.</p>}
              </div>
            </>
          )}
        </section>
      )}

      {activeTab === "members" && (
        <section className="mt-6 space-y-5">
          <div><h2 className="text-lg font-semibold text-slate-950">Pessoas e permissões</h2><p className="mt-1 text-sm text-slate-600">Controle quem pode ver e movimentar este Walletspace.</p></div>
          {memberAdmin && (
            <form onSubmit={handleMemberSubmit} className="grid gap-3 rounded-2xl border border-slate-200 bg-white p-4 sm:grid-cols-[1fr_13rem_auto] sm:items-end">
              <div><label htmlFor="member-email" className="mb-1.5 block text-sm font-semibold">E-mail da pessoa</label><input id="member-email" name="email" type="email" autoComplete="email" required placeholder="pessoa@email.com" className="min-h-11 w-full rounded-xl border border-slate-300 px-3 text-sm" /></div>
              <div><label htmlFor="member-role" className="mb-1.5 block text-sm font-semibold">Permissão</label><select id="member-role" name="role" className="min-h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm"><option value="5">Visualizador</option><option value="4">Membro</option><option value="3">Gestor</option>{space.myRole === 1 && <option value="2">Administrador</option>}</select></div>
              <button disabled={isBusy} className="min-h-11 rounded-xl bg-violet-900 px-4 text-sm font-semibold text-white disabled:opacity-50">{isBusy ? "Adicionando..." : "Adicionar"}</button>
              <p className="text-xs leading-5 text-slate-500 sm:col-span-3">A pessoa precisa ter uma conta Aurum com este e-mail. O acesso é concedido imediatamente.</p>
            </form>
          )}
          {membersQuery.isPending ? <p className="text-sm text-slate-500">Carregando membros...</p> : membersQuery.isError ? <ErrorMessage message={getAuthErrorMessage(membersQuery.error, "Não foi possível carregar os membros.")} /> : membersQuery.data?.length ? (
            <div className="overflow-hidden rounded-2xl border border-slate-200 bg-white">
              <div className="divide-y divide-slate-100">{membersQuery.data.map((member) => (
                <article key={member.userId} className="flex flex-wrap items-center gap-3 p-4 sm:px-5">
                  <span className="flex size-10 shrink-0 items-center justify-center rounded-full bg-violet-100 text-xs font-bold text-violet-900">{(member.fullName || member.email).slice(0, 2).toUpperCase()}</span>
                  <div className="min-w-0 flex-1"><h3 className="truncate text-sm font-semibold text-slate-950">{member.fullName || member.email}</h3><p className="truncate text-xs text-slate-500">{member.fullName ? member.email : `Entrou em ${new Date(member.joinedAt).toLocaleDateString("pt-BR")}`}</p></div>
                  {memberAdmin && member.role !== 1 && (space.myRole === 1 || member.role !== 2) ? <><label className="sr-only" htmlFor={`role-${member.userId}`}>Permissão de {member.email}</label><select id={`role-${member.userId}`} value={member.role} disabled={isBusy} onChange={(event) => { const role = Number(event.target.value) as WalletspaceRole; void performAction(() => memberRoleMutation.mutateAsync({ userId: member.userId, role })); }} className="min-h-9 max-w-44 rounded-lg border border-slate-300 bg-white px-2 text-xs font-semibold text-slate-700">{space.myRole === 1 && <option value="2">Administrador</option>}<option value="3">Gestor</option><option value="4">Membro</option><option value="5">Visualizador</option></select><button type="button" onClick={() => { if (window.confirm(`Remover ${member.email} deste Walletspace?`)) void performAction(() => removeMemberMutation.mutateAsync(member.userId)); }} className="rounded-lg px-2 py-1 text-xs font-semibold text-slate-500 hover:bg-rose-50 hover:text-rose-700">Remover</button></> : <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold text-slate-600">{roleNames[member.role]}</span>}
                </article>
              ))}</div>
            </div>
          ) : <EmptyPanel title="Nenhum membro encontrado" description="As pessoas adicionadas a este espaço aparecerão aqui." />}
        </section>
      )}

      {activeTab === "settings" && (
        <section className="mt-6 space-y-5">
          <div><h2 className="text-lg font-semibold text-slate-950">Configurações do espaço</h2><p className="mt-1 text-sm text-slate-600">Nome, propriedade e acesso a este Walletspace.</p></div>
          {memberAdmin && (
            <form onSubmit={handleRename} className="rounded-2xl border border-slate-200 bg-white p-5">
              <h3 className="font-semibold text-slate-950">Renomear</h3>
              <div className="mt-3 flex flex-col gap-3 sm:flex-row">
                <label htmlFor="rename-space" className="sr-only">Nome do Walletspace</label>
                <input id="rename-space" name="name" defaultValue={space.name} minLength={3} maxLength={100} required className="min-h-11 flex-1 rounded-xl border border-slate-300 px-3 text-sm outline-none focus:border-violet-700 focus:ring-4 focus:ring-violet-100" />
                <button disabled={isBusy} className="min-h-11 rounded-xl bg-violet-900 px-4 text-sm font-semibold text-white disabled:opacity-50">{renameMutation.isPending ? "Salvando..." : "Salvar nome"}</button>
              </div>
            </form>
          )}
          <div className="rounded-2xl border border-slate-200 bg-white p-5">
            <div className="flex items-center justify-between gap-3"><h3 className="font-semibold text-slate-950">Usuários</h3><button type="button" onClick={() => showTab("members")} className="text-sm font-semibold text-violet-800 hover:underline">Gerenciar membros e permissões →</button></div>
            <p className="mt-2 text-sm text-slate-600">{space.memberCount} {space.memberCount === 1 ? "pessoa tem" : "pessoas têm"} acesso. Sua permissão: {roleNames[space.myRole]}.</p>
          </div>
          {space.myRole === 1 && (
            <form onSubmit={handleTransfer} className="rounded-2xl border border-slate-200 bg-white p-5">
              <h3 className="font-semibold text-slate-950">Transferir propriedade</h3>
              <p className="mt-1 text-sm text-slate-600">O novo proprietário assume o controle total e você vira Administrador.</p>
              <div className="mt-3 flex flex-col gap-3 sm:flex-row">
                <label htmlFor="new-owner" className="sr-only">Novo proprietário</label>
                <select id="new-owner" name="newOwner" required defaultValue="" className="min-h-11 flex-1 rounded-xl border border-slate-300 bg-white px-3 text-sm"><option value="" disabled>Selecione um membro</option>{(membersQuery.data ?? []).filter((member) => member.role !== 1).map((member) => <option key={member.userId} value={member.userId}>{member.fullName || member.email}</option>)}</select>
                <button disabled={isBusy} className="min-h-11 rounded-xl border border-slate-300 px-4 text-sm font-semibold text-slate-800 hover:bg-violet-50 disabled:opacity-50">Transferir</button>
              </div>
            </form>
          )}
          {space.myRole !== 1 && (
            <div className="rounded-2xl border border-amber-200 bg-amber-50 p-5">
              <h3 className="font-semibold text-amber-950">Sair do espaço</h3>
              <p className="mt-1 text-sm text-amber-900">Você perderá o acesso a este Walletspace.</p>
              <button type="button" disabled={isBusy} onClick={() => { if (window.confirm("Sair deste Walletspace?")) void performAction(() => leaveSpaceMutation.mutateAsync()); }} className="mt-3 min-h-11 rounded-xl bg-amber-700 px-4 text-sm font-semibold text-white disabled:opacity-50">Sair do espaço</button>
            </div>
          )}
          {space.myRole === 1 && (
            <div className="rounded-2xl border border-rose-200 bg-rose-50 p-5">
              <h3 className="font-semibold text-rose-950">Zona de perigo</h3>
              <p className="mt-1 text-sm text-rose-900">Excluir remove contas, cartões e lançamentos para todos. Esta ação não pode ser desfeita. Digite <strong>{space.name}</strong> para confirmar.</p>
              <div className="mt-3 flex flex-col gap-3 sm:flex-row">
                <label htmlFor="delete-confirm" className="sr-only">Confirmar nome</label>
                <input id="delete-confirm" value={deleteConfirm} onChange={(event) => setDeleteConfirm(event.target.value)} className="min-h-11 flex-1 rounded-xl border border-rose-300 bg-white px-3 text-sm" />
                <button type="button" disabled={isBusy || deleteConfirm !== space.name} onClick={() => void performAction(() => deleteSpaceMutation.mutateAsync())} className="min-h-11 rounded-xl bg-rose-700 px-4 text-sm font-semibold text-white disabled:opacity-50">Excluir espaço</button>
              </div>
            </div>
          )}
        </section>
      )}
    </div>
  );
}
