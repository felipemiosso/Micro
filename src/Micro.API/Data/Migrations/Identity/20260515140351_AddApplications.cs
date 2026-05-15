using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Micro.API.Data.Migrations.Identity
{
    /// <inheritdoc />
    public partial class AddApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobPostingId = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CandidateEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CandidatePhone = table.Column<string>(type: "text", nullable: true),
                    ResumePath = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Applications_JobPostings_JobPostingId",
                        column: x => x.JobPostingId,
                        principalTable: "JobPostings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_JobPostingId_CandidateEmail",
                table: "Applications",
                columns: new[] { "JobPostingId", "CandidateEmail" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_Status",
                table: "Applications",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Applications");
        }
    }
}
