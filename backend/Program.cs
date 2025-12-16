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

// TEMPORARY: DISABLE AUTH FOR TESTING
// app.UseWhen(context => !context.Request.Path.StartsWithSegments("/health") &&
//                        !context.Request.Path.StartsWithSegments("/test-db") &&
//                        !context.Request.Path.StartsWithSegments("/api/auth") &&
//                        !context.Request.Path.StartsWithSegments("/api/create-test-users") &&
//                        !context.Request.Path.StartsWithSegments("/swagger") &&
//                        !context.Request.Path.StartsWithSegments("/") &&
//                        context.Request.Path != "/",
//     appBuilder =>
// {
//     appBuilder.UseMiddleware<SimpleAuthMiddleware>();
// });

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
        
        // Test 2: List tables
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
        
        // Test 3: Try to query users
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

app.MapPost("/api/create-test-users", async (ApplicationDbContext db) =>
{
    try
    {
        Console.WriteLine("Creating Users table and test users...");
        
        // 1. Create Users table if not exists
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
        
        Console.WriteLine("✅ Users table created/verified");
        
        // 2. Clear existing users
        await db.Database.ExecuteSqlRawAsync("DELETE FROM \"Users\"");
        Console.WriteLine("✅ Cleared existing users");
        
        // 3. Create new users with proper BCrypt hashes
        var adminHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
        var memberHash = BCrypt.Net.BCrypt.HashPassword("Member@123");
        
        Console.WriteLine($"Admin hash: {adminHash.Substring(0, Math.Min(30, adminHash.Length))}...");
        Console.WriteLine($"Member hash: {memberHash.Substring(0, Math.Min(30, memberHash.Length))}...");
        
        await db.Database.ExecuteSqlRawAsync(@"
            INSERT INTO ""Users"" (""Email"", ""PasswordHash"", ""FirstName"", ""LastName"", ""Role"", ""IsActive"")
            VALUES ('newadmin@community.com', {0}, 'Admin', 'User', 'Admin', true)
        ", adminHash);
        
        await db.Database.ExecuteSqlRawAsync(@"
            INSERT INTO ""Users"" (""Email"", ""PasswordHash"", ""FirstName"", ""LastName"", ""Role"", ""IsActive"")
            VALUES ('newmember@community.com', {0}, 'John', 'Doe', 'Member', true)
        ", memberHash);
        
        Console.WriteLine("✅ Test users created");
        
        // 4. Verify
        var users = await db.Database.SqlQueryRaw<dynamic>(
            "SELECT \"UserId\", \"Email\", \"FirstName\", \"LastName\", \"Role\" FROM \"Users\"")
            .ToListAsync();
        
        return Results.Ok(new { 
            success = true,
            message = "Users table created and test users added",
            users = users,
            userCount = users.Count
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
        createTestUsers = "/api/create-test-users (POST)"
    });
});

// Apply database setup and seed data ON STARTUP
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    Console.WriteLine("🚀 Application starting - setting up database...");
    
    // 1. Test connection
    var canConnect = dbContext.Database.CanConnect();
    Console.WriteLine($"✅ Database connection: {canConnect}");
    
    // 2. Create Users table if it doesn't exist
    Console.WriteLine("Creating Users table if not exists...");
    await dbContext.Database.ExecuteSqlRawAsync(@"
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
    Console.WriteLine("✅ Users table created/verified");
    
    // 3. Check if users exist
    var usersCount = await dbContext.Database.SqlQueryRaw<int>("SELECT COUNT(*) FROM \"Users\"").FirstOrDefaultAsync();
    Console.WriteLine($"👥 Users count: {usersCount}");
    
    // 4. Seed users if none exist
    if (usersCount == 0)
    {
        Console.WriteLine("\n🌱 Seeding initial users...");
        
        // Create admin user
        var adminHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
        await dbContext.Database.ExecuteSqlRawAsync(@"
            INSERT INTO ""Users"" (""Email"", ""PasswordHash"", ""FirstName"", ""LastName"", ""Role"", ""IsActive"")
            VALUES ('newadmin@community.com', {0}, 'Admin', 'User', 'Admin', true)
        ", adminHash);
        Console.WriteLine("✅ Admin user created");
        
        // Create member user  
        var memberHash = BCrypt.Net.BCrypt.HashPassword("Member@123");
        await dbContext.Database.ExecuteSqlRawAsync(@"
            INSERT INTO ""Users"" (""Email"", ""PasswordHash"", ""FirstName"", ""LastName"", ""Role"", ""IsActive"")
            VALUES ('newmember@community.com', {0}, 'John', 'Doe', 'Member', true)
        ", memberHash);
        Console.WriteLine("✅ Member user created");
        
        Console.WriteLine("✅ Seeding complete!");
    }
    
    Console.WriteLine("✅ Database setup complete!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Database setup error: {ex.Message}");
    Console.WriteLine($"📝 Full error: {ex}");
}

Console.WriteLine("🚀 Application starting...");
app.Run();