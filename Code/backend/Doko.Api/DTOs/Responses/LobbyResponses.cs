namespace Doko.Api.DTOs.Responses;

public record LobbyJoinResponse(
    string LobbyId,
    string Token,
    int SeatIndex,
    string? ActiveGameId = null
);

public record LobbyListItemResponse(string LobbyId, bool[] Seats, bool IsStarted);

public record LobbyViewResponse(
    string LobbyId,
    bool[] Seats,
    bool IsStarted,
    int[] Standings,
    int StartVoteCount = 0,
    string? ActiveGameId = null,
    int[]? OpaSeats = null
);

public record StartLobbyGameResponse(string GameId);
