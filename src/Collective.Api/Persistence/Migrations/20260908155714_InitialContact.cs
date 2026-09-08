using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Collective.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contact_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    company = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    reason = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    product_slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    source_page = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    ip_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contact_requests", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_contact_requests_created_at",
                table: "contact_requests",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_contact_requests_email",
                table: "contact_requests",
                column: "email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contact_requests");
        }
    }
}
