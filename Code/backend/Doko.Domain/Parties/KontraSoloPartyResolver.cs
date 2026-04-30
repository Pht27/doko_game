using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
using Doko.Domain.Hands;
using Doko.Domain.Players;

namespace Doko.Domain.Parties;

/// <summary>
/// Kontrasolo party resolution: the Kontrasolo player is Kontra (solo); all others are Re.
/// Parties are fixed from the start — fully resolved immediately.
/// Only Re players who originally held a ♣ Queen make effective announcements; others are button-only.
/// </summary>
public sealed class KontraSoloPartyResolver(PlayerSeat kontraSoloPlayer) : IPartyResolver
{
    private static readonly CardType KreuzDame = new(Suit.Kreuz, Rank.Dame);

    public Party? ResolveParty(PlayerSeat player, GameState state) =>
        player == kontraSoloPlayer ? Party.Kontra : Party.Re;

    public bool IsFullyResolved(GameState state) => true;

    public int? AnnouncementBaseDeadline(GameState state) => 5;

    public bool IsAnnouncementEffective(PlayerSeat player, GameState state)
    {
        if (player == kontraSoloPlayer)
            return false;
        var hands = GetInitialHands(state);
        if (hands is null)
            return true;
        return hands[player].Cards.Any(c => c.Type == KreuzDame);
    }

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
}
