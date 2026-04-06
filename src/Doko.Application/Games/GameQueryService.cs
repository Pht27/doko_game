using Doko.Application.Abstractions;
using Doko.Application.Games.Queries;
using Doko.Domain.Announcements;
using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Hands;
using Doko.Domain.Parties;
using Doko.Domain.Players;
using Doko.Domain.Reservations;
using Doko.Domain.Rules;
using Doko.Domain.Sonderkarten;
using Doko.Domain.Tricks;

namespace Doko.Application.Games;

public sealed class GameQueryService(IGameRepository repository) : IGameQueryService
{
    public async Task<PlayerGameView?> GetPlayerViewAsync(
        GameId gameId,
        PlayerId requestingPlayer,
        CancellationToken ct = default
    )
    {
        var state = await repository.GetAsync(gameId, ct);
        if (state is null)
            return null;

        var playerState = state.Players.FirstOrDefault(p => p.Id == requestingPlayer);
        if (playerState is null)
            return null;

        var hand = playerState.Hand.Cards;
        bool isMyTurn = state.Phase == GamePhase.Playing && state.CurrentTurn == requestingPlayer;

        // Legal cards: only relevant when it's this player's turn
        IReadOnlyList<Card> legalCards = [];
        if (isMyTurn && state.CurrentTrick is not null or { Cards.Count: 0 })
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

        // Other players' public state
        var others = state
            .Players.Where(p => p.Id != requestingPlayer)
            .Select(p => new PlayerPublicState(p.Id, p.Seat, p.KnownParty, p.Hand.Cards.Count))
            .ToList();

        // Current trick summary
        TrickSummary? currentTrickSummary = state.CurrentTrick is { Cards.Count: > 0 }
            ? ToTrickSummary(state.CompletedTricks.Count, state.CurrentTrick, null)
            : null;

        // Completed tricks summaries
        var completedSummaries = state
            .ScoredTricks.Select((r, i) => ToTrickSummary(i, r.Trick, r.Winner))
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
            isCheckPhaseTurn
            && state.Phase == GamePhase.ReservationSoloCheck
            && singleVorbehalt;

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
                GamePhase.ReservationSoloCheck =>
                    ReservationRegistry.GetEligibleSolos(requestingPlayer, playerState.Hand, state.Rules),
                GamePhase.ReservationArmutCheck =>
                    ReservationRegistry.GetEligibleArmut(requestingPlayer, playerState.Hand, state.Rules),
                GamePhase.ReservationSchmeissenCheck =>
                    ReservationRegistry.GetEligibleSchmeissen(requestingPlayer, playerState.Hand, state.Rules),
                GamePhase.ReservationHochzeitCheck =>
                    ReservationRegistry.GetEligibleHochzeit(requestingPlayer, playerState.Hand, state.Rules),
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
            state.Phase == GamePhase.ArmutCardExchange
            && state.ArmutRichPlayer == requestingPlayer;
        int? armutCardReturnCount = shouldReturnArmutCards ? state.ArmutTransferCount : null;

        return new PlayerGameView(
            gameId,
            state.Phase,
            requestingPlayer,
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
            EligibleReservations = eligibleReservations,
            MustDeclareReservation = mustDeclareReservation,
            ShouldRespondToArmut = shouldRespondToArmut,
            ShouldReturnArmutCards = shouldReturnArmutCards,
            ArmutCardReturnCount = armutCardReturnCount,
        };
    }

    private static TrickSummary ToTrickSummary(int trickNumber, Trick trick, PlayerId? winner)
    {
        var cards = trick.Cards.Select(tc => new TrickCardSummary(tc.Player, tc.Card)).ToList();
        return new TrickSummary(trickNumber, cards, winner);
    }
}
