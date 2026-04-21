using Doko.Domain.Cards;
using Doko.Domain.GameFlow;
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
        if (state.InitialHands is null)
            return true;
        return state.InitialHands[player].Cards.Any(c => c.Type == KreuzDame);
    }
}
