using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Doko.Api.Hubs;

[Authorize]
public class GameHub : Hub
{
    public async Task JoinGame(string gameId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
    }

    public async Task LeaveGame(string gameId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId);
    }

    public async Task JoinLobby(string lobbyId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"lobby_{lobbyId}");
    }

    public async Task LeaveLobby(string lobbyId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"lobby_{lobbyId}");
    }
}
