using Doko.Domain.Announcements;
using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Players;

namespace Doko.Application.Games.Queries;

public record PlayerGameView(
    GameId GameId,
    GamePhase Phase,
    PlayerId RequestingPlayer,
    IReadOnlyList<Card> Hand,
    IReadOnlyList<Card> LegalCards,
    IReadOnlyList<AnnouncementType> LegalAnnouncements,
    /// <summary>For each card in hand: the sonderkarten the player could activate by playing it, with display metadata.</summary>
    IReadOnlyDictionary<CardId, IReadOnlyList<SonderkarteInfo>> EligibleSonderkartenPerCard,
    IReadOnlyList<PlayerPublicState> OtherPlayers,
    TrickSummary? CurrentTrick,
    IReadOnlyList<TrickSummary> CompletedTricks,
    PlayerId CurrentTurn,
    bool IsMyTurn);
