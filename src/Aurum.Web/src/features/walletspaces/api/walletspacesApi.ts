import { api } from "../../../services/api";
import type {
  CreditCard,
  DashboardSummary,
  FinancialAccount,
  FinancialSummary,
  FinancialTransaction,
  SaveCreditCard,
  SaveFinancialAccount,
  SaveFinancialTransaction,
  SharedFinancialTransaction,
  TransactionShare,
  Walletspace,
  WalletspaceMember,
  WalletspaceRole,
  TransactionSeriesScope,
} from "../types/walletspace.types";

export async function getWalletspaces() {
  const response = await api.get<Walletspace[]>("/walletspaces");
  return response.data;
}

export async function getWalletspace(walletspaceId: string) {
  const response = await api.get<Walletspace>(`/walletspaces/${walletspaceId}`);
  return response.data;
}

export async function createWalletspace(name: string) {
  const response = await api.post<Walletspace>("/walletspaces", { name });
  return response.data;
}

export async function renameWalletspace(walletspaceId: string, name: string) {
  await api.put(`/walletspaces/${walletspaceId}`, { name });
}

export async function deleteWalletspace(walletspaceId: string) {
  await api.delete(`/walletspaces/${walletspaceId}`);
}

export async function leaveWalletspace(walletspaceId: string) {
  await api.post(`/walletspaces/${walletspaceId}/leave`);
}

export async function transferOwnership(walletspaceId: string, newOwnerId: string) {
  await api.post(`/walletspaces/${walletspaceId}/members/transfer-ownership`, { newOwnerId });
}

export async function getAccounts() {
  const response = await api.get<FinancialAccount[]>("/me/accounts");
  return response.data;
}

export async function createAccount(account: SaveFinancialAccount) {
  const response = await api.post<FinancialAccount>("/me/accounts", account);
  return response.data;
}

export async function updateAccount(accountId: string, account: SaveFinancialAccount) {
  await api.put(`/me/accounts/${accountId}`, account);
}

export async function deleteAccount(accountId: string) {
  await api.delete(`/me/accounts/${accountId}`);
}

export async function getCards() {
  const response = await api.get<CreditCard[]>("/me/cards");
  return response.data;
}

export async function createCard(card: SaveCreditCard) {
  const response = await api.post<CreditCard>("/me/cards", card);
  return response.data;
}

export async function updateCard(cardId: string, card: SaveCreditCard) {
  await api.put(`/me/cards/${cardId}`, card);
}

export async function deleteCard(cardId: string) {
  await api.delete(`/me/cards/${cardId}`);
}

export async function getTransactions(from?: string, to?: string) {
  const response = await api.get<FinancialTransaction[]>("/me/transactions", {
    params: { from, to },
  });
  return response.data;
}

export async function getSharedTransactions(
  walletspaceId: string,
  from?: string,
  to?: string,
) {
  const response = await api.get<SharedFinancialTransaction[]>(
    `/walletspaces/${walletspaceId}/transactions`,
    { params: { from, to } },
  );
  return response.data;
}

export async function createTransaction(transaction: SaveFinancialTransaction) {
  const response = await api.post<FinancialTransaction>("/me/transactions", transaction);
  return response.data;
}

export async function updateTransaction(
  transactionId: string,
  transaction: SaveFinancialTransaction,
  scope: TransactionSeriesScope = "single",
) {
  await api.put(`/me/transactions/${transactionId}`, transaction, { params: { scope } });
}

export async function deleteTransaction(
  transactionId: string,
  scope: TransactionSeriesScope = "single",
) {
  await api.delete(`/me/transactions/${transactionId}`, { params: { scope } });
}

export async function getTransactionShares(transactionId: string) {
  const response = await api.get<TransactionShare[]>(`/me/transactions/${transactionId}/shares`);
  return response.data;
}

export async function shareTransaction(transactionId: string, walletspaceId: string) {
  const response = await api.post<TransactionShare>(
    `/me/transactions/${transactionId}/shares`,
    { walletspaceId },
  );
  return response.data;
}

export async function removeTransactionShare(transactionId: string, walletspaceId: string) {
  await api.delete(`/me/transactions/${transactionId}/shares/${walletspaceId}`);
}

export async function getMembers(walletspaceId: string) {
  const response = await api.get<WalletspaceMember[]>(
    `/walletspaces/${walletspaceId}/members`,
  );
  return response.data;
}

export async function addMember(
  walletspaceId: string,
  email: string,
  role: WalletspaceRole,
) {
  const response = await api.post<WalletspaceMember>(
    `/walletspaces/${walletspaceId}/members`,
    { email, role },
  );
  return response.data;
}

export async function updateMemberRole(
  walletspaceId: string,
  userId: string,
  role: WalletspaceRole,
) {
  await api.put(`/walletspaces/${walletspaceId}/members/${userId}`, { role });
}

export async function removeMember(walletspaceId: string, userId: string) {
  await api.delete(`/walletspaces/${walletspaceId}/members/${userId}`);
}

export async function getFinancialSummary(
  walletspaceId: string,
  from: string,
  to: string,
) {
  const response = await api.get<FinancialSummary>(
    `/walletspaces/${walletspaceId}/reports/summary`,
    { params: { from, to } },
  );
  return response.data;
}

export async function getDashboardSummary() {
  const response = await api.get<DashboardSummary>("/dashboard/summary");
  return response.data;
}
