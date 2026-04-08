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
        if (totalCardsPlayed >= deadline)
            return false;

        // Party membership check
        var party = state.PartyResolver.ResolveParty(player, state);
        if (party is null)
            return false;

        var expectedType = party == Party.Re ? AnnouncementType.Re : AnnouncementType.Kontra;

        // Consecutive ordering: Re/Kontra must come before Keine90, etc.
        // The player's party must have already made the preceding announcement.
        var partyAnnouncements = state
            .Announcements.Where(a => state.PartyResolver.ResolveParty(a.Player, state) == party)
            .Select(a => a.Type)
            .ToHashSet();

        switch (type)
        {
            case AnnouncementType.Re or AnnouncementType.Kontra:
                if (type != expectedType)
                    return false;
                return !partyAnnouncements.Contains(type);

            case AnnouncementType.Keine90:
                return partyAnnouncements.Contains(expectedType)
                    && !partyAnnouncements.Contains(AnnouncementType.Keine90);

            case AnnouncementType.Keine60:
                return partyAnnouncements.Contains(AnnouncementType.Keine90)
                    && !partyAnnouncements.Contains(AnnouncementType.Keine60);

            case AnnouncementType.Keine30:
                return partyAnnouncements.Contains(AnnouncementType.Keine60)
                    && !partyAnnouncements.Contains(AnnouncementType.Keine30);

            case AnnouncementType.Schwarz:
                return partyAnnouncements.Contains(AnnouncementType.Keine30)
                    && !partyAnnouncements.Contains(AnnouncementType.Schwarz);

            default:
                return false;
        }
    }

    /// <summary>Returns true if the player is required to announce Re or Kontra (Pflichtansage rule).</summary>
    public static bool IsMandatory(PlayerId player, GameState state)
    {
        if (!state.Rules.EnforcePflichtansage)
            return false;
        if (state.CompletedTricks.Count == 0)
            return false;

        var party = state.PartyResolver.ResolveParty(player, state);
        if (party is null)
            return false;

        var partyAnnouncements = state
            .Announcements.Where(a => state.PartyResolver.ResolveParty(a.Player, state) == party)
            .Select(a => a.Type)
            .ToHashSet();

        var expectedBase = party == Party.Re ? AnnouncementType.Re : AnnouncementType.Kontra;

        // First trick ≥ 35: winner must announce Re/Kontra
        var firstTrick = state.CompletedTricks[0];
        if (firstTrick.Points >= 35)
        {
            var firstWinner = firstTrick.Winner(state.TrumpEvaluator, state.Rules.DulleRule);
            if (firstWinner == player && !partyAnnouncements.Contains(expectedBase))
                return true;
        }

        // Second trick ≥ 35: winner must announce the next level consecutively
        if (state.CompletedTricks.Count >= 2)
        {
            var secondTrick = state.CompletedTricks[1];
            if (secondTrick.Points >= 35)
            {
                var secondWinner = secondTrick.Winner(state.TrumpEvaluator, state.Rules.DulleRule);
                if (secondWinner == player)
                {
                    // Must announce next level: Keine90 if same party already announced base, else base
                    if (!partyAnnouncements.Contains(expectedBase))
                        return true;
                    if (!partyAnnouncements.Contains(AnnouncementType.Keine90))
                        return true;
                }
            }
        }

        return false;
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
        var loserPoints = winner == Party.Re ? result.KontraPoints : result.RePoints;

        var winnerAnnounced = state
            .Announcements.Where(a => state.PartyResolver.ResolveParty(a.Player, state) == winner)
            .Select(a => a.Type)
            .ToHashSet();

        var baseAnnouncement = winner == Party.Re ? AnnouncementType.Re : AnnouncementType.Kontra;

        int missing = 0;
        if (loserPoints < 120 && !winnerAnnounced.Contains(baseAnnouncement))
            missing++;
        if (loserPoints < 90 && !winnerAnnounced.Contains(AnnouncementType.Keine90))
            missing++;
        if (loserPoints < 60 && !winnerAnnounced.Contains(AnnouncementType.Keine60))
            missing++;
        if (loserPoints < 30 && !winnerAnnounced.Contains(AnnouncementType.Keine30))
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
