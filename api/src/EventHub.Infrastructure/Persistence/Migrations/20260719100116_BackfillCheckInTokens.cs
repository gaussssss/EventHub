using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Rattrapage : certaines bases ont appliqué AddParticipationCostAndCheckInToken
    /// AVANT que son backfill n'y soit ajouté (migration déjà marquée appliquée →
    /// l'édition ultérieure du fichier est ignorée par EF). Ces bases gardaient le
    /// jeton zéro par défaut, identique pour toutes les activités : QR inutilisable
    /// (le serveur rejette Guid.Empty) et surtout devinable. On régénère ici un
    /// jeton aléatoire distinct (GUID v4 via randomblob) pour chaque ligne restée
    /// à zéro. Idempotent : ne touche rien si le backfill d'origine a déjà opéré.
    /// </summary>
    public partial class BackfillCheckInTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
            // Rien à défaire : on ne restaure jamais un jeton nul.
        }
    }
}
