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

export async function getAccounts(walletspaceId: string) {
  const response = await api.get<FinancialAccount[]>(
    `/walletspaces/${walletspaceId}/accounts`,
  );
  return response.data;
}

export async function createAccount(
  walletspaceId: string,
  account: SaveFinancialAccount,
) {
  const response = await api.post<FinancialAccount>(
    `/walletspaces/${walletspaceId}/accounts`,
    account,
  );
  return response.data;
}

export async function updateAccount(
  walletspaceId: string,
  accountId: string,
  account: SaveFinancialAccount,
) {
  await api.put(
    `/walletspaces/${walletspaceId}/accounts/${accountId}`,
    account,
  );
}

export async function deleteAccount(walletspaceId: string, accountId: string) {
  await api.delete(`/walletspaces/${walletspaceId}/accounts/${accountId}`);
}

export async function getCards(walletspaceId: string) {
  const response = await api.get<CreditCard[]>(
    `/walletspaces/${walletspaceId}/cards`,
  );
  return response.data;
}

export async function createCard(walletspaceId: string, card: SaveCreditCard) {
  const response = await api.post<CreditCard>(
    `/walletspaces/${walletspaceId}/cards`,
    card,
  );
  return response.data;
}

export async function updateCard(
  walletspaceId: string,
  cardId: string,
  card: SaveCreditCard,
) {
  await api.put(`/walletspaces/${walletspaceId}/cards/${cardId}`, card);
}

export async function deleteCard(walletspaceId: string, cardId: string) {
  await api.delete(`/walletspaces/${walletspaceId}/cards/${cardId}`);
}

export async function getTransactions(
  walletspaceId: string,
  from?: string,
  to?: string,
) {
  const response = await api.get<FinancialTransaction[]>(
    `/walletspaces/${walletspaceId}/transactions`,
    { params: { from, to } },
  );
  return response.data;
}

export async function createTransaction(
  walletspaceId: string,
  transaction: SaveFinancialTransaction,
) {
  const response = await api.post<FinancialTransaction>(
    `/walletspaces/${walletspaceId}/transactions`,
    transaction,
  );
  return response.data;
}

export async function updateTransaction(
  walletspaceId: string,
  transactionId: string,
  transaction: SaveFinancialTransaction,
  scope: TransactionSeriesScope = "single",
) {
  await api.put(
    `/walletspaces/${walletspaceId}/transactions/${transactionId}`,
    transaction,
    { params: { scope } },
  );
}

export async function deleteTransaction(
  walletspaceId: string,
  transactionId: string,
  scope: TransactionSeriesScope = "single",
) {
  await api.delete(
    `/walletspaces/${walletspaceId}/transactions/${transactionId}`,
    { params: { scope } },
  );
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
