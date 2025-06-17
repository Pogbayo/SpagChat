using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpagChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReaddedRowVersionToChatRoomUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ChatRoomUsers",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ChatRoomUsers");
        }
    }
}
