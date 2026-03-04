using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WpfApp22.Migrations
{
    public partial class Возрат : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Поставщики",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Название = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Контакты = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Поставщики", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Склады",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Название = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Адрес = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Склады", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Товары",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Название = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Артикул = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Категория = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Товары", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Остатки",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Товар_ID = table.Column<int>(type: "int", nullable: false),
                    Склад_ID = table.Column<int>(type: "int", nullable: false),
                    Количество = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    В_Резерве = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Остатки", x => x.ID);
                    table.CheckConstraint("CHK_Остатки", "[Количество] >= 0 AND [В_Резерве] >= 0");
                    table.ForeignKey(
                        name: "FK_Остатки_Склады",
                        column: x => x.Склад_ID,
                        principalTable: "Склады",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Остатки_Товары",
                        column: x => x.Товар_ID,
                        principalTable: "Товары",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Приход",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Товар_ID = table.Column<int>(type: "int", nullable: false),
                    Склад_ID = table.Column<int>(type: "int", nullable: false),
                    Поставщик_ID = table.Column<int>(type: "int", nullable: false),
                    Количество = table.Column<int>(type: "int", nullable: false),
                    Цена = table.Column<decimal>(type: "money", nullable: false),
                    Дата = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Приход", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Приход_Поставщики",
                        column: x => x.Поставщик_ID,
                        principalTable: "Поставщики",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Приход_Склады",
                        column: x => x.Склад_ID,
                        principalTable: "Склады",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Приход_Товары",
                        column: x => x.Товар_ID,
                        principalTable: "Товары",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Расход",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Товар_ID = table.Column<int>(type: "int", nullable: false),
                    Склад_ID = table.Column<int>(type: "int", nullable: false),
                    Количество = table.Column<int>(type: "int", nullable: false),
                    Причина = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Дата = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Расход", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Расход_Склады",
                        column: x => x.Склад_ID,
                        principalTable: "Склады",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Расход_Товары",
                        column: x => x.Товар_ID,
                        principalTable: "Товары",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Остатки_Склад_ID",
                table: "Остатки",
                column: "Склад_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_Товар_Склад",
                table: "Остатки",
                columns: new[] { "Товар_ID", "Склад_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Приход_Поставщик_ID",
                table: "Приход",
                column: "Поставщик_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Приход_Склад_ID",
                table: "Приход",
                column: "Склад_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Приход_Товар_ID",
                table: "Приход",
                column: "Товар_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Расход_Склад_ID",
                table: "Расход",
                column: "Склад_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Расход_Товар_ID",
                table: "Расход",
                column: "Товар_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Склады_Название",
                table: "Склады",
                column: "Название");

            migrationBuilder.CreateIndex(
                name: "IX_Товары_Артикул",
                table: "Товары",
                column: "Артикул",
                unique: true,
                filter: "[Артикул] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Товары_Название",
                table: "Товары",
                column: "Название");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Остатки");

            migrationBuilder.DropTable(
                name: "Приход");

            migrationBuilder.DropTable(
                name: "Расход");

            migrationBuilder.DropTable(
                name: "Поставщики");

            migrationBuilder.DropTable(
                name: "Склады");

            migrationBuilder.DropTable(
                name: "Товары");
        }
    }
}
