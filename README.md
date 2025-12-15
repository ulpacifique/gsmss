# Community Finance Management System (CFMS)

## 📋 Project Overview

The Community Finance Management System is a comprehensive web application designed to help communities manage their financial activities, including savings goals, contributions, loans, and member interactions. The system provides separate interfaces for administrators and members, enabling efficient financial management and transparency within a community.

## 🎯 Objective

The primary objective of this project is to create a robust, user-friendly platform that allows:

- **Community Members** to:
  - Create and track personal savings goals
  - Make contributions towards goals
  - Request loans from the community fund
  - View their financial account status
  - Communicate with administrators
  - Manage their profile and profile picture

- **Administrators** to:
  - Manage community members
  - Create and manage savings goals
  - Review and approve/reject contributions
  - Review and approve/reject loan requests
  - Generate comprehensive reports with export functionality
  - Monitor community financial health
  - Communicate with members

## ✨ Complete Functionality List

### Member Features

#### 1. Dashboard
- **Personal Account Overview**
  - View total account balance (sum of all approved contributions)
  - Track active goals and their progress percentages
  - View contribution statistics (total contributed, current month contributions)
  - See active loans and their status
  - View overdue loans with warnings

- **Loan Management**
  - Request loans from community fund (up to 12.5% of total fund balance)
  - View loan eligibility and maximum loan amount
  - Make loan payments with payment reference tracking
  - View loan history with status (Pending/Approved/Rejected)
  - Track loan due dates and remaining amounts
  - View interest rates and total amounts

- **Quick Actions**
  - Quick access to create contributions
  - Quick access to view goals
  - Quick access to request loans

#### 2. Goals Management
- **View All Goals**
  - Browse all available savings goals
  - See goal details (name, description, target amount, dates)
  - View goal status (Active/Completed/Cancelled)
  - See community progress towards goals
  - View goal start and end dates

- **Join Goals**
  - Join any active goal
  - Set personal target amount for each goal
  - Track personal progress vs. community progress
  - View personal contribution history per goal

- **Goal Details**
  - View detailed goal information
  - See all members participating
  - View contribution timeline
  - Track progress with visual indicators

#### 3. Contributions
- **Create Contributions**
  - Select a goal from active goals dropdown
  - Enter contribution amount
  - Add payment reference (transaction ID, receipt number, etc.)
  - Submit for admin approval
  - Automatic goal membership if not already a member

- **View Contributions**
  - View all personal contributions
  - Filter by status (Pending/Approved/Rejected)
  - See contribution details (goal, amount, date, status)
  - View rejection reasons if applicable
  - Track contribution history

- **Contribution Status**
  - Pending: Awaiting admin approval
  - Approved: Contribution added to goal
  - Rejected: Contribution rejected with reason

#### 4. Loan Management
- **Request Loans**
  - Calculate maximum loan amount (12.5% of community fund)
  - Enter loan amount and purpose
  - Submit loan request
  - View loan eligibility requirements
  - Check if user has outstanding loans

- **Loan Payments**
  - View active loans
  - Make partial or full payments
  - Add payment reference
  - Track payment history
  - View remaining balance

- **Loan Tracking**
  - View all loan requests and their status
  - See loan details (amount, interest, due date)
  - Track overdue loans
  - View payment history

#### 5. Profile Management
- **Personal Information**
  - Update first name and last name
  - Update email address (with uniqueness validation)
  - Update phone number
  - View account creation date
  - View role (Member/Admin)

- **Profile Picture**
  - Upload profile picture (base64 encoded, max 2MB)
  - Preview before uploading
  - View profile picture in header
  - Automatic fallback to initials if no picture

#### 6. Messaging System
- **Conversations**
  - View all conversations with other users
  - Start new conversations with any user
  - Search users to start conversations
  - See unread message counts
  - View last message preview

- **Messages**
  - Send and receive messages in real-time
  - View message history
  - See message timestamps
  - Mark messages as read
  - View sender/receiver profile pictures

### Administrator Features

#### 1. Admin Dashboard
- **Community Statistics**
  - Total goals count
  - Total contributions count
  - Total members count
  - Pending contributions count
  - Quick action buttons for common tasks

