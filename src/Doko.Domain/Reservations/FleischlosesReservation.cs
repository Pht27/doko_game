using Doko.Domain.Hands;
using Doko.Domain.Parties;
using Doko.Domain.Players;
using Doko.Domain.Rules;
using Doko.Domain.Trump;

namespace Doko.Domain.Reservations;

/// <summary>Fleischloses (Nullo): no trump; plain rank A > 10 > K > Q > J > 9.</summary>
public sealed class FleischlosesReservation : IReservation
{
    private readonly PlayerId _soloPlayer;

    public FleischlosesReservation(PlayerId soloPlayer) => _soloPlayer = soloPlayer;

    public ReservationPriority Priority => ReservationPriority.Fleischloses;

    public bool IsEligible(Hand hand, RuleSet rules) => rules.AllowFleischloses;

    public GameModeContext Apply()
        => new(NoTrumpEvaluator.Instance, new SoloPartyResolver(_soloPlayer));
}
