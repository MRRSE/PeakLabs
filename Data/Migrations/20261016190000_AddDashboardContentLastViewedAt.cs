using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace PeakLabs.Data.Migrations;

[DbContext(typeof(PeakLabs.Data.ApplicationDbContext))]
[Migration("20261016190000_AddDashboardContentLastViewedAt")]
public partial class AddDashboardContentLastViewedAt : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "ArticlesLastViewedAt",
            table: "AspNetUsers",
            type: "datetimeoffset",
            nullable: false,
            defaultValueSql: "SYSUTCDATETIME()");

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "PortfolioLastViewedAt",
            table: "AspNetUsers",
            type: "datetimeoffset",
            nullable: false,
            defaultValueSql: "SYSUTCDATETIME()");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ArticlesLastViewedAt", table: "AspNetUsers");
        migrationBuilder.DropColumn(name: "PortfolioLastViewedAt", table: "AspNetUsers");
    }
}
