using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cashback.Infra.Repository.Migrations
{
    public partial class Purchases : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Reselers",
                keyColumn: "Id",
                keyValue: new Guid("0f72550c-5bc6-48bd-bc9c-178e7f559d7c"));

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false, defaultValue: new DateTime(2020, 9, 5, 9, 16, 2, 835, DateTimeKind.Local).AddTicks(4371)),
                    Value = table.Column<decimal>(nullable: false),
                    Status = table.Column<string>(type: "CHAR(1)", nullable: false, defaultValue: "W"),
                    ResellerId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Reselers_ResellerId",
                        column: x => x.ResellerId,
                        principalTable: "Reselers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Reselers",
                columns: new[] { "Id", "AutoApproved", "CPF", "Email", "Name", "Password" },
                values: new object[] { new Guid("5a48edc3-1cf2-44d9-a86c-ee78f545e64d"), true, "153.509.460-56", "15350946056@teste.com.br", "Usuário [153.509.460-56]", "5o+mGdBwgsYcaGB4NGW5sVvFuYQ2+v+vLp5xQWkNAuQ=" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_ResellerId",
                table: "PurchaseOrders",
                column: "ResellerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseOrders");

            migrationBuilder.DeleteData(
                table: "Reselers",
                keyColumn: "Id",
                keyValue: new Guid("5a48edc3-1cf2-44d9-a86c-ee78f545e64d"));

            migrationBuilder.InsertData(
                table: "Reselers",
                columns: new[] { "Id", "AutoApproved", "CPF", "Email", "Name", "Password" },
                values: new object[] { new Guid("0f72550c-5bc6-48bd-bc9c-178e7f559d7c"), true, "153.509.460-56", "15350946056@teste.com.br", "Usuário [153.509.460-56]", "5o+mGdBwgsYcaGB4NGW5sVvFuYQ2+v+vLp5xQWkNAuQ=" });
        }
    }
}
