using Doko.Domain.GameFlow;
using Doko.Domain.Parties;
using Doko.Domain.Players;
using Doko.Domain.Scoring;

namespace Doko.Domain.Announcements;

public static class AnnouncementRules
{
    /// <summary>Returns true if the player is allowed to make the given announcement in the current game state.</summary>
    public static bool CanAnnounce(PlayerSeat player, AnnouncementType type, GameState state)
    {
        if (!state.Rules.AllowAnnouncements)
            return false;

        // Kontrasolo player cannot announce at all — they already know they play solo.
        if (IsKontraSoloPlayer(player, state))
            return false;

        // Timing: each announcement shifts the deadline forward by 4 (one full trick).
        // All announcements — including button-only ones — extend the deadline to avoid info leaks.
        if (PastDeadline(state))
            return false;

        var party = state.PartyResolver.ResolveParty(player, state);
        if (party is null)
            return false;

        return state.PartyResolver.IsAnnouncementEffective(player, state)
            ? CanAnnounceEffective(type, party.Value, state)
            : CanAnnounceButtonOnly(type, party.Value, state);
    }

    /// <summary>
    /// After <see cref="AddCompletedTrickModification"/> has been applied, returns the
    /// <see cref="Announcement"/> the trick winner is required to make (Pflichtansage),
    /// or null if none applies. Only the first two tricks can trigger a Pflichtansage.
    /// </summary>
    public static Announcement? GetMandatoryAnnouncement(PlayerSeat winner, GameState state)
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

        var announced = EffectiveAnnouncementsFor(party.Value, state);

        AnnouncementType? mandatoryType = null;
        if (!announced.Contains(AnnouncementType.Win))
            mandatoryType = AnnouncementType.Win;
        else if (count == 2 && !announced.Contains(AnnouncementType.Keine90))
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

        // Feigheit does not apply in Soli (declared or silent) or forced Hochzeit solo
        if (
            state.ActiveReservation?.IsSolo == true
            || state.SilentMode is not null
            || state.HochzeitBecameForcedSolo
        )
            return false;

        // Feigheit does not apply when a Genscher changed the teams
        if (state.GenscherTeamsChanged)
            return false;

        var winner = result.Winner;
        var loserAugen = winner == Party.Re ? result.KontraAugen : result.ReAugen;
        var winnerAnnounced = EffectiveAnnouncementsFor(winner, state);

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

    // ── Private helpers ───────────────────────────────────────────────────────

    private static bool IsKontraSoloPlayer(PlayerSeat player, GameState state) =>
        state.SilentMode?.Type == SilentGameModeType.KontraSolo
        && state.SilentMode.Player == player;

    private static bool PastDeadline(GameState state)
    {
        var baseDeadline = state.PartyResolver.AnnouncementBaseDeadline(state);
        if (baseDeadline is null)
            return true;
        int cardsPlayed = state.CompletedTricks.Count * 4 + (state.CurrentTrick?.Cards.Count ?? 0);
        int deadline = baseDeadline.Value + 4 * state.Announcements.Count;
        return cardsPlayed > deadline;
    }

    private static bool CanAnnounceEffective(AnnouncementType type, Party party, GameState state)
    {
        var otherParty = party == Party.Re ? Party.Kontra : Party.Re;
        var myChain = EffectiveAnnouncementsFor(party, state);
        bool otherHasAbsage = HasAbsage(EffectiveAnnouncementsFor(otherParty, state));
        return FollowsChain(type, myChain, otherHasAbsage);
    }

    private static bool CanAnnounceButtonOnly(AnnouncementType type, Party party, GameState state)
    {
        // Button-only players share a chain so they can't detect each other via blocked moves.
        var sharedChain = ButtonOnlyAnnouncementsFor(party, state);
        return FollowsChain(type, sharedChain, otherHasAbsage: false);
    }

    private static bool FollowsChain(
        AnnouncementType type,
        ISet<AnnouncementType> chain,
        bool otherHasAbsage
    ) =>
        type switch
        {
            AnnouncementType.Win => !chain.Contains(AnnouncementType.Win),
            AnnouncementType.Keine90 => !otherHasAbsage
                && chain.Contains(AnnouncementType.Win)
                && !chain.Contains(AnnouncementType.Keine90),
            AnnouncementType.Keine60 => !otherHasAbsage
                && chain.Contains(AnnouncementType.Keine90)
                && !chain.Contains(AnnouncementType.Keine60),
            AnnouncementType.Keine30 => !otherHasAbsage
                && chain.Contains(AnnouncementType.Keine60)
                && !chain.Contains(AnnouncementType.Keine30),
            AnnouncementType.Schwarz => !otherHasAbsage
                && chain.Contains(AnnouncementType.Keine30)
                && !chain.Contains(AnnouncementType.Schwarz),
            _ => false,
        };

    private static bool HasAbsage(ISet<AnnouncementType> announcements) =>
        announcements.Contains(AnnouncementType.Keine90)
        || announcements.Contains(AnnouncementType.Keine60)
        || announcements.Contains(AnnouncementType.Keine30)
        || announcements.Contains(AnnouncementType.Schwarz);

    private static ISet<AnnouncementType> EffectiveAnnouncementsFor(Party party, GameState state) =>
        state
            .Announcements.Where(a =>
                a.IsEffective && state.PartyResolver.ResolveParty(a.Player, state) == party
            )
            .Select(a => a.Type)
            .ToHashSet();

    private static ISet<AnnouncementType> ButtonOnlyAnnouncementsFor(
        Party party,
        GameState state
    ) =>
        state
            .Announcements.Where(a =>
                !a.IsEffective && state.PartyResolver.ResolveParty(a.Player, state) == party
            )
            .Select(a => a.Type)
            .ToHashSet();
}