- **Quick Actions**
  - Create new goal
  - Manage members
  - Review contributions
  - Manage loan requests
  - View reports

#### 2. Member Management
- **View All Members**
  - List all registered members
  - View member details (name, email, phone, role)
  - See member status (Active/Inactive)
  - View member profile pictures
  - See member registration date

- **Member Actions**
  - Activate inactive members
  - Deactivate active members
  - Delete members (hard delete with confirmation)
  - Update member information
  - View member statistics

- **Member Search & Filter**
  - Search members by name or email
  - Filter by status
  - Sort by various criteria

#### 3. Goal Management
- **Create Goals**
  - Set goal name and description
  - Set target amount
  - Set start and end dates
  - Set goal status
  - View created goals

- **Edit Goals**
  - Update goal details
  - Modify target amounts
  - Extend or modify dates
  - Change goal status
  - View goal progress

- **Goal Overview**
  - View all goals and their status
  - See goal progress percentages
  - View goal members
  - Track goal contributions

#### 4. Contribution Review
- **Pending Contributions**
  - View all pending contributions
  - See contribution details (member, goal, amount, date)
  - View payment references
  - Quick approve/reject actions

- **Contribution Actions**
  - Approve contributions (adds to goal balance)
  - Reject contributions (with optional reason)
  - View contribution history
  - Filter by status

- **Contribution Statistics**
  - Total contributions
  - Approved contributions
  - Rejected contributions
  - Pending contributions

#### 5. Loan Management
- **Loan Requests**
  - View all loan requests
  - Filter by status (Pending/Approved/Rejected)
  - See loan details (member, amount, purpose, date)
  - View member account balance
  - Check community fund availability

- **Loan Actions**
  - Approve loan requests (with fund verification)
  - Reject loan requests (with reason)
  - View all loans
  - Track active loans
  - Monitor overdue loans

- **Loan Monitoring**
  - View loan statistics
  - Track outstanding loans
  - Monitor loan payments
  - View loan history

#### 6. General Reports
- **Comprehensive Member Report**
  - View all members with detailed statistics
  - See member contributions summary
  - View member goals participation
  - Track member loans
  - Identify top contributors (favorites)

- **Report Features**
  - **Search & Filter**
    - Search by name, email, or phone
    - Filter by various criteria
    - Real-time search results
  
  - **Sorting Options**
    - Sort by total contributed (high to low)
    - Sort by contribution count (high to low)
    - Sort by goals joined (high to low)
    - Sort by name (A to Z)

  - **Summary Statistics**
    - Total members count
    - Total contributed amount
    - Total contributions count
    - Top contributors count

  - **Member Details**
    - Expandable member cards
    - View all contributions per member
    - View all goals per member
    - View all loans per member
    - See member profile pictures

  - **Export Functionality**
    - Export report to CSV
    - Includes all member data
    - Formatted for Excel compatibility
    - Timestamped file names

- **Report Display**
  - Professional card-based layout
  - Responsive design for all screen sizes
  - Color-coded status badges
  - Visual progress indicators
  - Expandable detail sections

#### 7. Messaging System
- **Member Communication**
  - View all conversations
  - Start conversations with any member
  - Send messages to members
  - Receive messages from members
  - Manage multiple conversations

## 🛠️ Technology Stack

### Backend
- **Framework**: ASP.NET Core Web API (.NET 10.0)
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **Authentication**: Custom Simple Authentication Middleware (Header-based)
- **Password Hashing**: BCrypt.Net
- **Architecture**: 
  - Repository Pattern
  - Service Layer Pattern
  - DTOs (Data Transfer Objects)
  - Dependency Injection

### Frontend
- **Framework**: React 18 with TypeScript
- **Build Tool**: Vite
- **State Management**: 
  - React Query (TanStack Query) for server state
  - React Context API for authentication state
- **Routing**: React Router DOM
- **HTTP Client**: Axios with interceptors
- **Styling**: Tailwind CSS with custom design system
- **Icons**: Heroicons

