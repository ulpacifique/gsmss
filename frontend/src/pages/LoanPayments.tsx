import { useQuery } from "@tanstack/react-query";
import { fetchMyLoanPayments } from "../api/loanPayments";
import { useAuth } from "../state/AuthContext";
import { ReceiptRefundIcon } from "@heroicons/react/24/outline";

const LoanPayments = () => {
  const { isAuthed } = useAuth();

  const { data: payments, isLoading } = useQuery({
    queryKey: ["my-loan-payments"],
    queryFn: fetchMyLoanPayments,
    enabled: isAuthed,
    staleTime: 30000, // Consider data fresh for 30 seconds
  });

  if (!isAuthed) {
    return <p className="text-center text-rose-600">Please log in to view your loan payments.</p>;
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-slate-900">Loan Payment History</h1>
        <p className="mt-2 text-slate-600">
          View proof of all your loan payments with payment references and dates
        </p>
      </div>

      {isLoading && (
        <div className="card">
          <p className="text-center text-slate-500">Loading payment history...</p>
        </div>
      )}

      {!isLoading && (!payments || payments.length === 0) && (
        <div className="card">
          <div className="flex flex-col items-center justify-center py-12 text-center">
            <ReceiptRefundIcon className="h-16 w-16 text-slate-300 mb-4" />
            <p className="text-lg font-semibold text-slate-700">No Payment History</p>
            <p className="mt-2 text-sm text-slate-500">
              You haven't made any loan payments yet. Payments will appear here once you start paying your loans.
            </p>
          </div>
        </div>
      )}

      {!isLoading && payments && payments.length > 0 && (
        <div className="card">
          <div className="overflow-x-auto">
            <table className="min-w-full text-sm">
              <thead className="text-left text-slate-500 border-b border-slate-200">
                <tr>
                  <th className="px-4 py-3 font-semibold">Payment Date</th>
                  <th className="px-4 py-3 font-semibold">Amount</th>
                  <th className="px-4 py-3 font-semibold">Payment Reference</th>
                  <th className="px-4 py-3 font-semibold">Loan ID</th>
                  <th className="px-4 py-3 font-semibold">Notes</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {payments.map((payment) => (
                  <tr key={payment.paymentId} className="hover:bg-slate-50">
                    <td className="px-4 py-3">
                      {new Date(payment.paymentDate).toLocaleDateString("en-US", {
                        year: "numeric",
                        month: "long",
                        day: "numeric",
                        hour: "2-digit",
                        minute: "2-digit",
                      })}
                    </td>
                    <td className="px-4 py-3 font-semibold text-green-600">
                      {payment.amount.toLocaleString(undefined, {
                        style: "currency",
                        currency: "USD",
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2,
                      })}
                    </td>
                    <td className="px-4 py-3 font-mono text-xs bg-slate-50 px-2 py-1 rounded">
                      {payment.paymentReference}
                    </td>
                    <td className="px-4 py-3 text-slate-600">#{payment.loanId}</td>
                    <td className="px-4 py-3 text-slate-500 text-xs">
                      {payment.notes || "-"}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="mt-4 p-4 bg-blue-50 rounded-lg border border-blue-200">
            <p className="text-sm text-blue-800">
              <strong>Payment Proof:</strong> All payments are tracked with unique payment references. 
              You can use these references as proof of payment. Each payment includes the date, amount, 
              and reference number for your records.
            </p>
          </div>
        </div>
      )}
    </div>
  );
};

export default LoanPayments;


