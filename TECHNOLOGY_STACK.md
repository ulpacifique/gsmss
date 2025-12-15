# Technology Stack

## Backend
**ASP.NET Core Web API** (not ASP.NET Web API)

- **Framework**: .NET 10.0 (ASP.NET Core)
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: Custom header-based authentication middleware
- **Architecture**: RESTful API with Service-Repository pattern

### Key Technologies:
- ASP.NET Core 10.0
- Entity Framework Core (ORM)
- SQL Server Database
- BCrypt.Net (Password hashing)
- C# 10.0

## Frontend
- **Framework**: React 19 with TypeScript
- **Build Tool**: Vite
- **Styling**: Tailwind CSS
- **State Management**: React Query (TanStack Query)
- **HTTP Client**: Axios
- **Routing**: React Router v7

### Key Technologies:
- React 19.2.3
- TypeScript 5.9.3
- Vite 7.2.4
- Tailwind CSS 3.4.15
- @tanstack/react-query 5.90.12

## Difference: ASP.NET Core Web API vs ASP.NET Web API

**ASP.NET Core Web API** (What we're using):
- Cross-platform (Windows, Linux, macOS)
- Modern, lightweight, and fast
- Built on .NET Core/.NET 5+
- Uses `Microsoft.AspNetCore.*` packages
- Supports dependency injection out of the box
- Can run on Kestrel web server

**ASP.NET Web API** (Legacy):
- Windows-only
- Built on .NET Framework
- Older technology (pre-2016)
- Uses `System.Web.Http` namespace
- Requires IIS

**Our project uses ASP.NET Core Web API** - the modern, cross-platform version.


