using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Longstone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFundsAndInstruments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Funds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Lei = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Isin = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    FundType = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    BaseCurrency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    BenchmarkIndex = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    InceptionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Instruments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Isin = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    Sedol = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    Ticker = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Exchange = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    CountryOfListing = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Sector = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    AssetClass = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    MarketCapitalisation = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CouponRate = table.Column<decimal>(type: "TEXT", precision: 8, scale: 6, nullable: true),
                    MaturityDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CouponFrequency = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    DayCountConvention = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    LastCouponDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FaceValue = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instruments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FundManagers",
                columns: table => new
                {
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundManagers", x => new { x.FundId, x.UserId });
                    table.ForeignKey(
                        name: "FK_FundManagers_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundManagers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MandateRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FundId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RuleType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Parameters = table.Column<string>(type: "TEXT", nullable: false),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MandateRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MandateRules_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FundManagers_UserId",
                table: "FundManagers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Funds_Isin",
                table: "Funds",
                column: "Isin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Funds_Lei",
                table: "Funds",
                column: "Lei",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Funds_Name",
                table: "Funds",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_Isin",
                table: "Instruments",
                column: "Isin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_Sedol",
                table: "Instruments",
                column: "Sedol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_Ticker",
                table: "Instruments",
                column: "Ticker");

            migrationBuilder.CreateIndex(
                name: "IX_MandateRules_FundId",
                table: "MandateRules",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_MandateRules_FundId_IsActive",
                table: "MandateRules",
                columns: new[] { "FundId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FundManagers");

            migrationBuilder.DropTable(
                name: "Instruments");

            migrationBuilder.DropTable(
                name: "MandateRules");

            migrationBuilder.DropTable(
                name: "Funds");
        }
    }
}
