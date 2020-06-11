using Microsoft.EntityFrameworkCore.Migrations;

namespace main.Core.Migrations
{
    public partial class DropIsExpiredBansColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_expired",
                table: "bans");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "is_expired",
                table: "bans",
                type: "enum('Y','N')",
                nullable: false,
                defaultValue: 'N');
        }
    }
}
