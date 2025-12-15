import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import api from "../../api/client";
import { useState } from "react";

interface Loan {
  loanId: number;
  userId: number;
  userName: string;
  principalAmount: number;
  interestRate: number;
  totalAmount: number;
  remainingAmount: number;
  paidAmount: number;
  requestedDate: string;
  dueDate: string;
  status: string;
  approvedBy?: number;
  approvedByName?: string;
  approvedAt?: string;
  rejectionReason?: string;
  isOverdue: boolean;
  daysUntilDue: number;
  createdAt: string;
}

const AdminLoans = () => {
  const queryClient = useQueryClient();
  const [filter, setFilter] = useState<"all" | "pending" | "approved" | "rejected">("pending");
  const [rejectionReason, setRejectionReason] = useState<{ [key: number]: string }>({});

  const { data: allLoans, isLoading } = useQuery({
    queryKey: ["admin-loans"],
    queryFn: async () => {
      const { data } = await api.get<Loan[]>("/api/admin/loans");
      return data;
    },
  });

  const { data: pendingLoans } = useQuery({
    queryKey: ["admin-pending-loans"],
    queryFn: async () => {
      const { data } = await api.get<Loan[]>("/api/admin/loans/pending");
      return data;
    },
  });

  const approveMutation = useMutation({
    mutationFn: async (loanId: number) => {
      const { data } = await api.put(`/api/admin/loans/${loanId}/approve`, {});
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-loans"] });
      queryClient.invalidateQueries({ queryKey: ["admin-pending-loans"] });
    },
  });

  const rejectMutation = useMutation({
    mutationFn: async ({ loanId, reason }: { loanId: number; reason?: string }) => {
      const { data } = await api.put(`/api/admin/loans/${loanId}/reject`, {
        rejectionReason: reason || "Loan request rejected by administrator",
      });
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-loans"] });
      queryClient.invalidateQueries({ queryKey: ["admin-pending-loans"] });
      setRejectionReason({});
    },
  });

  const filteredLoans = allLoans?.filter((loan) => {
    if (filter === "pending") return loan.status === "Pending";
    if (filter === "approved") return loan.status === "Approved";
    if (filter === "rejected") return loan.status === "Rejected";
    return true;
  }) || [];

  const loansToShow = filter === "pending" ? pendingLoans || [] : filteredLoans;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-slate-900">Loan Management</h1>
          <p className="text-sm text-slate-600 mt-1">
            Review and manage loan requests from members
          </p>
        </div>
      </div>

      {/* Filter Tabs */}
      <div className="flex gap-2 border-b border-slate-200">
        <button
          onClick={() => setFilter("pending")}
          className={`px-4 py-2 text-sm font-medium transition-colors border-b-2 ${
            filter === "pending"
              ? "border-blue-600 text-blue-600"
              : "border-transparent text-slate-600 hover:text-slate-900"
          }`}
        >
          Pending ({pendingLoans?.length || 0})
        </button>
        <button
          onClick={() => setFilter("approved")}
          className={`px-4 py-2 text-sm font-medium transition-colors border-b-2 ${
            filter === "approved"
              ? "border-blue-600 text-blue-600"
              : "border-transparent text-slate-600 hover:text-slate-900"
          }`}
        >
          Approved
        </button>
        <button
          onClick={() => setFilter("rejected")}
          className={`px-4 py-2 text-sm font-medium transition-colors border-b-2 ${
            filter === "rejected"
              ? "border-blue-600 text-blue-600"
              : "border-transparent text-slate-600 hover:text-slate-900"
          }`}
        >
          Rejected
        </button>
        <button
          onClick={() => setFilter("all")}
          className={`px-4 py-2 text-sm font-medium transition-colors border-b-2 ${
            filter === "all"
              ? "border-blue-600 text-blue-600"
              : "border-transparent text-slate-600 hover:text-slate-900"
          }`}
        >
          All Loans
        </button>
      </div>

      {/* Loans Table */}
      <div className="card overflow-x-auto">
        {isLoading ? (
          <div className="p-8 text-center text-slate-500">Loading loans...</div>
        ) : loansToShow.length === 0 ? (
          <div className="p-8 text-center text-slate-500">
            No {filter === "all" ? "" : filter} loans found.
          </div>
        ) : (
          <table className="min-w-full text-sm">
            <thead className="bg-slate-50 text-left">
              <tr>
                <th className="px-4 py-3 font-semibold text-slate-700">Member</th>
                <th className="px-4 py-3 font-semibold text-slate-700">Amount</th>
                <th className="px-4 py-3 font-semibold text-slate-700">Interest Rate</th>
                <th className="px-4 py-3 font-semibold text-slate-700">Total Amount</th>
                <th className="px-4 py-3 font-semibold text-slate-700">Requested Date</th>
                <th className="px-4 py-3 font-semibold text-slate-700">Due Date</th>
                <th className="px-4 py-3 font-semibold text-slate-700">Status</th>
                {filter === "pending" && (
                  <th className="px-4 py-3 font-semibold text-slate-700">Actions</th>
                )}
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {loansToShow.map((loan) => (
                <tr key={loan.loanId} className="hover:bg-slate-50 transition-colors">
                  <td className="px-4 py-3 font-medium text-slate-900">{loan.userName}</td>
                  <td className="px-4 py-3 text-slate-700">
                    {loan.principalAmount.toLocaleString(undefined, {
                      style: "currency",
                      currency: "USD",
                    })}
                  </td>
                  <td className="px-4 py-3 text-slate-700">{loan.interestRate}%</td>
                  <td className="px-4 py-3 text-slate-700">
                    {loan.totalAmount.toLocaleString(undefined, {
                      style: "currency",
                      currency: "USD",
                    })}
                  </td>
                  <td className="px-4 py-3 text-slate-600">
                    {new Date(loan.requestedDate).toLocaleDateString()}
                  </td>
                  <td className="px-4 py-3 text-slate-600">
                    {new Date(loan.dueDate).toLocaleDateString()}
                  </td>
                  <td className="px-4 py-3">
                    <span
                      className={`badge ${
                        loan.status === "Approved"
                          ? "badge-success"
                          : loan.status === "Rejected"
                          ? "badge-danger"
                          : loan.status === "Pending"
                          ? "badge-warning"
                          : "badge-info"
                      }`}
                    >
                      {loan.status}
                    </span>
                    {loan.isOverdue && loan.status === "Approved" && (
                      <span className="ml-2 text-xs text-red-600 font-semibold">(Overdue)</span>
                    )}
                  </td>
                  {filter === "pending" && (
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => approveMutation.mutate(loan.loanId)}
                          disabled={approveMutation.isPending}
                          className="btn-success text-xs px-3 py-1.5"
                        >
                          Approve
                        </button>
                        <div className="relative">
                          <button
                            onClick={() => {
                              const reason = prompt("Enter rejection reason (optional):");
                              if (reason !== null) {
                                rejectMutation.mutate({
                                  loanId: loan.loanId,
                                  reason: reason || undefined,
                                });
                              }
                            }}
                            disabled={rejectMutation.isPending}
                            className="btn-danger text-xs px-3 py-1.5"
                          >
                            Reject
                          </button>
                        </div>
                      </div>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {approveMutation.isError && (
        <div className="rounded-md bg-rose-50 px-4 py-3 text-sm text-rose-700">
          Failed to approve loan: {(approveMutation.error as any)?.response?.data?.message || "An error occurred"}
        </div>
      )}

      {rejectMutation.isError && (
        <div className="rounded-md bg-rose-50 px-4 py-3 text-sm text-rose-700">
          Failed to reject loan: {(rejectMutation.error as any)?.response?.data?.message || "An error occurred"}
        </div>
      )}
    </div>
  );
};

export default AdminLoans;


