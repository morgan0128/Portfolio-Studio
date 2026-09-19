using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApplication6.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AlbumItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_AlbumPhoto_AlbumsId_Order",
                table: "AlbumPhoto");

            migrationBuilder.AddColumn<string>(
                name: "AlbumItemType",
                table: "AlbumPhoto",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "PhotoDisplay");

            migrationBuilder.AddColumn<int>(
                name: "AlbumPhotoDisplayItemId",
                table: "AlbumPhoto",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AlbumItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlbumId = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    ItemType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DisplayMode = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumItems", x => x.Id);
                    table.UniqueConstraint("AK_AlbumItems_AlbumId_Id_ItemType", x => new { x.AlbumId, x.Id, x.ItemType });
                    table.CheckConstraint("CK_AlbumItems_Order_NonNegative", "\"Order\" >= 0");
                    table.CheckConstraint("CK_AlbumItems_PhotoDisplayMode_Range", "\"ItemType\" <> 'PhotoDisplay' OR (\"DisplayMode\" IS NOT NULL AND \"DisplayMode\" BETWEEN 0 AND 1)");
                    table.ForeignKey(
                        name: "FK_AlbumItems_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Preserve each existing photo's album position in a singleton display item.
            migrationBuilder.Sql("""
                INSERT INTO "AlbumItems" ("AlbumId", "Order", "ItemType", "DisplayMode")
                SELECT "AlbumsId", "Order", 'PhotoDisplay', 0
                FROM "AlbumPhoto";

                UPDATE "AlbumPhoto" AS ap
                SET "AlbumPhotoDisplayItemId" = ai."Id", "Order" = 0
                FROM "AlbumItems" AS ai
                WHERE ai."AlbumId" = ap."AlbumsId" AND ai."Order" = ap."Order";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_AlbumPhoto_AlbumsId_AlbumPhotoDisplayItemId_AlbumItemType",
                table: "AlbumPhoto",
                columns: new[] { "AlbumsId", "AlbumPhotoDisplayItemId", "AlbumItemType" });

            migrationBuilder.CreateIndex(
                name: "UX_AlbumPhoto_AlbumPhotoDisplayItemId_Order",
                table: "AlbumPhoto",
                columns: new[] { "AlbumPhotoDisplayItemId", "Order" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlbumPhoto_ItemType_PhotoDisplay",
                table: "AlbumPhoto",
                sql: "\"AlbumItemType\" = 'PhotoDisplay'");

            migrationBuilder.CreateIndex(
                name: "UX_AlbumItems_AlbumId_Order",
                table: "AlbumItems",
                columns: new[] { "AlbumId", "Order" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AlbumPhoto_AlbumItems_AlbumId_ItemId_ItemType",
                table: "AlbumPhoto",
                columns: new[] { "AlbumsId", "AlbumPhotoDisplayItemId", "AlbumItemType" },
                principalTable: "AlbumItems",
                principalColumns: new[] { "AlbumId", "Id", "ItemType" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlbumPhoto_AlbumItems_AlbumId_ItemId_ItemType",
                table: "AlbumPhoto");

            migrationBuilder.DropIndex(
                name: "IX_AlbumPhoto_AlbumsId_AlbumPhotoDisplayItemId_AlbumItemType",
                table: "AlbumPhoto");

            migrationBuilder.DropIndex(
                name: "UX_AlbumPhoto_AlbumPhotoDisplayItemId_Order",
                table: "AlbumPhoto");

            // Flatten photo items back into the album-wide order used by the old model.
            migrationBuilder.Sql("""
                WITH ordered AS (
                    SELECT ap."AlbumsId", ap."PhotosId",
                        (ROW_NUMBER() OVER (
                            PARTITION BY ap."AlbumsId"
                            ORDER BY ai."Order", ap."Order") - 1)::integer AS "Order"
                    FROM "AlbumPhoto" AS ap
                    JOIN "AlbumItems" AS ai ON ai."Id" = ap."AlbumPhotoDisplayItemId"
                )
                UPDATE "AlbumPhoto" AS ap
                SET "Order" = ordered."Order"
                FROM ordered
                WHERE ordered."AlbumsId" = ap."AlbumsId" AND ordered."PhotosId" = ap."PhotosId";
                """);

            migrationBuilder.DropTable(
                name: "AlbumItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AlbumPhoto_ItemType_PhotoDisplay",
                table: "AlbumPhoto");

            migrationBuilder.DropColumn(
                name: "AlbumItemType",
                table: "AlbumPhoto");

            migrationBuilder.DropColumn(
                name: "AlbumPhotoDisplayItemId",
                table: "AlbumPhoto");

            migrationBuilder.CreateIndex(
                name: "UX_AlbumPhoto_AlbumsId_Order",
                table: "AlbumPhoto",
                columns: new[] { "AlbumsId", "Order" },
                unique: true);
        }
    }
}
