using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PojisteniApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class CorrectPersonFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Person",
                newName: "Street");

            migrationBuilder.RenameColumn(
                name: "ValidUntil",
                table: "Insurance",
                newName: "ValidTo");

            migrationBuilder.AlterColumn<int>(
                name: "InsuranceAmount",
                table: "Insurance",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Street",
                table: "Person",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "ValidTo",
                table: "Insurance",
                newName: "ValidUntil");

            migrationBuilder.AlterColumn<decimal>(
                name: "InsuranceAmount",
                table: "Insurance",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
