# Group Savings Management System (GSMS)

## Project Overview
The Group Savings Management System (GSMS) is a web-based platform designed to help savings groups manage contributions, track progress toward financial goals, and maintain transparent financial records. This system eliminates manual record-keeping errors and provides real-time visibility into group savings progress.

## Problem Statement
Many savings groups struggle with manual record-keeping, unclear processes, and lack of transparency. Members make contributions toward specific group goals—such as emergency funds, investment projects, or community support—but tracking these payments and progress toward goals is inefficient without a digital system. This results in errors, mistrust, delays, and difficulty determining how close the group is to reaching its goals.

## Key Features

### Authentication & Authorization
- Secure member and admin login systems
- Role-based access control

### Member Features
- View group savings goals and progress
- Submit contributions with payment references
- Track contribution status (pending/approved/rejected)
- Access personal contribution reports
- User-friendly dashboard

### Admin Features
- Create and manage group savings goals
- Approve or reject member contributions
- Manage member accounts
- Generate financial reports
- Comprehensive admin dashboard

### Financial Management
- Real-time goal progress tracking
- Contribution history and audit trails
- Financial reporting capabilities
- Target amount and deadline monitoring

## Technology Stack
- **Backend**: ASP.NET Core
- **Frontend**: Razor Pages / MVC
- **Database**: SQL Server
- **Authentication**: ASP.NET Core Identity
- **Architecture**: MVC Pattern

## Database Schema

### Core Tables
- **Users** (UserID, Username, Password, Role)
- **Members** (MemberID, UserID, FullName, Phone, JoinDate)
- **Goals** (GoalID, Name, TargetAmount, DueDate)
- **Contributions** (ContributionID, MemberID, GoalID, Amount, PaymentReference, Status, DateSubmitted, DateApproved)

## Getting Started

### Prerequisites
- .NET 6.0 or later
- SQL Server
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/group-savings-management-system.git
   cd group-savings-management-system
Database Setup

Update connection string in appsettings.json

Run database migrations

bash
dotnet ef database update
Run the application

bash
dotnet run
Access the application

Navigate to https://localhost:7000

Use default admin credentials (to be set in initial migration)

Contributing
Please read CONTRIBUTING.md for details on our code of conduct and the process for submitting pull requests.

License
This project is licensed under the MIT License - see the LICENSE.md file for details.

text

**To use this:**
1. Copy the entire text above
2. Create/edit your `README.md` file in your project
3. Paste this content
4. Commit and push to GitHub

This will display with proper headings, bullet points, code formatting, and section organization on GitHub. The markdown formatting will make it look professional and easy to read.
# Community Savings Group Financial Management System

A .NET web application for transparent financial management solutions for community savings groups, enabling members to track contributions and administrators to manage group finances efficiently.

## Features

### Member Functions
- **Member login** - Secure access to personal dashboard
- **View goal(s) and progress** - Track savings goals and achievement status
- **Submit contributions with payment reference** - Make contributions with transaction tracking
- **View contribution status** - Check approval status of submissions
- **View contribution reports** - Access personal contribution history and summaries

### Admin Functions
- **Admin login** - Secure administrative access
- **Create/update savings goals** - Manage group savings targets
- **Approve/reject contributions** - Review and validate member contributions
- **Manage members** - Handle member accounts and permissions
- **Generate financial reports** - Create comprehensive financial summaries

## Non-Functional Requirements
- **Secure authentication and authorization** - Robust security measures
- **Simple, intuitive web interface** - User-friendly design
- **Fast load times and responsive design** - Optimized performance
- **Reliable SQL Server database integration** - Stable data management
- **Data consistency and validation** - Accurate and reliable information
- **Mobile-friendly design** - Accessible on all devices

## Use Cases

### Member Use Cases
1. Login to system
2. Browse available savings goals
3. Submit financial contributions
4. Monitor contribution approval status
5. View personal contribution history

### Admin Use Cases
1. System authentication
2. Create and modify savings goals
3. Review and process contributions
4. Member account management
5. Generate comprehensive reports

## Technology Stack
- **Backend**: .NET Framework/ASP.NET
- **Database**: SQL Server
- **Frontend**: HTML, CSS, JavaScript
- **Authentication**: ASP.NET Identity

## Installation & Setup
*(Add your specific installation instructions here)*

1. Clone the repository
2. Restore NuGet packages
3. Update database connection string in `Web.config`
4. Run database migrations
5. Build and run the solution

## Contributing

We welcome contributions! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## Development Team

| Name | Student ID | Role |
|------|------------|------|
| ISHIMWE Gwiza Ruth | 26082 | Team Member |
| Rubayiza David | 26439 | Team Member |
| Hirwa Willy | 25308 | Team Member |
| Fatime Dadi Wardougou | 25858 | Team Member |
| UWIHIRWE Pacifique Lazaro | 25443 | Team Member |

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## Support

For support and questions, please contact the development team or create an issue in the repository.

---

**Developed as a .NET Final Project**  
Building transparent financial management solutions for community savings groups
