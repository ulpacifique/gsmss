import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  approveContribution,
  fetchPendingContributionsAdmin,
  rejectContribution,
} from "../../api/admin";

const AdminContributions = () => {
  const qc = useQueryClient();
  const { data } = useQuery({
    queryKey: ["admin-pending-contributions"],
    queryFn: fetchPendingContributionsAdmin,
  });

  const approve = useMutation({
    mutationFn: approveContribution,
    onSuccess: () =>
      qc.invalidateQueries({ queryKey: ["admin-pending-contributions"] }),
  });

  const reject = useMutation({
    mutationFn: ({ id, reason }: { id: number; reason?: string }) =>
      rejectContribution(id, reason),
    onSuccess: () =>
      qc.invalidateQueries({ queryKey: ["admin-pending-contributions"] }),
  });

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-2xl font-semibold">Pending contributions</h1>
        <p className="text-sm text-slate-600">
          From `/api/admin/contributions/pending` with quick approve/reject.
        </p>
      </div>
      <div className="card overflow-x-auto">
        <table className="min-w-full text-sm">
          <thead className="text-left text-slate-500">
            <tr>
              <th className="px-3 py-2">Member</th>
              <th className="px-3 py-2">Goal</th>
              <th className="px-3 py-2">Amount</th>
              <th className="px-3 py-2">Actions</th>
            </tr>
          </thead>
          <tbody>
            {data?.map((c) => (
              <tr key={c.contributionId} className="border-t">
                <td className="px-3 py-2 font-medium">{c.userName}</td>
                <td className="px-3 py-2">{c.goalName}</td>
                <td className="px-3 py-2">{c.amount.toLocaleString()}</td>
                <td className="px-3 py-2 space-x-2">
                  <button
                    onClick={() => approve.mutate(c.contributionId)}
                    className="rounded-md bg-emerald-600 px-3 py-1 text-white hover:bg-emerald-500"
                  >
                    Approve
                  </button>
                  <button
                    onClick={() => reject.mutate({ id: c.contributionId })}
                    className="rounded-md bg-rose-600 px-3 py-1 text-white hover:bg-rose-500"
                  >
                    Reject
                  </button>
                </td>
              </tr>
            ))}
            {!data?.length && (
              <tr>
                <td className="px-3 py-4 text-slate-500" colSpan={4}>
                  No pending contributions.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default AdminContributions;

