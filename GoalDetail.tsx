import { useQuery } from "@tanstack/react-query";
import { useParams } from "react-router-dom";
import { fetchGoalById } from "../api/goals";

const GoalDetail = () => {
  const { id } = useParams<{ id: string }>();

  const goalId = Number(id);
  const { data, isLoading, error } = useQuery({
    queryKey: ["goal", goalId],
    queryFn: () => fetchGoalById(goalId),
    enabled: Number.isFinite(goalId),
  });

  if (!Number.isFinite(goalId)) return <p>Invalid goal id</p>;

  return (
    <div className="space-y-4">
      {isLoading && <p>Loading goal...</p>}
      {error && (
        <p className="text-rose-600">
          Failed to load goal: {(error as any).message}
        </p>
      )}
      {data && (
        <div className="card space-y-2">
          <h1 className="text-2xl font-semibold">{data.goalName}</h1>
          <p className="text-sm text-slate-600">{data.description}</p>
          <div className="grid gap-4 sm:grid-cols-3 text-sm">
            <div>
              <div className="text-slate-500">Target amount</div>
              <div className="font-semibold">
                {data.targetAmount.toLocaleString()}
              </div>
            </div>
            <div>
              <div className="text-slate-500">Current amount</div>
              <div className="font-semibold">
                {data.currentAmount.toLocaleString()}
              </div>
            </div>
            <div>
              <div className="text-slate-500">Progress</div>
              <div className="font-semibold">{data.progressPercentage}%</div>
            </div>
          </div>
          <div className="text-sm text-slate-600">
            Ends on {new Date(data.endDate).toLocaleDateString()} • Status:{" "}
            {data.status}
          </div>
        </div>
      )}
    </div>
  );
};

export default GoalDetail;

