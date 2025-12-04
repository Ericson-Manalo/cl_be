using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cl_be.Migrations
{
    /// <inheritdoc />
    public partial class AddCartItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CartItems",
                schema: "SalesLT",
                columns: table => new
                {
                    CartItemID = table.Column<int>(type: "int", nullable: false, comment: "Primary key for CartItem records.")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerID = table.Column<int>(type: "int", nullable: false, comment: "Customer who owns the cart. Foreign key to Customer.CustomerID."),
                    ProductID = table.Column<int>(type: "int", nullable: false, comment: "Product added to cart. Foreign key to Product.ProductID."),
                    Quantity = table.Column<int>(type: "int", nullable: false, comment: "Quantity of the product in the cart."),
                    UnitPrice = table.Column<decimal>(type: "money", nullable: false, comment: "Unit price at the moment the item was added to the cart."),
                    DateAdded = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())", comment: "Date the product was added to the cart.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems_CartItemID", x => x.CartItemID);
                    table.ForeignKey(
                        name: "FK_CartItems_Customer_CustomerID",
                        column: x => x.CustomerID,
                        principalSchema: "SalesLT",
                        principalTable: "Customer",
                        principalColumn: "CustomerID");
                    table.ForeignKey(
                        name: "FK_CartItems_Product_ProductID",
                        column: x => x.ProductID,
                        principalSchema: "SalesLT",
                        principalTable: "Product",
                        principalColumn: "ProductID");
                },
                comment: "Shopping cart items selected by users but not yet purchased.");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CustomerID",
                schema: "SalesLT",
                table: "CartItems",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductID",
                schema: "SalesLT",
                table: "CartItems",
                column: "ProductID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItems",
                schema: "SalesLT");
        }
    }
}
