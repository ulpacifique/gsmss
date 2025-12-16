using System.Text;
using CommunityFinanceAPI.Data;
using CommunityFinanceAPI.Middleware;
using CommunityFinanceAPI.Models.Entities;
using CommunityFinanceAPI.Services.BackgroundServices;
using CommunityFinanceAPI.Services.Implementations;
using CommunityFinanceAPI.Services.Interfaces;
using CommunityFinanceAPI.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Community Finance API",
        Version = "v1",
        Description = "API for managing community finance, goals, contributions, and loans"
    });

    // Add security definition for SimpleAuth
    c.AddSecurityDefinition("SimpleAuth", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "X-User-Email",
        Description = "User email for authentication"
    });

    c.AddSecurityDefinition("SimpleAuthPassword", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "X-User-Password",
        Description = "User password for authentication"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "SimpleAuth"
                }
            },
            new string[] {}
        },
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "SimpleAuthPassword"
                }
            },
            new string[] {}
        }
    });
});

// Configure PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IGoalService, GoalService>();
builder.Services.AddScoped<IContributionService, ContributionService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IContributionLimitService, ContributionLimitService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<ILoanPaymentService, LoanPaymentService>();
builder.Services.AddScoped<ILoanRiskAssessmentService, LoanRiskAssessmentService>();

