using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeakLabs.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleViewCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Article",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Article");
        }
    }
}
