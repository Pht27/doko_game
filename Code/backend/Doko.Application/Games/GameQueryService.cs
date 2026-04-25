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

        return new PlayerGameView(
            gameId,
            state.Phase,
            requestingPlayer,
            GetAnnouncementDisplayParty(requestingPlayer, state),
            hand,
            BuildLegalCards(playerState, hand, isMyTurn, state),
            BuildLegalAnnouncements(requestingPlayer, state),
            BuildEligibleSonderkarten(hand, isMyTurn, state),
            BuildOtherPlayers(requestingPlayer, state),
            BuildCurrentTrickSummary(state),
            state.ScoredTricks.Select((r, i) => ToCompletedTrickSummary(i, r)).ToList(),
            state.CurrentTurn,
            isMyTurn
        )
        {
            HandSorted = BuildHandSorted(hand, state),
            ShouldDeclareHealth = BuildShouldDeclareHealth(requestingPlayer, state),
            ShouldDeclareReservation = IsCheckPhaseTurn(requestingPlayer, state),
            EligibleReservations = BuildEligibleReservations(requestingPlayer, playerState, state),
            MustDeclareReservation = BuildMustDeclareReservation(requestingPlayer, state),
            ShouldRespondToArmut = BuildShouldRespondToArmut(requestingPlayer, state),
            ShouldReturnArmutCards =
                state.Phase == GamePhase.ArmutCardExchange
                && state.ArmutRichPlayer == requestingPlayer,
            ArmutCardReturnCount = BuildArmutCardReturnCount(requestingPlayer, state),
            ArmutExchangeCardCount = state.ArmutReturnedTrump.HasValue
                ? state.ArmutTransferCount
                : null,
            ArmutReturnedTrump = state.ArmutReturnedTrump,
            ActiveGameMode = state.ActiveReservation?.Priority.ToString(),
            ShouldChooseSchwarzesSauSolo = BuildShouldChooseSchwarzesSauSolo(
                requestingPlayer,
                state
            ),
            EligibleSchwarzesSauSolos = BuildEligibleSchwarzesSauSolos(requestingPlayer, state),
        };
    }

    private static IReadOnlyList<Card> BuildLegalCards(
        PlayerState playerState,
        IReadOnlyList<Card> hand,
        bool isMyTurn,
        GameState state
    )
    {
        if (!isMyTurn)
            return [];
        if (state.CurrentTrick is null)
            return hand.ToList();
        return hand.Where(c =>
                CardPlayValidator.CanPlay(
                    c,
                    playerState.Hand,
                    state.CurrentTrick,
                    state.TrumpEvaluator
                )
            )
            .ToList();
    }

    private static IReadOnlyList<AnnouncementType> BuildLegalAnnouncements(
        PlayerSeat player,
        GameState state
    )
    {
        if (state.Phase != GamePhase.Playing)
            return [];
        return Enum.GetValues<AnnouncementType>()
            .Where(t => AnnouncementRules.CanAnnounce(player, t, state))
            .ToList();
    }

    private static Dictionary<CardId, IReadOnlyList<SonderkarteInfo>> BuildEligibleSonderkarten(
        IReadOnlyList<Card> hand,
        bool isMyTurn,
        GameState state
    )
    {
        if (!isMyTurn)
            return [];
        var result = new Dictionary<CardId, IReadOnlyList<SonderkarteInfo>>();
        foreach (var card in hand)
        {
            var eligible = SonderkarteRegistry
                .GetEligibleForCard(card, state, state.Rules)
                .Select(s => SonderkarteInfo.For(s.Type))
                .ToList();
            if (eligible.Count > 0)
                result[card.Id] = eligible;
        }
        return result;
    }

    private static List<PlayerPublicState> BuildOtherPlayers(
        PlayerSeat requestingPlayer,
        GameState state
    )
    {
        // Parties are revealed immediately in solos, Armut, and Hochzeit once partner found.
        // In Normalspiel and pre-Findungsstich Hochzeit parties stay hidden until announced.
        bool revealParties =
            state.ActiveReservation is not null
            && (state.ActiveReservation.IsSolo || state.PartyResolver.IsFullyResolved(state));

        return state
            .Players.Where(p => p.Seat != requestingPlayer)
            .Select(p =>
            {
                var hasAnnounced = state.Announcements.Any(a => a.Player == p.Seat);
                var displayParty = GetAnnouncementDisplayParty(p.Seat, state);
                var knownParty = revealParties
                    ? displayParty
                    : p.KnownParty ?? (hasAnnounced ? displayParty : null);

                var highestAnn = state
                    .Announcements.Where(a => a.Player == p.Seat)
                    .MaxBy(a => a.Type);
                string? announcementLabel = highestAnn?.Type switch
                {
                    AnnouncementType.Win => knownParty switch
                    {
                        Party.Re => "Re",
                        Party.Kontra => "Kontra",
                        _ => null,
                    },
                    null => null,
                    var t => t.ToString(),
                };

                return new PlayerPublicState(
                    p.Seat,
                    knownParty,
                    p.Hand.Cards.Count,
                    announcementLabel
                );
            })
            .ToList();
    }

    private static TrickSummary? BuildCurrentTrickSummary(GameState state) =>
        state.CurrentTrick is { Cards.Count: > 0 }
            ? ToCurrentTrickSummary(state.CompletedTricks.Count, state.CurrentTrick, state)
            : null;

    private static IReadOnlyList<Card> BuildHandSorted(IReadOnlyList<Card> hand, GameState state) =>
        hand.OrderByDescending(c => state.TrumpEvaluator.IsTrump(c.Type))
            .ThenByDescending(c =>
                state.TrumpEvaluator.IsTrump(c.Type) ? state.TrumpEvaluator.GetTrumpRank(c.Type) : 0
            )
            .ThenBy(c => (int)c.Type.Suit)
            .ThenByDescending(c =>
                state.TrumpEvaluator.IsTrump(c.Type) ? 0 : state.TrumpEvaluator.GetPlainRank(c.Type)
            )
            .ToList();

    private static bool BuildShouldDeclareHealth(PlayerSeat player, GameState state) =>
        state.Phase == GamePhase.ReservationHealthCheck
        && state.PendingReservationResponders.Count > 0
        && state.PendingReservationResponders[0] == player;

    private static bool IsCheckPhaseTurn(PlayerSeat player, GameState state) =>
        state.Phase
            is GamePhase.ReservationSoloCheck
                or GamePhase.ReservationArmutCheck
                or GamePhase.ReservationSchmeissenCheck
                or GamePhase.ReservationHochzeitCheck
        && state.PendingReservationResponders.Count > 0
        && state.PendingReservationResponders[0] == player;

    private static bool BuildMustDeclareReservation(PlayerSeat player, GameState state)
    {
        bool singleVorbehalt = state.HealthDeclarations.Count(kv => kv.Value) == 1;
        return IsCheckPhaseTurn(player, state)
            && state.Phase == GamePhase.ReservationSoloCheck
            && singleVorbehalt;
    }

    private static IReadOnlyList<ReservationPriority> BuildEligibleReservations(
        PlayerSeat player,
        PlayerState playerState,
        GameState state
    )
    {
        if (!IsCheckPhaseTurn(player, state))
            return [];

        bool singleVorbehalt = state.HealthDeclarations.Count(kv => kv.Value) == 1;
        return state.Phase switch
        {
            GamePhase.ReservationSoloCheck when singleVorbehalt => ReservationRegistry.GetEligible(
                player,
                playerState.Hand,
                state.Rules
            ),
            GamePhase.ReservationSoloCheck => ReservationRegistry.GetEligibleSolos(
                player,
                playerState.Hand,
                state.Rules
            ),
            GamePhase.ReservationArmutCheck => ReservationRegistry.GetEligibleArmut(
                player,
                playerState.Hand,
                state.Rules
            ),
            GamePhase.ReservationSchmeissenCheck => ReservationRegistry.GetEligibleSchmeissen(
                player,
                playerState.Hand,
                state.Rules
            ),
            GamePhase.ReservationHochzeitCheck => ReservationRegistry.GetEligibleHochzeit(
                player,
                playerState.Hand,
                state.Rules
            ),
            _ => [],
        };
    }

    private static bool BuildShouldRespondToArmut(PlayerSeat player, GameState state) =>
        state.Phase == GamePhase.ArmutPartnerFinding
        && state.PendingReservationResponders.Count > 0
        && state.PendingReservationResponders[0] == player;

    private static int? BuildArmutCardReturnCount(PlayerSeat player, GameState state) =>
        state.Phase == GamePhase.ArmutCardExchange && state.ArmutRichPlayer == player
            ? state.ArmutTransferCount
            : null;

    private static bool BuildShouldChooseSchwarzesSauSolo(PlayerSeat player, GameState state) =>
        state.Phase == GamePhase.SchwarzesSauSoloSelect && state.CurrentTurn == player;

    /// <summary>
    /// All solo priorities are eligible in Schwarze Sau — no hand restriction.
    /// Returns the full list only for the player whose turn it is; empty for observers.
    /// </summary>
    private static IReadOnlyList<ReservationPriority> BuildEligibleSchwarzesSauSolos(
        PlayerSeat player,
        GameState state
    )
    {
        if (!BuildShouldChooseSchwarzesSauSolo(player, state))
            return [];

        return Enum.GetValues<ReservationPriority>()
            .Where(p => (int)p <= (int)ReservationPriority.SchlankerMartin)
            .ToList();
    }

    /// In Kontrasolo, non-solo players see their Kreuz-Dame-based party for announcement labels.
    /// They don't know they're "Re" in the Kontrasolo sense; the button should reflect normal rules.
    private static Party? GetAnnouncementDisplayParty(PlayerSeat player, GameState state)
    {
        if (
            state.SilentMode?.Type == SilentGameModeType.KontraSolo
            && state.SilentMode.Player != player
        )
            return NormalPartyResolver.Instance.ResolveParty(player, state);
        return state.PartyResolver.ResolveParty(player, state);
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
