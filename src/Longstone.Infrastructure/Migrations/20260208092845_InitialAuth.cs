using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Longstone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserRole = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    EntityType = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    EntityId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    BeforeState = table.Column<string>(type: "TEXT", nullable: true),
                    AfterState = table.Column<string>(type: "TEXT", nullable: true),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    SessionId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CorrelationId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    TraceId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Permission = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Scope = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPermissionOverrides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Permission = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Scope = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    IsGranted = table.Column<bool>(type: "INTEGER", nullable: false),
                    OverriddenBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    OverriddenAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermissionOverrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPermissionOverrides_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_EntityType",
                table: "AuditEvents",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_EntityType_EntityId",
                table: "AuditEvents",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_Timestamp",
                table: "AuditEvents",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_UserId",
                table: "AuditEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_Role_Permission",
                table: "RolePermissions",
                columns: new[] { "Role", "Permission" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissionOverrides_UserId_Permission",
                table: "UserPermissionOverrides",
                columns: new[] { "UserId", "Permission" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditEvents");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "UserPermissionOverrides");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
