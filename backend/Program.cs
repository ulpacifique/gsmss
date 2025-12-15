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

// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// REMOVE JWT HELPER registration since we're not using JWT
// builder.Services.AddSingleton<JwtHelper>(); // COMMENT THIS OUT

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

// REMOVE: app.UseHttpsRedirection(); // Force HTTP on port 5154
app.UseCors("AllowAll");

// Use custom authentication middleware BEFORE exception handling
app.UseMiddleware<SimpleAuthMiddleware>();

// Use custom exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// REMOVE JWT HELPER registration since we're not using JWT
// app.UseAuthentication(); // Not using JWT
app.UseAuthorization();

app.MapControllers();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    try
    {
        Console.WriteLine("Checking database and applying migrations...");
        
        // Check if InitialCreate migration needs to be marked as applied
        try
        {
            var connection = dbContext.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
                connection.Open();
            
            // Check if Users table exists but InitialCreate not in history
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
                    AND NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251213140242_InitialCreate')
                    BEGIN
                        IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
                        BEGIN
                            CREATE TABLE [__EFMigrationsHistory] (
                                [MigrationId] nvarchar(150) NOT NULL,
                                [ProductVersion] nvarchar(32) NOT NULL,
                                CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                            );
                        END
                        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                        VALUES ('20251213140242_InitialCreate', '10.0.0');
                        PRINT 'InitialCreate migration marked as applied';
                    END
                ";
                cmd.ExecuteNonQuery();
            }
            
            if (connection.State == ConnectionState.Open)
                connection.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Note: {ex.Message}");
        }
        
        // Apply pending migrations
        var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
        if (pendingMigrations.Any())
        {
            Console.WriteLine($"Applying {pendingMigrations.Count()} pending migrations...");
            dbContext.Database.Migrate();
            Console.WriteLine("Migrations applied successfully.");
        }
        else
        {
            Console.WriteLine("Database is up to date. No migrations needed.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Migration error: {ex.Message}");
        Console.WriteLine("The application will continue, but some features may not work.");
    }

    // Seed a NEW admin user with BCrypt
    if (!dbContext.Users.Any(u => u.Email == "newadmin@community.com"))
    {
        Console.WriteLine("Creating new admin user with BCrypt...");

        var newAdmin = new User
        {
            Email = "newadmin@community.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"), // Using BCrypt
            FirstName = "Admin",
            LastName = "User",
            PhoneNumber = "123-456-7890",
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(newAdmin);
        dbContext.SaveChanges();

        Console.WriteLine("✅ New admin created!");
        Console.WriteLine("   Email: newadmin@community.com");
        Console.WriteLine("   Password: Admin@123");
    }

    // Seed a member user with BCrypt
    if (!dbContext.Users.Any(u => u.Email == "newmember@community.com"))
    {
        Console.WriteLine("Creating new member user with BCrypt...");

        var newMember = new User
        {
            Email = "newmember@community.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Member@123"), // Using BCrypt
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "123-456-7890",
            Role = "Member",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(newMember);
        dbContext.SaveChanges();

        Console.WriteLine("✅ New member created!");
        Console.WriteLine("   Email: newmember@community.com");
        Console.WriteLine("   Password: Member@123");
    }
}

app.Run();
