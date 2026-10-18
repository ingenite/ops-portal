using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpsPortal.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class LinkWorkItemsToApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationId",
                table: "WorkItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_ApplicationId",
                table: "WorkItems",
                column: "ApplicationId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItems_Applications_ApplicationId",
                table: "WorkItems",
                column: "ApplicationId",
                principalTable: "Applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkItems_Applications_ApplicationId",
                table: "WorkItems");

            migrationBuilder.DropIndex(
                name: "IX_WorkItems_ApplicationId",
                table: "WorkItems");

            migrationBuilder.DropColumn(
                name: "ApplicationId",
                table: "WorkItems");
        }
    }
}