using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PeakLabs.Data;

#nullable disable

namespace PeakLabs.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261115120000_AddDefaultSiteTheme")]
public sealed class AddDefaultSiteTheme : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "DefaultTheme",
            table: "SiteSettings",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: false,
            defaultValue: "light");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "DefaultTheme", table: "SiteSettings");
    }
}
