using Doko.Domain.Lobby;

namespace Doko.Application.Lobbies.Queries;

public record LobbyView(LobbyId LobbyId, int PlayerCount, bool IsFull, bool IsStarted);
