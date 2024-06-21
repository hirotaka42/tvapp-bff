using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xiao.TVapp.Bff.Migrations
{
    /// <inheritdoc />
    public partial class AddNullableCreatedAtToStreamingUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "StreamingUrls",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "StreamingUrls",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StreamingUrls_EpisodeId",
                table: "StreamingUrls",
                column: "EpisodeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StreamingUrls_EpisodeId",
                table: "StreamingUrls");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "StreamingUrls");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "StreamingUrls");
        }
    }
}
