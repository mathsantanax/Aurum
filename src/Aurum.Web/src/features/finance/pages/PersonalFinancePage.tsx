import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import type { FormEvent } from "react";
import { getAuthErrorMessage } from "../../auth/utils/getAuthErrorMessage";
import {
  createAccount,
  createCard,
  createTransaction,
  deleteAccount,
  deleteCard,
  deleteTransaction,
  getAccounts,
  getCards,
  getTransactionShares,
  getTransactions,
  getWalletspaces,
  removeTransactionShare,
  shareTransaction,
  updateAccount,
  updateCard,
  updateTransaction,
} from "../../walletspaces/api/walletspacesApi";
import type {
  CreditCard,
  FinancialAccount,
  FinancialAccountType,
  FinancialTransaction,
  FinancialTransactionStatus,
  FinancialTransactionType,
  SaveFinancialTransaction,
  TransactionSeriesScope,
  TransactionShare,
  Walletspace,
} from "../../walletspaces/types/walletspace.types";

const currency = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });
type FinanceTab = "accounts" | "cards" | "transactions";

const accountTypeNames: Record<FinancialAccountType, string> = {
  1: "Conta corrente",
  2: "Poupança",
  3: "Dinheiro",
  4: "Investimentos",
  5: "Outra",
};

function ErrorMessage({ message }: { message: string }) {
  return <div role="alert" className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-800">{message}</div>;
}

function LoadingError({
  isPending,
  isError,
  error,
  pendingText,
  errorText,
}: {
  isPending: boolean;
  isError: boolean;
  error: unknown;
  pendingText: string;
  errorText: string;
}) {
  if (isPending) return <p role="status" className="text-sm text-slate-500">{pendingText}</p>;
  if (isError) return <ErrorMessage message={getAuthErrorMessage(error, errorText)} />;
  return null;
}

function askSeriesScope(action: string): TransactionSeriesScope | null {
  const answer = window.prompt(
    `Este lançamento faz parte de uma série.\n${action}:\n1 = somente este\n2 = este e os seguintes\n3 = toda a série\n(vazio cancela)`,
    "1",
  );
  if (answer === "1") return "single";
  if (answer === "2") return "future";
  if (answer === "3") return "all";
  return null;
}

