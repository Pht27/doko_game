namespace Doko.Api.DTOs.Responses;

public record LobbyJoinResponse(
    string LobbyId,
    byte PlayerId,
    bool IsHost,
    string Token,
    int PlayerCount
);

public record LobbyViewResponse(string LobbyId, int PlayerCount, bool IsFull, bool IsStarted);

public record StartLobbyGameResponse(string GameId);
