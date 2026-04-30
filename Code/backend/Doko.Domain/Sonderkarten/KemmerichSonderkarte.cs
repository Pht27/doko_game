using Doko.Domain.Cards;
using Doko.Domain.GameFlow;

namespace Doko.Domain.Sonderkarten;

/// <summary>
/// Kemmerich: player originally held both ♥ Jacks → when playing either ♥ Jack,
/// may withdraw one announcement from their own party (only one total withdrawal allowed).
/// Which announcement to withdraw is interactive; Apply returns null — the game engine
/// prompts the player and then applies a WithdrawAnnouncementModification.
/// </summary>
public sealed class KemmerichSonderkarte : SonderkarteBase
{
    private static readonly CardType HerzBube = new(Suit.Herz, Rank.Bube);

    public override SonderkarteType Type => SonderkarteType.Kemmerich;
    public override CardType TriggeringCard => HerzBube;

    public override bool WindowClosesWhenDeclined => false;

    public override bool AreConditionsMet(PlayingState state)
    {
        if (IsActive(state, SonderkarteType.Kemmerich))
            return false;
        if (!OriginallyHeldBoth(state, HerzBube))
            return false;

        // Need at least one announcement from the current player's party to withdraw
        var playerParty = state.PartyResolver.ResolveParty(state.CurrentTurn, state);
        if (playerParty is null)
            return false;

        return state.Announcements.Any(a =>
            state.PartyResolver.ResolveParty(a.Player, state) == playerParty
        );
    }

    // Announcement withdrawal is interactive; PlayCardHandler prompts the player and
    // applies WithdrawAnnouncementModification after receiving the choice.
}