### Development Tools
- **Package Manager**: npm (frontend), NuGet (backend)
- **Version Control**: Git
- **Database Migrations**: Entity Framework Core Migrations

## 📁 Project Structure

```
CommunityFinanceAPI/
├── CommunityFinanceAPI/          # Backend API
│   ├── Controllers/              # API Controllers
│   │   ├── AdminController.cs   # Admin endpoints
│   │   ├── AuthController.cs    # Authentication
│   │   ├── GoalsController.cs   # Goals management
│   │   ├── LoansController.cs   # Loan management
│   │   ├── MembersController.cs # Member endpoints
│   │   ├── MessagesController.cs # Messaging
│   │   └── ReportsController.cs # Reports
│   ├── Services/                 # Business Logic Layer
│   │   ├── Interfaces/          # Service Interfaces
│   │   └── Implementations/      # Service Implementations
│   ├── Models/                   # Data Models
│   │   ├── Entities/            # Database Entities
│   │   └── DTOs/                 # Data Transfer Objects
│   ├── Data/                     # Database Context
│   ├── Middleware/               # Custom Middleware
│   │   └── SimpleAuthMiddleware.cs
│   ├── Migrations/                # Database Migrations
│   └── Program.cs                # Application Entry Point
│
└── frontend/                     # Frontend React Application
    ├── src/
    │   ├── api/                  # API Client Functions
    │   ├── components/           # Reusable Components
    │   │   ├── Layout.tsx       # Main layout with navigation
    │   │   └── Notifications.tsx # Notification component
    │   ├── pages/                # Page Components
    │   │   ├── admin/            # Admin Pages
    │   │   │   ├── AdminDashboard.tsx
    │   │   │   ├── AdminUsers.tsx
    │   │   │   ├── AdminGoals.tsx
    │   │   │   ├── AdminContributions.tsx
    │   │   │   ├── AdminLoans.tsx
    │   │   │   └── AdminGeneralReport.tsx
    │   │   ├── Dashboard.tsx
    │   │   ├── Goals.tsx
    │   │   ├── Contributions.tsx
    │   │   ├── Profile.tsx
    │   │   └── Chat.tsx
    │   ├── routes/               # Route Configuration
    │   ├── state/                # State Management
    │   │   └── AuthContext.tsx
    │   ├── types/                # TypeScript Types
    │   └── App.tsx               # Main App Component
    └── package.json              # Frontend Dependencies
```

## 🚀 Getting Started

### Prerequisites
- .NET 10.0 SDK
- Node.js (v18 or higher)
- SQL Server (2019 or higher)
- npm or yarn

### Backend Setup

1. **Navigate to the backend directory:**
   ```powershell
   cd CommunityFinanceAPI\CommunityFinanceAPI
   ```

2. **Update the database connection string** in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=YOUR_SERVER;Initial Catalog=CommunityFinanceDB;Integrated Security=True;Trust Server Certificate=True"
     }
   }
   ```

3. **Restore packages:**
   ```powershell
   dotnet restore
   ```

4. **Apply database migrations:**
   ```powershell
   dotnet ef database update --context ApplicationDbContext
   ```

5. **Run the backend:**
   ```powershell
   dotnet run
   ```
   The API will be available at `http://localhost:5154`

### Frontend Setup

1. **Navigate to the frontend directory:**
   ```powershell
   cd frontend
   ```

2. **Install dependencies:**
   ```powershell
   npm install
   ```

3. **Update API base URL** in `src/api/client.ts` if needed:
   ```typescript
   const API_BASE_URL = "http://localhost:5154";
   ```

4. **Run the development server:**
   ```powershell
   npm run dev
   ```
   The frontend will be available at `http://localhost:5173`

## 🔐 Authentication

The system uses a custom header-based authentication mechanism:

- **Headers Required:**
  - `X-User-Email`: User's email address
  - `X-User-Password`: User's password (plain text, sent over HTTPS in production)

- **Public Endpoints:**
  - `/api/auth/register`
  - `/api/auth/login`

- **Protected Endpoints:**
  - All other endpoints require authentication headers

