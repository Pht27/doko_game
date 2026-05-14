using Doko.Analog.Entities;
using Microsoft.EntityFrameworkCore;

namespace Doko.Analog.Services;

public class AnalogRoundsService(AnalogDbContext db)
{
    public async Task<RoundListPage> GetRoundsAsync(
        int page,
        int pageSize,
        CancellationToken ct = default
    )
    {
        var total = await db.Rounds.CountAsync(ct);

        var rounds = await db
            .Rounds.AsNoTracking()
            .OrderByDescending(r => r.PlayedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new
            {
                r.Id,
                r.PlayedAt,
                r.WinningParty,
                r.Points,
                GameMode = r.GameMode.Name,
                RePlayers = r
                    .Teams.Where(t => t.Party == Party.Re)
                    .SelectMany(t => t.Members.OrderBy(m => m.Position))
                    .Select(m => new PlayerInfo(m.Player.Id, m.Player.Name))
                    .ToList(),
                KontraPlayers = r
                    .Teams.Where(t => t.Party == Party.Kontra)
                    .SelectMany(t => t.Members.OrderBy(m => m.Position))
                    .Select(m => new PlayerInfo(m.Player.Id, m.Player.Name))
                    .ToList(),
                Comment = (string?)
                    db.Comments.Where(c => c.RoundId == r.Id).Select(c => c.Text).FirstOrDefault(),
            })
            .ToListAsync(ct);

        var items = rounds
            .Select(r => new RoundListItem(
                r.Id,
                r.PlayedAt,
                r.WinningParty,
                r.Points,
                r.GameMode,
                r.RePlayers.ToArray(),
                r.KontraPlayers.ToArray(),
                r.Comment
            ))
            .ToArray();

        return new RoundListPage(total, page, items);
    }

    public async Task<RoundDetail?> GetRoundAsync(int id, CancellationToken ct = default)
    {
        var round = await db
            .Rounds.AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new
            {
                r.Id,
                r.PlayedAt,
                r.WinningParty,
                r.Points,
                GameMode = new GameModeInfo(r.GameMode.Id, r.GameMode.Name, r.GameMode.IsSolo),
                Teams = r
                    .Teams.Select(t => new
                    {
                        t.Party,
                        Players = t
                            .Members.OrderBy(m => m.Position)
                            .Select(m => new PlayerInfo(m.Player.Id, m.Player.Name))
                            .ToList(),
                        SpecialCards = db
                            .RoundSpecialCards.Where(rsc => rsc.RoundId == id && rsc.TeamId == t.Id)
                            .Select(rsc => new SpecialCardInfo(
                                rsc.SpecialCard.Id,
                                rsc.SpecialCard.Name
                            ))
                            .ToList(),
                        ExtraPoints = db
                            .RoundExtraPoints.Where(rep => rep.RoundId == id && rep.TeamId == t.Id)
                            .Select(rep => new ExtraPointEntry(
                                rep.ExtraPoint.Id,
                                rep.ExtraPoint.Name,
                                rep.Count
                            ))
                            .ToList(),
                    })
                    .ToList(),
                Comment = (string?)
                    db.Comments.Where(c => c.RoundId == id).Select(c => c.Text).FirstOrDefault(),
            })
            .FirstOrDefaultAsync(ct);

        if (round is null)
            return null;

        var teams = round
            .Teams.Select(t => new RoundTeamDetail(
                t.Party,
                t.Players.ToArray(),
                t.SpecialCards.ToArray(),
                t.ExtraPoints.ToArray()
            ))
            .ToArray();

