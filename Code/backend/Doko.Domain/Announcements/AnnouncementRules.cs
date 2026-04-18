using Doko.Domain.GameFlow;
using Doko.Domain.Parties;
using Doko.Domain.Players;
using Doko.Domain.Scoring;

namespace Doko.Domain.Announcements;

public static class AnnouncementRules
{
    /// <summary>Returns true if the player is allowed to make the given announcement in the current game state.</summary>
    public static bool CanAnnounce(PlayerId player, AnnouncementType type, GameState state)
    {
        if (!state.Rules.AllowAnnouncements)
            return false;

        // Timing: base deadline depends on the game type (e.g. in Hochzeit it is relative to the
        // Findungsstich). Each announcement shifts the deadline forward by 4 (one full trick).
        var baseDeadline = state.PartyResolver.AnnouncementBaseDeadline(state);
        if (baseDeadline is null)
            return false;
        int totalCardsPlayed =
            state.CompletedTricks.Count * 4 + (state.CurrentTrick?.Cards.Count ?? 0);
        int totalAnnouncements = state.Announcements.Count;
        int deadline = baseDeadline.Value + 4 * totalAnnouncements;
        if (totalCardsPlayed > deadline)
            return false;

        // Party membership check
        var party = state.PartyResolver.ResolveParty(player, state);
        if (party is null)
            return false;

        var otherParty = party == Party.Re ? Party.Kontra : Party.Re;

        var partyAnnouncements = state
            .Announcements.Where(a => state.PartyResolver.ResolveParty(a.Player, state) == party)
            .Select(a => a.Type)
            .ToHashSet();

        var otherPartyAnnouncements = state
            .Announcements.Where(a =>
                state.PartyResolver.ResolveParty(a.Player, state) == otherParty
            )
            .Select(a => a.Type)
            .ToHashSet();

        // Once one party has made an Absage (Keine90+), the other party may no longer make Absagen.
        bool otherPartyHasAbsage =
            otherPartyAnnouncements.Contains(AnnouncementType.Keine90)
            || otherPartyAnnouncements.Contains(AnnouncementType.Keine60)
            || otherPartyAnnouncements.Contains(AnnouncementType.Keine30)
            || otherPartyAnnouncements.Contains(AnnouncementType.Schwarz);

        // Consecutive ordering: Win must come before Keine90, etc.
        // The player's party must have already made the preceding announcement.
        return type switch
        {
            AnnouncementType.Win => !partyAnnouncements.Contains(AnnouncementType.Win),
            AnnouncementType.Keine90 => !otherPartyHasAbsage
                && partyAnnouncements.Contains(AnnouncementType.Win)
                && !partyAnnouncements.Contains(AnnouncementType.Keine90),
            AnnouncementType.Keine60 => !otherPartyHasAbsage
                && partyAnnouncements.Contains(AnnouncementType.Keine90)
                && !partyAnnouncements.Contains(AnnouncementType.Keine60),
            AnnouncementType.Keine30 => !otherPartyHasAbsage
                && partyAnnouncements.Contains(AnnouncementType.Keine60)
                && !partyAnnouncements.Contains(AnnouncementType.Keine30),
            AnnouncementType.Schwarz => !otherPartyHasAbsage
                && partyAnnouncements.Contains(AnnouncementType.Keine30)
                && !partyAnnouncements.Contains(AnnouncementType.Schwarz),
            _ => false,
        };
    }

    /// <summary>
    /// After <see cref="AddCompletedTrickModification"/> has been applied, returns the
    /// <see cref="Announcement"/> the trick winner is required to make (Pflichtansage),
    /// or null if none applies. Only the first two tricks can trigger a Pflichtansage.
    /// </summary>
    public static Announcement? GetMandatoryAnnouncement(PlayerId winner, GameState state)
    {
        if (!state.Rules.EnforcePflichtansage)
            return null;

        int count = state.CompletedTricks.Count;
        if (count == 0 || count > 2)
            return null;

        var latestTrick = state.CompletedTricks[count - 1];
        if (latestTrick.Points < 35)
            return null;

        // Second trick requires the first trick to also have ≥ 35 Augen
        if (count == 2 && state.CompletedTricks[0].Points < 35)
            return null;

        var party = state.PartyResolver.ResolveParty(winner, state);
        if (party is null)
            return null;

        var partyAnnouncements = state
            .Announcements.Where(a => state.PartyResolver.ResolveParty(a.Player, state) == party)
            .Select(a => a.Type)
            .ToHashSet();

        AnnouncementType? mandatoryType = null;

        if (!partyAnnouncements.Contains(AnnouncementType.Win))
            mandatoryType = AnnouncementType.Win;
        else if (count == 2 && !partyAnnouncements.Contains(AnnouncementType.Keine90))
            mandatoryType = AnnouncementType.Keine90;

        if (mandatoryType is null)
            return null;

        int trickNum = count - 1;
        return new Announcement(winner, mandatoryType.Value, trickNum, 0) { IsMandatory = true };
    }

    /// <summary>Returns true if the winning party violated the Feigheit (cowardice) rule.</summary>
    public static bool ViolatesFeigheit(GameResult result, GameState state)
    {
        if (!state.Rules.EnforceFeigheit)
            return false;

        // Feigheit does not apply in Soli
        if (state.ActiveReservation is not null)
            return false;

        var winner = result.Winner;
        var loserAugen = winner == Party.Re ? result.KontraAugen : result.ReAugen;

        var winnerAnnounced = state
            .Announcements.Where(a => state.PartyResolver.ResolveParty(a.Player, state) == winner)
            .Select(a => a.Type)
            .ToHashSet();

        int missing = 0;
        if (loserAugen < 120 && !winnerAnnounced.Contains(AnnouncementType.Win))
            missing++;
        if (loserAugen < 90 && !winnerAnnounced.Contains(AnnouncementType.Keine90))
            missing++;
        if (loserAugen < 60 && !winnerAnnounced.Contains(AnnouncementType.Keine60))
            missing++;
        if (loserAugen < 30 && !winnerAnnounced.Contains(AnnouncementType.Keine30))
            missing++;

        bool loserWonNoTricks = !state.CompletedTricks.Any(t =>
            state.PartyResolver.ResolveParty(
                t.Winner(state.TrumpEvaluator, state.Rules.DulleRule),
                state
            ) != winner
        );

        if (loserWonNoTricks && !winnerAnnounced.Contains(AnnouncementType.Schwarz))
            missing++;

        return missing > 2;
    }
}