- **Authentication Flow:**
  1. User registers/logs in
  2. Credentials stored in localStorage
  3. Axios interceptor adds headers to all requests
  4. Middleware validates credentials on each request
  5. User object stored in HttpContext.Items

## 📊 Database Schema

### Key Entities

- **Users**: Community members and administrators
  - UserId, Email, PasswordHash, FirstName, LastName, PhoneNumber, ProfilePictureUrl, Role, IsActive, CreatedAt, UpdatedAt

- **SavingsGoals**: Savings goals created by admins
  - GoalId, GoalName, Description, TargetAmount, CurrentAmount, StartDate, EndDate, Status, CreatedBy, CreatedAt, UpdatedAt

- **MemberGoals**: Member participation in goals
  - MemberGoalId, UserId, GoalId, PersonalTarget, CurrentAmount, JoinedAt

- **Contributions**: Member contributions towards goals
  - ContributionId, UserId, GoalId, Amount, PaymentReference, Status, ReviewedBy, ReviewedAt, RejectionReason, CreatedAt, UpdatedAt

- **Loans**: Loan requests and records
  - LoanId, UserId, PrincipalAmount, InterestRate, TotalAmount, RemainingAmount, RequestedDate, DueDate, PaidAmount, Status, ApprovedBy, ApprovedAt, RejectionReason, CreatedAt, UpdatedAt

- **Notifications**: System notifications
  - NotificationId, UserId, Title, Message, Type, IsRead, CreatedAt

- **Messages**: Communication between users
  - MessageId, SenderId, ReceiverId, Content, IsRead, CreatedAt

- **Groups**: User groups (for future features)
- **RecurringContributions**: Scheduled contributions (for future features)

## 🎨 UI/UX Features

### Design System
- **Color Scheme**: Blue/Indigo gradient theme
- **Typography**: Clean, readable fonts with proper hierarchy
- **Spacing**: Consistent spacing using Tailwind utilities
- **Shadows**: Subtle shadows for depth
- **Borders**: Rounded corners for modern look
- **Responsive**: Mobile-first design approach

### Components
- **Cards**: Elevated cards with shadows and rounded corners
- **Buttons**: Gradient buttons with hover effects
- **Inputs**: Styled inputs with focus states
- **Badges**: Color-coded status badges
- **Navigation**: Dropdown menus for admin functions
- **Tables**: Responsive tables with hover effects

### Features
- **Profile Pictures**: Support for user profile images with fallback to initials
- **Real-time Updates**: Automatic data refresh using React Query
- **Loading States**: Skeleton loaders and spinners
- **Error Handling**: User-friendly error messages with retry options
- **Search & Filter**: Real-time search and sorting
- **Export**: CSV export functionality
- **Responsive Design**: Works on desktop, tablet, and mobile

## 🔧 Key Features Implementation

### Loan System
- Maximum loan amount: 12.5% of community fund balance
- Interest rate: 5% default
- Admin approval required
- Automatic overdue tracking
- Payment tracking with references

### Contribution System
- Members can contribute to any active goal
- Admin approval required
- Automatic goal membership when contributing
- Payment reference tracking
- Status tracking (Pending/Approved/Rejected)

### Goal System
- Admins create goals with target amounts and dates
- Members can join goals and set personal targets
- Progress tracking for both community and personal goals
- Status management (Active/Completed/Cancelled)

### Report System
- Comprehensive member statistics
- Search and filter functionality
- Multiple sorting options
- Export to CSV
- Expandable member details
- Summary statistics cards

## 📝 Complete API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user

### Members
- `GET /api/members/profile` - Get user profile
- `PUT /api/members/profile` - Update profile
- `GET /api/members/goals` - Get user's goals
- `POST /api/members/contributions` - Create contribution
- `GET /api/members/contribution-stats` - Get contribution statistics

### Goals
- `GET /api/goals` - Get all goals
- `GET /api/goals/{id}` - Get goal by ID
- `POST /api/goals` - Create goal (Admin)
- `PUT /api/goals/{id}` - Update goal (Admin)
- `POST /api/goals/{id}/join` - Join a goal

