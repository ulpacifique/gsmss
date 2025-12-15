# AI Risk Assessment Feature

## Overview
The AI Risk Assessment feature provides intelligent loan risk analysis to help admins make informed decisions about loan approvals.

## How It Works

### Risk Scoring (0-100)
- **0-25**: Low Risk ✅
- **26-50**: Medium Risk ⚠️
- **51-75**: High Risk ⚠️⚠️
- **76-100**: Very High Risk ❌

### Risk Factors Analyzed

1. **Contribution History**
   - Total contributions
   - Number of contributions
   - Average contribution amount
   - Days since last contribution

2. **Loan History**
   - Active loans count
   - Outstanding loan amount
   - Overdue loans count
   - Previous repayment behavior

3. **Contribution Tier**
   - Bronze: +5 risk
   - Silver: Neutral
   - Gold: -10 risk
   - Platinum: -10 risk

4. **Loan Amount vs Contributions**
   - Requested amount relative to total contributions
   - Higher ratio = higher risk

5. **Loan Purpose**
   - Emergency/urgent: +5 risk
   - Investment/business: -5 risk

### Recommendations
The AI provides:
- Risk level assessment
- Recommended loan amount (may be reduced for high risk)
- Risk factors identified
- Actionable recommendations

## API Endpoints

### Assess Loan Risk
`POST /api/loans/assess-risk`
- Body: `{ amount: number, purpose?: string }`
- Returns: Risk assessment with score, level, factors, and recommendations

### Get User Risk Score
`GET /api/loans/risk-score`
- Returns: User's current risk score and profile

### Request Loan (with Risk Assessment)
`POST /api/loans/request`
- Now includes automatic risk assessment
- Returns: `{ Loan: LoanResponse, RiskAssessment: LoanRiskAssessmentResponse }`

## Usage

When a member requests a loan:
1. System automatically performs risk assessment
2. Risk score and factors are calculated
3. Admin can view risk assessment when reviewing loan
4. Recommendations help guide approval decisions

## Benefits

- **For Admins**: Better decision-making with data-driven insights
- **For Members**: Transparent risk assessment helps understand loan eligibility
- **For System**: Reduces bad loans and improves community fund health


