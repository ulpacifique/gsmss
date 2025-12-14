import { downloadUrl } from "../../api/reports";

const AdminReports = () => {
  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-2xl font-semibold">Reports & exports</h1>
        <p className="text-sm text-slate-600">
          Direct download links to the API export endpoints.
        </p>
      </div>
      <div className="card space-y-3">
        <ExportLink label="Export contributions" href={downloadUrl.contributions} />
        <ExportLink label="Export members" href={downloadUrl.members} />
        <ExportLink label="Export goals" href={downloadUrl.goals} />
        <ExportLink label="Financial report" href={downloadUrl.financial} />
        <ExportLink label="Audit report" href={downloadUrl.audit} />
      </div>
    </div>
  );
};

const ExportLink = ({ label, href }: { label: string; href: string }) => (
  <a
    href={href}
    className="flex items-center justify-between rounded-md border border-slate-200 px-3 py-2 text-sm hover:bg-slate-50"
  >
    <span>{label}</span>
    <span className="text-blue-600">Download</span>
  </a>
);

export default AdminReports;

