using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpagChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CorrectedChatRoomUserPrimaryKeyTypo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChatUserRoomUserId",
                table: "ChatRoomUsers",
                newName: "ChatRoomUserId");

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

            migrationBuilder.RenameColumn(
                name: "ChatRoomUserId",
                table: "ChatRoomUsers",
                newName: "ChatUserRoomUserId");
        }
    }
}
