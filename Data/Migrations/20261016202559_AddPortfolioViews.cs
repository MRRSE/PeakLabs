using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeakLabs.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPortfolioViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Portfolio",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PortfolioView",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortfolioItemId = table.Column<int>(type: "int", nullable: false),
                    ViewedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioView", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortfolioView_Portfolio_PortfolioItemId",
                        column: x => x.PortfolioItemId,
                        principalTable: "Portfolio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioView_PortfolioItemId",
                table: "PortfolioView",
                column: "PortfolioItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioView_ViewedAt",
                table: "PortfolioView",
                column: "ViewedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortfolioView");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Portfolio");
        }
    }
}
