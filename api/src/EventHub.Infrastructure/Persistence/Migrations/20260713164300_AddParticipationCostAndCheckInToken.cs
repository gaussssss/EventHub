using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddParticipationCostAndCheckInToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CheckInToken",
                table: "Activities",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "ParticipationCost",
                table: "Activities",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            // Backfill : chaque activité existante reçoit un jeton ALÉATOIRE distinct.
            // Sans cela, toutes partageraient le jeton zéro par défaut — devinable,
            // donc émargement forgeable. (GUID v4 généré côté SQLite via randomblob.)
            migrationBuilder.Sql(
                "UPDATE Activities SET CheckInToken = " +
                "lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || " +
                "substr(lower(hex(randomblob(2))), 2) || '-' || " +
                "substr('89ab', (abs(random()) % 4) + 1, 1) || substr(lower(hex(randomblob(2))), 2) || '-' || " +
                "lower(hex(randomblob(6))) " +
                "WHERE CheckInToken = '00000000-0000-0000-0000-000000000000';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckInToken",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ParticipationCost",
                table: "Activities");
        }
    }
}
