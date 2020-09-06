using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cashback.Infra.Repository.Migrations
{
    public partial class Reseller : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reselers",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CPF = table.Column<string>(type: "CHAR(14)", nullable: false),
                    Name = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    Email = table.Column<string>(type: "VARCHAR(250)", nullable: false),
                    Password = table.Column<string>(type: "VARCHAR(64)", nullable: false),
                    AutoApproved = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reselers", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Reselers",
                columns: new[] { "Id", "AutoApproved", "CPF", "Email", "Name", "Password" },
                values: new object[] { new Guid("0f72550c-5bc6-48bd-bc9c-178e7f559d7c"), true, "153.509.460-56", "15350946056@teste.com.br", "Usuário [153.509.460-56]", "5o+mGdBwgsYcaGB4NGW5sVvFuYQ2+v+vLp5xQWkNAuQ=" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reselers");
        }
    }
}
