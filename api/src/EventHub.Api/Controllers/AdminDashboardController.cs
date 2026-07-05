using System.Globalization;
using System.Text;
using EventHub.Domain.ReadModels;
using EventHub.Application.Admin;
using EventHub.Application.Common.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Controllers;

[Authorize(Roles = "admin,moderator")]
[ApiController]
[Route("api/admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly ISender _sender;

    public AdminDashboardController(ISender sender) => _sender = sender;

    /// <summary>KPIs du tableau de bord (GET /api/admin/dashboard/overview).</summary>
    [HttpGet("dashboard/overview")]
    public async Task<ActionResult<DashboardOverviewDto>> Overview(
        CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetDashboardOverviewQuery(), cancellationToken));

    /// <summary>Statistiques d'une activité (GET /api/admin/dashboard/activities/{id}).</summary>
    [HttpGet("dashboard/activities/{id:guid}")]
    public async Task<ActionResult<ActivityDashboardDto>> ActivityDashboard(
        Guid id, CancellationToken cancellationToken)
    {
        var dto = await _sender.Send(new GetActivityDashboardQuery(id), cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    /// <summary>Export CSV des inscriptions (GET /api/admin/exports/registrations.csv).</summary>
    [HttpGet("exports/registrations.csv")]
    public async Task<IActionResult> ExportRegistrations(CancellationToken cancellationToken)
    {
        var rows = await _sender.Send(new GetRegistrationsExportQuery(), cancellationToken);

        var csv = new StringBuilder();
        csv.AppendLine("activityId,activityTitle,userId,userName,userEmail,status,registeredAt");
        foreach (var r in rows)
        {
            csv.Append(r.ActivityId).Append(',')
                .Append(Escape(r.ActivityTitle)).Append(',')
                .Append(r.UserId).Append(',')
                .Append(Escape(r.UserName)).Append(',')
                .Append(Escape(r.UserEmail)).Append(',')
                .Append(r.Status).Append(',')
                .Append(r.RegisteredAt.ToString("O", CultureInfo.InvariantCulture))
                .Append('\n');
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", "registrations.csv");
    }

    /// <summary>Échappe un champ CSV (RFC 4180 : guillemets doublés si nécessaire).</summary>
    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }
}
