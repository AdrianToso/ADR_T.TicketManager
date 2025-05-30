using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ADR_T.TicketManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUsuarioDeDominio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketComments_User_AutorId",
                table: "TicketComments");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_User_AsignadoUserId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_User_CreadoByUserId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_User_UserId",
                table: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "Users");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketComments_Users_AutorId",
                table: "TicketComments",
                column: "AutorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_AsignadoUserId",
                table: "Tickets",
                column: "AsignadoUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_CreadoByUserId",
                table: "Tickets",
                column: "CreadoByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Users_UserId",
                table: "UserRoles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketComments_Users_AutorId",
                table: "TicketComments");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Users_AsignadoUserId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Users_CreadoByUserId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Users_UserId",
                table: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "User");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketComments_User_AutorId",
                table: "TicketComments",
                column: "AutorId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_User_AsignadoUserId",
                table: "Tickets",
                column: "AsignadoUserId",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_User_CreadoByUserId",
                table: "Tickets",
                column: "CreadoByUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_User_UserId",
                table: "UserRoles",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
