using Doko.Domain.Hands;
using Doko.Domain.Parties;
using Doko.Domain.Players;
using Doko.Domain.Rules;
using Doko.Domain.Trump;

namespace Doko.Domain.Reservations;

/// <summary>
/// Schlanker Martin: normal trump rules, but no Sonderkarten or Extrapunkte, and
/// tie-breaking is reversed (second identical card beats first). Goal: fewest tricks.
/// Has the lowest sub-priority within Soli.
/// </summary>
public sealed class SchlankerMartinReservation : IReservation
{
    private readonly PlayerId _soloPlayer;

    public SchlankerMartinReservation(PlayerId soloPlayer) => _soloPlayer = soloPlayer;

    public ReservationPriority Priority => ReservationPriority.SchlankerMartin;

    public bool IsEligible(Hand hand, RuleSet rules) => rules.AllowSchlankerMartin;

    public GameModeContext Apply()
        => new(NormalTrumpEvaluator.Instance, new SoloPartyResolver(_soloPlayer));
}
