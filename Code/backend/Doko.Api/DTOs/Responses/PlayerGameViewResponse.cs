namespace Doko.Api.DTOs.Responses;

public record PlayerGameViewResponse(
    string GameId,
    string Phase,
    int RequestingPlayer,
    string? OwnParty,
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
    IReadOnlyList<string> EligibleReservations
)
{
    /// <summary>True when it is this player's turn to declare health status (Gesund/Vorbehalt).</summary>
    public bool ShouldDeclareHealth { get; init; } = false;

    /// <summary>True when it is this player's turn to declare in any check phase (even if no eligible reservation — player must pass).</summary>
    public bool ShouldDeclareReservation { get; init; } = false;

    /// <summary>True when the player must declare a reservation and may not pass.</summary>
    public bool MustDeclareReservation { get; init; } = false;

    /// <summary>True when it is this player's turn to respond to an Armut partner request.</summary>
    public bool ShouldRespondToArmut { get; init; } = false;

    /// <summary>True when this player (the rich player) must return cards in the Armut exchange.</summary>
    public bool ShouldReturnArmutCards { get; init; } = false;

    /// <summary>How many cards must be returned. Null outside ArmutCardExchange phase.</summary>
    public int? ArmutCardReturnCount { get; init; } = null;

    /// <summary>Cards exchanged in Armut. Non-null after exchange completes (for the announcement).</summary>
    public int? ArmutExchangeCardCount { get; init; } = null;

    /// <summary>Whether the returned cards included trump. Non-null after exchange completes.</summary>
    public bool? ArmutReturnedTrump { get; init; } = null;

    /// <summary>The active game mode (e.g. "KaroSolo", "Hochzeit", "Armut"). Null = Normalspiel.</summary>
    public string? ActiveGameMode { get; init; } = null;

    /// <summary>True when this player must choose a solo during SchwarzesSauSoloSelect.</summary>
    public bool ShouldChooseSchwarzesSauSolo { get; init; } = false;

    /// <summary>Solos available for selection during SchwarzesSauSoloSelect. Empty otherwise.</summary>
    public IReadOnlyList<string> EligibleSchwarzesSauSolos { get; init; } = [];

    /// <summary>The requesting player's highest announcement label, or null if none made.</summary>
    public string? OwnHighestAnnouncement { get; init; } = null;

    /// <summary>Populated when phase is Finished so rejoining players see the result screen immediately.</summary>
    public GameResultDto? FinishedResult { get; init; } = null;

    /// <summary>Current new-game vote count. Populated together with FinishedResult.</summary>
    public int NewGameVoteCount { get; init; } = 0;
}
