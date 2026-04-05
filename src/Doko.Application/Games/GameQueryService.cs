using Doko.Application.Abstractions;
using Doko.Application.Games.Queries;
using Doko.Domain.Announcements;
using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Players;
using Doko.Domain.Rules;
using Doko.Domain.Sonderkarten;
using Doko.Domain.Tricks;
using Doko.Application.Games.Queries;

namespace Doko.Application.Games;

public sealed class GameQueryService(IGameRepository repository) : IGameQueryService
{
    public async Task<PlayerGameView?> GetPlayerViewAsync(
        GameId gameId, PlayerId requestingPlayer, CancellationToken ct = default)
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
            legalCards = hand
                .Where(c => CardPlayValidator.CanPlay(c, playerState.Hand, trick, state.TrumpEvaluator))
                .ToList();
        }
        else if (isMyTurn)
        {
            // Leading a new trick — all cards are legal
            legalCards = hand.ToList();
        }

        // Legal announcements
        var legalAnnouncements = state.Phase == GamePhase.Playing
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
                var eligible = SonderkarteRegistry.GetEligibleForCard(card, state, state.Rules)
                    .Select(s => SonderkarteInfo.For(s.Type))
                    .ToList();
                if (eligible.Count > 0)
                    eligiblePerCard[card.Id] = eligible;
            }
        }

        // Other players' public state
        var others = state.Players
            .Where(p => p.Id != requestingPlayer)
            .Select(p => new PlayerPublicState(
                p.Id,
                p.Seat,
                p.KnownParty,
                p.Hand.Cards.Count))
            .ToList();

        // Current trick summary
        TrickSummary? currentTrickSummary = state.CurrentTrick is { Cards.Count: > 0 }
            ? ToTrickSummary(state.CompletedTricks.Count, state.CurrentTrick, null)
            : null;

        // Completed tricks summaries
        var completedSummaries = state.ScoredTricks
            .Select((r, i) => ToTrickSummary(i, r.Trick, r.Winner))
            .ToList();

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
            isMyTurn);
    }

    private static TrickSummary ToTrickSummary(int trickNumber, Trick trick, PlayerId? winner)
    {
        var cards = trick.Cards
            .Select(tc => new TrickCardSummary(tc.Player, tc.Card))
            .ToList();
        return new TrickSummary(trickNumber, cards, winner);
    }
}
