using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PeakLabs.Data;

#nullable disable

namespace PeakLabs.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261027000000_AddSiteSettings")]
public partial class AddSiteSettings : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SiteSettings",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false),
                SiteTitle = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                SiteDescription = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                SiteUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                SeoTitle = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                SeoDescription = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                OgImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                ContactEmail = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                ContactPhone = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                Linkedin = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                Github = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                Instagram = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_SiteSettings", x => x.Id));
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "SiteSettings");
}
