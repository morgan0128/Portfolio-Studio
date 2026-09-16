using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication6.Backend.Migrations
{
    /// <inheritdoc />
    public partial class pp_new_constraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_PortfolioPages_Unpublished_NoNavbar",
                table: "PortfolioPages",
                sql: "\"Published\" = TRUE OR \"NavbarOrder\" = -1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PortfolioPages_Unpublished_NoNavbar",
                table: "PortfolioPages");
        }
    }
}
