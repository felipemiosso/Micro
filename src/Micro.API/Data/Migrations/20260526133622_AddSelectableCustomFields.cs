using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Micro.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSelectableCustomFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsGlobal",
                table: "CustomFieldDefinitions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "JobPostingCustomFields",
                columns: table => new
                {
                    JobPostingId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomFieldDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssociatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostingCustomFields", x => new { x.JobPostingId, x.CustomFieldDefinitionId });
                    table.ForeignKey(
                        name: "FK_JobPostingCustomFields_CustomFieldDefinitions_CustomFieldDe~",
                        column: x => x.CustomFieldDefinitionId,
                        principalTable: "CustomFieldDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobPostingCustomFields_JobPostings_JobPostingId",
                        column: x => x.JobPostingId,
                        principalTable: "JobPostings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequisitionCustomFields",
                columns: table => new
                {
                    RequisitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomFieldDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssociatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequisitionCustomFields", x => new { x.RequisitionId, x.CustomFieldDefinitionId });
                    table.ForeignKey(
                        name: "FK_RequisitionCustomFields_CustomFieldDefinitions_CustomFieldD~",
                        column: x => x.CustomFieldDefinitionId,
                        principalTable: "CustomFieldDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequisitionCustomFields_Requisitions_RequisitionId",
                        column: x => x.RequisitionId,
                        principalTable: "Requisitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobPostingCustomFields_CustomFieldDefinitionId",
                table: "JobPostingCustomFields",
                column: "CustomFieldDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_RequisitionCustomFields_CustomFieldDefinitionId",
                table: "RequisitionCustomFields",
                column: "CustomFieldDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobPostingCustomFields");

            migrationBuilder.DropTable(
                name: "RequisitionCustomFields");

            migrationBuilder.DropColumn(
                name: "IsGlobal",
                table: "CustomFieldDefinitions");
        }
    }
}
