# Project Structure Explanation

This document explains the architecture and structure of the Community Finance API project, including what each folder and file does.

---

## 🏗️ Backend Structure (ASP.NET Core Web API)

### 📁 **CommunityFinanceAPI/** (Main Backend Project)

#### 📂 **Controllers/**
**Purpose**: Contains API controllers that handle HTTP requests and responses. Controllers are the entry points for API endpoints.

**Files**:
- `AdminController.cs` - Handles admin-specific operations (user management, loan approvals, reports)
- `AuthController.cs` - Handles authentication (login, register)
- `ContributionsController.cs` - Manages contribution-related endpoints (create, approve, reject contributions)
- `GoalsController.cs` - Manages savings goals (create, update, get goals)
- `LoansController.cs` - Handles loan operations (request loan, pay loan, approve/reject loans)
- `MembersController.cs` - Member-specific endpoints (profile, contributions, goals)
- `MessagesController.cs` - Handles messaging between users
- `NotificationsController.cs` - Manages user notifications
- `ReportsController.cs` - Generates reports and analytics

**What Controllers Do**:
- Receive HTTP requests (GET, POST, PUT, DELETE)
- Validate request data
- Call service methods to perform business logic
- Return HTTP responses (JSON, status codes)
- Handle authentication/authorization checks

---

#### 📂 **Services/**
**Purpose**: Contains business logic layer. Services contain the core application logic, separate from controllers.

