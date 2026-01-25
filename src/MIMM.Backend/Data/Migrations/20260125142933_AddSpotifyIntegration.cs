using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MIMM.Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSpotifyIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SpotifyAccessToken",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SpotifyConnectedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpotifyRefreshToken",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpotifyState",
                table: "Users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SpotifyStateExpiresAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SpotifyTokenExpiresAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpotifyUserId",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpotifyId",
                table: "Entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpotifyUri",
                table: "Entries",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MusicBrainzArtists",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicBrainzArtists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MusicBrainzRecordings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ArtistId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    ArtistName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ReleaseId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    ReleaseTitle = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CoverUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicBrainzRecordings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MusicBrainzReleases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ReleaseDate = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CoverArtUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicBrainzReleases", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MusicBrainzRecordings_ArtistId",
                table: "MusicBrainzRecordings",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicBrainzRecordings_ReleaseId",
                table: "MusicBrainzRecordings",
                column: "ReleaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MusicBrainzArtists");

            migrationBuilder.DropTable(
                name: "MusicBrainzRecordings");

            migrationBuilder.DropTable(
                name: "MusicBrainzReleases");

            migrationBuilder.DropColumn(
                name: "SpotifyAccessToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SpotifyConnectedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SpotifyRefreshToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SpotifyState",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SpotifyStateExpiresAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SpotifyTokenExpiresAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SpotifyUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SpotifyId",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "SpotifyUri",
                table: "Entries");
        }
    }
}
