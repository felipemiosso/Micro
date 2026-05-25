using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Micro.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStageSpecificData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "InterviewScheduledDate",
                table: "Applications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InterviewerName",
                table: "Applications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OfferDeadline",
                table: "Applications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OfferProposedSalary",
                table: "Applications",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OfferTargetStartDate",
                table: "Applications",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InterviewScheduledDate",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "InterviewerName",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "OfferDeadline",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "OfferProposedSalary",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "OfferTargetStartDate",
                table: "Applications");
        }
    }
}
