using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PojisteniApp2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedImageData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "Person",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "Person");
        }
    }
}
