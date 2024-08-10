using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eComStoreWeb.Migrations
{
    public partial class MakeGenreOptional : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Genre_GenreId",
                table: "Products");

            migrationBuilder.AlterColumn<int>(
                name: "GenreId",
                table: "Products",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Genre_GenreId",
                table: "Products",
                column: "GenreId",
                principalTable: "Genre",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Genre_GenreId",
                table: "Products");

            migrationBuilder.AlterColumn<int>(
                name: "GenreId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Genre_GenreId",
                table: "Products",
                column: "GenreId",
                principalTable: "Genre",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
