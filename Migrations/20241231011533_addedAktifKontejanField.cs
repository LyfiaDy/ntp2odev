using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obs.Migrations
{

    public partial class addedAktifKontejanField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AktifKontenjan",
                table: "Siniflar",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AktifKontenjan",
                table: "Siniflar");
        }
    }
}