**Structure**:
- **Interfaces/** - Defines contracts (interfaces) that services must implement
  - `IAuthService.cs` - Authentication service interface
  - `IContributionService.cs` - Contribution service interface
  - `IGoalService.cs` - Goal service interface
  - `ILoanService.cs` - Loan service interface
  - `IMessageService.cs` - Message service interface
  - `INotificationService.cs` - Notification service interface
  - `IReportService.cs` - Report service interface
  - `IUserService.cs` - User service interface

- **Implementations/** - Contains actual service implementations
  - `AuthService.cs` - Handles user registration, login, password hashing
  - `ContributionService.cs` - Business logic for contributions (validation, approval workflow)
  - `GoalService.cs` - Logic for managing savings goals
  - `LoanService.cs` - Loan calculations, approvals, payments
  - `MessageService.cs` - Message sending, conversation management
  - `NotificationService.cs` - Creating and managing notifications
  - `ReportService.cs` - Generating reports and statistics
  - `UserService.cs` - User profile management

**What Services Do**:
- Contain business logic (calculations, validations, workflows)
- Interact with database through DbContext
- Transform entities to DTOs (Data Transfer Objects)
- Handle complex operations that span multiple entities
- Are injected into controllers via dependency injection

---

#### 📂 **Models/**
**Purpose**: Contains data models that represent the structure of data in the application.

**Structure**:
- **Entities/** - Database entities (tables in SQL Server)
  - `User.cs` - Represents a user/member in the database
  - `SavingsGoal.cs` - Represents a savings goal
  - `Contribution.cs` - Represents a contribution made by a member
  - `Loan.cs` - Represents a loan request/payment
  - `MemberGoal.cs` - Represents a member's participation in a goal
  - `Message.cs` - Represents a message between users
  - `Notification.cs` - Represents a notification
  - `Group.cs` - Represents a group (for future group contributions)
  - `RecurringContribution.cs` - Represents recurring contributions (for future use)
  - `BaseEntity.cs` - Base class with common properties (CreatedAt, UpdatedAt)

- **DTOs/** - Data Transfer Objects (data structures for API requests/responses)
  - `AuthDTOs.cs` - Login/Register request/response models
  - `UserDTOs.cs` - User-related DTOs
  - `GoalDTOs.cs` - Goal-related DTOs
  - `ContributionDTOs.cs` - Contribution-related DTOs
  - `LoanDTOs.cs` - Loan-related DTOs
  - `MessageDTOs.cs` - Message-related DTOs
  - `NotificationDTOs.cs` - Notification-related DTOs
  - `ReportDTOs.cs` - Report-related DTOs
  - `MemberGoalDTOs.cs` - Member goal participation DTOs

**What Models Do**:
- **Entities**: Define database table structure, relationships, constraints
- **DTOs**: Define what data is sent/received via API (simpler, safer than exposing entities directly)

---

#### 📂 **Data/**
**Purpose**: Contains database context and database-related configuration.

**Files**:
- `AppDbContext.cs` - Entity Framework Core DbContext
  - Represents the database connection
  - Defines `DbSet<T>` properties for each entity (tables)
  - Configures entity relationships, indexes, constraints
  - Handles database migrations

**What Data Folder Does**:
- Manages database connection
- Configures Entity Framework Core
- Defines how entities map to database tables
- Handles database queries through LINQ

---

#### 📂 **Migrations/**
**Purpose**: Contains Entity Framework Core database migrations - scripts that create/modify database schema.

**What Migrations Do**:
- Track database schema changes over time
- Create/update/drop database tables, columns, indexes
- Maintain database version history
- Allow rolling back database changes

**Files**:
- `20251213140242_InitialCreate.cs` - Initial database schema creation
- `20251213233154_AddNotificationsMessagesGroups.cs` - Added notifications, messages, groups tables
- `20251214003144_AddProfilePictureUrlToUsers.cs` - Added profile picture column to users
- `20251214013457_IncreaseProfilePictureUrlSize.cs` - Increased profile picture column size
- `ApplicationDbContextModelSnapshot.cs` - Current state of database model

**How Migrations Work**:
1. Developer changes entity models
2. Run `dotnet ef migrations add MigrationName`
3. EF Core generates migration file with SQL commands
4. Run `dotnet ef database update` to apply changes to database

---

#### 📂 **Middleware/**
**Purpose**: Contains custom middleware that processes HTTP requests/responses in the pipeline.

**Files**:
- `SimpleAuthMiddleware.cs` - Custom authentication middleware
  - Intercepts all HTTP requests
  - Validates `X-User-Email` and `X-User-Password` headers
  - Authenticates users by checking credentials against database
  - Stores authenticated user in `HttpContext.Items` for controllers to use
  - Allows public endpoints (login, register) to bypass authentication

- `ExceptionHandlingMiddleware.cs` - Global exception handler
  - Catches unhandled exceptions
  - Returns user-friendly error messages
  - Logs errors for debugging

**What Middleware Does**:
- Executes before controllers
- Can modify requests/responses
- Can short-circuit request pipeline (e.g., return 401 if not authenticated)
- Runs in order defined in `Program.cs`

---

#### 📂 **Repositories/**
**Purpose**: (Currently empty but structure exists) Would contain repository pattern implementation.

**What Repositories Would Do** (if implemented):
- Abstract database access layer
- Provide a clean interface for data operations
- Separate data access logic from business logic
- Make code more testable

**Current State**: The project uses services directly with DbContext, but repository pattern could be added here.

---

#### 📂 **Utilities/**
**Purpose**: Contains utility classes and helper functions.

**Files**:
- `EmailService.cs` - Email sending functionality (for future use)
- `JwtHelper.cs` - JWT token helper (for future JWT authentication)

**What Utilities Do**:
- Provide reusable helper functions
- Encapsulate common operations
- Keep code DRY (Don't Repeat Yourself)

---

#### 📂 **Properties/**
**Purpose**: Contains project configuration files.

**Files**:
- `launchSettings.json` - Defines how the application runs (ports, URLs, environment)

---

#### 📄 **Program.cs**
**Purpose**: Application entry point and configuration.

**What Program.cs Does**:
- Configures services (dependency injection)
- Sets up database connection
- Configures middleware pipeline
- Applies database migrations on startup
- Configures CORS (Cross-Origin Resource Sharing)
- Sets up Swagger/OpenAPI documentation
- Starts the web server

---

#### 📄 **appsettings.json**
**Purpose**: Application configuration file.

**Contains**:
- Database connection strings
- Logging configuration
- Application settings

---

---

## 🎨 Frontend Structure (React + TypeScript)

### 📁 **frontend/** (React Application)

#### 📂 **src/** (Source Code)

##### 📂 **api/**
**Purpose**: Contains API client functions that communicate with the backend.

**Files**:
- `client.ts` - Axios instance configuration
  - Sets base URL
  - Adds authentication headers (`X-User-Email`, `X-User-Password`)
  - Handles request/response interceptors
  - Manages error handling

- `auth.ts` - Authentication API calls (login, register)
- `admin.ts` - Admin API calls (user management, reports)
- `contributions.ts` - Contribution API calls
- `goals.ts` - Goal API calls
- `loans.ts` - Loan API calls
- `members.ts` - Member API calls (profile, contributions)
- `messages.ts` - Message API calls
- `reports.ts` - Report API calls

**What API Folder Does**:
- Abstracts HTTP requests to backend
- Provides typed functions for API calls
- Handles authentication headers automatically
- Centralizes API endpoint URLs

---

##### 📂 **components/**
**Purpose**: Contains reusable React components.

**Files**:
- `Layout.tsx` - Main application layout
  - Contains navigation sidebar
  - Shows header with user info
  - Wraps all pages
  - Handles routing structure

- `Notifications.tsx` - Notification dropdown component
  - Displays user notifications
  - Shows unread count
  - Allows marking notifications as read

**What Components Do**:
- Reusable UI elements
- Encapsulate UI logic
- Can be used across multiple pages
- Follow React component patterns

---

##### 📂 **pages/**
**Purpose**: Contains page components - full pages that users see.

**Structure**:
- **admin/** - Admin-only pages
  - `AdminDashboard.tsx` - Admin overview dashboard
  - `AdminUsers.tsx` - User management page
  - `AdminGoals.tsx` - Goal management page
  - `AdminContributions.tsx` - Contribution management page
  - `AdminLoans.tsx` - Loan approval page
  - `AdminGeneralReport.tsx` - Comprehensive reports page
  - `AdminReports.tsx` - Reports page (alternative)

- **Root pages**:
  - `Dashboard.tsx` - Member dashboard (goals, loans, account)
  - `Goals.tsx` - Browse and join goals
  - `GoalDetail.tsx` - View goal details
  - `Contributions.tsx` - Create contributions
  - `Profile.tsx` - User profile management
  - `Chat.tsx` - Messaging interface
  - `Login.tsx` - Login page
  - `Register.tsx` - Registration page

**What Pages Do**:
- Represent full screens/pages
- Use components and API functions
- Handle user interactions
- Display data from backend

---

##### 📂 **state/**
**Purpose**: Contains state management code.

**Files**:
- `AuthContext.tsx` - Authentication context
  - Manages user authentication state
  - Stores user credentials in localStorage
  - Provides `user`, `isAuthed`, `login`, `logout` to entire app
  - Uses React Context API

**What State Folder Does**:
- Manages global application state
- Provides state to components via Context
- Handles authentication state
- Could be extended for other global state

---

##### 📂 **routes/**
**Purpose**: Contains routing configuration and route protection.

**Files**:
- `ProtectedRoute.tsx` - Route protection component
  - Checks if user is authenticated
  - Redirects to login if not authenticated
  - Wraps protected routes

**What Routes Folder Does**:
- Defines route protection logic
- Handles authentication checks for routes
- Could contain route configuration

---

##### 📂 **types/**
**Purpose**: Contains TypeScript type definitions.

**Files**:
- `api.ts` - TypeScript interfaces/types for API data
  - `UserResponse` - User data structure
  - `GoalResponse` - Goal data structure
  - `LoanResponse` - Loan data structure
  - `ContributionResponse` - Contribution data structure
  - And many more...

**What Types Do**:
- Provides type safety
- Auto-completion in IDE
- Prevents type errors
- Documents data structures

---

##### 📂 **App.tsx**
**Purpose**: Main application component - root of React app.

**What App.tsx Does**:
- Defines all routes
- Sets up React Router
- Wraps app with providers (QueryClient, AuthContext)
- Renders appropriate page based on URL

---

##### 📂 **main.tsx**
**Purpose**: Application entry point.

**What main.tsx Does**:
- Renders React app to DOM
- Sets up React Query client
- Initializes React Router
- Mounts App component

---

##### 📂 **index.css**
**Purpose**: Global CSS styles.

**What index.css Does**:
- Contains Tailwind CSS imports
- Defines custom CSS classes
- Sets global styles (body, fonts, etc.)
- Defines design system (buttons, cards, etc.)

---

#### 📂 **public/**
**Purpose**: Contains static assets served directly.

**Files**:
- `vite.svg` - Vite logo (example static file)

**What Public Does**:
- Files here are served at root URL
- Not processed by build tools
- Accessible via direct URL

---

#### 📄 **package.json**
**Purpose**: Frontend dependencies and scripts.

**Contains**:
- List of npm packages (React, TypeScript, Tailwind, etc.)
- Scripts (dev, build, preview)
- Project metadata

---

#### 📄 **vite.config.js** (if exists)
**Purpose**: Vite build tool configuration.

**What It Does**:
- Configures Vite bundler
- Sets up development server
- Configures build process

---

#### 📄 **tailwind.config.js**
**Purpose**: Tailwind CSS configuration.

**What It Does**:
- Configures Tailwind CSS
- Defines custom colors, fonts, spacing
- Sets up design system

---

#### 📄 **tsconfig.json**
**Purpose**: TypeScript compiler configuration.

**What It Does**:
- Configures TypeScript compiler
- Sets compilation options
- Defines file includes/excludes

---

## 🔄 How Backend and Frontend Work Together

1. **User Action**: User clicks button in React frontend
2. **API Call**: Frontend calls function in `src/api/` folder
3. **HTTP Request**: Axios sends HTTP request to backend (with auth headers)
4. **Middleware**: `SimpleAuthMiddleware` validates credentials
5. **Controller**: Request reaches appropriate controller in `Controllers/`
6. **Service**: Controller calls service method from `Services/Implementations/`
7. **Database**: Service uses `AppDbContext` to query database
8. **Response**: Data flows back: Database → Service → Controller → HTTP Response
9. **Frontend**: React receives response, updates UI with React Query

---

## 📊 Data Flow Example: Creating a Contribution

1. **Frontend**: User fills form in `pages/Contributions.tsx`
2. **API Call**: Calls `createContribution()` from `api/contributions.ts`
3. **HTTP POST**: Sends POST request to `/api/contributions`
4. **Middleware**: `SimpleAuthMiddleware` authenticates user
5. **Controller**: `ContributionsController.CreateContribution()` receives request
6. **Service**: Controller calls `ContributionService.CreateContributionAsync()`
7. **Validation**: Service validates data, checks business rules
8. **Database**: Service uses `AppDbContext.Contributions.Add()` to save
9. **Response**: Service returns DTO, Controller returns HTTP 200 OK
10. **Frontend**: React Query updates cache, UI refreshes automatically

---

## 🎯 Key Concepts

### **Separation of Concerns**
- **Controllers**: Handle HTTP, don't contain business logic
- **Services**: Contain business logic, don't know about HTTP
- **Models**: Define data structure, don't contain logic
- **Frontend Pages**: Handle UI, call API functions
- **API Functions**: Handle HTTP communication, don't contain UI logic

### **Dependency Injection**
- Services are registered in `Program.cs`
- Controllers receive services via constructor
- Makes code testable and maintainable

### **Type Safety**
- Backend: C# types ensure data integrity
- Frontend: TypeScript types prevent runtime errors
- DTOs ensure API contracts are maintained

---

## 📝 Summary

**Backend**:
- **Controllers** = HTTP endpoints
- **Services** = Business logic
- **Models** = Data structures
- **Data** = Database access
- **Migrations** = Database schema changes
- **Middleware** = Request processing pipeline

**Frontend**:
- **Pages** = Full screens
- **Components** = Reusable UI elements
- **API** = Backend communication
- **State** = Global state management
- **Types** = TypeScript definitions
- **Routes** = Navigation and protection

This architecture ensures:
- ✅ Code is organized and maintainable
- ✅ Business logic is separated from HTTP handling
- ✅ Frontend and backend are decoupled
- ✅ Code is testable
- ✅ Changes in one layer don't break others


