export type WalletspaceRole = 1 | 2 | 3 | 4 | 5;
export type FinancialAccountType = 1 | 2 | 3 | 4 | 5;
export type FinancialTransactionType = 1 | 2;
export type FinancialTransactionStatus = 1 | 2;
export type FinancialTransactionRecurrence = 0 | 1 | 2;
export type TransactionSeriesScope = "single" | "future" | "all";

export interface Walletspace {
  id: string;
  name: string;
  myRole: WalletspaceRole;
  memberCount: number;
  createdAt: string;
}

export interface FinancialAccount {
  id: string;
  walletspaceId: string;
  name: string;
  type: FinancialAccountType;
  institution: string | null;
  openingBalance: number;
  currentBalance: number;
}

export interface SaveFinancialAccount {
  name: string;
  type: FinancialAccountType;
  institution?: string;
  openingBalance: number;
}

export interface CreditCard {
  id: string;
  walletspaceId: string;
  name: string;
  lastFourDigits: string;
  creditLimit: number;
  closingDay: number;
  dueDay: number;
  outstanding: number;
}

export interface SaveCreditCard {
  name: string;
  lastFourDigits: string;
  creditLimit: number;
  closingDay: number;
  dueDay: number;
}

export interface FinancialTransaction {
  id: string;
  walletspaceId: string;
  financialAccountId: string | null;
  creditCardId: string | null;
  description: string;
  category: string | null;
  amount: number;
  type: FinancialTransactionType;
  status: FinancialTransactionStatus;
  transactionDate: string;
  dueDate: string | null;
  recurrence: FinancialTransactionRecurrence;
  seriesId: string | null;
  installmentNumber: number | null;
  installmentCount: number | null;
}

export interface SaveFinancialTransaction {
  financialAccountId: string | null;
  creditCardId: string | null;
  description: string;
  category?: string;
  amount: number;
  type: FinancialTransactionType;
  status: FinancialTransactionStatus;
  transactionDate: string;
  dueDate?: string | null;
  recurrence?: FinancialTransactionRecurrence;
  occurrences?: number;
  amountIsPerInstallment?: boolean;
}

export interface WalletspaceMember {
  userId: string;
  email: string;
  fullName: string | null;
  role: WalletspaceRole;
  joinedAt: string;
}

export interface FinancialSummary {
  walletspaceId: string;
  from: string;
  to: string;
  income: number;
  expense: number;
  net: number;
  pendingCount: number;
  categories: { category: string; type: FinancialTransactionType; total: number }[];
}

export interface DashboardSummary {
  walletspaceCount: number;
  accountBalance: number;
  incomeThisMonth: number;
  expenseThisMonth: number;
  walletspaces: Walletspace[];
}
