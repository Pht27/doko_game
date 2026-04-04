using Doko.Domain.Hands;
using Doko.Domain.Parties;
using Doko.Domain.Players;
using Doko.Domain.Rules;
using Doko.Domain.Trump;

namespace Doko.Domain.Reservations;

/// <summary>
/// Knochenloses: no trump; plain rank A > K > Q > J > 10 > 9 (tens are low).
/// Goal: fewest tricks. Game ends immediately when solo player wins a trick.
/// </summary>
public sealed class KnochenlosesReservation : IReservation
{
    private readonly PlayerId _soloPlayer;

    public KnochenlosesReservation(PlayerId soloPlayer) => _soloPlayer = soloPlayer;

    public ReservationPriority Priority => ReservationPriority.Knochenloses;

    public bool IsEligible(Hand hand, RuleSet rules) => rules.AllowKnochenloses;

    public GameModeContext Apply()
        => new(KnochenloseTrumpEvaluator.Instance, new SoloPartyResolver(_soloPlayer));
}
