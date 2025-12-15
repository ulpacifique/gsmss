using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommunityFinanceAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddContributionLimitsAndRewards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only create new tables - skip column alterations that may already be applied
            migrationBuilder.CreateTable(
                name: "ContributionLimits",
                columns: table => new
                {
                    LimitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GoalId = table.Column<int>(type: "int", nullable: false),
                    FixedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MinimumAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaximumAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaximumTotalPerUser = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContributionLimits", x => x.LimitId);
                    table.ForeignKey(
                        name: "FK_ContributionLimits_SavingsGoals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "SavingsGoals",
                        principalColumn: "GoalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContributionRewards",
                columns: table => new
                {
                    RewardId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RewardName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContributionThreshold = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RewardAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RewardType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContributionRewards", x => x.RewardId);
                });

            // Create indexes only if they don't exist (skip Status indexes as Status is nvarchar(max) and cannot be indexed)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Loans_RequestedDate' AND object_id = OBJECT_ID('Loans'))
                BEGIN
                    CREATE INDEX IX_Loans_RequestedDate ON Loans(RequestedDate);
                END
            ");

            migrationBuilder.CreateIndex(
                name: "IX_ContributionLimits_GoalId",
                table: "ContributionLimits",
                column: "GoalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContributionLimits");

            migrationBuilder.DropTable(
                name: "ContributionRewards");
        }
    }
}
