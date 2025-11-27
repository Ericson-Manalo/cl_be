using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cl_be.Migrations
{
    /// <inheritdoc />
    public partial class TestDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                schema: "SalesLT",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "PasswordSalt",
                schema: "SalesLT",
                table: "Customer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                schema: "SalesLT",
                table: "Customer",
                type: "varchar(128)",
                unicode: false,
                maxLength: 128,
                nullable: false,
                defaultValue: "",
                comment: "Password for the e-mail account.");

            migrationBuilder.AddColumn<string>(
                name: "PasswordSalt",
                schema: "SalesLT",
                table: "Customer",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                comment: "Random value concatenated with the password string before the password is hashed.");
        }
    }
}
