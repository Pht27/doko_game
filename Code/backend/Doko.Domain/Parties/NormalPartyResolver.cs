using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Hands;
using Doko.Domain.Players;

namespace Doko.Domain.Parties;

/// <summary>Standard party resolution: Re = players originally dealt ♣ Queen.</summary>
public sealed class NormalPartyResolver : IPartyResolver
{
    public static readonly NormalPartyResolver Instance = new();

    private static readonly CardType KreuzDame = new(Suit.Kreuz, Rank.Dame);

    public Party? ResolveParty(PlayerSeat player, GameState state)
    {
        var hands = GetInitialHands(state);
        if (hands is null)
            return null;
        return hands[player].Cards.Any(c => c.Type == KreuzDame)
            ? Party.Re
            : Party.Kontra;
    }

    public bool IsFullyResolved(GameState state) => GetInitialHands(state) is not null;

    private static IReadOnlyDictionary<PlayerSeat, Hands.Hand>? GetInitialHands(GameState state) =>
        state switch
        {
            ReservationState r => r.InitialHands,
            ArmutFlowState a => a.InitialHands,
            PlayingState p => p.InitialHands,
            ScoringState s => s.InitialHands,
            FinishedState f => f.InitialHands,
            _ => null,
        };

    public int? AnnouncementBaseDeadline(GameState state) => 5;
}
