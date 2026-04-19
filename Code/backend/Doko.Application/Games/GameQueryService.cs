using Doko.Application.Abstractions;
using Doko.Application.Games.Queries;
using Doko.Domain.Announcements;
using Doko.Domain.Cards;
using Doko.Domain.Extrapunkte;
using Doko.Domain.GameFlow;
using Doko.Domain.Hands;
using Doko.Domain.Parties;
using Doko.Domain.Players;
using Doko.Domain.Reservations;
using Doko.Domain.Rules;
using Doko.Domain.Scoring;
using Doko.Domain.Sonderkarten;
using Doko.Domain.Tricks;

namespace Doko.Application.Games;

public sealed class GameQueryService(IGameRepository repository) : IGameQueryService
{
    public async Task<PlayerGameView?> GetPlayerViewAsync(
        GameId gameId,
        PlayerSeat requestingPlayer,
        CancellationToken ct = default
    )
    {
        var state = await repository.GetAsync(gameId, ct);
        if (state is null)
            return null;

        var playerState = state.Players.FirstOrDefault(p => p.Seat == requestingPlayer);
        if (playerState is null)
            return null;

        var hand = playerState.Hand.Cards;
        bool isMyTurn = state.Phase == GamePhase.Playing && state.CurrentTurn == requestingPlayer;

        // Own party — null when not yet known (e.g. Hochzeit before Findungsstich)
        var ownParty = state.PartyResolver.ResolveParty(requestingPlayer, state);

        // Legal cards: only relevant when it's this player's turn
        IReadOnlyList<Card> legalCards = [];
        if (isMyTurn && state.CurrentTrick is not null)
        {
            var trick = state.CurrentTrick ?? new Trick();
            legalCards = hand.Where(c =>
                    CardPlayValidator.CanPlay(c, playerState.Hand, trick, state.TrumpEvaluator)
                )
                .ToList();
        }
        else if (isMyTurn)
        {
            // Leading a new trick — all cards are legal
            legalCards = hand.ToList();
        }

        // Legal announcements
        var legalAnnouncements =
            state.Phase == GamePhase.Playing
                ? Enum.GetValues<AnnouncementType>()
                    .Where(t => AnnouncementRules.CanAnnounce(requestingPlayer, t, state))
                    .ToList()
                : (IReadOnlyList<AnnouncementType>)[];

        // Eligible sonderkarten per card in hand — include display metadata
        var eligiblePerCard = new Dictionary<CardId, IReadOnlyList<SonderkarteInfo>>();
        if (isMyTurn)
        {
            foreach (var card in hand)
            {
                var eligible = SonderkarteRegistry
                    .GetEligibleForCard(card, state, state.Rules)
                    .Select(s => SonderkarteInfo.For(s.Type))
                    .ToList();
                if (eligible.Count > 0)
                    eligiblePerCard[card.Id] = eligible;
            }
        }

        // Parties are revealed immediately in: all solos, Armut, and Hochzeit once partner found.
        // In Normalspiel and pre-Findungsstich Hochzeit parties stay hidden until announced.
        bool revealParties =
            state.ActiveReservation is not null
            && (state.ActiveReservation.IsSolo || state.PartyResolver.IsFullyResolved(state));

        // Active game mode label (null = Normalspiel)
        var activeGameMode = state.ActiveReservation?.Priority.ToString();

        // Other players' public state
        var others = state
            .Players.Where(p => p.Seat != requestingPlayer)
            .Select(p =>
            {
                // Reveal party when mode dictates it, or when the player has announced
                var hasAnnounced = state.Announcements.Any(a => a.Player == p.Seat);
                var knownParty = revealParties
                    ? state.PartyResolver.ResolveParty(p.Seat, state)
                    : p.KnownParty
                        ?? (hasAnnounced ? state.PartyResolver.ResolveParty(p.Seat, state) : null);

                // Most specific announcement: highest enum value wins
                var highestAnn = state
                    .Announcements.Where(a => a.Player == p.Seat)
                    .MaxBy(a => a.Type);
                string? announcementLabel = null;
                if (highestAnn is not null)
                {
                    announcementLabel =
                        highestAnn.Type == AnnouncementType.Win
                            ? knownParty == Party.Re
                                ? "Re"
                                : knownParty == Party.Kontra
                                    ? "Kontra"
                                    : null
                            : highestAnn.Type.ToString();
                }

                return new PlayerPublicState(
                    p.Seat,
                    knownParty,
                    p.Hand.Cards.Count,
                    announcementLabel
                );
            })
            .ToList();

        // Current trick summary
        TrickSummary? currentTrickSummary = state.CurrentTrick is { Cards.Count: > 0 }
            ? ToCurrentTrickSummary(state.CompletedTricks.Count, state.CurrentTrick, state)
            : null;

        // Completed tricks summaries
        var completedSummaries = state
            .ScoredTricks.Select((r, i) => ToCompletedTrickSummary(i, r))
            .ToList();

        // Hand sorted by trump (highest to lowest), then plain suits grouped by suit and sorted
        var handSorted = hand.OrderByDescending(c => state.TrumpEvaluator.IsTrump(c.Type))
            .ThenByDescending(c =>
                state.TrumpEvaluator.IsTrump(c.Type) ? state.TrumpEvaluator.GetTrumpRank(c.Type) : 0
            )
            .ThenBy(c => (int)c.Type.Suit)
            .ThenByDescending(c =>
                state.TrumpEvaluator.IsTrump(c.Type) ? 0 : state.TrumpEvaluator.GetPlainRank(c.Type)
            )
            .ToList();

        // Health check
        bool shouldDeclareHealth =
            state.Phase == GamePhase.ReservationHealthCheck
            && state.PendingReservationResponders.Count > 0
            && state.PendingReservationResponders[0] == requestingPlayer;

        // Eligible reservations in a check phase (only when it's this player's turn)
        bool isCheckPhaseTurn =
            state.Phase
                is GamePhase.ReservationSoloCheck
                    or GamePhase.ReservationArmutCheck
                    or GamePhase.ReservationSchmeissenCheck
                    or GamePhase.ReservationHochzeitCheck
            && state.PendingReservationResponders.Count > 0
            && state.PendingReservationResponders[0] == requestingPlayer;

        bool singleVorbehalt = state.HealthDeclarations.Count(kv => kv.Value) == 1;
        bool mustDeclareReservation =
            isCheckPhaseTurn && state.Phase == GamePhase.ReservationSoloCheck && singleVorbehalt;

        IReadOnlyList<ReservationPriority> eligibleReservations = [];
        if (isCheckPhaseTurn)
        {
            eligibleReservations = state.Phase switch
            {
                GamePhase.ReservationSoloCheck when singleVorbehalt =>
                    ReservationRegistry.GetEligible(
                        requestingPlayer,
                        playerState.Hand,
                        state.Rules
                    ),
                GamePhase.ReservationSoloCheck => ReservationRegistry.GetEligibleSolos(
                    requestingPlayer,
                    playerState.Hand,
                    state.Rules
                ),
                GamePhase.ReservationArmutCheck => ReservationRegistry.GetEligibleArmut(
                    requestingPlayer,
                    playerState.Hand,
                    state.Rules
                ),
                GamePhase.ReservationSchmeissenCheck => ReservationRegistry.GetEligibleSchmeissen(
                    requestingPlayer,
                    playerState.Hand,
                    state.Rules
                ),
                GamePhase.ReservationHochzeitCheck => ReservationRegistry.GetEligibleHochzeit(
                    requestingPlayer,
                    playerState.Hand,
                    state.Rules
                ),
                _ => [],
            };
        }

        // Armut partner finding
        bool shouldRespondToArmut =
            state.Phase == GamePhase.ArmutPartnerFinding
            && state.PendingReservationResponders.Count > 0
            && state.PendingReservationResponders[0] == requestingPlayer;

        // Armut card exchange
        bool shouldReturnArmutCards =
            state.Phase == GamePhase.ArmutCardExchange && state.ArmutRichPlayer == requestingPlayer;
        int? armutCardReturnCount = shouldReturnArmutCards ? state.ArmutTransferCount : null;

        // Armut exchange announcement — shown to all players after exchange completes
        int? armutExchangeCardCount = state.ArmutReturnedTrump.HasValue
            ? state.ArmutTransferCount
            : null;
        bool? armutReturnedTrump = state.ArmutReturnedTrump;

        return new PlayerGameView(
            gameId,
            state.Phase,
            requestingPlayer,
            ownParty,
            hand,
            legalCards,
            legalAnnouncements,
            eligiblePerCard,
            others,
            currentTrickSummary,
            completedSummaries,
            state.CurrentTurn,
            isMyTurn
        )
        {
            HandSorted = handSorted,
            ShouldDeclareHealth = shouldDeclareHealth,
            ShouldDeclareReservation = isCheckPhaseTurn,
            EligibleReservations = eligibleReservations,
            MustDeclareReservation = mustDeclareReservation,
            ShouldRespondToArmut = shouldRespondToArmut,
            ShouldReturnArmutCards = shouldReturnArmutCards,
            ArmutCardReturnCount = armutCardReturnCount,
            ArmutExchangeCardCount = armutExchangeCardCount,
            ArmutReturnedTrump = armutReturnedTrump,
            ActiveGameMode = activeGameMode,
        };
    }

