using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_waste.Migrations.ProductDB
{
    /// <inheritdoc />
    public partial class ProductCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ListedDate = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PickupLocation = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    VideoUrl = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    VideoFile = table.Column<byte[]>(type: "varbinary(MAX)", nullable: true),
                    PhotoFile = table.Column<byte[]>(type: "varbinary(MAX)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
