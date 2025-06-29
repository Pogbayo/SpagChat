using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpagChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditedMessageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientMessageId",
                table: "Messages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientMessageId",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
