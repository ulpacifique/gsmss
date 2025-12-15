using Microsoft.AspNetCore.Mvc;

namespace CommunityFinanceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIHelpController : ControllerBase
    {
        private readonly ILogger<AIHelpController> _logger;

        public AIHelpController(ILogger<AIHelpController> logger)
        {
            _logger = logger;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> AskQuestion([FromBody] AIHelpRequest request)
        {
            try
            {
                if (HttpContext.Items["AuthenticatedUser"] is not Models.Entities.User user)
                    return Unauthorized(new { message = "User not authenticated" });

                if (string.IsNullOrWhiteSpace(request.Question))
                    return BadRequest(new { message = "Question is required" });

                var response = await GenerateAIResponseAsync(request.Question, user.Role);
                
                return Ok(new AIHelpResponse
                {
                    Answer = response,
                    SuggestedActions = GetSuggestedActions(request.Question, user.Role)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AI help request");
                return StatusCode(500, new { message = "An error occurred while processing your question" });
            }
        }

        [HttpGet("topics")]
        public IActionResult GetHelpTopics()
        {
            try
            {
                var topics = new[]
                {
                    new { Topic = "Contributions", Description = "Learn how to make contributions to goals", Icon = "💰" },
                    new { Topic = "Loans", Description = "Understand loan eligibility and payment process", Icon = "🏦" },
                    new { Topic = "Goals", Description = "Create and manage savings goals", Icon = "🎯" },
                    new { Topic = "Profile", Description = "Update your profile and settings", Icon = "👤" },
                    new { Topic = "Reports", Description = "View your financial reports and statistics", Icon = "📊" },
                    new { Topic = "Notifications", Description = "Manage your notifications and alerts", Icon = "🔔" }
                };

                return Ok(topics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting help topics");
                return StatusCode(500, new { message = "An error occurred while retrieving help topics" });
            }
        }

        private async Task<string> GenerateAIResponseAsync(string question, string userRole)
        {
            // Enhanced rule-based AI response system with deeper, contextual answers
            // In production, you could integrate with OpenAI, Azure OpenAI, or similar services
            var lowerQuestion = question.ToLower();

            // Profile - More specific responses based on question wording
            if (lowerQuestion.Contains("profile"))
            {
                if (lowerQuestion.Contains("how") && (lowerQuestion.Contains("update") || lowerQuestion.Contains("change") || lowerQuestion.Contains("edit")))
                {
                    return "How to Update Your Profile:\n\nStep-by-step instructions:\n1. Click on 'Profile' in the left sidebar (it's at the bottom)\n2. You'll see your current profile information\n3. To update your name:\n   - Click in the 'First Name' or 'Last Name' field\n   - Type your new name\n   - Click 'Save Changes'\n\n4. To update your email:\n   - Click in the 'Email' field\n   - Enter your new email address\n   - Click 'Save Changes'\n\n5. To update your phone:\n   - Click in the 'Phone Number' field\n   - Enter your new phone number\n   - Click 'Save Changes'\n\n6. To upload a profile picture:\n   - Click 'Choose File' under Profile Picture\n   - Select an image from your computer (JPG, PNG, or GIF)\n   - The image will be converted and saved\n   - Your picture will appear in the header and chat\n\nNote: All changes are saved immediately when you click 'Save Changes'. Your profile picture can be up to several MB in size.";
                }
                else if (lowerQuestion.Contains("picture") || lowerQuestion.Contains("photo") || lowerQuestion.Contains("image"))
                {
                    return "Profile Picture Guide:\n\nUploading a Profile Picture:\n1. Go to the Profile page\n2. Scroll to the 'Profile Picture' section\n3. Click 'Choose File' button\n4. Select an image file from your device (supports JPG, PNG, GIF)\n5. The image will be automatically converted to base64 format\n6. Click 'Save Changes' to upload\n\nWhere Your Picture Appears:\n- In the top-right corner of the header (next to your name)\n- In chat conversations\n- In admin member lists\n- In reports and notifications\n\nTips:\n- Use a clear, professional photo\n- Square images work best\n- The system supports large images (up to several MB)\n- You can update your picture anytime";
                }
                else if (lowerQuestion.Contains("view") || lowerQuestion.Contains("see") || lowerQuestion.Contains("check"))
                {
                    return "Viewing Your Profile:\n\nTo see your profile information:\n1. Click 'Profile' in the left sidebar\n2. You'll see:\n   - Your full name\n   - Email address\n   - Phone number\n   - Profile picture (if uploaded)\n   - Your role (Member/Admin)\n\nYour profile also shows:\n- Account balance (total contributions)\n- Contribution history\n- Loan status\n- Active goals you've joined\n\nNote: Only you can see your full profile details. Admins can see basic information in member lists.";
                }
                else
                {
                    return "Profile Management Overview:\n\nYour profile contains:\n- Personal Information: Name, Email, Phone Number\n- Profile Picture: Upload an image to personalize your account\n- Account Summary: View your contributions and loan status\n\nKey Features:\n✓ Update your information anytime\n✓ Upload and change your profile picture\n✓ View your financial summary\n✓ See your contribution history\n\nTo get started, click 'Profile' in the sidebar. From there you can update any information or upload a picture.";
                }
            }

            // Contributions - More detailed responses
            if (lowerQuestion.Contains("contribution") || lowerQuestion.Contains("contribute"))
            {
                if (lowerQuestion.Contains("how") && (lowerQuestion.Contains("make") || lowerQuestion.Contains("create") || lowerQuestion.Contains("add")))
                {
                    return "How to Make a Contribution:\n\nDetailed Steps:\n1. Navigate to 'Contributions' in the left sidebar\n2. Select a Goal:\n   - Choose from the dropdown menu\n   - Only 'Active' goals are shown\n   - Each goal shows its target amount and progress\n\n3. Enter Contribution Amount:\n   - Type the amount you want to contribute\n   - Check the contribution limits shown below:\n     • Fixed amount (if set by admin)\n     • Minimum amount per contribution\n     • Maximum amount per contribution\n     • Maximum total per user for this goal\n\n4. Add Payment Reference:\n   - Enter a unique reference number\n   - This could be a transaction ID, receipt number, or payment confirmation\n   - This helps track your payment\n\n5. Click 'Create Contribution'\n6. Your contribution will be submitted for admin approval\n\nAfter Submission:\n- You'll receive a confirmation\n- Admin will review and approve/reject\n- Once approved, it counts toward your loan eligibility\n- You'll automatically join the goal if not already a member";
                }
                else if (lowerQuestion.Contains("limit") || lowerQuestion.Contains("maximum") || lowerQuestion.Contains("minimum"))
                {
                    return "Contribution Limits Explained:\n\nTypes of Limits:\n1. Fixed Amount:\n   - Admin sets a specific amount for all contributions\n   - You must contribute exactly this amount\n   - Example: If fixed amount is $100, you can only contribute $100\n\n2. Minimum Amount:\n   - The smallest contribution allowed\n   - Your contribution must be at least this amount\n   - Example: Minimum $50 means you can't contribute less than $50\n\n3. Maximum Per Contribution:\n   - The largest single contribution allowed\n   - You can make multiple contributions up to this limit\n   - Example: Max $500 means each contribution can't exceed $500\n\n4. Maximum Total Per User:\n   - Total amount you can contribute to this goal\n   - Prevents over-contribution\n   - Example: Max total $2,000 means all your contributions combined can't exceed $2,000\n\nThese limits are set by admins per goal. Check the contribution form to see which limits apply.";
                }
                else if (lowerQuestion.Contains("tier") || lowerQuestion.Contains("eligibility") || lowerQuestion.Contains("bronze") || lowerQuestion.Contains("silver") || lowerQuestion.Contains("gold") || lowerQuestion.Contains("platinum"))
                {
                    return "Contribution Tiers and Loan Eligibility:\n\nYour total approved contributions determine your tier:\n\n🏅 Bronze Tier ($0 - $499):\n   - Loan multiplier: 1x your contributions\n   - Max loan: 10% of community balance\n   - Example: $300 contributions = up to $300 loan\n\n🥈 Silver Tier ($500 - $1,999):\n   - Loan multiplier: 1.5x your contributions\n   - Max loan: 15% of community balance\n   - Example: $1,000 contributions = up to $1,500 loan\n\n🥇 Gold Tier ($2,000 - $4,999):\n   - Loan multiplier: 2x your contributions\n   - Max loan: 20% of community balance\n   - Example: $3,000 contributions = up to $6,000 loan\n\n💎 Platinum Tier ($5,000+):\n   - Loan multiplier: 3x your contributions\n   - Max loan: 25% of community balance\n   - Example: $6,000 contributions = up to $18,000 loan\n\nYour actual max loan is the minimum of:\n• Your contributions × tier multiplier\n• Tier percentage of total community balance\n• Regular contributor option (1x or 10%)\n• Available community funds";
                }
                else
                {
                    return "Contributions Overview:\n\nWhat are Contributions?\nContributions are payments you make toward community savings goals. They help build the community fund and determine your loan eligibility.\n\nMaking Contributions:\n1. Go to Contributions page\n2. Select an active goal\n3. Enter amount (check limits)\n4. Add payment reference\n5. Submit for approval\n\nBenefits:\n✓ Builds community savings\n✓ Increases your loan eligibility\n✓ Earns contribution tier status\n✓ Supports community goals\n\nYour contributions are tracked and shown in your profile. Only approved contributions count toward your tier.";
                }
            }

            // Loans - More detailed responses
            if (lowerQuestion.Contains("loan") || lowerQuestion.Contains("borrow"))
            {
                if (lowerQuestion.Contains("how") && (lowerQuestion.Contains("request") || lowerQuestion.Contains("apply") || lowerQuestion.Contains("get")))
                {
                    return "How to Request a Loan:\n\nStep-by-Step Process:\n1. Check Your Eligibility:\n   - Go to Dashboard\n   - Look at 'Max Loan Amount' card\n   - Ensure you have at least one approved contribution\n   - Check your contribution tier\n\n2. Request the Loan:\n   - Click 'Request Loan' button on Dashboard\n   - Enter Loan Amount:\n     • Must not exceed your max loan amount\n     • Check available community funds\n     • Consider your repayment ability\n   - Enter Loan Purpose:\n     • Describe what you need the loan for\n     • Be specific (e.g., 'Medical expenses', 'Education fees')\n     • This helps admin make a decision\n\n3. Submit Request:\n   - Click 'Submit Request'\n   - Your request goes to admin for review\n   - You'll receive a notification when reviewed\n\n4. After Approval:\n   - Loan appears in 'Active Loans' section\n   - You can make payments anytime\n   - Track remaining balance\n   - Payment due date is shown\n\nLoan Terms:\n- Interest rate: 5% (default)\n- Loan duration: 30 days (default)\n- You can pay early or in installments";
                }
                else if (lowerQuestion.Contains("how") && (lowerQuestion.Contains("pay") || lowerQuestion.Contains("repay") || lowerQuestion.Contains("payment")))
                {
                    return "How to Pay Your Loan:\n\nMaking a Loan Payment:\n1. Go to Dashboard\n2. Find 'Active Loans' section\n3. Click 'Pay' button on the loan you want to pay\n4. Enter Payment Amount:\n   - Can be partial or full payment\n   - Maximum is the remaining balance\n   - Minimum is typically $0.01\n5. Enter Payment Reference:\n   - Add a unique reference number\n   - This could be:\n     • Bank transaction ID\n     • Receipt number\n     • Payment confirmation code\n     • Any unique identifier\n6. Click 'Submit Payment'\n\nPayment Tracking:\n- View all payments in 'Payment History' page\n- Each payment shows:\n  • Date and time\n  • Amount paid\n  • Payment reference (for proof)\n  • Remaining balance\n\nNotifications:\n- You'll receive a notification when payment is processed\n- Admin is notified of your payment\n- When fully paid, you'll get a completion notification\n\nTips:\n- Keep payment references for your records\n- Payments reduce your remaining balance\n- You can pay multiple times until fully paid";
                }
                else if (lowerQuestion.Contains("eligibility") || lowerQuestion.Contains("qualify") || lowerQuestion.Contains("eligible"))
                {
                    return "Loan Eligibility Requirements:\n\nBasic Requirements:\n✓ At least one approved contribution\n✓ No outstanding loans (must pay existing loans first)\n✓ Community fund must have available funds\n✓ Meet tier-based loan limits\n\nHow Max Loan is Calculated:\n\nThe system uses THREE methods and takes the LOWEST:\n\nMethod 1: Contribution-Based (Tier Multiplier)\n- Bronze: 1x your contributions\n- Silver: 1.5x your contributions\n- Gold: 2x your contributions\n- Platinum: 3x your contributions\n\nMethod 2: Community Balance Percentage\n- Bronze: 10% of total community balance\n- Silver: 15% of total community balance\n- Gold: 20% of total community balance\n- Platinum: 25% of total community balance\n\nMethod 3: Regular Contributor Option\n- 1x your contributions OR\n- 10% of total community balance\n- Whichever is lower\n\nFinal Check:\n- Your max loan is capped by available community funds\n- Available funds = Total balance - Outstanding loans\n\nExample:\nIf you have $1,000 contributions (Silver tier):\n- Method 1: $1,000 × 1.5 = $1,500\n- Method 2: If community has $10,000, then 15% = $1,500\n- Method 3: $1,000 × 1 = $1,000 OR 10% = $1,000\n- Your max loan = $1,000 (lowest of all)";
                }
                else if (lowerQuestion.Contains("overdue") || lowerQuestion.Contains("late") || lowerQuestion.Contains("due"))
                {
                    return "Overdue Loans:\n\nWhat Happens When a Loan is Overdue:\n- Your loan becomes overdue after the due date\n- You'll receive automatic notifications\n- Admin is notified about overdue loans\n- The system checks daily for overdue loans\n\nNotifications You'll Receive:\n- Warning when loan becomes overdue\n- Number of days overdue\n- Remaining balance amount\n- Reminder to make a payment\n\nWhat to Do:\n1. Check your Dashboard for overdue loans\n2. Make a payment as soon as possible\n3. Contact admin if you need an extension\n4. Partial payments are accepted\n\nPreventing Overdue Loans:\n- Set reminders before due date\n- Make payments early\n- Pay in installments if needed\n- Monitor your loan status regularly\n\nNote: Overdue loans don't prevent new contributions, but you must pay them before requesting new loans.";
                }
                else
                {
                    return "Loans Overview:\n\nLoan Features:\n- Request loans based on your contribution tier\n- Flexible repayment (partial or full payments)\n- Payment tracking with references\n- Automatic notifications\n\nLoan Process:\n1. Make contributions to build eligibility\n2. Check your max loan amount on Dashboard\n3. Request a loan (admin approval required)\n4. Make payments with references\n5. Track payment history\n\nLoan Terms:\n- Interest: 5% (added to principal)\n- Duration: 30 days (default)\n- Early payment: Allowed\n- Installments: Supported\n\nYour loan eligibility increases with more contributions. Check the Dashboard to see your current tier and max loan amount.";
                }
            }

            // Goals - More detailed responses
            if (lowerQuestion.Contains("goal") || lowerQuestion.Contains("savings"))
            {
                if (lowerQuestion.Contains("how") && (lowerQuestion.Contains("join") || lowerQuestion.Contains("participate")))
                {
                    return "How to Join a Goal:\n\nAutomatic Joining:\n- When you make a contribution to a goal, you automatically become a member\n- No separate 'join' button needed\n- Your first contribution adds you to the goal\n\nProcess:\n1. Go to Contributions page\n2. Select a goal from the dropdown\n3. Make your first contribution\n4. You're now a member of that goal\n\nViewing Your Goals:\n- Go to Dashboard\n- See 'Your goals' section\n- Shows all goals you've contributed to\n- Displays:\n  • Goal name and description\n  • Your total contributions\n  • Goal progress percentage\n  • Target amount and deadline\n\nGoal Status:\n- Active: Currently accepting contributions\n- Completed: Reached target amount\n- Cancelled: Goal was cancelled by admin\n\nNote: You can contribute to multiple goals. Each contribution counts toward that specific goal.";
                }
                else if (lowerQuestion.Contains("create") || lowerQuestion.Contains("make") || lowerQuestion.Contains("new"))
                {
                    if (userRole == "Admin")
                    {
                        return "How to Create a Goal (Admin Only):\n\nSteps:\n1. Go to Admin Dashboard\n2. Click 'Manage Goals' in the sidebar\n3. Click 'Create New Goal' button\n4. Fill in the form:\n   - Goal Name: Descriptive name (e.g., 'Emergency Fund 2024')\n   - Description: Details about the goal\n   - Target Amount: Total amount needed\n   - Deadline: When the goal should be completed\n   - Status: Set to 'Active' to start accepting contributions\n5. Click 'Create Goal'\n\nManaging Goals:\n- Edit existing goals\n- Change status (Active/Completed/Cancelled)\n- Update target amounts\n- Extend deadlines\n\nSetting Contribution Limits:\n- After creating a goal, set contribution limits\n- Go to Admin → Manage Goals\n- Set fixed amounts, min/max per contribution\n- Set maximum total per user\n\nTips:\n- Create clear, specific goal names\n- Set realistic target amounts\n- Consider contribution limits to manage participation";
                    }
                    else
                    {
                        return "Creating Goals:\n\nOnly admins can create goals. If you're a member:\n- You can suggest goals to your admin\n- Contact admin through Messages\n- Admins review and create goals\n\nAs a member, you can:\n- Join existing goals by contributing\n- View all active goals\n- Track your contributions to each goal\n- See goal progress\n\nTo request a new goal, message your admin with:\n- Goal name and purpose\n- Suggested target amount\n- Why it's important to the community";
                    }
                }
                else
                {
                    return "Savings Goals:\n\nWhat are Goals?\nGoals are community savings targets. Members contribute money toward achieving these goals.\n\nGoal Types:\n- Emergency funds\n- Community projects\n- Special events\n- Investment opportunities\n\nHow Goals Work:\n1. Admin creates a goal with target amount\n2. Members contribute to the goal\n3. Progress is tracked automatically\n4. Goal is completed when target is reached\n\nViewing Goals:\n- See all goals on the Goals page\n- View goal details and progress\n- Check which goals you've joined\n- See your contribution history per goal\n\nGoal Status:\n- Active: Accepting contributions\n- Completed: Target reached\n- Cancelled: No longer active";
                }
            }

            // Payments - More detailed responses
            if (lowerQuestion.Contains("payment") || lowerQuestion.Contains("pay") || lowerQuestion.Contains("proof"))
            {
                if (lowerQuestion.Contains("history") || lowerQuestion.Contains("view") || lowerQuestion.Contains("see") || lowerQuestion.Contains("track"))
                {
                    return "Viewing Payment History:\n\nAccessing Payment History:\n1. Click 'Payment History' in the left sidebar\n2. You'll see a table with all your loan payments\n\nPayment Information Shown:\n- Payment Date: When you made the payment\n- Amount: How much you paid\n- Payment Reference: Unique ID for proof\n- Loan ID: Which loan the payment was for\n- Notes: Any additional notes\n\nUsing Payment References:\n- Each payment has a unique reference\n- Use this as proof of payment\n- Keep references for your records\n- Share with admin if needed\n\nFiltering Payments:\n- All payments are shown chronologically\n- Most recent payments appear first\n- You can see payments for all your loans\n\nExport/Print:\n- Take screenshots for records\n- Payment references serve as receipts\n- All payments are permanently recorded";
                }
                else if (lowerQuestion.Contains("proof") || lowerQuestion.Contains("receipt") || lowerQuestion.Contains("reference"))
                {
                    return "Payment Proof and References:\n\nWhat is a Payment Reference?\nA payment reference is a unique identifier for each payment you make. It serves as proof of payment.\n\nHow Payment References Work:\n- You enter a reference when making a payment\n- It can be:\n  • Bank transaction ID\n  • Receipt number\n  • Payment confirmation code\n  • Any unique identifier\n- The system stores it permanently\n\nUsing References as Proof:\n- View references in Payment History\n- Each payment shows its reference\n- Use references to verify payments\n- Share with admin if needed\n\nFinding Your References:\n1. Go to Payment History page\n2. Find the payment you need\n3. Copy the Payment Reference\n4. Use it as needed\n\nTips:\n- Use meaningful references (e.g., 'BANK-TXN-12345')\n- Keep your own records\n- References are permanent\n- They appear in notifications too";
                }
                else
                {
                    return "Loan Payments:\n\nMaking Payments:\n- Go to Dashboard\n- Find your active loan\n- Click 'Pay' button\n- Enter amount and reference\n- Submit payment\n\nPayment Features:\n✓ Partial payments allowed\n✓ Multiple payments per loan\n✓ Payment references for proof\n✓ Automatic notifications\n✓ Payment history tracking\n\nPayment Tracking:\n- All payments are recorded\n- View in Payment History page\n- See remaining balance\n- Track payment dates\n\nNotifications:\n- You're notified when payment is processed\n- Admin receives notification\n- Completion notification when fully paid";
                }
            }

            // Notifications - More detailed responses
            if (lowerQuestion.Contains("notification") || lowerQuestion.Contains("alert") || lowerQuestion.Contains("message"))
            {
                return "Notifications System:\n\nTypes of Notifications:\n\n1. Loan Notifications:\n   - Loan request approved/rejected\n   - Payment received confirmation\n   - Loan fully paid\n   - Loan overdue warnings\n\n2. Contribution Notifications:\n   - Contribution approved/rejected\n   - Contribution limit reached\n   - Goal progress updates\n\n3. System Notifications:\n   - Profile updates\n   - Account changes\n   - System announcements\n\n4. Message Notifications:\n   - New messages from admin/members\n   - Unread message count\n\nViewing Notifications:\n- Check notification bell (if enabled)\n- View in Messages page\n- Notifications appear in real-time\n\nManaging Notifications:\n- Mark as read\n- View notification details\n- Click to go to related page\n- Notifications are stored permanently\n\nNote: You'll always be notified about important actions like loan approvals, payments, and overdue loans.";
            }

            // Admin features - More detailed responses
            if (userRole == "Admin" && (lowerQuestion.Contains("admin") || lowerQuestion.Contains("manage") || lowerQuestion.Contains("member")))
            {
                if (lowerQuestion.Contains("member"))
                {
                    return "Managing Members (Admin):\n\nAccess:\n1. Go to Admin Dashboard\n2. Click 'Manage Members' in sidebar\n\nMember Management Features:\n\nView Members:\n- See all registered members\n- View member details:\n  • Name, email, phone\n  • Contribution total\n  • Loan status\n  • Active/inactive status\n\nEdit Members:\n- Update member information\n- Change email, phone\n- Update names\n\nDelete/Deactivate Members:\n- Remove members from system\n- Deactivate instead of delete (recommended)\n- Preserves data history\n\nSearch and Filter:\n- Search by name or email\n- Filter by status\n- Sort by contributions\n- Limit results (top 5/10/20/all)\n\nMember Reports:\n- View contribution history\n- Check loan history\n- See goal participation\n- Export member data\n\nTips:\n- Use search to find members quickly\n- Review member contributions before deleting\n- Deactivate instead of delete when possible";
                }
                else if (lowerQuestion.Contains("loan") && (lowerQuestion.Contains("approve") || lowerQuestion.Contains("review")))
                {
                    return "Approving Loans (Admin):\n\nProcess:\n1. Go to Admin Dashboard\n2. Click 'Loan Requests' in sidebar\n3. View pending loans\n\nReviewing Loans:\n- Check member's contribution history\n- Verify loan amount is within limits\n- Review loan purpose\n- Check for outstanding loans\n\nApproving a Loan:\n1. Click 'Approve' button\n2. Loan is immediately approved\n3. Member receives notification\n4. Loan appears in active loans\n\nRejecting a Loan:\n1. Click 'Reject' button\n2. Enter rejection reason\n3. Member receives notification\n4. Loan is marked as rejected\n\nLoan Management:\n- View all loans (pending/approved/rejected)\n- Filter by status\n- See loan details\n- Monitor overdue loans\n- Track payments\n\nTips:\n- Verify member eligibility\n- Check available community funds\n- Consider loan purpose\n- Review member's payment history";
                }
                else
                {
                    return "Admin Features Overview:\n\nMain Admin Functions:\n\n1. Admin Dashboard:\n   - Overview of system statistics\n   - Quick actions\n   - Recent activity\n\n2. Manage Members:\n   - View, edit, delete members\n   - Search and filter\n   - View member reports\n\n3. Manage Goals:\n   - Create, edit goals\n   - Set contribution limits\n   - Track goal progress\n\n4. Review Contributions:\n   - Approve/reject contributions\n   - View contribution history\n   - Manage contribution limits\n\n5. Loan Requests:\n   - Approve/reject loans\n   - View all loans\n   - Monitor overdue loans\n\n6. General Report:\n   - Comprehensive system report\n   - Member statistics\n   - Goal analytics\n   - Export to CSV\n\nAccess all features from the Admin Dashboard sidebar.";
                }
            }

            // Default response with more helpful information
            return "I'm here to help! I can provide detailed information about:\n\n📋 Getting Started:\n- How to make contributions\n- How to request loans\n- How to update your profile\n\n💰 Financial Features:\n- Contribution tiers and limits\n- Loan eligibility calculation\n- Payment processes\n- Payment history and proof\n\n🎯 Goals:\n- Joining goals\n- Tracking progress\n- Goal management (admin)\n\n⚙️ Account Management:\n- Profile updates\n- Profile picture upload\n- Viewing account information\n\n🔔 Notifications:\n- Types of notifications\n- Managing notifications\n- Alert system\n\nTry asking specific questions like:\n- 'How do I update my profile?'\n- 'How do I make a contribution?'\n- 'How do I request a loan?'\n- 'How do I pay my loan?'\n- 'What is my contribution tier?'\n\nOr browse the help topics for quick access to common questions.";
        }

        private List<string> GetSuggestedActions(string question, string userRole)
        {
            var lowerQuestion = question.ToLower();
            var actions = new List<string>();

            if (lowerQuestion.Contains("contribution"))
            {
                if (lowerQuestion.Contains("how") && (lowerQuestion.Contains("make") || lowerQuestion.Contains("create")))
                {
                    actions.Add("Go to Contributions page to make a contribution");
                    actions.Add("Check contribution limits for your selected goal");
                }
                else if (lowerQuestion.Contains("limit") || lowerQuestion.Contains("maximum") || lowerQuestion.Contains("minimum"))
                {
                    actions.Add("View contribution limits on Contributions page");
                    actions.Add("Check goal-specific limits");
                }
                else if (lowerQuestion.Contains("tier") || lowerQuestion.Contains("eligibility"))
                {
                    actions.Add("Check your contribution tier on Dashboard");
                    actions.Add("View your total contributions");
                }
                else
                {
                    actions.Add("Go to Contributions page");
                    actions.Add("View contribution limits");
                    actions.Add("Check your contribution tier");
                }
            }
            else if (lowerQuestion.Contains("loan"))
            {
                if (lowerQuestion.Contains("how") && (lowerQuestion.Contains("request") || lowerQuestion.Contains("apply")))
                {
                    actions.Add("Go to Dashboard to request a loan");
                    actions.Add("Check your max loan amount");
                }
                else if (lowerQuestion.Contains("how") && (lowerQuestion.Contains("pay") || lowerQuestion.Contains("repay")))
                {
                    actions.Add("Go to Dashboard to pay your loan");
                    actions.Add("View Payment History for past payments");
                }
                else if (lowerQuestion.Contains("eligibility") || lowerQuestion.Contains("qualify"))
                {
                    actions.Add("Check your loan eligibility on Dashboard");
                    actions.Add("View your contribution tier");
                }
                else if (lowerQuestion.Contains("overdue") || lowerQuestion.Contains("due"))
                {
                    actions.Add("Check your active loans on Dashboard");
                    actions.Add("Make a payment to clear overdue status");
                }
                else
                {
                    actions.Add("View Dashboard for loan eligibility");
                    actions.Add("Check loan payment history");
                    actions.Add("Request a new loan");
                }
            }
            else if (lowerQuestion.Contains("goal"))
            {
                if (lowerQuestion.Contains("how") && (lowerQuestion.Contains("join") || lowerQuestion.Contains("participate")))
                {
                    actions.Add("Make a contribution to join a goal");
                    actions.Add("Browse available goals");
                }
                else if (lowerQuestion.Contains("create") || lowerQuestion.Contains("make"))
                {
                    if (userRole == "Admin")
                    {
                        actions.Add("Go to Admin → Manage Goals");
                        actions.Add("Click 'Create New Goal'");
                    }
                    else
                    {
                        actions.Add("Contact admin to request a new goal");
                        actions.Add("Use Messages to communicate with admin");
                    }
                }
                else
                {
                    actions.Add("Browse available goals");
                    actions.Add("View goal progress");
                    actions.Add("Make a contribution to join");
                }
            }
            else if (lowerQuestion.Contains("profile"))
            {
                if (lowerQuestion.Contains("how") && (lowerQuestion.Contains("update") || lowerQuestion.Contains("change")))
                {
                    actions.Add("Go to Profile page");
                    actions.Add("Click on fields to edit");
                    actions.Add("Click 'Save Changes' when done");
                }
                else if (lowerQuestion.Contains("picture") || lowerQuestion.Contains("photo"))
                {
                    actions.Add("Go to Profile page");
                    actions.Add("Click 'Choose File' under Profile Picture");
                    actions.Add("Select an image and save");
                }
                else if (lowerQuestion.Contains("view") || lowerQuestion.Contains("see"))
                {
                    actions.Add("Go to Profile page to view your profile");
                    actions.Add("Check your account summary");
                }
                else
                {
                    actions.Add("Go to Profile page");
                    actions.Add("Update your information");
                    actions.Add("Upload a profile picture");
                }
            }
            else if (lowerQuestion.Contains("payment") || lowerQuestion.Contains("pay"))
            {
                if (lowerQuestion.Contains("history") || lowerQuestion.Contains("view") || lowerQuestion.Contains("track"))
                {
                    actions.Add("Go to Payment History page");
                    actions.Add("View all your loan payments");
                }
                else if (lowerQuestion.Contains("proof") || lowerQuestion.Contains("reference"))
                {
                    actions.Add("Go to Payment History page");
                    actions.Add("Copy payment references");
                }
                else
                {
                    actions.Add("Go to Dashboard to make a payment");
                    actions.Add("View Payment History");
                }
            }
            else if (lowerQuestion.Contains("notification") || lowerQuestion.Contains("alert"))
            {
                actions.Add("Check Messages for notifications");
                actions.Add("View notification history");
            }
            else if (userRole == "Admin" && lowerQuestion.Contains("admin"))
            {
                if (lowerQuestion.Contains("member"))
                {
                    actions.Add("Go to Admin → Manage Members");
                    actions.Add("Search and filter members");
                }
                else if (lowerQuestion.Contains("loan"))
                {
                    actions.Add("Go to Admin → Loan Requests");
                    actions.Add("Review pending loans");
                }
                else
                {
                    actions.Add("Go to Admin Dashboard");
                    actions.Add("Access admin features from sidebar");
                }
            }

            return actions;
        }
    }

    public class AIHelpRequest
    {
        public string Question { get; set; } = string.Empty;
    }

    public class AIHelpResponse
    {
        public string Answer { get; set; } = string.Empty;
        public List<string> SuggestedActions { get; set; } = new();
    }
}