### Loans
- `POST /api/loans/request` - Request a loan
- `GET /api/loans/my-account` - Get account details
- `GET /api/loans/my-loans` - Get user's loans
- `POST /api/loans/{id}/pay` - Make loan payment

### Admin
- `GET /api/admin/users` - Get all users
- `GET /api/admin/users/members` - Get all members
- `GET /api/admin/users/{id}` - Get user by ID
- `PUT /api/admin/users/{id}` - Update user
- `DELETE /api/admin/users/{id}` - Delete user
- `PUT /api/admin/users/{id}/activate` - Activate user
- `PUT /api/admin/users/{id}/deactivate` - Deactivate user
- `GET /api/admin/dashboard/stats` - Get dashboard statistics
- `GET /api/admin/goals` - Get all goals
- `GET /api/admin/contributions` - Get all contributions
- `GET /api/admin/contributions/pending` - Get pending contributions
- `PUT /api/admin/contributions/{id}/status` - Update contribution status
- `GET /api/admin/loans` - Get all loans
- `GET /api/admin/loans/pending` - Get pending loans
- `PUT /api/admin/loans/{id}/approve` - Approve loan
- `PUT /api/admin/loans/{id}/reject` - Reject loan

### Messages
- `GET /api/messages/conversations` - Get all conversations
- `GET /api/messages/conversation/{userId}` - Get conversation with user
- `POST /api/messages` - Send message
- `PUT /api/messages/mark-read/{senderId}/{receiverId}` - Mark messages as read

### Notifications
- `GET /api/notifications/{userId}` - Get user notifications
- `PUT /api/notifications/{id}/read` - Mark notification as read
- `PUT /api/notifications/{userId}/read-all` - Mark all as read

## 🐛 Troubleshooting

### Backend Issues

1. **Port already in use:**
   ```powershell
   netstat -ano | findstr :5154
   taskkill /F /PID <PID_NUMBER>
   ```

2. **Database migration errors:**
   - Ensure SQL Server is running
   - Check connection string in `appsettings.json`
   - Verify database exists
   - Run: `dotnet ef database update --context ApplicationDbContext`

3. **Profile picture upload fails:**
   - Ensure migration `IncreaseProfilePictureUrlSize` is applied
   - Check that `ProfilePictureUrl` column is `nvarchar(MAX)`
   - Verify file size is under 2MB

4. **Authentication errors:**
   - Check that credentials are being sent in headers
   - Verify user exists in database
   - Check password hash format

### Frontend Issues

1. **API connection errors:**
   - Verify backend is running on port 5154
   - Check CORS settings in backend
   - Verify API base URL in `src/api/client.ts`
   - Check browser console for errors

2. **Build errors:**
   - Delete `node_modules` and `package-lock.json`
   - Run `npm install` again
   - Clear browser cache

3. **Authentication issues:**
   - Clear localStorage
   - Log out and log back in
   - Check that credentials are being stored

4. **Report loading errors:**
   - Click "Retry" button
   - Check that you're logged in as admin
   - Verify backend is running

## 🔮 Future Enhancements

- Automated SMS/Email notifications
- Group-based contributions
- Recurring contribution scheduling
- Advanced data visualization (charts and graphs)
- PDF export for reports
- File upload for documents
- Multi-currency support
- Mobile app version
- Two-factor authentication
- Audit logs
- Advanced reporting with date ranges
- Bulk operations for admins

## 📄 License

This project is proprietary software developed for community financial management.

## 👨‍💻 Development Notes

- The system uses a custom authentication middleware instead of JWT tokens for simplicity
- Profile pictures are stored as base64 strings in the database (consider moving to file storage in production)
- All monetary values are stored as `decimal(18,2)` for precision
- The system supports soft deletes for users (IsActive flag)
- Money values are displayed with proper truncation to prevent overflow
- Sorting and filtering work on client-side for better performance
- CSV export includes all visible data in the report

## 📞 Support

For issues or questions, please contact the development team or refer to the project documentation.

---

**Version**: 1.0.0  
**Last Updated**: December 2025  
**Technology**: ASP.NET Core Web API + React + TypeScript
