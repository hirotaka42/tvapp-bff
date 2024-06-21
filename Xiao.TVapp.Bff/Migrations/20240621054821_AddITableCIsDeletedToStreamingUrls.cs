using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xiao.TVapp.Bff.Migrations
{
    /// <inheritdoc />
    public partial class AddITableCIsDeletedToStreamingUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "StreamingUrls");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StreamingUrls",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StreamingUrls");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "StreamingUrls",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
