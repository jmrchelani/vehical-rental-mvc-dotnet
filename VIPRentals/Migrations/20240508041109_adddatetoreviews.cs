using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VIPRentals.Migrations
{
    /// <inheritdoc />
    public partial class adddatetoreviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Date",
                table: "Review",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "Review");
        }
    }
}
