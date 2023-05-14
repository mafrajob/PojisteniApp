using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PojisteniApp2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedInsuranceAuthorId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorId",
                table: "Insurance",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "Insurance");
        }
    }
}