        return new RoundDetail(
            round.Id,
            round.PlayedAt,
            round.WinningParty,
            round.Points,
            round.GameMode,
            teams,
            round.Comment
        );
    }

    public async Task<bool> AnyPlayerInactiveAsync(int[] playerIds, CancellationToken ct = default)
    {
        var ids = playerIds.ToList();
        return await db.Players.Where(p => ids.Contains(p.Id) && p.IsActive).CountAsync(ct)
            != playerIds.Length;
    }

    public async Task<string?> PlausibilityErrorAsync(
        RoundInput input,
        CancellationToken ct = default
    )
    {
        var gameMode = await db
            .GameModes.AsNoTracking()
            .FirstOrDefaultAsync(gm => gm.Id == input.GameModeId, ct);
        if (gameMode is null)
            return "invalid_game_mode";

        var reCount = input.Teams.Where(t => t.Party == Party.Re).Sum(t => t.PlayerIds.Length);
        var kontraCount = input
            .Teams.Where(t => t.Party == Party.Kontra)
            .Sum(t => t.PlayerIds.Length);

        if (gameMode.IsSolo)
        {
            var soloCount = gameMode.SoloParty == Party.Re ? reCount : kontraCount;
            var otherCount = gameMode.SoloParty == Party.Re ? kontraCount : reCount;
            if (soloCount != 1 || otherCount != 3)
                return "solo_party_distribution_invalid";
        }
        else
        {
            if (reCount != 2 || kontraCount != 2)
                return "normal_party_distribution_invalid";
        }

        var allSpecialCardIds = input.Teams.SelectMany(t => t.SpecialCardIds).ToHashSet();

        if (
            allSpecialCardIds.Contains(SpecialCardIds.Hyperschweinchen)
            && !allSpecialCardIds.Contains(SpecialCardIds.Superschweinchen)
        )
            return "hyperschweinchen_requires_superschweinchen";
        if (
            allSpecialCardIds.Contains(SpecialCardIds.Superschweinchen)
            && !allSpecialCardIds.Contains(SpecialCardIds.Schweinchen)
        )
            return "superschweinchen_requires_schweinchen";
        if (
            allSpecialCardIds.Contains(SpecialCardIds.Gegengenscherdamen)
            && !allSpecialCardIds.Contains(SpecialCardIds.Genscherdamen)
        )
            return "gegengenscherdamen_requires_genscherdamen";
        if (
            allSpecialCardIds.Contains(SpecialCardIds.Heidfrau)
            && !allSpecialCardIds.Contains(SpecialCardIds.Heidmann)
        )
            return "heidfrau_requires_heidmann";

        int ExtraCount(int id) =>
            input
                .Teams.SelectMany(t => t.ExtraPoints)
                .Where(ep => ep.ExtraPointId == id)
                .Sum(ep => ep.Count);

        if (ExtraCount(ExtraPointIds.Karlchen) + ExtraCount(ExtraPointIds.Agathe) > 1)
            return "karlchen_agathe_limit_exceeded";
        if (ExtraCount(ExtraPointIds.Kaffeekranzchen) > 2)
            return "kaffeekranzchen_limit_exceeded";
        if (ExtraCount(ExtraPointIds.KlabautermannGefangen) > 2)
            return "klabautermann_limit_exceeded";
        if (ExtraCount(ExtraPointIds.GansGefangen) + ExtraCount(ExtraPointIds.Fischauge) > 2)
            return "gans_fischauge_limit_exceeded";
        if (ExtraCount(ExtraPointIds.FuchsGefangen) > 2)
            return "fuchs_gefangen_limit_exceeded";

        return null;
    }

    private static class SpecialCardIds
    {
        public const int Gegengenscherdamen = 1;
        public const int Genscherdamen = 2;
        public const int Heidfrau = 3;
        public const int Heidmann = 4;
        public const int Hyperschweinchen = 5;
        public const int Schweinchen = 8;
        public const int Superschweinchen = 9;
    }

    private static class ExtraPointIds
    {
        public const int Agathe = 1;
        public const int Fischauge = 3;
        public const int FuchsGefangen = 4;
        public const int GansGefangen = 5;
        public const int Kaffeekranzchen = 6;
        public const int Karlchen = 7;
        public const int KlabautermannGefangen = 8;
    }

    public async Task<AnalogRound> CreateRoundAsync(
        RoundInput input,
        CancellationToken ct = default
    )
    {
        var round = new AnalogRound
        {
            PlayedAt = input.PlayedAt,
            WinningParty = input.WinningParty,
            Points = input.Points,
            GameModeId = input.GameModeId,
        };
        db.Rounds.Add(round);
        await db.SaveChangesAsync(ct);

        await InsertTeamsAsync(round.Id, input.Teams, ct);

        if (!string.IsNullOrEmpty(input.Comment))
        {
            db.Comments.Add(new AnalogComment { RoundId = round.Id, Text = input.Comment });
            await db.SaveChangesAsync(ct);
        }

        return round;
    }

    public async Task<AnalogRound?> UpdateRoundAsync(
        int id,
        RoundInput input,
        CancellationToken ct = default
    )
    {
        var round = await db.Rounds.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (round is null)
            return null;

        await DeleteRoundRelationsAsync(id, ct);

        round.PlayedAt = input.PlayedAt;
        round.WinningParty = input.WinningParty;
        round.Points = input.Points;
        round.GameModeId = input.GameModeId;
        await db.SaveChangesAsync(ct);

        await InsertTeamsAsync(id, input.Teams, ct);

        if (!string.IsNullOrEmpty(input.Comment))
        {
            db.Comments.Add(new AnalogComment { RoundId = id, Text = input.Comment });
            await db.SaveChangesAsync(ct);
        }

        return round;
    }

    public async Task<bool> DeleteRoundAsync(int id, CancellationToken ct = default)
    {
        var round = await db.Rounds.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (round is null)
            return false;

        await DeleteRoundRelationsAsync(id, ct);
        db.Rounds.Remove(round);
        await db.SaveChangesAsync(ct);
        return true;
    }

    private async Task InsertTeamsAsync(int roundId, TeamInput[] teams, CancellationToken ct)
    {
        foreach (var teamInput in teams)
        {
            var team = new AnalogTeam { RoundId = roundId, Party = teamInput.Party };
            db.Teams.Add(team);
            await db.SaveChangesAsync(ct);

            for (int i = 0; i < teamInput.PlayerIds.Length; i++)
            {
                db.TeamMembers.Add(
                    new AnalogTeamMember
                    {
                        TeamId = team.Id,
                        PlayerId = teamInput.PlayerIds[i],
                        Position = i,
                    }
                );
            }

            foreach (var scId in teamInput.SpecialCardIds)
            {
                db.RoundSpecialCards.Add(
                    new AnalogRoundSpecialCard
                    {
                        RoundId = roundId,
                        TeamId = team.Id,
                        SpecialCardId = scId,
                    }
                );
            }

            foreach (var ep in teamInput.ExtraPoints)
            {
                db.RoundExtraPoints.Add(
                    new AnalogRoundExtraPoint
                    {
                        RoundId = roundId,
                        TeamId = team.Id,
                        ExtraPointId = ep.ExtraPointId,
                        Count = ep.Count,
                    }
                );
            }

            await db.SaveChangesAsync(ct);
        }
    }

    private async Task DeleteRoundRelationsAsync(int roundId, CancellationToken ct)
    {
        var teamIds = await db
            .Teams.Where(t => t.RoundId == roundId)
            .Select(t => t.Id)
            .ToListAsync(ct);

        var members = await db.TeamMembers.Where(m => teamIds.Contains(m.TeamId)).ToListAsync(ct);
        db.TeamMembers.RemoveRange(members);

        var specialCards = await db
            .RoundSpecialCards.Where(rsc => rsc.RoundId == roundId)
            .ToListAsync(ct);
        db.RoundSpecialCards.RemoveRange(specialCards);

        var extraPoints = await db
            .RoundExtraPoints.Where(rep => rep.RoundId == roundId)
            .ToListAsync(ct);
        db.RoundExtraPoints.RemoveRange(extraPoints);

        var comments = await db.Comments.Where(c => c.RoundId == roundId).ToListAsync(ct);
        db.Comments.RemoveRange(comments);

        var teams = await db.Teams.Where(t => t.RoundId == roundId).ToListAsync(ct);
        db.Teams.RemoveRange(teams);

        await db.SaveChangesAsync(ct);
    }
}

