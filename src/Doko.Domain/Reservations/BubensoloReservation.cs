using Doko.Domain.Hands;
using Doko.Domain.Parties;
using Doko.Domain.Players;
using Doko.Domain.Rules;
using Doko.Domain.Trump;

namespace Doko.Domain.Reservations;

/// <summary>Bubensolo: only Jacks are trump.</summary>
public sealed class BubensoloReservation : IReservation
{
    private readonly PlayerId _soloPlayer;

    public BubensoloReservation(PlayerId soloPlayer) => _soloPlayer = soloPlayer;

    public ReservationPriority Priority => ReservationPriority.Bubensolo;

    public bool IsEligible(Hand hand, RuleSet rules) => rules.AllowBubensolo;

    public GameModeContext Apply() =>
        new(BubensoloTrumpEvaluator.Instance, new SoloPartyResolver(_soloPlayer));
}
