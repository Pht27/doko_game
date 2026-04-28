using Doko.Domain.Cards;
using Doko.Domain.Hands;
using Doko.Domain.Parties;
using Doko.Domain.Players;
using Doko.Domain.Rules;
using Doko.Domain.Trump;

namespace Doko.Domain.Reservations;

/// <summary>
/// Hochzeit: player holds both ♣ Queens; they name a condition (first trick / first Fehl /
/// first trump) — the first other player to win that type of trick becomes the partner (Re party).
/// If no partner found in 3 qualifying tricks, becomes Stille Hochzeit (solo).
/// </summary>
public sealed class HochzeitReservation : IReservation
{
    private readonly PlayerSeat _hochzeitPlayer;
    private readonly HochzeitCondition _condition;

    private static readonly CardType KreuzDame = new(Suit.Kreuz, Rank.Dame);

    public HochzeitReservation(PlayerSeat hochzeitPlayer, HochzeitCondition condition)
    {
        _hochzeitPlayer = hochzeitPlayer;
        _condition = condition;
    }

    public ReservationPriority Priority => ReservationPriority.Hochzeit;

    public bool IsEligible(Hand hand, RuleSet rules) =>
        rules.AllowHochzeit && hand.Cards.Count(c => c.Type == KreuzDame) >= 2;

    public GameModeContext BuildContext() =>
        new(NormalTrumpEvaluator.Instance, new HochzeitPartyResolver(_hochzeitPlayer, _condition));
}
