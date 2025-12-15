namespace CommunityFinanceAPI.Models.DTOs
{
    public class LoanRiskAssessmentResponse
    {
        public string RiskLevel { get; set; } = string.Empty; // Low, Medium, High, Very High
        public int RiskScore { get; set; } // 0-100, where 0 is lowest risk
        public decimal RecommendedAmount { get; set; }
        public List<string> RiskFactors { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public string AssessmentSummary { get; set; } = string.Empty;
    }

    public class LoanRiskScoreResponse
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int RiskScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public decimal TotalContributions { get; set; }
        public int ContributionCount { get; set; }
        public int ActiveLoansCount { get; set; }
        public decimal OutstandingLoanAmount { get; set; }
        public int OverdueLoansCount { get; set; }
        public double AverageContributionAmount { get; set; }
        public int DaysSinceLastContribution { get; set; }
        public string ContributionTier { get; set; } = string.Empty;
    }
}


