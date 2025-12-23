using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cl_be.Migrations
{
    /// <inheritdoc />
    public partial class AddCartAndCartItemTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cart_Customer_CustomerId",
                table: "Cart");

            migrationBuilder.DropForeignKey(
                name: "FK_Cart_Product_ProductId",
                table: "Cart");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cart",
                table: "Cart");

            migrationBuilder.DropIndex(
                name: "IX_Cart_ProductId",
                table: "Cart");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Cart");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Cart");

            migrationBuilder.RenameTable(
                name: "Cart",
                newName: "Cart",
                newSchema: "SalesLT");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                schema: "SalesLT",
                table: "Cart",
                newName: "CustomerID");

            migrationBuilder.RenameColumn(
                name: "CartId",
                schema: "SalesLT",
                table: "Cart",
                newName: "CartID");

            migrationBuilder.RenameIndex(
                name: "IX_Cart_CustomerId",
                schema: "SalesLT",
                table: "Cart",
                newName: "IX_Cart_CustomerID");

            migrationBuilder.AlterTable(
                name: "Cart",
                schema: "SalesLT",
                comment: "Shopping cart for customers.");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerID",
                schema: "SalesLT",
                table: "Cart",
                type: "int",
                nullable: false,
                comment: "Customer identification number. Foreign key to Customer.CustomerID.",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CartID",
                schema: "SalesLT",
                table: "Cart",
                type: "int",
                nullable: false,
                comment: "Primary key for Cart records.",
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "SalesLT",
                table: "Cart",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                comment: "Date and time the cart was created.");

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                schema: "SalesLT",
                table: "Cart",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                comment: "Date and time the cart was last updated.");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cart_CartID",
                schema: "SalesLT",
                table: "Cart",
                column: "CartID");

            migrationBuilder.CreateTable(
                name: "CartItem",
                schema: "SalesLT",
                columns: table => new
                {
                    CartItemID = table.Column<int>(type: "int", nullable: false, comment: "Primary key for CartItem records.")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CartID = table.Column<int>(type: "int", nullable: false, comment: "Foreign key to Cart.CartID."),
                    ProductID = table.Column<int>(type: "int", nullable: false, comment: "Foreign key to Product.ProductID."),
                    Quantity = table.Column<int>(type: "int", nullable: false, comment: "Quantity of product in cart."),
                    AddedDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())", comment: "Date and time the item was added to cart.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItem_CartItemID", x => x.CartItemID);
                    table.ForeignKey(
                        name: "FK_CartItem_Cart_CartID",
                        column: x => x.CartID,
                        principalSchema: "SalesLT",
                        principalTable: "Cart",
                        principalColumn: "CartID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItem_Product_ProductID",
                        column: x => x.ProductID,
                        principalSchema: "SalesLT",
                        principalTable: "Product",
                        principalColumn: "ProductID");
                },
                comment: "Individual products in a shopping cart.");

            migrationBuilder.CreateIndex(
                name: "IX_CartItem_CartID",
                schema: "SalesLT",
                table: "CartItem",
                column: "CartID");

            migrationBuilder.CreateIndex(
                name: "IX_CartItem_ProductID",
                schema: "SalesLT",
                table: "CartItem",
                column: "ProductID");

            migrationBuilder.AddForeignKey(
                name: "FK_Cart_Customer_CustomerID",
                schema: "SalesLT",
                table: "Cart",
                column: "CustomerID",
                principalSchema: "SalesLT",
                principalTable: "Customer",
                principalColumn: "CustomerID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cart_Customer_CustomerID",
                schema: "SalesLT",
                table: "Cart");

            migrationBuilder.DropTable(
                name: "CartItem",
                schema: "SalesLT");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cart_CartID",
                schema: "SalesLT",
                table: "Cart");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "SalesLT",
                table: "Cart");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                schema: "SalesLT",
                table: "Cart");

            migrationBuilder.RenameTable(
                name: "Cart",
                schema: "SalesLT",
                newName: "Cart");

            migrationBuilder.RenameColumn(
                name: "CustomerID",
                table: "Cart",
                newName: "CustomerId");

            migrationBuilder.RenameColumn(
                name: "CartID",
                table: "Cart",
                newName: "CartId");

            migrationBuilder.RenameIndex(
                name: "IX_Cart_CustomerID",
                table: "Cart",
                newName: "IX_Cart_CustomerId");

            migrationBuilder.AlterTable(
                name: "Cart",
                oldComment: "Shopping cart for customers.");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Cart",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Customer identification number. Foreign key to Customer.CustomerID.");

            migrationBuilder.AlterColumn<int>(
                name: "CartId",
                table: "Cart",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Primary key for Cart records.")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "Cart",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Cart",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cart",
                table: "Cart",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_Cart_ProductId",
                table: "Cart",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cart_Customer_CustomerId",
                table: "Cart",
                column: "CustomerId",
                principalSchema: "SalesLT",
                principalTable: "Customer",
                principalColumn: "CustomerID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cart_Product_ProductId",
                table: "Cart",
                column: "ProductId",
                principalSchema: "SalesLT",
                principalTable: "Product",
                principalColumn: "ProductID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
