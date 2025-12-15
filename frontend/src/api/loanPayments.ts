import api from "./client";

export interface LoanPayment {
  paymentId: number;
  loanId: number;
  userId: number;
  userName: string;
  amount: number;
  paymentReference: string;
  paymentDate: string;
  notes?: string;
  createdAt: string;
}

export const fetchLoanPayments = async (loanId: number): Promise<LoanPayment[]> => {
  const { data } = await api.get<LoanPayment[]>(`/api/loans/payments/loan/${loanId}`);
  return data;
};

export const fetchMyLoanPayments = async (): Promise<LoanPayment[]> => {
  const { data } = await api.get<LoanPayment[]>("/api/loans/payments/my-payments");
  return data;
};

export const fetchLoanPaymentById = async (paymentId: number): Promise<LoanPayment> => {
  const { data } = await api.get<LoanPayment>(`/api/loans/payments/${paymentId}`);
  return data;
};


