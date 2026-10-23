using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeakLabs.Data.Migrations
{
    /// <inheritdoc />
    public partial class DashboardTrafficAndAccountDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.CreateTable(
                name: "SiteVisit",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    VisitedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteVisit", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SiteVisit_VisitedAt",
                table: "SiteVisit",
                column: "VisitedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteVisit");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");

        }
    }
}
