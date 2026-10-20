using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PeakLabs.Data;

#nullable disable

namespace PeakLabs.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261021120000_AddDeveloperProfileImage")]
public partial class AddDeveloperProfileImage : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ProfileImagePath",
            table: "Developers",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ProfileImagePath", table: "Developers");
    }
}
