namespace Doko.Api.DTOs.Responses;

public record LobbyJoinResponse(
    string LobbyId,
    byte PlayerId,
    string Token,
    int SeatIndex
);

public record LobbyListItemResponse(string LobbyId, bool[] Seats, bool IsStarted);

public record LobbyViewResponse(string LobbyId, bool[] Seats, bool IsStarted, int[] Standings);

public record StartLobbyGameResponse(string GameId);
