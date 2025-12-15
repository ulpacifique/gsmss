using CommunityFinanceAPI.Data;
using CommunityFinanceAPI.Models.DTOs;
using CommunityFinanceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommunityFinanceAPI.Services.Implementations
{
    public class LoanRiskAssessmentService : ILoanRiskAssessmentService
    {
        private readonly ApplicationDbContext _context;

        public LoanRiskAssessmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LoanRiskAssessmentResponse> AssessLoanRiskAsync(int userId, decimal requestedAmount, string loanPurpose)
        {
            var userRiskScore = await GetUserRiskScoreAsync(userId);
            var riskFactors = new List<string>();
            var recommendations = new List<string>();
            var riskScore = userRiskScore.RiskScore;

            // Adjust risk score based on loan amount relative to user's contributions
            var contributionRatio = userRiskScore.TotalContributions > 0 
                ? requestedAmount / userRiskScore.TotalContributions 
                : 0m;

            // Higher ratio = higher risk
            if (contributionRatio > 2.0m)
            {
                riskScore += 20;
                riskFactors.Add($"Requested amount ({requestedAmount:C}) is {contributionRatio:F1}x your total contributions ({userRiskScore.TotalContributions:C})");
                recommendations.Add("Consider requesting a smaller amount closer to your contribution level");
            }
            else if (contributionRatio > 1.5m)
            {
                riskScore += 10;
                riskFactors.Add($"Requested amount is {contributionRatio:F1}x your contributions");
            }

            // Check loan purpose for risk indicators
            var lowerPurpose = loanPurpose.ToLower();
            if (lowerPurpose.Contains("emergency") || lowerPurpose.Contains("medical") || lowerPurpose.Contains("urgent"))
            {
                riskScore += 5; // Slight increase for emergency loans
                riskFactors.Add("Emergency/urgent loan purpose");
            }
            else if (lowerPurpose.Contains("investment") || lowerPurpose.Contains("business"))
            {
                riskScore -= 5; // Slight decrease for investment/business loans
                recommendations.Add("Business/investment loans show good financial planning");
            }

            // Check if user has overdue loans
            if (userRiskScore.OverdueLoansCount > 0)
            {
                riskScore += 30;
                riskFactors.Add($"User has {userRiskScore.OverdueLoansCount} overdue loan(s)");
                recommendations.Add("Clear existing overdue loans before requesting new ones");
            }

            // Check if user has multiple active loans
            if (userRiskScore.ActiveLoansCount > 1)
            {
                riskScore += 15;
                riskFactors.Add($"User has {userRiskScore.ActiveLoansCount} active loans");
                recommendations.Add("Consider paying off existing loans first");
            }

            // Check contribution history
            if (userRiskScore.ContributionCount < 3)
            {
                riskScore += 10;
                riskFactors.Add($"Limited contribution history ({userRiskScore.ContributionCount} contributions)");
                recommendations.Add("Build more contribution history to improve loan eligibility");
            }

            // Check days since last contribution
            if (userRiskScore.DaysSinceLastContribution > 90)
            {
                riskScore += 15;
                riskFactors.Add($"No contributions in the last {userRiskScore.DaysSinceLastContribution} days");
                recommendations.Add("Recent contributions show active participation");
            }

            // Check contribution tier
            if (userRiskScore.ContributionTier == "Bronze")
            {
                riskScore += 5;
                riskFactors.Add("Bronze tier contributor (lower contribution level)");
            }
            else if (userRiskScore.ContributionTier == "Platinum" || userRiskScore.ContributionTier == "Gold")
            {
                riskScore -= 10;
                recommendations.Add($"Excellent contribution tier ({userRiskScore.ContributionTier}) shows strong commitment");
            }

            // Ensure risk score is within bounds
            riskScore = Math.Max(0, Math.Min(100, riskScore));

            // Determine risk level
            string riskLevel;
            if (riskScore <= 25)
                riskLevel = "Low";
            else if (riskScore <= 50)
                riskLevel = "Medium";
            else if (riskScore <= 75)
                riskLevel = "High";
            else
                riskLevel = "Very High";

            // Calculate recommended amount (conservative approach)
            var recommendedAmount = requestedAmount;
            if (riskScore > 50)
            {
                // Reduce recommended amount for higher risk
                recommendedAmount = requestedAmount * 0.7m; // 30% reduction
                recommendations.Add($"Due to risk factors, consider reducing loan amount to {recommendedAmount:C}");
            }

            // Generate assessment summary
            var summary = riskLevel switch
            {
                "Low" => $"Low risk loan request. User has strong contribution history ({userRiskScore.ContributionTier} tier) and good repayment capacity.",
                "Medium" => $"Medium risk loan request. Some risk factors present but manageable with proper monitoring.",
                "High" => $"High risk loan request. Multiple risk factors identified. Requires careful review and possibly reduced amount.",
                "Very High" => $"Very high risk loan request. Strong recommendation to reject or significantly reduce amount. Multiple concerning factors present.",
                _ => "Risk assessment completed."
            };

            return new LoanRiskAssessmentResponse
            {
                RiskLevel = riskLevel,
                RiskScore = riskScore,
                RecommendedAmount = recommendedAmount,
                RiskFactors = riskFactors,
                Recommendations = recommendations,
                AssessmentSummary = summary
            };
        }

        public async Task<LoanRiskScoreResponse> GetUserRiskScoreAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            // Get contribution statistics
            var contributions = await _context.Contributions
                .Where(c => c.UserId == userId && c.Status == "Approved")
                .ToListAsync();

            var totalContributions = contributions.Sum(c => c.Amount);
            var contributionCount = contributions.Count;
            var averageContribution = contributionCount > 0 ? (double)totalContributions / contributionCount : 0;
            var lastContribution = contributions.OrderByDescending(c => c.SubmittedAt).FirstOrDefault();
            var daysSinceLastContribution = lastContribution != null 
                ? (DateTime.UtcNow - lastContribution.SubmittedAt).Days 
                : 999;

            // Get loan statistics
            var loans = await _context.Loans
                .Where(l => l.UserId == userId)
                .ToListAsync();

            var activeLoans = loans.Where(l => l.Status == "Approved" && l.RemainingAmount > 0).ToList();
            var activeLoansCount = activeLoans.Count;
            var outstandingLoanAmount = activeLoans.Sum(l => l.RemainingAmount);
            var overdueLoans = activeLoans.Where(l => DateTime.UtcNow > l.DueDate).ToList();
            var overdueLoansCount = overdueLoans.Count;

            // Determine contribution tier
            string contributionTier;
            if (totalContributions >= 5000m)
                contributionTier = "Platinum";
            else if (totalContributions >= 2000m)
                contributionTier = "Gold";
            else if (totalContributions >= 500m)
                contributionTier = "Silver";
            else
                contributionTier = "Bronze";

            // Calculate base risk score (0-100, lower is better)
            var baseRiskScore = 50; // Start at medium risk

            // Adjust based on contribution history
            if (contributionCount < 3)
                baseRiskScore += 15;
            else if (contributionCount >= 10)
                baseRiskScore -= 10;

            // Adjust based on overdue loans
            baseRiskScore += overdueLoansCount * 20;

            // Adjust based on active loans
            baseRiskScore += activeLoansCount * 5;

            // Adjust based on days since last contribution
            if (daysSinceLastContribution > 90)
                baseRiskScore += 10;
            else if (daysSinceLastContribution < 30)
                baseRiskScore -= 5;

            // Adjust based on tier
            if (contributionTier == "Platinum" || contributionTier == "Gold")
                baseRiskScore -= 10;
            else if (contributionTier == "Bronze")
                baseRiskScore += 5;

            return new LoanRiskScoreResponse
            {
                UserId = userId,
                UserName = $"{user.FirstName} {user.LastName}",
                RiskScore = Math.Max(0, Math.Min(100, baseRiskScore)),
                RiskLevel = baseRiskScore <= 25 ? "Low" : baseRiskScore <= 50 ? "Medium" : baseRiskScore <= 75 ? "High" : "Very High",
                TotalContributions = totalContributions,
                ContributionCount = contributionCount,
                ActiveLoansCount = activeLoansCount,
                OutstandingLoanAmount = outstandingLoanAmount,
                OverdueLoansCount = overdueLoansCount,
                AverageContributionAmount = averageContribution,
                DaysSinceLastContribution = daysSinceLastContribution,
                ContributionTier = contributionTier
            };
        }
    }
}


