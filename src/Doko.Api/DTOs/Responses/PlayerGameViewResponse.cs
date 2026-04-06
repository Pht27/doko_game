namespace Doko.Api.DTOs.Responses;

public record PlayerGameViewResponse(
    string GameId,
    string Phase,
    int RequestingPlayer,
    IReadOnlyList<CardDto> Hand,
    IReadOnlyList<CardDto> HandSorted,
    IReadOnlyList<CardDto> LegalCards,
    IReadOnlyList<string> LegalAnnouncements,
    IReadOnlyDictionary<int, IReadOnlyList<SonderkarteInfoDto>> EligibleSonderkartenPerCard,
    IReadOnlyList<PlayerPublicStateDto> OtherPlayers,
    TrickSummaryDto? CurrentTrick,
    IReadOnlyList<TrickSummaryDto> CompletedTricks,
    int CurrentTurn,
    bool IsMyTurn,
    IReadOnlyList<string> EligibleReservations);
