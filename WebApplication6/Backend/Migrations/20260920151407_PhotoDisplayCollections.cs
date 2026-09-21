using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication6.Backend.Migrations
{
    /// <inheritdoc />
    public partial class PhotoDisplayCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlbumPhoto_AlbumItems_AlbumId_ItemId_ItemType",
                table: "AlbumPhoto");

            migrationBuilder.DropIndex(
                name: "UX_AlbumItems_AlbumId_Order",
                table: "AlbumItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AlbumItems_PhotoDisplayMode_Range",
                table: "AlbumItems");

            migrationBuilder.AddColumn<string>(
                name: "CollectionItemType",
                table: "AlbumItems",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true,
                defaultValue: "PhotoDisplayCollection");

            migrationBuilder.AddColumn<bool>(
                name: "DisplaysDescription",
                table: "AlbumItems",
                type: "boolean",
                nullable: true,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "DisplaysName",
                table: "AlbumItems",
                type: "boolean",
                nullable: true,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "DisplaysYearContentCreated",
                table: "AlbumItems",
                type: "boolean",
                nullable: true,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "PhotoDisplayCollectionId",
                table: "AlbumItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PhotoId",
                table: "AlbumItems",
                type: "integer",
                nullable: true);

            // Static singleton containers become displays. Keep groups, carousels and empty containers as collections.
            migrationBuilder.Sql("""
                UPDATE "AlbumItems" AS item
                SET "ItemType" = 'PhotoDisplayCollection'
                WHERE item."ItemType" = 'PhotoDisplay'
                  AND (item."DisplayMode" <> 0 OR
                       (SELECT count(*) FROM "AlbumPhoto" AS ap WHERE ap."AlbumPhotoDisplayItemId" = item."Id") <> 1);

                UPDATE "AlbumItems" AS item
                SET "PhotoId" = ap."PhotosId",
                    "DisplaysName" = ap."DisplaysName",
                    "DisplaysDescription" = ap."DisplaysDescription",
                    "DisplaysYearContentCreated" = ap."DisplaysYearContentCreated",
                    "DisplayMode" = NULL
                FROM "AlbumPhoto" AS ap
                WHERE ap."AlbumPhotoDisplayItemId" = item."Id" AND item."ItemType" = 'PhotoDisplay';

                INSERT INTO "AlbumItems" ("AlbumId", "Order", "ItemType", "PhotoId", "PhotoDisplayCollectionId",
                                          "CollectionItemType", "DisplaysName", "DisplaysDescription", "DisplaysYearContentCreated")
                SELECT ap."AlbumsId", ap."Order", 'PhotoDisplay', ap."PhotosId", item."Id",
                       'PhotoDisplayCollection', ap."DisplaysName", ap."DisplaysDescription", ap."DisplaysYearContentCreated"
                FROM "AlbumPhoto" AS ap
                JOIN "AlbumItems" AS item ON item."Id" = ap."AlbumPhotoDisplayItemId"
                WHERE item."ItemType" = 'PhotoDisplayCollection';
                """);

            migrationBuilder.DropTable(
                name: "AlbumPhoto");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumItems_AlbumId_PhotoDisplayCollectionId_CollectionItemT~",
                table: "AlbumItems",
                columns: new[] { "AlbumId", "PhotoDisplayCollectionId", "CollectionItemType" });

            migrationBuilder.CreateIndex(
                name: "IX_AlbumItems_PhotoId",
                table: "AlbumItems",
                column: "PhotoId");

            migrationBuilder.CreateIndex(
                name: "UX_AlbumItems_AlbumId_Order",
                table: "AlbumItems",
                columns: new[] { "AlbumId", "Order" },
                unique: true,
                filter: "\"PhotoDisplayCollectionId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "UX_AlbumItems_AlbumId_PhotoId",
                table: "AlbumItems",
                columns: new[] { "AlbumId", "PhotoId" },
                unique: true,
                filter: "\"ItemType\" = 'PhotoDisplay'");

            migrationBuilder.CreateIndex(
                name: "UX_AlbumItems_PhotoDisplayCollectionId_Order",
                table: "AlbumItems",
                columns: new[] { "PhotoDisplayCollectionId", "Order" },
                unique: true,
                filter: "\"PhotoDisplayCollectionId\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlbumItems_CollectionItemType",
                table: "AlbumItems",
                sql: "\"PhotoDisplayCollectionId\" IS NULL OR (\"CollectionItemType\" IS NOT NULL AND \"CollectionItemType\" = 'PhotoDisplayCollection')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlbumItems_PhotoDisplay_Columns",
                table: "AlbumItems",
                sql: "\"ItemType\" = 'PhotoDisplay' OR (\"PhotoId\" IS NULL AND \"PhotoDisplayCollectionId\" IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlbumItems_PhotoDisplay_Required",
                table: "AlbumItems",
                sql: "\"ItemType\" <> 'PhotoDisplay' OR (\"PhotoId\" IS NOT NULL AND \"DisplaysName\" IS NOT NULL AND \"DisplaysDescription\" IS NOT NULL AND \"DisplaysYearContentCreated\" IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlbumItems_PhotoDisplayCollection_Mode",
                table: "AlbumItems",
                sql: "\"ItemType\" <> 'PhotoDisplayCollection' OR (\"DisplayMode\" IS NOT NULL AND \"DisplayMode\" BETWEEN 0 AND 1)");

            migrationBuilder.AddForeignKey(
                name: "FK_AlbumItems_AlbumItems_AlbumId_CollectionId_ItemType",
                table: "AlbumItems",
                columns: new[] { "AlbumId", "PhotoDisplayCollectionId", "CollectionItemType" },
                principalTable: "AlbumItems",
                principalColumns: new[] { "AlbumId", "Id", "ItemType" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AlbumItems_Photos_PhotoId",
                table: "AlbumItems",
                column: "PhotoId",
                principalTable: "Photos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlbumItems_AlbumItems_AlbumId_CollectionId_ItemType",
                table: "AlbumItems");

            migrationBuilder.DropForeignKey(
                name: "FK_AlbumItems_Photos_PhotoId",
                table: "AlbumItems");

            migrationBuilder.DropIndex(
                name: "IX_AlbumItems_AlbumId_PhotoDisplayCollectionId_CollectionItemT~",
                table: "AlbumItems");

            migrationBuilder.DropIndex(
                name: "IX_AlbumItems_PhotoId",
                table: "AlbumItems");

            migrationBuilder.DropIndex(
                name: "UX_AlbumItems_AlbumId_Order",
                table: "AlbumItems");

            migrationBuilder.DropIndex(
                name: "UX_AlbumItems_AlbumId_PhotoId",
                table: "AlbumItems");

            migrationBuilder.DropIndex(
                name: "UX_AlbumItems_PhotoDisplayCollectionId_Order",
                table: "AlbumItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AlbumItems_CollectionItemType",
                table: "AlbumItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AlbumItems_PhotoDisplay_Columns",
                table: "AlbumItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AlbumItems_PhotoDisplay_Required",
                table: "AlbumItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AlbumItems_PhotoDisplayCollection_Mode",
                table: "AlbumItems");

            migrationBuilder.CreateTable(
                name: "AlbumPhoto",
                columns: table => new
                {
                    AlbumsId = table.Column<int>(type: "integer", nullable: false),
                    PhotosId = table.Column<int>(type: "integer", nullable: false),
                    AlbumPhotoDisplayItemId = table.Column<int>(type: "integer", nullable: false),
                    AlbumItemType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "PhotoDisplay"),
                    DisplaysDescription = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DisplaysName = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DisplaysYearContentCreated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumPhoto", x => new { x.AlbumsId, x.PhotosId });
                    table.CheckConstraint("CK_AlbumPhoto_ItemType_PhotoDisplay", "\"AlbumItemType\" = 'PhotoDisplay'");
                    table.CheckConstraint("CK_AlbumPhoto_Order_NonNegative", "\"Order\" >= 0");
                    table.ForeignKey(
                        name: "FK_AlbumPhoto_AlbumItems_AlbumId_ItemId_ItemType",
                        columns: x => new { x.AlbumsId, x.AlbumPhotoDisplayItemId, x.AlbumItemType },
                        principalTable: "AlbumItems",
                        principalColumns: new[] { "AlbumId", "Id", "ItemType" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumPhoto_Albums_AlbumsId",
                        column: x => x.AlbumsId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumPhoto_Photos_PhotosId",
                        column: x => x.PhotosId,
                        principalTable: "Photos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Rebuild the old association rows before removing the display-specific columns.
            migrationBuilder.Sql("""
                UPDATE "AlbumItems"
                SET "ItemType" = 'PhotoDisplay', "DisplayMode" = COALESCE("DisplayMode", 0)
                WHERE "ItemType" IN ('PhotoDisplay', 'PhotoDisplayCollection');

                INSERT INTO "AlbumPhoto" ("AlbumsId", "PhotosId", "AlbumPhotoDisplayItemId", "Order",
                                          "DisplaysName", "DisplaysDescription", "DisplaysYearContentCreated")
                SELECT "AlbumId", "PhotoId", COALESCE("PhotoDisplayCollectionId", "Id"),
                       CASE WHEN "PhotoDisplayCollectionId" IS NULL THEN 0 ELSE "Order" END,
                       "DisplaysName", "DisplaysDescription", "DisplaysYearContentCreated"
                FROM "AlbumItems" WHERE "PhotoId" IS NOT NULL;

                DELETE FROM "AlbumItems" WHERE "PhotoDisplayCollectionId" IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "CollectionItemType",
                table: "AlbumItems");

            migrationBuilder.DropColumn(
                name: "DisplaysDescription",
                table: "AlbumItems");

            migrationBuilder.DropColumn(
                name: "DisplaysName",
                table: "AlbumItems");

            migrationBuilder.DropColumn(
                name: "DisplaysYearContentCreated",
                table: "AlbumItems");

            migrationBuilder.DropColumn(
                name: "PhotoDisplayCollectionId",
                table: "AlbumItems");

            migrationBuilder.DropColumn(
                name: "PhotoId",
                table: "AlbumItems");

            migrationBuilder.CreateIndex(
                name: "UX_AlbumItems_AlbumId_Order",
                table: "AlbumItems",
                columns: new[] { "AlbumId", "Order" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlbumItems_PhotoDisplayMode_Range",
                table: "AlbumItems",
                sql: "\"ItemType\" <> 'PhotoDisplay' OR (\"DisplayMode\" IS NOT NULL AND \"DisplayMode\" BETWEEN 0 AND 1)");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumPhoto_AlbumsId_AlbumPhotoDisplayItemId_AlbumItemType",
                table: "AlbumPhoto",
                columns: new[] { "AlbumsId", "AlbumPhotoDisplayItemId", "AlbumItemType" });

            migrationBuilder.CreateIndex(
                name: "IX_AlbumPhoto_PhotosId",
                table: "AlbumPhoto",
                column: "PhotosId");

            migrationBuilder.CreateIndex(
                name: "UX_AlbumPhoto_AlbumPhotoDisplayItemId_Order",
                table: "AlbumPhoto",
                columns: new[] { "AlbumPhotoDisplayItemId", "Order" },
                unique: true);
        }
    }
}
