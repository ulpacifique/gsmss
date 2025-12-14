import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { fetchDashboardStats, createGoalAdmin } from "../../api/admin";
import { useState } from "react";

const AdminDashboard = () => {
  const qc = useQueryClient();
  const { data } = useQuery({
    queryKey: ["admin-dashboard"],
    queryFn: fetchDashboardStats,
  });

  const [showGoalForm, setShowGoalForm] = useState(false);
  const [form, setForm] = useState({
    goalName: "",
    description: "",
    targetAmount: "",
    startDate: "",
    endDate: "",
  });

  const goalMutation = useMutation({
    mutationFn: createGoalAdmin,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["admin-dashboard"] });
      qc.invalidateQueries({ queryKey: ["admin-goals"] });
      setForm({
        goalName: "",
        description: "",
        targetAmount: "",
        startDate: "",
        endDate: "",
      });
      setShowGoalForm(false);
    },
  });

  const handleCreateGoal = (e: React.FormEvent) => {
    e.preventDefault();
    goalMutation.mutate({
      goalName: form.goalName,
      description: form.description || undefined,
      targetAmount: Number(form.targetAmount),
      startDate: form.startDate,
      endDate: form.endDate,
    });
  };

  return (
    <div className="space-y-8">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold">Admin Dashboard</h1>
          <p className="text-sm text-slate-600">
            Manage finances, goals, and members.
          </p>
        </div>
        <button
          onClick={() => setShowGoalForm(true)}
          className="rounded-md bg-indigo-600 px-4 py-2 text-white shadow-sm hover:bg-indigo-500"
        >
          + Create Goal
        </button>
      </div>

      <div className="grid gap-4 sm:grid-cols-4">
        <StatCard
          label="Total goals"
          value={data?.goalStats.totalGoals ?? 0}
        />
        <StatCard
          label="Total contributions"
          value={data?.contributionStats.totalContributions ?? 0}
        />
        <StatCard label="Members" value={data?.totalMembers ?? 0} />
        <StatCard
          label="Pending contributions"
          value={data?.pendingContributionsCount ?? 0}
        />
      </div>

      <div className="card">
        <h2 className="mb-3 text-lg font-semibold">Quick actions</h2>
        <div className="grid gap-3 sm:grid-cols-3">
          <QuickAction title="Manage Members" description="View and edit member profiles" href="/admin/users" />
          <QuickAction title="Review Contributions" description="Approve or reject pending contributions" href="/admin/contributions" />
          <QuickAction title="Goals" description="Create and manage goals" href="/admin/goals" />
        </div>
      </div>

      {showGoalForm && (
        <div className="card space-y-4">
          <div className="flex items-center justify-between">
            <h3 className="text-lg font-semibold">Create goal</h3>
            <button
              onClick={() => setShowGoalForm(false)}
              className="text-sm text-slate-500 hover:text-slate-700"
            >
              Close
            </button>
          </div>
          <form onSubmit={handleCreateGoal} className="space-y-3">
            <div className="grid gap-3 sm:grid-cols-2">
              <div>
                <label className="text-sm font-medium text-slate-700">Name</label>
                <input
                  required
                  value={form.goalName}
                  onChange={(e) => setForm((f) => ({ ...f, goalName: e.target.value }))}
                  className="mt-1 w-full rounded-md border border-slate-200 px-3 py-2"
                />
              </div>
              <div>
                <label className="text-sm font-medium text-slate-700">
                  Target amount
                </label>
                <input
                  required
                  type="number"
                  min="0.01"
                  step="0.01"
                  value={form.targetAmount}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, targetAmount: e.target.value }))
                  }
                  className="mt-1 w-full rounded-md border border-slate-200 px-3 py-2"
                />
              </div>
              <div>
                <label className="text-sm font-medium text-slate-700">
                  Start date
                </label>
                <input
                  required
                  type="date"
                  value={form.startDate}
                  onChange={(e) => setForm((f) => ({ ...f, startDate: e.target.value }))}
                  className="mt-1 w-full rounded-md border border-slate-200 px-3 py-2"
                />
              </div>
              <div>
                <label className="text-sm font-medium text-slate-700">End date</label>
                <input
                  required
                  type="date"
                  value={form.endDate}
                  onChange={(e) => setForm((f) => ({ ...f, endDate: e.target.value }))}
                  className="mt-1 w-full rounded-md border border-slate-200 px-3 py-2"
                />
              </div>
            </div>
            <div>
              <label className="text-sm font-medium text-slate-700">
                Description (optional)
              </label>
              <textarea
                value={form.description}
                onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
                className="mt-1 w-full rounded-md border border-slate-200 px-3 py-2"
                rows={3}
              />
            </div>
            <button
              type="submit"
              className="rounded-md bg-indigo-600 px-4 py-2 text-white hover:bg-indigo-500 disabled:opacity-60"
              disabled={goalMutation.isPending}
            >
              {goalMutation.isPending ? "Creating..." : "Create goal"}
            </button>
            {goalMutation.isError && (
              <p className="text-sm text-rose-600">
                Failed: {(goalMutation.error as any)?.message ?? "Error"}
              </p>
            )}
          </form>
        </div>
      )}
    </div>
  );
};

const StatCard = ({ label, value }: { label: string; value: number }) => (
  <div className="card">
    <p className="text-sm text-slate-500">{label}</p>
    <p className="text-2xl font-semibold">{value}</p>
  </div>
);

const QuickAction = ({
  title,
  description,
  href,
}: {
  title: string;
  description: string;
  href: string;
}) => (
  <a
    href={href}
    className="rounded-xl border border-slate-200 p-4 hover:border-indigo-200 hover:bg-indigo-50/40 transition"
  >
    <div className="font-semibold text-slate-900">{title}</div>
    <div className="text-sm text-slate-600">{description}</div>
  </a>
);

export default AdminDashboard;