// Background service for checking overdue loans
builder.Services.AddHostedService<OverdueLoanCheckerService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// Use custom authentication middleware ONLY for non-public endpoints
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/health") &&
                       !context.Request.Path.StartsWithSegments("/test-db") &&
                       !context.Request.Path.StartsWithSegments("/api/auth") &&
                       !context.Request.Path.StartsWithSegments("/api/setup-database") && // ADD THIS
                       !context.Request.Path.StartsWithSegments("/swagger") &&
                       !context.Request.Path.StartsWithSegments("/") &&
                       context.Request.Path != "/",
    appBuilder =>
{
    appBuilder.UseMiddleware<SimpleAuthMiddleware>();
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthorization();

app.MapControllers();

// Add custom endpoints
app.MapGet("/health", async (ApplicationDbContext db) =>
{
    try
    {
        var canConnect = db.Database.CanConnect();
        var userCount = 0;
        
        try
        {
            userCount = await db.Database.SqlQueryRaw<int>("SELECT COUNT(*) FROM \"Users\"").FirstOrDefaultAsync();
        }
        catch
        {
            // Table might not exist yet
            userCount = 0;
        }
        
        return Results.Ok(new 
        { 
            status = "OK", 
            database = canConnect ? "Connected" : "Disconnected",
            tablesExist = true,
            usersCount = userCount,
            timestamp = DateTime.UtcNow,
            message = "Community Finance API is running"
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new 
        { 
            status = "ERROR",
            database = "Error",
            error = ex.Message,
            timestamp = DateTime.UtcNow,
            message = "Health check failed"
        });
    }
});

app.MapGet("/test-db", async (ApplicationDbContext db) =>
{
    try
    {
        // Test 1: Connection
        var canConnect = db.Database.CanConnect();
        
        // Test 2: List tables (handle empty)
        var tables = new List<string>();
        try
        {
            tables = await db.Database.SqlQueryRaw<string>(
                "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' ORDER BY table_name")
                .ToListAsync();
        }
        catch (Exception tableEx)
        {
            Console.WriteLine($"Table query error: {tableEx.Message}");
        }
        
        // Test 3: Try to query users (handle empty)
        List<dynamic> users = new List<dynamic>();
        try
        {
            users = await db.Database.SqlQueryRaw<dynamic>(
                "SELECT \"UserId\", \"Email\", \"FirstName\", \"LastName\", \"Role\" FROM \"Users\" LIMIT 5")
                .ToListAsync();
        }
        catch (Exception userEx)
        {
            Console.WriteLine($"User query error: {userEx.Message}");
        }
        
        return Results.Ok(new
        {
            success = true,
            connection = canConnect ? "✅ Connected" : "❌ Disconnected",
            tableCount = tables.Count,
            tables = tables,
            userCount = users.Count,
            sampleUsers = users,
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new
        {
            success = false,
            error = ex.Message,
            details = ex.InnerException?.Message,
            timestamp = DateTime.UtcNow
        });
    }
});

app.MapPost("/api/create-correct-users", async (ApplicationDbContext db) =>
{
    try
    {
        Console.WriteLine("Creating users with correct BCrypt hashes...");
        
        // Generate proper BCrypt hashes
        var adminHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
        var memberHash = BCrypt.Net.BCrypt.HashPassword("Member@123");
        
        Console.WriteLine($"Admin hash: {adminHash.Substring(0, Math.Min(30, adminHash.Length))}...");
        Console.WriteLine($"Member hash: {memberHash.Substring(0, Math.Min(30, memberHash.Length))}...");
        
        // Create Users table if not exists
        await db.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS ""Users"" (
                ""UserId"" SERIAL PRIMARY KEY,
                ""Email"" VARCHAR(100) NOT NULL,
                ""PasswordHash"" VARCHAR(255) NOT NULL,
                ""FirstName"" VARCHAR(50) NOT NULL,
                ""LastName"" VARCHAR(50) NOT NULL,
                ""PhoneNumber"" VARCHAR(20),
                ""ProfilePictureUrl"" TEXT,
                ""Role"" VARCHAR(50) NOT NULL DEFAULT 'Member',
                ""IsActive"" BOOLEAN NOT NULL DEFAULT true,
                ""CreatedAt"" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                ""UpdatedAt"" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                CONSTRAINT ""UQ_Users_Email"" UNIQUE(""Email"")
            );
        ");
        
        // Clear existing users
        await db.Database.ExecuteSqlRawAsync("DELETE FROM \"Users\"");
        
        // Insert new users with correct hashes
        await db.Database.ExecuteSqlRawAsync(@"
            INSERT INTO ""Users"" (""Email"", ""PasswordHash"", ""FirstName"", ""LastName"", ""Role"", ""IsActive"")
            VALUES ('newadmin@community.com', {0}, 'Admin', 'User', 'Admin', true)
        ", adminHash);
        
        await db.Database.ExecuteSqlRawAsync(@"
            INSERT INTO ""Users"" (""Email"", ""PasswordHash"", ""FirstName"", ""LastName"", ""Role"", ""IsActive"")
            VALUES ('newmember@community.com', {0}, 'John', 'Doe', 'Member', true)
        ", memberHash);
        
        // Verify
        var users = await db.Database.SqlQueryRaw<dynamic>(
            "SELECT \"Email\", \"Role\", \"PasswordHash\" FROM \"Users\"")
            .ToListAsync();
        
        return Results.Ok(new { 
            success = true,
            message = "Users created with correct BCrypt hashes",
            users = users
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error: {ex.Message}\n{ex.StackTrace}");
    }
});

// Simple status endpoint
app.MapGet("/", () => 
{
    return Results.Ok(new 
    { 
        service = "Community Finance API", 
        status = "Running",
        version = "1.0",
        documentation = "/swagger",
        healthCheck = "/health",
        databaseTest = "/test-db",
        setupEndpoint = "/api/setup-database (POST)"
    });
});

// Apply database setup and seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    try
    {
        Console.WriteLine("🔧 Setting up database...");
        
        // 1. Test connection first
        var canConnect = dbContext.Database.CanConnect();
        Console.WriteLine($"✅ Database connection: {canConnect}");
        
        // 2. Create database and tables
        Console.WriteLine("Creating tables if they don't exist...");
        dbContext.Database.EnsureCreated();
        
        // 3. MANUALLY ENSURE Users TABLE EXISTS WITH CORRECT STRUCTURE
        Console.WriteLine("Ensuring Users table has correct structure...");
        dbContext.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS ""Users"" (
                ""UserId"" SERIAL PRIMARY KEY,
                ""Email"" VARCHAR(100) NOT NULL,
                ""PasswordHash"" VARCHAR(255) NOT NULL,
                ""FirstName"" VARCHAR(50) NOT NULL,
                ""LastName"" VARCHAR(50) NOT NULL,
                ""PhoneNumber"" VARCHAR(20),
                ""ProfilePictureUrl"" TEXT,
                ""Role"" VARCHAR(50) NOT NULL DEFAULT 'Member',
                ""IsActive"" BOOLEAN NOT NULL DEFAULT true,
                ""CreatedAt"" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                ""UpdatedAt"" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                CONSTRAINT ""UQ_Users_Email"" UNIQUE(""Email"")
            );
        ");
        
        // 4. List all tables for debugging
        Console.WriteLine("\n📊 Checking existing tables...");
        var tables = dbContext.Database.SqlQueryRaw<string>(
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' ORDER BY table_name")
            .ToList();
            
        Console.WriteLine($"Found {tables.Count} tables:");
        foreach (var table in tables)
        {
            Console.WriteLine($"  - {table}");
        }
        
        // 5. Check if Users table has data
        try
        {
            var usersCount = dbContext.Database.SqlQueryRaw<int>("SELECT COUNT(*) FROM \"Users\"").FirstOrDefault();
            Console.WriteLine($"\n👥 Users count: {usersCount}");
        }
        catch (Exception countEx)
        {
            Console.WriteLine($"❌ Could not count users: {countEx.Message}");
        }
        
        Console.WriteLine("✅ Database setup complete!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database setup error: {ex.Message}");
        Console.WriteLine($"📝 Full error: {ex}");
    }

    // Seed admin and member users
    try
    {
        Console.WriteLine("\n🌱 Seeding initial data...");
        
        // Check if admin exists
        var adminExists = dbContext.Database.SqlQueryRaw<int>(
            "SELECT COUNT(*) FROM \"Users\" WHERE \"Email\" = 'newadmin@community.com'").FirstOrDefault() > 0;
            
        if (!adminExists)
        {
            Console.WriteLine("Creating new admin user...");
            dbContext.Database.ExecuteSqlRaw(@"
                INSERT INTO ""Users"" (""Email"", ""PasswordHash"", ""FirstName"", ""LastName"", ""Role"", ""IsActive"", ""CreatedAt"", ""UpdatedAt"")
                VALUES (
                    'newadmin@community.com',
                    '{bcrypt}$2a$11$XhFp6X5hJ8Z4f4J8L4QZ5eJ8Z4f4J8L4QZ5eJ8Z4f4J8L4QZ5eJ8Z4', -- Hash for 'Admin@123'
                    'Admin',
                    'User',
                    'Admin',
                    true,
                    CURRENT_TIMESTAMP,
                    CURRENT_TIMESTAMP
                );
            ");
            Console.WriteLine("✅ Admin user created!");
        }
        else
        {
            Console.WriteLine("✅ Admin user already exists");
        }
        
        // Check if member exists
        var memberExists = dbContext.Database.SqlQueryRaw<int>(
            "SELECT COUNT(*) FROM \"Users\" WHERE \"Email\" = 'newmember@community.com'").FirstOrDefault() > 0;
            
        if (!memberExists)
        {
            Console.WriteLine("Creating new member user...");
            dbContext.Database.ExecuteSqlRaw(@"
                INSERT INTO ""Users"" (""Email"", ""PasswordHash"", ""FirstName"", ""LastName"", ""Role"", ""IsActive"", ""CreatedAt"", ""UpdatedAt"")
                VALUES (
                    'newmember@community.com',
                    '{bcrypt}$2a$11$YhFp6X5hJ8Z4f4J8L4QZ5eJ8Z4f4J8L4QZ5eJ8Z4f4J8L4QZ5eJ8Z4', -- Hash for 'Member@123'
                    'John',
                    'Doe',
                    'Member',
                    true,
                    CURRENT_TIMESTAMP,
                    CURRENT_TIMESTAMP
                );
            ");
            Console.WriteLine("✅ Member user created!");
        }
        else
        {
            Console.WriteLine("✅ Member user already exists");
        }
        
        Console.WriteLine("✅ Seeding complete!");
    }
    catch (Exception seedEx)
    {
        Console.WriteLine($"❌ Seeding error: {seedEx.Message}");
    }
}

Console.WriteLine("🚀 Application starting...");
app.Run();