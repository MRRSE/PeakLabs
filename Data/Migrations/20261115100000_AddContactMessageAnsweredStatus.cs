using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PeakLabs.Data;

#nullable disable

namespace PeakLabs.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261115100000_AddContactMessageAnsweredStatus")]
public sealed class AddContactMessageAnsweredStatus : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsAnswered",
            table: "ContactMessages",
            type: "bit",
            nullable: false,
            defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "IsAnswered", table: "ContactMessages");
    }
}
