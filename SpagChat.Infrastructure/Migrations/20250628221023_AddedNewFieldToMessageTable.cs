using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpagChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedNewFieldToMessageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientMessageId",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientMessageId",
                table: "Messages");
        }
    }
}
