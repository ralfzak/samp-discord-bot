using Microsoft.EntityFrameworkCore.Migrations;

namespace domain.Migrations
{
    public partial class AddLiftedBansColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "lifted",
                table: "bans",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lifted",
                table: "bans");
        }
    }
}
