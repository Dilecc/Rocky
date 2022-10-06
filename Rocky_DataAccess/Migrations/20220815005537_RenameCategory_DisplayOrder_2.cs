using Microsoft.EntityFrameworkCore.Migrations;

namespace Rocky.Migrations
{
    public partial class RenameCategory_DisplayOrder_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DispalyOrder2",
                table: "Category",
                newName: "DispalyOrder");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DispalyOrder",
                table: "Category",
                newName: "DispalyOrder2");
        }
    }
}