function SharePanel({
  transaction,
  walletspaces,
  walletspacesLoaded,
}: {
  transaction: FinancialTransaction;
  walletspaces: Walletspace[];
  walletspacesLoaded: boolean;
}) {
  const queryClient = useQueryClient();
  const [selectedWalletspace, setSelectedWalletspace] = useState("");
  const [error, setError] = useState<string | null>(null);
  const sharesQuery = useQuery({
    queryKey: ["transaction-shares", transaction.id],
    queryFn: () => getTransactionShares(transaction.id),
  });
  const addMutation = useMutation({
    mutationFn: () => shareTransaction(transaction.id, selectedWalletspace),
    onSuccess: async () => {
      setSelectedWalletspace("");
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["transaction-shares", transaction.id] }),
        queryClient.invalidateQueries({ queryKey: ["walletspace"] }),
      ]);
    },
  });
  const removeMutation = useMutation({
    mutationFn: (walletspaceId: string) => removeTransactionShare(transaction.id, walletspaceId),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["transaction-shares", transaction.id] }),
        queryClient.invalidateQueries({ queryKey: ["walletspace"] }),
      ]);
    },
  });
  const sharedIds = new Set((sharesQuery.data ?? []).map((share) => share.walletspaceId));
  const available = walletspaces.filter((walletspace) => !sharedIds.has(walletspace.id));

  async function perform(action: () => Promise<unknown>) {
    setError(null);
    try {
      await action();
    } catch (caught) {
      setError(getAuthErrorMessage(caught, "Não foi possível atualizar o compartilhamento."));
    }
  }

  return (
    <div className="mt-3 rounded-xl border border-violet-100 bg-violet-50/60 p-3">
      <h4 className="text-xs font-bold uppercase tracking-wide text-violet-900">Compartilhamento</h4>
      <LoadingError isPending={sharesQuery.isPending} isError={sharesQuery.isError} error={sharesQuery.error} pendingText="Carregando espaços..." errorText="Não foi possível carregar os compartilhamentos." />
      {sharesQuery.data?.length ? (
        <ul className="mt-2 space-y-2">
          {sharesQuery.data.map((share: TransactionShare) => (
            <li key={share.walletspaceId} className="flex items-center justify-between gap-3 text-sm">
              <span className="text-slate-700">{share.walletspaceName}</span>
              <button type="button" disabled={removeMutation.isPending} onClick={() => void perform(() => removeMutation.mutateAsync(share.walletspaceId))} className="text-xs font-semibold text-rose-700 hover:underline">Remover acesso</button>
            </li>
          ))}
        </ul>
      ) : sharesQuery.isSuccess ? <p className="mt-2 text-xs text-slate-600">Privada — ainda não compartilhada.</p> : null}
      {sharesQuery.isSuccess && available.length > 0 && (
        <form onSubmit={(event) => { event.preventDefault(); void perform(() => addMutation.mutateAsync()); }} className="mt-3 flex flex-col gap-2 sm:flex-row">
          <label className="sr-only" htmlFor={`share-${transaction.id}`}>Compartilhar com</label>
          <select id={`share-${transaction.id}`} required value={selectedWalletspace} onChange={(event) => setSelectedWalletspace(event.target.value)} className="min-h-9 flex-1 rounded-lg border border-slate-300 bg-white px-2 text-sm">
            <option value="" disabled>Selecione um Walletspace</option>
            {available.map((walletspace) => <option key={walletspace.id} value={walletspace.id}>{walletspace.name}</option>)}
          </select>
          <button type="submit" disabled={!selectedWalletspace || addMutation.isPending} className="min-h-9 rounded-lg bg-violet-900 px-3 text-xs font-semibold text-white disabled:opacity-50">{addMutation.isPending ? "Compartilhando..." : "Compartilhar"}</button>
        </form>
      )}
      {walletspacesLoaded && walletspaces.length === 0 && <p className="mt-2 text-xs text-slate-600">Crie ou participe de um Walletspace para compartilhar esta transação.</p>}
      {error && <div className="mt-2"><ErrorMessage message={error} /></div>}
    </div>
  );
}

