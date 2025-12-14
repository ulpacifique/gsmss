import { useQuery } from "@tanstack/react-query";
import { fetchGoals } from "../api/goals";
import { Link } from "react-router-dom";

const Goals = () => {
  const { data, isLoading, error } = useQuery({
    queryKey: ["goals"],
    queryFn: fetchGoals,
  });

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-3xl font-bold text-slate-900">Savings Goals</h1>
        <p className="mt-2 text-slate-600">
          View and track all community savings goals
        </p>
      </div>

      <div className="card overflow-x-auto">
        {isLoading && <p>Loading goals...</p>}
        {error && (
          <p className="text-rose-600">
            Failed to load goals: {(error as any).message}
          </p>
        )}
        <table className="min-w-full text-sm">
          <thead className="text-left text-slate-500">
            <tr>
              <th className="px-3 py-2">Name</th>
              <th className="px-3 py-2">Target</th>
              <th className="px-3 py-2">Current</th>
              <th className="px-3 py-2">Progress</th>
              <th className="px-3 py-2">Status</th>
              <th className="px-3 py-2">Ends</th>
            </tr>
          </thead>
          <tbody>
            {data?.map((goal) => (
              <tr key={goal.goalId} className="border-t">
                <td className="px-3 py-2 font-medium">
                  <Link to={`/goals/${goal.goalId}`} className="hover:underline">
                    {goal.goalName}
                  </Link>
                </td>
                <td className="px-3 py-2">
                  {goal.targetAmount.toLocaleString()}
                </td>
                <td className="px-3 py-2">
                  {goal.currentAmount.toLocaleString()}
                </td>
                <td className="px-3 py-2">{goal.progressPercentage}%</td>
                <td className="px-3 py-2">{goal.status}</td>
                <td className="px-3 py-2">
                  {new Date(goal.endDate).toLocaleDateString()}
                </td>
              </tr>
            ))}
            {!data?.length && !isLoading && (
              <tr>
                <td className="px-3 py-4 text-slate-500" colSpan={6}>
                  No goals yet.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default Goals;

