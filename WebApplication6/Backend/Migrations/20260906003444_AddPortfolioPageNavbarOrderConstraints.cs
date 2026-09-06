using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication6.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPortfolioPageNavbarOrderConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_PortfolioPages_NavbarOrder_NonNegative",
                table: "PortfolioPages",
                column: "NavbarOrder",
                unique: true,
                filter: "\"NavbarOrder\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PortfolioPages_NavbarOrder_Range",
                table: "PortfolioPages",
                sql: "\"NavbarOrder\" BETWEEN -1 AND 4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_PortfolioPages_NavbarOrder_NonNegative",
                table: "PortfolioPages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PortfolioPages_NavbarOrder_Range",
                table: "PortfolioPages");
        }
    }
}
