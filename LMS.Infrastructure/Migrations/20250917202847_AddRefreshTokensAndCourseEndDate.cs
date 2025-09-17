using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable
namespace LMS.Infrastructure.Migrations
{
            /// <inheritdoc />
            public partial class AddRefreshTokensAndCourseEndDate : Migration
            {
                /// <inheritdoc />
                protected override void Up(MigrationBuilder migrationBuilder)
                {
                    migrationBuilder.DropColumn(
                        name: "RefreshToken",
                        table: "ApplicationUser");
                    migrationBuilder.DropColumn(
                        name: "RefreshTokenExpireTime",
                        table: "ApplicationUser");
                    migrationBuilder.AddColumn<DateTime>(
                        name: "EndDate",
                        table: "Courses",
                        type: "datetime2",
                        nullable: true);
                    migrationBuilder.CreateTable(
                        name: "RefreshTokens",
                        columns: table => new
                        {
                            Id = table.Column<int>(type: "int", nullable: false)
                                .Annotation("SqlServer:Identity", "1, 1"),
                            UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                            TokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                            Expires = table.Column<DateTime>(type: "datetime2", nullable: false),
                            CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                            IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                            UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                            RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                            IsCompromised = table.Column<bool>(type: "bit", nullable: false),
                            ReplacedByTokenId = table.Column<int>(type: "int", nullable: true)
                        },
                        constraints: table =>
                        {
                            table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                            table.ForeignKey(
                                name: "FK_RefreshTokens_ApplicationUser_UserId",
                                column: x => x.UserId,
                                principalTable: "ApplicationUser",
                                principalColumn: "Id",
                                onDelete: ReferentialAction.Cascade);
                            table.ForeignKey(
                                name: "FK_RefreshTokens_RefreshTokens_ReplacedByTokenId",
                                column: x => x.ReplacedByTokenId,
                                principalTable: "RefreshTokens",
                                principalColumn: "Id");
                        });
                    migrationBuilder.CreateIndex(
                        name: "IX_RefreshTokens_ReplacedByTokenId",
                        table: "RefreshTokens",
                        column: "ReplacedByTokenId");
                    migrationBuilder.CreateIndex(
                        name: "IX_RefreshTokens_TokenHash",
                        table: "RefreshTokens",
                        column: "TokenHash",
                        unique: true);
                    migrationBuilder.CreateIndex(
                        name: "IX_RefreshTokens_UserId",
                        table: "RefreshTokens",
                        column: "UserId");
                }
                /// <inheritdoc />
                protected override void Down(MigrationBuilder migrationBuilder)
                {
                    migrationBuilder.DropTable(
                        name: "RefreshTokens");
                    migrationBuilder.DropColumn(
                        name: "EndDate",
                        table: "Courses");
                    migrationBuilder.AddColumn<string>(
                        name: "RefreshToken",
                        table: "ApplicationUser",
                        type: "nvarchar(max)",
                        nullable: true);
                    migrationBuilder.AddColumn<DateTime>(
                        name: "RefreshTokenExpireTime",
                        table: "ApplicationUser",
                        type: "datetime2",
                        nullable: false,
                        defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
                }
            }
}
