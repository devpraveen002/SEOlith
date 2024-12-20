using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SEOlith.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "seo_audits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WebsiteUrl = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    HasSitemap = table.Column<bool>(type: "boolean", nullable: false),
                    HasRobotsTxt = table.Column<bool>(type: "boolean", nullable: false),
                    HeadingStructure = table.Column<List<string>>(type: "jsonb", nullable: false),
                    ImageCount = table.Column<int>(type: "integer", nullable: false),
                    BrokenLinksCount = table.Column<int>(type: "integer", nullable: false),
                    LoadTime = table.Column<double>(type: "double precision", nullable: false),
                    IsMobileResponsive = table.Column<bool>(type: "boolean", nullable: false),
                    LastChecked = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seo_audits", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_seo_audits_LastChecked",
                table: "seo_audits",
                column: "LastChecked");

            migrationBuilder.CreateIndex(
                name: "IX_seo_audits_WebsiteUrl",
                table: "seo_audits",
                column: "WebsiteUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "seo_audits");
        }
    }
}