export function PersonalFinancePage() {
  const queryClient = useQueryClient();
  const [activeTab, setActiveTab] = useState<FinanceTab>("transactions");
  const [editingAccount, setEditingAccount] = useState<FinancialAccount | null>(null);
  const [editingCard, setEditingCard] = useState<CreditCard | null>(null);
  const [editingTransaction, setEditingTransaction] = useState<FinancialTransaction | null>(null);
  const [showShareFor, setShowShareFor] = useState<string | null>(null);
  const [transactionResource, setTransactionResource] = useState<"account" | "card">("account");
  const [actionError, setActionError] = useState<string | null>(null);

  const accountsQuery = useQuery({ queryKey: ["my-finances", "accounts"], queryFn: getAccounts });
  const cardsQuery = useQuery({ queryKey: ["my-finances", "cards"], queryFn: getCards });
  const transactionsQuery = useQuery({ queryKey: ["my-finances", "transactions"], queryFn: () => getTransactions() });
  const walletspacesQuery = useQuery({ queryKey: ["walletspaces"], queryFn: getWalletspaces });
  const accounts = accountsQuery.data ?? [];
  const cards = cardsQuery.data ?? [];
  const walletspaces = walletspacesQuery.data ?? [];

  const invalidatePersonalData = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ["my-finances"] }),
      queryClient.invalidateQueries({ queryKey: ["dashboard-summary"] }),
      queryClient.invalidateQueries({ queryKey: ["walletspace"] }),
    ]);
  };
  const accountMutation = useMutation({
    mutationFn: async ({ id, data }: { id?: string; data: { name: string; type: FinancialAccountType; openingBalance: number; institution?: string } }) =>
      id ? updateAccount(id, data) : createAccount(data),
    onSuccess: invalidatePersonalData,
  });
  const cardMutation = useMutation({
    mutationFn: async ({ id, data }: { id?: string; data: { name: string; lastFourDigits: string; creditLimit: number; closingDay: number; dueDay: number } }) =>
      id ? updateCard(id, data) : createCard(data),
    onSuccess: invalidatePersonalData,
  });
  const transactionMutation = useMutation({
    mutationFn: async ({ transaction, data, scope }: { transaction?: FinancialTransaction; data: SaveFinancialTransaction; scope: TransactionSeriesScope }) =>
      transaction ? updateTransaction(transaction.id, data, scope) : createTransaction(data),
    onSuccess: invalidatePersonalData,
  });
  const deleteAccountMutation = useMutation({ mutationFn: deleteAccount, onSuccess: invalidatePersonalData });
  const deleteCardMutation = useMutation({ mutationFn: deleteCard, onSuccess: invalidatePersonalData });
  const deleteTransactionMutation = useMutation({ mutationFn: ({ id, scope }: { id: string; scope: TransactionSeriesScope }) => deleteTransaction(id, scope), onSuccess: invalidatePersonalData });
  const isBusy = accountMutation.isPending || cardMutation.isPending || transactionMutation.isPending ||
    deleteAccountMutation.isPending || deleteCardMutation.isPending || deleteTransactionMutation.isPending;

  async function performAction(action: () => Promise<unknown>) {
    setActionError(null);
    try {
      await action();
      setEditingAccount(null);
      setEditingCard(null);
      setEditingTransaction(null);
    } catch (error) {
      setActionError(getAuthErrorMessage(error, "Não foi possível concluir a operação."));
    }
  }

  function handleAccountSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    void performAction(() => accountMutation.mutateAsync({
      id: editingAccount?.id,
      data: {
        name: String(form.get("name") ?? "").trim(),
        type: Number(form.get("type")) as FinancialAccountType,
        openingBalance: Number(form.get("openingBalance")),
        institution: String(form.get("institution") ?? "").trim() || undefined,
      },
    }));
  }

  function handleCardSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    void performAction(() => cardMutation.mutateAsync({
      id: editingCard?.id,
      data: {
        name: String(form.get("name") ?? "").trim(),
        lastFourDigits: String(form.get("lastFourDigits") ?? ""),
        creditLimit: Number(form.get("creditLimit")),
        closingDay: Number(form.get("closingDay")),
        dueDay: Number(form.get("dueDay")),
      },
    }));
  }

  function handleTransactionSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const recurrence = Number(form.get("recurrence")) as FinancialTransaction["recurrence"];
    const resourceId = String(form.get("resourceId") ?? "");
    const data: SaveFinancialTransaction = {
      financialAccountId: transactionResource === "account" ? resourceId || null : null,
      creditCardId: transactionResource === "card" ? resourceId || null : null,
      description: String(form.get("description") ?? "").trim(),
      category: String(form.get("category") ?? "").trim() || undefined,
      amount: Number(form.get("amount")),
      type: transactionResource === "card"
        ? 2
        : Number(form.get("type")) as FinancialTransactionType,
      status: Number(form.get("status")) as FinancialTransactionStatus,
      transactionDate: String(form.get("transactionDate") ?? ""),
      dueDate: String(form.get("dueDate") ?? "") || null,
      recurrence,
      occurrences: recurrence === 0 ? undefined : Number(form.get("occurrences")),
      amountIsPerInstallment: recurrence === 2 && form.get("amountIsPerInstallment") === "on",
    };
    const scope = editingTransaction?.seriesId ? askSeriesScope("Salvar alterações") : "single";
    if (!scope) return;
    void performAction(() => transactionMutation.mutateAsync({ transaction: editingTransaction ?? undefined, data, scope }));
  }

  function beginEditTransaction(transaction: FinancialTransaction) {
    setEditingTransaction(transaction);
    setTransactionResource(transaction.creditCardId ? "card" : "account");
    setActiveTab("transactions");
  }

  function handleDeleteTransaction(transaction: FinancialTransaction) {
    const scope = transaction.seriesId ? askSeriesScope("Remover") : window.confirm("Remover este lançamento?") ? "single" : null;
    if (!scope) return;
    void performAction(() => deleteTransactionMutation.mutateAsync({ id: transaction.id, scope }));
  }

  const transactions = transactionsQuery.data ?? [];

  return (
    <div className="mx-auto max-w-5xl">
      <p className="text-xs font-bold uppercase tracking-[0.18em] text-violet-800">Área privada</p>
      <h1 className="mt-2 text-3xl font-semibold tracking-tight text-slate-950 sm:text-4xl">Minhas finanças</h1>
      <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600">Contas, cartões e transações pertencem a você. Escolha individualmente quais transações deseja compartilhar com cada Walletspace.</p>
      {actionError && <div className="mt-5"><ErrorMessage message={actionError} /></div>}

      <nav aria-label="Seções financeiras" className="mt-6 flex gap-1 overflow-x-auto rounded-2xl border border-slate-200 bg-white p-1.5">
        {[
          ["transactions", "Transações"],
          ["accounts", "Contas"],
          ["cards", "Cartões"],
        ].map(([tab, label]) => (
          <button key={tab} type="button" onClick={() => setActiveTab(tab as FinanceTab)} aria-current={activeTab === tab ? "page" : undefined}
            className={`min-h-10 shrink-0 rounded-xl px-4 text-sm font-semibold ${activeTab === tab ? "bg-violet-900 text-white" : "text-slate-600 hover:bg-slate-100"}`}>
            {label}
          </button>
        ))}
      </nav>

      {activeTab === "accounts" && (
        <section className="mt-6 space-y-4">
          <h2 className="text-lg font-semibold text-slate-950">{editingAccount ? "Editar conta" : "Nova conta"}</h2>
          <form key={editingAccount?.id ?? "new-account"} onSubmit={handleAccountSubmit} className="grid gap-3 rounded-2xl border border-slate-200 bg-white p-4 sm:grid-cols-2">
            <label className="text-sm font-semibold">Nome<input name="name" required minLength={1} maxLength={100} defaultValue={editingAccount?.name} className="mt-1.5 min-h-11 w-full rounded-xl border border-slate-300 px-3 font-normal" /></label>
            <label className="text-sm font-semibold">Tipo<select name="type" defaultValue={editingAccount?.type ?? 1} className="mt-1.5 min-h-11 w-full rounded-xl border border-slate-300 bg-white px-3 font-normal">{Object.entries(accountTypeNames).map(([value, name]) => <option key={value} value={value}>{name}</option>)}</select></label>
            <label className="text-sm font-semibold">Instituição<input name="institution" maxLength={100} defaultValue={editingAccount?.institution ?? ""} className="mt-1.5 min-h-11 w-full rounded-xl border border-slate-300 px-3 font-normal" /></label>
            <label className="text-sm font-semibold">Saldo inicial<input name="openingBalance" type="number" step="0.01" required defaultValue={editingAccount?.openingBalance ?? 0} className="mt-1.5 min-h-11 w-full rounded-xl border border-slate-300 px-3 font-normal" /></label>
            <div className="flex gap-2 sm:col-span-2"><button disabled={isBusy} className="min-h-11 rounded-xl bg-violet-900 px-4 text-sm font-semibold text-white disabled:opacity-50">{editingAccount ? "Salvar alterações" : "Criar conta"}</button>{editingAccount && <button type="button" onClick={() => setEditingAccount(null)} className="min-h-11 rounded-xl border border-slate-300 px-4 text-sm font-semibold">Cancelar</button>}</div>
          </form>
          <LoadingError isPending={accountsQuery.isPending} isError={accountsQuery.isError} error={accountsQuery.error} pendingText="Carregando contas..." errorText="Não foi possível carregar as contas." />
          {accountsQuery.isSuccess && (accounts.length ? <div className="divide-y divide-slate-100 overflow-hidden rounded-2xl border border-slate-200 bg-white">{accounts.map((account) => <article key={account.id} className="flex flex-wrap items-center gap-3 p-4"><div className="min-w-0 flex-1"><h3 className="font-semibold text-slate-950">{account.name}</h3><p className="text-xs text-slate-500">{accountTypeNames[account.type]}{account.institution ? ` · ${account.institution}` : ""}</p></div><p className="font-semibold text-slate-950">{currency.format(account.currentBalance)}</p><button type="button" onClick={() => setEditingAccount(account)} className="text-sm font-semibold text-violet-800 hover:underline">Editar</button><button type="button" onClick={() => { if (window.confirm("Remover esta conta? Contas com lançamentos não podem ser removidas.")) void performAction(() => deleteAccountMutation.mutateAsync(account.id)); }} className="text-sm font-semibold text-rose-700 hover:underline">Remover</button></article>)}</div> : <p className="rounded-xl border border-dashed border-slate-300 p-6 text-center text-sm text-slate-600">Você ainda não cadastrou contas.</p>)}
        </section>
      )}

      {activeTab === "cards" && (
        <section className="mt-6 space-y-4">
          <h2 className="text-lg font-semibold text-slate-950">{editingCard ? "Editar cartão" : "Novo cartão"}</h2>
          <form key={editingCard?.id ?? "new-card"} onSubmit={handleCardSubmit} className="grid gap-3 rounded-2xl border border-slate-200 bg-white p-4 sm:grid-cols-2">
            <label className="text-sm font-semibold">Nome<input name="name" required minLength={1} maxLength={100} defaultValue={editingCard?.name} className="mt-1.5 min-h-11 w-full rounded-xl border border-slate-300 px-3 font-normal" /></label>
            <label className="text-sm font-semibold">Quatro últimos dígitos<input name="lastFourDigits" required pattern="[0-9]{4}" maxLength={4} defaultValue={editingCard?.lastFourDigits} className="mt-1.5 min-h-11 w-full rounded-xl border border-slate-300 px-3 font-normal" /></label>
            <label className="text-sm font-semibold">Limite<input name="creditLimit" type="number" min="0" step="0.01" required defaultValue={editingCard?.creditLimit ?? 0} className="mt-1.5 min-h-11 w-full rounded-xl border border-slate-300 px-3 font-normal" /></label>
            <div className="grid grid-cols-2 gap-3"><label className="text-sm font-semibold">Fechamento<input name="closingDay" type="number" min="1" max="31" required defaultValue={editingCard?.closingDay ?? 1} className="mt-1.5 min-h-11 w-full rounded-xl border border-slate-300 px-3 font-normal" /></label><label className="text-sm font-semibold">Vencimento<input name="dueDay" type="number" min="1" max="31" required defaultValue={editingCard?.dueDay ?? 10} className="mt-1.5 min-h-11 w-full rounded-xl border border-slate-300 px-3 font-normal" /></label></div>
            <div className="flex gap-2 sm:col-span-2"><button disabled={isBusy} className="min-h-11 rounded-xl bg-violet-900 px-4 text-sm font-semibold text-white disabled:opacity-50">{editingCard ? "Salvar alterações" : "Criar cartão"}</button>{editingCard && <button type="button" onClick={() => setEditingCard(null)} className="min-h-11 rounded-xl border border-slate-300 px-4 text-sm font-semibold">Cancelar</button>}</div>
          </form>
          <LoadingError isPending={cardsQuery.isPending} isError={cardsQuery.isError} error={cardsQuery.error} pendingText="Carregando cartões..." errorText="Não foi possível carregar os cartões." />
          {cardsQuery.isSuccess && (cards.length ? <div className="divide-y divide-slate-100 overflow-hidden rounded-2xl border border-slate-200 bg-white">{cards.map((card) => <article key={card.id} className="flex flex-wrap items-center gap-3 p-4"><div className="min-w-0 flex-1"><h3 className="font-semibold text-slate-950">{card.name} · •••• {card.lastFourDigits}</h3><p className="text-xs text-slate-500">Limite {currency.format(card.creditLimit)} · fecha dia {card.closingDay} · vence dia {card.dueDay}</p></div><p className="text-right text-sm font-semibold text-slate-950"><span className="block text-xs font-normal text-slate-500">Pendente</span>{currency.format(card.outstanding)}</p><button type="button" onClick={() => setEditingCard(card)} className="text-sm font-semibold text-violet-800 hover:underline">Editar</button><button type="button" onClick={() => { if (window.confirm("Remover este cartão? Cartões com lançamentos não podem ser removidos.")) void performAction(() => deleteCardMutation.mutateAsync(card.id)); }} className="text-sm font-semibold text-rose-700 hover:underline">Remover</button></article>)}</div> : <p className="rounded-xl border border-dashed border-slate-300 p-6 text-center text-sm text-slate-600">Você ainda não cadastrou cartões.</p>)}
        </section>
      )}

      {activeTab === "transactions" && (
        <section className="mt-6 space-y-4">
          <h2 className="text-lg font-semibold text-slate-950">{editingTransaction ? "Editar transação" : "Nova transação"}</h2>
          <form key={editingTransaction?.id ?? "new-transaction"} onSubmit={handleTransactionSubmit} className="grid gap-3 rounded-2xl border border-slate-200 bg-white p-4 sm:grid-cols-2">
            <label className="text-sm font-semibold">Lançar em<select value={transactionResource} onChange={(event) => setTransactionResource(event.target.value as "account" | "card")} disabled={Boolean(editingTransaction)} className="mt-1.5 min-h-11 w-full rounded-xl border border-slate-300 bg-white px-3 font-normal"><option value="account">Conta</option><option value="card">Cartão</option></select></label>
            <label className="text-sm font-semibold">{transactionResource === "account" ? "Conta" : "Cartão"}<select name="resourceId" required defaultValue={transactionResource === "account" ? editingTransaction?.financialAccountId ?? "" : editingTransaction?.creditCardId ?? ""} className="mt-1.5 min-h-11 w-full rounded-xl border border-slate-300 bg-white px-3 font-normal"><option value="" disabled>Selecione</option>{transactionResource === "account" ? accounts.map((account) => <option key={account.id} value={account.id}>{account.name}</option>) : cards.map((card) => <option key={card.id} value={card.id}>{card.name} · •••• {card.lastFourDigits}</option>)}</select></label>
            <label className="text-sm font-semibold">Descrição<input name="description" required maxLength={200} defaultValue={editingTransaction?.description} className="mt-1.5 min-h-11 w-full rounded-xl border border-slate-300 px-3 font-normal" /></label>
            <label className="text-sm font-semibold">Categoria<input name="category" maxLength={100} defaultValue={editingTransaction?.category ?? ""} className="mt-1.5 min-h-11 w-full rounded-xl border border-slate-300 px-3 font-normal" /></label>
            <label className="text-sm font-semibold">Valor<input name="amount" type="number" min="0.0001" step="0.01" required defaultValue={editingTransaction?.amount} className="mt-1.5 min-h-11 w-full rounded-xl border border-slate-300 px-3 font-normal" /></label>
            <label className="text-sm font-semibold">Tipo<select name="type" defaultValue={editingTransaction?.type ?? 2} disabled={transactionResource === "card"} className="mt-1.5 min-h-11 w-full rounded-xl border border-slate-300 bg-white px-3 font-normal"><option value="1">Receita</option><option value="2">Despesa</option></select></label>
            <label className="text-sm font-semibold">Situação<select name="status" defaultValue={editingTransaction?.status ?? 1} className="mt-1.5 min-h-11 w-full rounded-xl border border-slate-300 bg-white px-3 font-normal"><option value="1">Pendente</option><option value="2">Efetivada</option></select></label>
            <label className="text-sm font-semibold">Data<input name="transactionDate" type="date" required defaultValue={editingTransaction?.transactionDate ?? new Date().toISOString().slice(0, 10)} className="mt-1.5 min-h-11 w-full rounded-xl border border-slate-300 px-3 font-normal" /></label>
            <label className="text-sm font-semibold">Vencimento<input name="dueDate" type="date" defaultValue={editingTransaction?.dueDate ?? ""} className="mt-1.5 min-h-11 w-full rounded-xl border border-slate-300 px-3 font-normal" /></label>
            {!editingTransaction && <label className="text-sm font-semibold">Recorrência<select name="recurrence" defaultValue="0" className="mt-1.5 min-h-11 w-full rounded-xl border border-slate-300 bg-white px-3 font-normal"><option value="0">Nenhuma</option><option value="1">Fixa</option><option value="2">Parcelada</option></select></label>}
            {!editingTransaction && <label className="text-sm font-semibold">Ocorrências<input name="occurrences" type="number" min="2" max="120" defaultValue="12" className="mt-1.5 min-h-11 w-full rounded-xl border border-slate-300 px-3 font-normal" /><span className="mt-1 block text-xs font-normal text-slate-500">Usado quando a recorrência não for “Nenhuma”.</span></label>}
            {!editingTransaction && <label className="flex items-center gap-2 text-sm text-slate-700 sm:col-span-2"><input name="amountIsPerInstallment" type="checkbox" className="size-4 accent-violet-900" />O valor informado é o valor de cada parcela</label>}
            <div className="flex flex-wrap gap-2 sm:col-span-2"><button disabled={isBusy || (transactionResource === "account" ? accounts.length === 0 : cards.length === 0)} className="min-h-11 rounded-xl bg-violet-900 px-4 text-sm font-semibold text-white disabled:opacity-50">{editingTransaction ? "Salvar alterações" : "Criar transação"}</button>{editingTransaction && <button type="button" onClick={() => setEditingTransaction(null)} className="min-h-11 rounded-xl border border-slate-300 px-4 text-sm font-semibold">Cancelar</button>}</div>
          </form>
          <LoadingError isPending={transactionsQuery.isPending} isError={transactionsQuery.isError} error={transactionsQuery.error} pendingText="Carregando transações..." errorText="Não foi possível carregar as transações." />
          {transactionsQuery.isSuccess && (transactions.length ? <div className="space-y-3">{transactions.map((transaction) => {
            const account = accounts.find((item) => item.id === transaction.financialAccountId);
            const card = cards.find((item) => item.id === transaction.creditCardId);
            const isSharing = showShareFor === transaction.id;
            return <article key={transaction.id} className="rounded-2xl border border-slate-200 bg-white p-4 sm:p-5">
              <div className="flex flex-wrap items-start gap-3">
                <span aria-hidden="true" className={`flex size-10 items-center justify-center rounded-xl text-lg ${transaction.type === 1 ? "bg-emerald-50 text-emerald-800" : "bg-rose-50 text-rose-800"}`}>{transaction.type === 1 ? "+" : "−"}</span>
                <div className="min-w-0 flex-1"><h3 className="font-semibold text-slate-950">{transaction.description}</h3><p className="mt-1 text-xs text-slate-500">{account?.name ?? card?.name ?? "Conta removida"} · {new Date(`${transaction.transactionDate}T00:00:00`).toLocaleDateString("pt-BR")}{transaction.category ? ` · ${transaction.category}` : ""}</p></div>
                <div className="text-right"><p className={`font-semibold ${transaction.type === 1 ? "text-emerald-800" : "text-slate-950"}`}>{currency.format(transaction.type === 1 ? transaction.amount : -transaction.amount)}</p><p className="mt-1 text-xs text-slate-500">{transaction.status === 2 ? "Efetivada" : "Pendente"}{transaction.installmentNumber && transaction.installmentCount ? ` · ${transaction.installmentNumber}/${transaction.installmentCount}` : ""}</p></div>
              </div>
              <div className="mt-3 flex flex-wrap gap-3 border-t border-slate-100 pt-3">
                <button type="button" onClick={() => beginEditTransaction(transaction)} className="text-sm font-semibold text-violet-800 hover:underline">Editar</button>
                <button type="button" disabled={deleteTransactionMutation.isPending} onClick={() => handleDeleteTransaction(transaction)} className="text-sm font-semibold text-rose-700 hover:underline">Excluir</button>
                <button type="button" aria-expanded={isSharing} onClick={() => setShowShareFor(isSharing ? null : transaction.id)} className="text-sm font-semibold text-slate-700 hover:underline">{isSharing ? "Fechar compartilhamento" : "Compartilhar…"}</button>
              </div>
              {isSharing && <SharePanel transaction={transaction} walletspaces={walletspaces} walletspacesLoaded={walletspacesQuery.isSuccess} />}
            </article>;
          })}</div> : <p className="rounded-xl border border-dashed border-slate-300 p-6 text-center text-sm text-slate-600">Você ainda não cadastrou transações.</p>)}
          {walletspacesQuery.isError && <ErrorMessage message={getAuthErrorMessage(walletspacesQuery.error, "Não foi possível carregar seus Walletspaces para compartilhar.")} />}
        </section>
      )}
    </div>
  );
}