    private static TrickSummary ToCompletedTrickSummary(int trickNumber, TrickResult result)
    {
        bool hasFischauge = result.Awards.Any(a => a.Type == ExtrapunktType.Fischauge);

        var cards = result
            .Trick.Cards.Select(tc =>
            {
                bool faceDown = hasFischauge && tc.Card.Type == KaroNeun;
                return new TrickCardSummary(tc.Player, tc.Card, faceDown);
            })
            .ToList();

        return new TrickSummary(trickNumber, cards, result.Winner);
    }

    private static readonly CardType KaroNeun = new(Suit.Karo, Rank.Neun);

    /// <summary>
    /// Builds a TrickSummary for the current (incomplete) trick, computing per-card FaceDown flags.
    /// A ♦9 is shown face-down when: Fischauge is active, it is not the first card in the trick,
    /// and all previously played cards in the trick are Fehl (not trump).
    /// </summary>
    private static TrickSummary ToCurrentTrickSummary(int trickNumber, Trick trick, GameState state)
    {
        bool fischaugeActive = state.CompletedTricks.Any(t =>
            t.Cards.Any(tc => state.TrumpEvaluator.IsTrump(tc.Card.Type))
        );

        var cards = trick
            .Cards.Select(
                (tc, index) =>
                {
                    bool faceDown =
                        fischaugeActive
                        && tc.Card.Type == KaroNeun
                        && index > 0
                        && trick
                            .Cards.Take(index)
                            .All(prev => !state.TrumpEvaluator.IsTrump(prev.Card.Type));

                    return new TrickCardSummary(tc.Player, tc.Card, faceDown);
                }
            )
            .ToList();

        return new TrickSummary(trickNumber, cards, null);
    }
}