public record RoundInput(
    DateTime PlayedAt,
    Party WinningParty,
    int Points,
    int GameModeId,
    TeamInput[] Teams,
    string? Comment
);

public record TeamInput(
    Party Party,
    int[] PlayerIds,
    int[] SpecialCardIds,
    ExtraPointInput[] ExtraPoints
);

public record ExtraPointInput(int ExtraPointId, int Count);

public record RoundListPage(int Total, int Page, RoundListItem[] Items);

public record RoundListItem(
    int Id,
    DateTime PlayedAt,
    Party WinningParty,
    int Points,
    string GameMode,
    PlayerInfo[] RePlayers,
    PlayerInfo[] KontraPlayers,
    string? Comment
);

public record RoundDetail(
    int Id,
    DateTime PlayedAt,
    Party WinningParty,
    int Points,
    GameModeInfo GameMode,
    RoundTeamDetail[] Teams,
    string? Comment
);

public record RoundTeamDetail(
    Party Party,
    PlayerInfo[] Players,
    SpecialCardInfo[] SpecialCards,
    ExtraPointEntry[] ExtraPoints
);

public record GameModeInfo(int Id, string Name, bool IsSolo);

public record PlayerInfo(int Id, string Name);

public record SpecialCardInfo(int Id, string Name);

public record ExtraPointEntry(int Id, string Name, int Count);
