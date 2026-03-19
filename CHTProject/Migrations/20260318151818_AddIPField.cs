using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CHTProject.Migrations
{
    /// <inheritdoc />
    public partial class AddIPField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IP",
                table: "Logs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IP",
                table: "Logs");
        }
    }
}
