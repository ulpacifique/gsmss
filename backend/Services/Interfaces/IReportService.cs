using CommunityFinanceAPI.Models.DTOs;

namespace CommunityFinanceAPI.Services.Interfaces
{
    public interface IReportService
    {
        Task<FinancialReportResponse> GenerateFinancialReportAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<AuditReportResponse> GenerateAuditReportAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<byte[]> ExportContributionsToExcelAsync(int? userId = null, int? goalId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<byte[]> ExportMembersToExcelAsync();
        Task<byte[]> ExportGoalsToExcelAsync();
    }
}