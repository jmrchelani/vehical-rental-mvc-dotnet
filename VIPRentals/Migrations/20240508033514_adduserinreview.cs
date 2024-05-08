using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VIPRentals.Migrations
{
    /// <inheritdoc />
    public partial class adduserinreview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "User",
                table: "Review",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "User",
                table: "Review");
        }
    }
}
