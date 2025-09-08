using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedHousingApp.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRepeatWeeklyFromChores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RepeatWeekly",
                table: "Chores");

            migrationBuilder.AlterColumn<string>(
                name: "EventType",
                table: "CalendarEvents",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                table: "CalendarEvents",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RepeatWeekly",
                table: "Chores",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "EventType",
                table: "CalendarEvents",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                table: "CalendarEvents",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
