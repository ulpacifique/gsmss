import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { fetchMyGoals } from "../api/members";
import { fetchGoalStats } from "../api/goals";
import { fetchMyContributionStats } from "../api/members";
import { getMyAccount, requestLoan, payLoan } from "../api/loans";
import { useAuth } from "../state/AuthContext";
import type { CreateLoanRequest, PayLoanRequest } from "../types/api";

const Dashboard = () => {
  const { isAuthed } = useAuth();
  const queryClient = useQueryClient();
  const [showLoanRequest, setShowLoanRequest] = useState(false);
  const [showPayLoan, setShowPayLoan] = useState<number | null>(null);
  const [loanAmount, setLoanAmount] = useState("");
  const [loanPurpose, setLoanPurpose] = useState("");
  const [payAmount, setPayAmount] = useState("");
  const [paymentRef, setPaymentRef] = useState("");

  const { data: myGoals } = useQuery({
    queryKey: ["my-goals"],
    queryFn: fetchMyGoals,
    enabled: isAuthed,
    staleTime: 30000, // Consider data fresh for 30 seconds
    retry: (failureCount, error: any) => {
      if (error?.response?.status === 401) return false;
      return failureCount < 3;
    },
  });

  const { data: goalStats } = useQuery({
    queryKey: ["goal-stats"],
    queryFn: fetchGoalStats,
    enabled: isAuthed,
    staleTime: 30000, // Consider data fresh for 30 seconds
    retry: (failureCount, error: any) => {
      if (error?.response?.status === 401) return false;
      return failureCount < 3;
    },
  });

  const { data: myContributionStats } = useQuery({
    queryKey: ["my-contribution-stats"],
    queryFn: fetchMyContributionStats,
    enabled: isAuthed,
    staleTime: 30000, // Consider data fresh for 30 seconds
    retry: (failureCount, error: any) => {
      if (error?.response?.status === 401) return false;
      return failureCount < 3;
    },
  });

  const { data: myAccount } = useQuery({
    queryKey: ["my-account"],
    queryFn: getMyAccount,
    enabled: isAuthed,
    staleTime: 10000, // Consider data fresh for 10 seconds (more frequently updated)
    retry: (failureCount, error: any) => {
      if (error?.response?.status === 401) return false;
      return failureCount < 3;
    },
  });

  const requestLoanMutation = useMutation({
    mutationFn: (data: CreateLoanRequest) => requestLoan(data),
    onMutate: async (newLoan) => {
      // Cancel outgoing refetches
      await queryClient.cancelQueries({ queryKey: ["my-account"] });
      await queryClient.cancelQueries({ queryKey: ["my-loans"] });
      
      // Snapshot previous values
      const previousAccount = queryClient.getQueryData(["my-account"]);
      
      // Optimistically update UI
      queryClient.setQueryData(["my-account"], (old: any) => {
        if (!old) return old;
        return {
          ...old,
          activeLoansCount: (old.activeLoansCount || 0) + 1,
        };
      });
      
      return { previousAccount };
    },
    onSuccess: () => {
      // Refetch to get accurate data
      queryClient.invalidateQueries({ queryKey: ["my-account"] });
      queryClient.invalidateQueries({ queryKey: ["my-loans"] });
      setShowLoanRequest(false);
      setLoanAmount("");
      setLoanPurpose("");
      alert("Loan request submitted successfully!");
    },
    onError: (error: any, newLoan, context) => {
      // Rollback on error
      if (context?.previousAccount) {
        queryClient.setQueryData(["my-account"], context.previousAccount);
      }
      const errorMessage = error?.response?.data?.message || error?.message || "Failed to request loan";
      console.error("Loan request error:", error);
      alert(errorMessage);
    },
  });

  const payLoanMutation = useMutation({
    mutationFn: ({ loanId, data }: { loanId: number; data: PayLoanRequest }) =>
      payLoan(loanId, data),
    onMutate: async ({ loanId, data }) => {
      // Cancel outgoing refetches
      await queryClient.cancelQueries({ queryKey: ["my-account"] });
      await queryClient.cancelQueries({ queryKey: ["my-loans"] });
      
      // Snapshot previous values
      const previousAccount = queryClient.getQueryData(["my-account"]);
      
      // Optimistically update UI
      queryClient.setQueryData(["my-account"], (old: any) => {
        if (!old) return old;
        return {
          ...old,
          outstandingLoanAmount: Math.max(0, (old.outstandingLoanAmount || 0) - data.amount),
        };
      });
      
      return { previousAccount };
    },
    onSuccess: () => {
      // Refetch to get accurate data
      queryClient.invalidateQueries({ queryKey: ["my-account"] });
      queryClient.invalidateQueries({ queryKey: ["my-loans"] });
      setShowPayLoan(null);
      setPayAmount("");
      setPaymentRef("");
      alert("Payment processed successfully!");
    },
    onError: (error: any, variables, context) => {
      // Rollback on error
      if (context?.previousAccount) {
        queryClient.setQueryData(["my-account"], context.previousAccount);
      }
      alert(error?.response?.data?.message || "Failed to process payment");
    },
  });

  const handleRequestLoan = (e: React.FormEvent) => {
    e.preventDefault();
    if (!loanAmount || parseFloat(loanAmount) <= 0) {
      alert("Please enter a valid loan amount");
      return;
    }
    requestLoanMutation.mutate({
      amount: parseFloat(loanAmount),
      purpose: loanPurpose || null,
    });
  };

  const handlePayLoan = (e: React.FormEvent, loanId: number) => {
    e.preventDefault();
    if (!payAmount || parseFloat(payAmount) <= 0) {
      alert("Please enter a valid payment amount");
      return;
    }
    if (!paymentRef.trim()) {
      alert("Please enter a payment reference");
      return;
    }
    payLoanMutation.mutate({
      loanId,
      data: {
        amount: parseFloat(payAmount),
        paymentReference: paymentRef,
      },
    });
  };

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold text-slate-900">Dashboard</h1>
        <p className="mt-2 text-slate-600">
          Overview of your financial goals, contributions, and account status
        </p>
      </div>

      <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
        <div className="stat-card">
          <div className="flex items-center justify-between">
            <div className="min-w-0 flex-1">
              <p className="text-sm font-medium text-slate-600">Account Balance</p>
              <p className="mt-2 text-2xl sm:text-3xl font-bold text-slate-900 truncate">
                {(myAccount?.accountBalance ?? 0).toLocaleString(undefined, {
                  style: "currency",
                  currency: "USD",
                  minimumFractionDigits: 0,
                  maximumFractionDigits: 0,
                })}
              </p>
            </div>
            <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-gradient-to-br from-emerald-500 to-teal-600 text-white shadow-md">
              <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
          </div>
        </div>
        <div className="stat-card">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-slate-600">Active Goals</p>
              <p className="mt-2 text-3xl font-bold text-slate-900">
                {goalStats?.activeGoals ?? 0}
              </p>
            </div>
            <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-gradient-to-br from-blue-500 to-indigo-600 text-white shadow-md">
              <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
              </svg>
            </div>
          </div>
        </div>
        <div className="stat-card">
          <div className="flex items-center justify-between">
            <div className="min-w-0 flex-1">
              <p className="text-sm font-medium text-slate-600">Total Contributed</p>
              <p className="mt-2 text-2xl sm:text-3xl font-bold text-slate-900 truncate">
                {(myContributionStats?.totalAmount ?? 0).toLocaleString(undefined, {
                  style: "currency",
                  currency: "USD",
                  minimumFractionDigits: 0,
                  maximumFractionDigits: 0,
                })}
              </p>
            </div>
            <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-gradient-to-br from-purple-500 to-pink-600 text-white shadow-md">
              <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 9V7a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2m2 4h10a2 2 0 002-2v-6a2 2 0 00-2-2H9a2 2 0 00-2 2v6a2 2 0 002 2zm7-5a2 2 0 11-4 0 2 2 0 014 0z" />
              </svg>
            </div>
          </div>
        </div>
        <div className="stat-card">
          <div className="flex items-center justify-between">
            <div className="min-w-0 flex-1">
              <p className="text-sm font-medium text-slate-600">Outstanding Loans</p>
              <p className="mt-2 text-2xl sm:text-3xl font-bold text-red-600 truncate">
                {(myAccount?.outstandingLoanAmount ?? 0).toLocaleString(undefined, {
                  style: "currency",
                  currency: "USD",
                  minimumFractionDigits: 0,
                  maximumFractionDigits: 0,
                })}
              </p>
            </div>
            <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-gradient-to-br from-red-500 to-rose-600 text-white shadow-md">
              <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
          </div>
        </div>
      </div>

      {/* Loan Request Section */}
      <div className="card">
        <div className="mb-6 flex items-center justify-between">
          <div>
            <h2 className="text-xl font-bold text-slate-900">Loan Management</h2>
            <p className="mt-1 text-sm text-slate-600">Request or manage your loans</p>
          </div>
          <button
            onClick={() => setShowLoanRequest(!showLoanRequest)}
            className={showLoanRequest ? "btn-secondary" : "btn-primary"}
          >
            {showLoanRequest ? "Cancel" : "Request Loan"}
          </button>
        </div>

        {showLoanRequest && (
          <form onSubmit={handleRequestLoan} className="mb-4 rounded-md border border-slate-200 p-4">
            <div className="mb-4">
              <label className="block text-sm font-medium text-slate-700">
                Loan Amount
              </label>
              <input
                type="number"
                step="0.01"
                min="0.01"
                value={loanAmount}
                onChange={(e) => setLoanAmount(e.target.value)}
                className="mt-1 w-full rounded-md border border-slate-200 px-3 py-2"
                required
              />
              {myAccount && (
                <p className="mt-1 text-xs text-slate-500">
                  Max loan:{" "}
                  {myAccount.accountBalance > 0
                    ? (myAccount.accountBalance * 0.125).toLocaleString(undefined, {
                        style: "currency",
                        currency: "USD",
                      })
                    : "$0.00"}
                </p>
              )}
            </div>
            <div className="mb-4">
              <label className="block text-sm font-medium text-slate-700">
                Purpose (Optional)
              </label>
              <textarea
                value={loanPurpose}
                onChange={(e) => setLoanPurpose(e.target.value)}
                className="mt-1 w-full rounded-md border border-slate-200 px-3 py-2"
                rows={2}
              />
            </div>
            <button
              type="submit"
              disabled={requestLoanMutation.isPending}
              className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
            >
              {requestLoanMutation.isPending ? "Submitting..." : "Submit Request"}
            </button>
          </form>
        )}

        {/* Active Loans */}
        {myAccount && myAccount.activeLoans.length > 0 && (
          <div className="mt-4">
            <h3 className="mb-2 text-sm font-semibold">Active Loans</h3>
            <div className="space-y-2">
              {myAccount.activeLoans.map((loan) => (
                <div
                  key={loan.loanId}
                  className="rounded-md border border-slate-200 p-3"
                >
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="font-medium">
                        {loan.principalAmount.toLocaleString(undefined, {
                          style: "currency",
                          currency: "USD",
                        })}{" "}
                        @ {loan.interestRate}% interest
                      </p>
                      <p className="text-sm text-slate-500">
                        Remaining:{" "}
                        {loan.remainingAmount.toLocaleString(undefined, {
                          style: "currency",
                          currency: "USD",
                        })}{" "}
                        | Due: {new Date(loan.dueDate).toLocaleDateString()}
                        {loan.isOverdue && (
                          <span className="ml-2 text-red-600">(Overdue)</span>
                        )}
                      </p>
                    </div>
                    <button
                      onClick={() =>
                        setShowPayLoan(showPayLoan === loan.loanId ? null : loan.loanId)
                      }
                      className="rounded-md bg-green-600 px-3 py-1 text-sm font-medium text-white hover:bg-green-700"
                    >
                      Pay
                    </button>
                  </div>
                  {showPayLoan === loan.loanId && (
                    <form
                      onSubmit={(e) => handlePayLoan(e, loan.loanId)}
                      className="mt-3 rounded-md border border-slate-200 bg-slate-50 p-3"
                    >
                      <div className="mb-2">
                        <label className="block text-xs font-medium text-slate-700">
                          Payment Amount (Max:{" "}
                          {loan.remainingAmount.toLocaleString(undefined, {
                            style: "currency",
                            currency: "USD",
                          })}
                          )
                        </label>
                        <input
                          type="number"
                          step="0.01"
                          min="0.01"
                          max={loan.remainingAmount}
                          value={payAmount}
                          onChange={(e) => setPayAmount(e.target.value)}
                          className="mt-1 w-full rounded-md border border-slate-200 px-2 py-1 text-sm"
                          required
                        />
                      </div>
                      <div className="mb-2">
                        <label className="block text-xs font-medium text-slate-700">
                          Payment Reference
                        </label>
                        <input
                          type="text"
                          value={paymentRef}
                          onChange={(e) => setPaymentRef(e.target.value)}
                          className="mt-1 w-full rounded-md border border-slate-200 px-2 py-1 text-sm"
                          required
                        />
                      </div>
                      <button
                        type="submit"
                        disabled={payLoanMutation.isPending}
                        className="rounded-md bg-green-600 px-3 py-1 text-xs font-medium text-white hover:bg-green-700 disabled:opacity-50"
                      >
                        {payLoanMutation.isPending ? "Processing..." : "Submit Payment"}
                      </button>
                    </form>
                  )}
                </div>
              ))}
            </div>
          </div>
        )}
      </div>

      <div className="card">
        <div className="mb-4 flex items-center justify-between">
          <h2 className="text-lg font-semibold">Your goals</h2>
        </div>
        <div className="overflow-x-auto">
          <table className="min-w-full text-sm">
            <thead className="text-left text-slate-500">
              <tr>
                <th className="px-3 py-2">Goal</th>
                <th className="px-3 py-2">Personal target</th>
                <th className="px-3 py-2">Progress</th>
                <th className="px-3 py-2">Joined</th>
              </tr>
            </thead>
            <tbody>
              {myGoals?.map((g) => (
                <tr key={g.memberGoalId} className="border-t">
                  <td className="px-3 py-2 font-medium">{g.goalName}</td>
                  <td className="px-3 py-2">
                    {g.personalTarget.toLocaleString()}
                  </td>
                  <td className="px-3 py-2">
                    {g.personalProgressPercentage.toFixed(1)}%
                  </td>
                  <td className="px-3 py-2">
                    {new Date(g.joinedAt).toLocaleDateString()}
                  </td>
                </tr>
              ))}
              {!myGoals?.length && (
                <tr>
                  <td className="px-3 py-4 text-slate-500" colSpan={4}>
                    You have not joined any goals yet.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;

