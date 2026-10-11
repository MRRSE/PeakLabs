using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PeakLabs.Data;

#nullable disable

namespace PeakLabs.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261012120000_AddPortfolio")]
public partial class AddPortfolio : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Portfolio",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ProjectImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                ProjectName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                ProjectDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                ProjectLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                AddedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Portfolio", x => x.Id));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Portfolio");
    }
}
