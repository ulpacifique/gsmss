import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createContribution,
  fetchMyContributions,
} from "../api/contributions";
import { useState } from "react";
import { fetchGoals } from "../api/goals";
import { useAuth } from "../state/AuthContext";

const Contributions = () => {
  const qc = useQueryClient();
  const { isAuthed } = useAuth();
  const { data: contributions } = useQuery({
    queryKey: ["my-contributions"],
    queryFn: fetchMyContributions,
    enabled: isAuthed,
    retry: (failureCount, error: any) => {
      if (error?.response?.status === 401) return false;
      return failureCount < 3;
    },
  });

  const { data: goals } = useQuery({
    queryKey: ["goals"],
    queryFn: fetchGoals,
    enabled: isAuthed,
    retry: (failureCount, error: any) => {
      if (error?.response?.status === 401) return false;
      return failureCount < 3;
    },
  });

  const [form, setForm] = useState({
    goalId: "",
    amount: "",
    paymentReference: "",
  });

  const mutation = useMutation({
    mutationFn: createContribution,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["my-contributions"] });
      setForm({ goalId: "", amount: "", paymentReference: "" });
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.goalId || !form.amount) return;
    mutation.mutate({
      goalId: Number(form.goalId),
      amount: Number(form.amount),
      paymentReference: form.paymentReference?.trim() || undefined, // Optional - backend will use member name
    });
  };

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold text-slate-900">Contributions</h1>
        <p className="mt-2 text-slate-600">
          Submit a contribution to your savings goals and view your contribution history
        </p>
      </div>

      <form onSubmit={handleSubmit} className="card space-y-5">
        <div className="grid gap-5 sm:grid-cols-3">
          <div>
            <label className="mb-1.5 block text-sm font-semibold text-slate-700">Goal</label>
            <select
              required
              value={form.goalId}
              onChange={(e) => setForm((f) => ({ ...f, goalId: e.target.value }))}
              className="input"
            >
              <option value="" disabled>Select goal</option>
              {goals?.filter(g => g.status === "Active").map((g) => (
                <option key={g.goalId} value={g.goalId}>
                  {g.goalName}
                </option>
              ))}
            </select>
          </div>
          <div>
            <label className="mb-1.5 block text-sm font-semibold text-slate-700">
              Amount
            </label>
            <input
              type="number"
              required
              min="0"
              step="0.01"
              value={form.amount}
              onChange={(e) =>
                setForm((f) => ({ ...f, amount: e.target.value }))
              }
              className="input"
              placeholder="0.00"
            />
          </div>
          <div>
            <label className="mb-1.5 block text-sm font-semibold text-slate-700">
              Payment Reference <span className="text-slate-400">(optional)</span>
            </label>
            <input
              value={form.paymentReference}
              onChange={(e) =>
                setForm((f) => ({ ...f, paymentReference: e.target.value }))
              }
              className="input"
              placeholder="Auto-filled with your name if left empty"
            />
          </div>
        </div>
        <button
          type="submit"
          className="btn-primary w-full"
          disabled={mutation.isPending}
        >
          {mutation.isPending ? (
            <span className="flex items-center justify-center gap-2">
              <svg className="h-4 w-4 animate-spin" viewBox="0 0 24 24">
                <circle
                  className="opacity-25"
                  cx="12"
                  cy="12"
                  r="10"
                  stroke="currentColor"
                  strokeWidth="4"
                  fill="none"
                />
                <path
                  className="opacity-75"
                  fill="currentColor"
                  d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                />
              </svg>
              Submitting...
            </span>
          ) : (
            "Create Contribution"
          )}
        </button>
        {mutation.isError && (
          <div className="rounded-lg bg-red-50 px-4 py-3 text-sm text-red-700 ring-1 ring-red-200">
            <strong>Error:</strong> {(mutation.error as any)?.response?.data?.message ?? (mutation.error as any)?.message ?? "An error occurred while creating contribution"}
          </div>
        )}
      </form>

      <div className="card overflow-x-auto">
        <h2 className="mb-4 text-lg font-bold text-slate-900">Contribution History</h2>
        <div className="overflow-hidden rounded-lg border border-slate-200">
          <table className="min-w-full divide-y divide-slate-200">
            <thead className="bg-slate-50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-700">Goal</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-700">Amount</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-700">Status</th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-700">Submitted</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-200 bg-white">
              {contributions?.map((c) => (
                <tr key={c.contributionId} className="hover:bg-slate-50 transition-colors">
                  <td className="whitespace-nowrap px-4 py-3 font-medium text-slate-900">{c.goalName}</td>
                  <td className="whitespace-nowrap px-4 py-3 text-slate-900">
                    {c.amount.toLocaleString(undefined, {
                      style: "currency",
                      currency: "USD",
                    })}
                  </td>
                  <td className="whitespace-nowrap px-4 py-3">
                    <span className={`badge ${
                      c.status === "Approved" ? "badge-success" :
                      c.status === "Pending" ? "badge-warning" :
                      "badge-danger"
                    }`}>
                      {c.status}
                    </span>
                  </td>
                  <td className="whitespace-nowrap px-4 py-3 text-sm text-slate-600">
                    {new Date(c.submittedAt).toLocaleDateString()} {new Date(c.submittedAt).toLocaleTimeString()}
                  </td>
                </tr>
              ))}
              {!contributions?.length && (
                <tr>
                  <td className="px-4 py-8 text-center text-slate-500" colSpan={4}>
                    No contributions yet. Create your first contribution above.
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

export default Contributions;

