using Microsoft.EntityFrameworkCore;
using CommunityFinanceAPI.Models.Entities;

namespace CommunityFinanceAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                return;
            }
            
            // Suppress the pending model changes warning
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            
            // #region agent log
            System.IO.File.AppendAllText(@"d:\Dotnet\CommunityFinanceAPI\.cursor\debug.log", 
                System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "C", location = "AppDbContext.OnConfiguring", message = "DbContext configured", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
            // #endregion
        }

        public DbSet<User> Users { get; set; }
        public DbSet<SavingsGoal> SavingsGoals { get; set; }
        public DbSet<Contribution> Contributions { get; set; }
        public DbSet<MemberGoal> MemberGoals { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<RecurringContribution> RecurringContributions { get; set; }
        public DbSet<ContributionLimit> ContributionLimits { get; set; }
        public DbSet<ContributionReward> ContributionRewards { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<LoanPayment> LoanPayments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Suppress pending model changes warning
            modelBuilder.HasAnnotation("Relational:MaxIdentifierLength", 128);
            
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Role).HasConversion<string>();
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(u => u.UpdatedAt).HasDefaultValueSql("GETDATE()");
                
                // #region agent log
                try {
                    System.IO.File.AppendAllText(@"d:\Dotnet\CommunityFinanceAPI\.cursor\debug.log", 
                        System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "C", location = "AppDbContext.OnModelCreating:47", message = "User entity configured", data = new { hasIgnore = true }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                } catch { /* Ignore file lock errors */ }
                // #endregion
            });

            // Configure SavingsGoal entity
            modelBuilder.Entity<SavingsGoal>(entity =>
            {
                entity.Property(g => g.Status).HasConversion<string>();
                entity.Property(g => g.CurrentAmount).HasDefaultValue(0);
                entity.Property(g => g.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(g => g.UpdatedAt).HasDefaultValueSql("GETDATE()");

                // Explicitly map the CreatedGoals navigation to avoid a shadow FK (UserId)
                entity.HasOne(g => g.CreatedByUser)
                      .WithMany(u => u.CreatedGoals)
                      .HasForeignKey(g => g.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Contribution entity
            modelBuilder.Entity<Contribution>(entity =>
            {
                entity.Property(c => c.Status).HasConversion<string>();
                entity.Property(c => c.SubmittedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(c => c.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(c => c.UpdatedAt).HasDefaultValueSql("GETDATE()");
                
                // Add indexes for frequently queried columns
                entity.HasIndex(c => c.UserId);
                entity.HasIndex(c => c.GoalId);
                entity.HasIndex(c => c.Status);
                entity.HasIndex(c => new { c.UserId, c.Status });
                
                // Explicitly configure User relationship to avoid shadow properties
                entity.HasOne(c => c.User)
                      .WithMany(u => u.Contributions)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired();
                
                // Explicitly configure Goal relationship to avoid shadow properties
                entity.HasOne(c => c.Goal)
                      .WithMany(g => g.Contributions)
                      .HasForeignKey(c => c.GoalId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired();
                
                // Explicitly configure ReviewedBy relationship to avoid shadow properties
                entity.HasOne(c => c.ReviewedByUser)
                      .WithMany()
                      .HasForeignKey(c => c.ReviewedBy)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired(false);
            });

            // Configure MemberGoal entity
            modelBuilder.Entity<MemberGoal>(entity =>
            {
                entity.HasIndex(mg => new { mg.UserId, mg.GoalId }).IsUnique();
                entity.Property(mg => mg.JoinedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(mg => mg.PersonalTarget).HasDefaultValue(0);
                entity.Property(mg => mg.CurrentAmount).HasDefaultValue(0);
            });

            // Configure Loan entity
            modelBuilder.Entity<Loan>(entity =>
            {
                entity.Property(l => l.Status).HasConversion<string>();
                entity.Property(l => l.InterestRate).HasDefaultValue(5.0m);
                entity.Property(l => l.PaidAmount).HasDefaultValue(0);
                entity.Property(l => l.RequestedDate).HasDefaultValueSql("GETDATE()");
                entity.Property(l => l.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(l => l.UpdatedAt).HasDefaultValueSql("GETDATE()");
                
                // Add indexes for frequently queried columns
                entity.HasIndex(l => l.UserId);
                entity.HasIndex(l => l.Status);
                entity.HasIndex(l => new { l.Status, l.RemainingAmount });
                entity.HasIndex(l => l.RequestedDate);
                
                entity.HasOne(l => l.User)
                      .WithMany(u => u.Loans)
                      .HasForeignKey(l => l.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(l => l.ApprovedByUser)
                      .WithMany(u => u.ApprovedLoans)
                      .HasForeignKey(l => l.ApprovedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Notification entity
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.Property(n => n.Type).HasConversion<string>();
                entity.Property(n => n.IsRead).HasDefaultValue(false);
                entity.Property(n => n.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(n => n.UpdatedAt).HasDefaultValueSql("GETDATE()");
                
                entity.HasOne(n => n.User)
                      .WithMany()
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Message entity
            modelBuilder.Entity<Message>(entity =>
            {
                entity.Property(m => m.MessageType).HasConversion<string>();
                entity.Property(m => m.IsRead).HasDefaultValue(false);
                entity.Property(m => m.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(m => m.UpdatedAt).HasDefaultValueSql("GETDATE()");
                
                entity.HasOne(m => m.Sender)
                      .WithMany()
                      .HasForeignKey(m => m.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(m => m.Receiver)
                      .WithMany()
                      .HasForeignKey(m => m.ReceiverId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Group entity
            modelBuilder.Entity<Group>(entity =>
            {
                entity.Property(g => g.IsActive).HasDefaultValue(true);
                entity.Property(g => g.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(g => g.UpdatedAt).HasDefaultValueSql("GETDATE()");
                
                entity.HasOne(g => g.Creator)
                      .WithMany()
                      .HasForeignKey(g => g.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure GroupMember entity
            modelBuilder.Entity<GroupMember>(entity =>
            {
                entity.HasIndex(gm => new { gm.GroupId, gm.UserId }).IsUnique();
                entity.Property(gm => gm.JoinedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(gm => gm.MonthlyContributionAmount).HasDefaultValue(0);
                entity.Property(gm => gm.IsActive).HasDefaultValue(true);
                entity.Property(gm => gm.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(gm => gm.UpdatedAt).HasDefaultValueSql("GETDATE()");
                
                entity.HasOne(gm => gm.Group)
                      .WithMany(g => g.Members)
                      .HasForeignKey(gm => gm.GroupId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(gm => gm.User)
                      .WithMany()
                      .HasForeignKey(gm => gm.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure RecurringContribution entity
            modelBuilder.Entity<RecurringContribution>(entity =>
            {
                entity.Property(rc => rc.Frequency).HasConversion<string>();
                entity.Property(rc => rc.IsActive).HasDefaultValue(true);
                entity.Property(rc => rc.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(rc => rc.UpdatedAt).HasDefaultValueSql("GETDATE()");
                
                entity.HasOne(rc => rc.User)
                      .WithMany()
                      .HasForeignKey(rc => rc.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(rc => rc.Goal)
                      .WithMany()
                      .HasForeignKey(rc => rc.GoalId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ContributionLimit entity
            modelBuilder.Entity<ContributionLimit>(entity =>
            {
                entity.Property(cl => cl.IsActive).HasDefaultValue(true);
                entity.Property(cl => cl.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(cl => cl.UpdatedAt).HasDefaultValueSql("GETDATE()");
                
                entity.HasOne(cl => cl.Goal)
                      .WithMany()
                      .HasForeignKey(cl => cl.GoalId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ContributionReward entity
            modelBuilder.Entity<ContributionReward>(entity =>
            {
                entity.Property(cr => cr.IsActive).HasDefaultValue(true);
                entity.Property(cr => cr.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(cr => cr.UpdatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Configure Permission entity
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasIndex(p => p.PermissionName).IsUnique();
                entity.Property(p => p.IsActive).HasDefaultValue(true);
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(p => p.UpdatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Configure UserPermission entity
            modelBuilder.Entity<UserPermission>(entity =>
            {
                entity.HasIndex(up => new { up.UserId, up.PermissionId }).IsUnique();
                entity.Property(up => up.IsGranted).HasDefaultValue(true);
                entity.Property(up => up.GrantedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(up => up.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(up => up.UpdatedAt).HasDefaultValueSql("GETDATE()");
                
                entity.HasOne(up => up.User)
                      .WithMany()
                      .HasForeignKey(up => up.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(up => up.Permission)
                      .WithMany(p => p.UserPermissions)
                      .HasForeignKey(up => up.PermissionId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(up => up.GrantedByUser)
                      .WithMany()
                      .HasForeignKey(up => up.GrantedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure LoanPayment entity
            modelBuilder.Entity<LoanPayment>(entity =>
            {
                entity.Property(lp => lp.PaymentDate).HasDefaultValueSql("GETDATE()");
                entity.Property(lp => lp.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(lp => lp.UpdatedAt).HasDefaultValueSql("GETDATE()");
                
                entity.HasOne(lp => lp.Loan)
                      .WithMany(l => l.LoanPayments)
                      .HasForeignKey(lp => lp.LoanId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(lp => lp.User)
                      .WithMany()
                      .HasForeignKey(lp => lp.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && 
                    (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                ((BaseEntity)entityEntry.Entity).UpdatedAt = DateTime.UtcNow;

                if (entityEntry.State == EntityState.Added)
                {
                    ((BaseEntity)entityEntry.Entity).CreatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}