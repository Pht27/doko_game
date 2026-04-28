using Doko.Domain.Hands;
using Doko.Domain.Parties;
using Doko.Domain.Players;
using Doko.Domain.Rules;
using Doko.Domain.Trump;

namespace Doko.Domain.Reservations;

/// <summary>Damensolo: only Queens are trump.</summary>
public sealed class DamensoloReservation : IReservation
{
    private readonly PlayerSeat _soloPlayer;

    public DamensoloReservation(PlayerSeat soloPlayer) => _soloPlayer = soloPlayer;

    public ReservationPriority Priority => ReservationPriority.Damensolo;

    public bool IsEligible(Hand hand, RuleSet rules) => rules.AllowDamensolo;

    public GameModeContext BuildContext() =>
        new(DamensoloTrumpEvaluator.Instance, new SoloPartyResolver(_soloPlayer));
}
