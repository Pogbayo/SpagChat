using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpagChat.Infrastructure.Migrations
{
    public partial class AddedAMessageReadByTableToKeepTrackOfReadMessagesByUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessageReadBy",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    ReadAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageReadBy", x => new { x.MessageId, x.UserId });
                    table.ForeignKey(
                        name: "FK_MessageReadBy_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "MessageId",
                        onDelete: ReferentialAction.Restrict); // <--- Restrict, not Cascade
                    // table.ForeignKey(
                    //     name: "FK_MessageReadBy_AspNetUsers_UserId",
                    //     column: x => x.UserId,
                    //     principalTable: "AspNetUsers",
                    //     principalColumn: "Id",
                    //     onDelete: ReferentialAction.Restrict); // <--- Restrict, not Cascade
                });

            // migrationBuilder.CreateIndex(
            //     name: "IX_MessageReadBy_UserId",
            //     table: "MessageReadBy",
            //     column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageReadBy");
        }
    }
}