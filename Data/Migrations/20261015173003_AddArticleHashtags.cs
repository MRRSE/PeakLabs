using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeakLabs.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleHashtags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArticleHashtag",
                columns: table => new
                {
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleHashtag", x => new { x.ArticleId, x.Tag });
                    table.ForeignKey(
                        name: "FK_ArticleHashtag_Article_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Article",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticleHashtag");
        }
    }
}
