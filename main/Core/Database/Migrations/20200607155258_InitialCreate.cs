using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.Data.EntityFrameworkCore.Metadata;

namespace main.Core.Database.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bans",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint(20)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    userid = table.Column<long>(type: "bigint(20)", nullable: false),
                    name = table.Column<string>(unicode: false, maxLength: 256, nullable: true),
                    by_userid = table.Column<long>(type: "bigint(20)", nullable: false),
                    by_name = table.Column<string>(unicode: false, maxLength: 256, nullable: true),
                    expires_on = table.Column<DateTime>(nullable: true),
                    is_expired = table.Column<string>(type: "enum('Y','N')", nullable: false, defaultValueSql: "'N'"),
                    reason = table.Column<string>(unicode: false, maxLength: 256, nullable: true),
                    banned_on = table.Column<DateTimeOffset>(nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "verifications",
                columns: table => new
                {
                    userid = table.Column<long>(type: "bigint(20)", nullable: false),
                    forum_id = table.Column<int>(type: "int(11)", nullable: true),
                    forum_name = table.Column<string>(unicode: false, maxLength: 128, nullable: true),
                    verified_by = table.Column<string>(unicode: false, maxLength: 256, nullable: true),
                    verified_on = table.Column<DateTime>(nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    deleted_on = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.userid);
                });

            migrationBuilder.CreateIndex(
                name: "uid",
                table: "bans",
                column: "userid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bans");

            migrationBuilder.DropTable(
                name: "verifications");
        }
    }
}
