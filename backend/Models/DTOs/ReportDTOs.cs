namespace CommunityFinanceAPI.Models.DTOs
{
    public class FinancialReportResponse
    {
        public DateTime ReportDate { get; set; }
        public int TotalMembers { get; set; }
        public int TotalGoals { get; set; }
        public decimal TotalContributions { get; set; }
        public decimal TotalDisbursed { get; set; }
        public decimal CurrentBalance { get; set; }
        public List<GoalSummary> GoalSummaries { get; set; } = new();
        public List<MonthlyContribution> MonthlyContributions { get; set; } = new();
        public List<MemberContributionSummary> TopContributors { get; set; } = new();
    }

    public class GoalSummary
    {
        public string GoalName { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal ProgressPercentage { get; set; }
        public int MemberCount { get; set; }
    }

    public class MonthlyContribution
    {
        public string Month { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal Amount { get; set; }
    }

    public class MemberContributionSummary
    {
        public string MemberName { get; set; } = string.Empty;
        public decimal TotalContributed { get; set; }
        public int ContributionCount { get; set; }
    }

    public class AuditReportResponse
    {
        public List<AuditLog> ContributionAudits { get; set; } = new();
        public List<AuditLog> UserAudits { get; set; } = new();
        public List<AuditLog> GoalAudits { get; set; } = new();
    }

    public class AuditLog
    {
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Details { get; set; } = string.Empty;
    }
}