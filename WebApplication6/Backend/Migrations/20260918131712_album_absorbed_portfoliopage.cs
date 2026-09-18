using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApplication6.Backend.Migrations
{
    /// <inheritdoc />
    public partial class album_absorbed_portfoliopage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortfolioPages");

            migrationBuilder.AddColumn<int>(
                name: "LayoutPreset",
                table: "Albums",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NavTitle",
                table: "Albums",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "NavbarOrder",
                table: "Albums",
                type: "integer",
                nullable: false,
                defaultValue: -1);

            migrationBuilder.AddColumn<bool>(
                name: "Published",
                table: "Albums",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "UX_Albums_NavbarOrder_NonNegative",
                table: "Albums",
                column: "NavbarOrder",
                unique: true,
                filter: "\"NavbarOrder\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Albums_NavbarOrder_Range",
                table: "Albums",
                sql: "\"NavbarOrder\" BETWEEN -1 AND 4");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Albums_Unpublished_NoNavbar",
                table: "Albums",
                sql: "\"Published\" = TRUE OR \"NavbarOrder\" = -1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Albums_NavbarOrder_NonNegative",
                table: "Albums");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Albums_NavbarOrder_Range",
                table: "Albums");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Albums_Unpublished_NoNavbar",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "LayoutPreset",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "NavTitle",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "NavbarOrder",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "Published",
                table: "Albums");

            migrationBuilder.CreateTable(
                name: "PortfolioPages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlbumId = table.Column<int>(type: "integer", nullable: false),
                    LayoutPreset = table.Column<int>(type: "integer", nullable: false),
                    NavTitle = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NavbarOrder = table.Column<int>(type: "integer", nullable: false),
                    Published = table.Column<bool>(type: "boolean", nullable: false),
                    Title = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioPages", x => x.Id);
                    table.CheckConstraint("CK_PortfolioPages_NavbarOrder_Range", "\"NavbarOrder\" BETWEEN -1 AND 4");
                    table.CheckConstraint("CK_PortfolioPages_Unpublished_NoNavbar", "\"Published\" = TRUE OR \"NavbarOrder\" = -1");
                    table.ForeignKey(
                        name: "FK_PortfolioPages_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioPages_AlbumId",
                table: "PortfolioPages",
                column: "AlbumId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_PortfolioPages_NavbarOrder_NonNegative",
                table: "PortfolioPages",
                column: "NavbarOrder",
                unique: true,
                filter: "\"NavbarOrder\" >= 0");
        }
    }
}
