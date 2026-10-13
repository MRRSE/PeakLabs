using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeakLabs.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleAuthorAndStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorId",
                table: "Article",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Article",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "published");

            migrationBuilder.CreateIndex(
                name: "IX_Article_AuthorId",
                table: "Article",
                column: "AuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Article_AspNetUsers_AuthorId",
                table: "Article",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Article_AspNetUsers_AuthorId",
                table: "Article");

            migrationBuilder.DropIndex(
                name: "IX_Article_AuthorId",
                table: "Article");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "Article");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Article");
        }
    }
}
