using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedHousingApp.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PaidByUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsSettled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expenses_Users_PaidByUserId",
                        column: x => x.PaidByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseUser",
                columns: table => new
                {
                    SharedExpensesId = table.Column<int>(type: "INTEGER", nullable: false),
                    SharedWithUsersId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseUser", x => new { x.SharedExpensesId, x.SharedWithUsersId });
                    table.ForeignKey(
                        name: "FK_ExpenseUser_Expenses_SharedExpensesId",
                        column: x => x.SharedExpensesId,
                        principalTable: "Expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpenseUser_Users_SharedWithUsersId",
                        column: x => x.SharedWithUsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chores_AssignedToUserId",
                table: "Chores",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_PaidByUserId",
                table: "Expenses",
                column: "PaidByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseUser_SharedWithUsersId",
                table: "ExpenseUser",
                column: "SharedWithUsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chores_Users_AssignedToUserId",
                table: "Chores",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chores_Users_AssignedToUserId",
                table: "Chores");

            migrationBuilder.DropTable(
                name: "ExpenseUser");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Chores_AssignedToUserId",
                table: "Chores");
        }
    }
}
