using Microsoft.AspNetCore.SignalR;

namespace EventHub.Api.Hubs;

/// <summary>
/// Hub temps réel EventHub. Le serveur pousse des évènements aux clients :
/// place libérée (liste d'attente), cœurs gagnés, nouveau commentaire, etc.
/// Les méthodes de diffusion s'ajouteront au fil des slices métier.
/// </summary>
public class NotificationsHub : Hub
{
}
