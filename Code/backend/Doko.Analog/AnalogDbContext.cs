using Doko.Analog.Entities;
using Doko.Analog.Stats;
using Microsoft.EntityFrameworkCore;

namespace Doko.Analog;

public class AnalogDbContext(DbContextOptions<AnalogDbContext> options) : DbContext(options)
{
    public DbSet<AnalogPlayer> Players => Set<AnalogPlayer>();
    public DbSet<AnalogRound> Rounds => Set<AnalogRound>();
    public DbSet<AnalogTeam> Teams => Set<AnalogTeam>();
    public DbSet<AnalogTeamMember> TeamMembers => Set<AnalogTeamMember>();
    public DbSet<AnalogGameMode> GameModes => Set<AnalogGameMode>();
    public DbSet<AnalogSpecialCard> SpecialCards => Set<AnalogSpecialCard>();
    public DbSet<AnalogExtraPoint> ExtraPoints => Set<AnalogExtraPoint>();
    public DbSet<AnalogRoundSpecialCard> RoundSpecialCards => Set<AnalogRoundSpecialCard>();
    public DbSet<AnalogRoundExtraPoint> RoundExtraPoints => Set<AnalogRoundExtraPoint>();
    public DbSet<AnalogComment> Comments => Set<AnalogComment>();
    public DbSet<PlayerLeaderboardEntry> PlayerLeaderboard => Set<PlayerLeaderboardEntry>();
    public DbSet<PlayerRoundHistoryEntry> PlayerRoundHistory => Set<PlayerRoundHistoryEntry>();
    public DbSet<PlayerStatsEntry> PlayerStats => Set<PlayerStatsEntry>();
    public DbSet<PlayerGameModeStatsEntry> PlayerGameModeStats => Set<PlayerGameModeStatsEntry>();
    public DbSet<PlayerSpecialCardStatsEntry> PlayerSpecialCardStats =>
        Set<PlayerSpecialCardStatsEntry>();
    public DbSet<PlayerExtraPointStatsEntry> PlayerExtraPointStats =>
        Set<PlayerExtraPointStatsEntry>();
    public DbSet<PlayerPartnerStatsEntry> PlayerPartnerStats => Set<PlayerPartnerStatsEntry>();
    public DbSet<GameModeStatsEntry> GameModeStats => Set<GameModeStatsEntry>();
    public DbSet<SpecialCardStatsEntry> SpecialCardStats => Set<SpecialCardStatsEntry>();
    public DbSet<ExtraPointStatsEntry> ExtraPointStats => Set<ExtraPointStatsEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("analog");

        modelBuilder.Entity<PlayerLeaderboardEntry>(e =>
        {
            e.HasNoKey();
            e.ToView("player_leaderboard", "analytics");
            e.Property(p => p.PlayerId).HasColumnName("player_id");
            e.Property(p => p.Name).HasColumnName("name");
            e.Property(p => p.IsActive).HasColumnName("is_active");
            e.Property(p => p.TotalPoints).HasColumnName("total_points");
            e.Property(p => p.GamesPlayed).HasColumnName("games_played");
            e.Property(p => p.Wins).HasColumnName("wins");
            e.Property(p => p.Losses).HasColumnName("losses");
            e.Property(p => p.WinRate).HasColumnName("win_rate");
            e.Property(p => p.AvgPointsPerGame).HasColumnName("avg_points_per_game");
        });

        modelBuilder.Entity<PlayerRoundHistoryEntry>(e =>
        {
            e.HasNoKey();
            e.ToView("player_round_history", "analytics");
            e.Property(p => p.PlayerId).HasColumnName("player_id");
            e.Property(p => p.RoundId).HasColumnName("round_id");
            e.Property(p => p.PlayedAt).HasColumnName("played_at");
            e.Property(p => p.PointDelta).HasColumnName("point_delta");
            e.Property(p => p.Won).HasColumnName("won");
            e.Property(p => p.CumulativePoints).HasColumnName("cumulative_points");
        });

        modelBuilder.Entity<AnalogPlayer>(e =>
        {
            e.ToTable("player");
            e.Property(p => p.Name).HasMaxLength(50);
            e.Property(p => p.StartingPoints).HasColumnType("decimal(10,1)");
            e.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
            e.Property(p => p.HeroCard).HasMaxLength(10).IsRequired(false);
        });

        modelBuilder.Entity<AnalogGameMode>(e =>
        {
            e.ToTable("game_mode");
            e.Property(p => p.Name).HasMaxLength(100);
            e.Property(p => p.SoloParty).HasConversion<int?>();
            e.HasData(
                new AnalogGameMode
                {
                    Id = 1,
                    Name = "Armut",
                    IsSolo = false,
                },
                new AnalogGameMode
                {
                    Id = 2,
                    Name = "Hochzeit",
                    IsSolo = false,
                },
                new AnalogGameMode
                {
                    Id = 3,
                    Name = "Normal",
                    IsSolo = false,
                },
                new AnalogGameMode
                {
                    Id = 4,
                    Name = "Bubensolo",
                    IsSolo = true,
                    SoloParty = Party.Re,
                },
                new AnalogGameMode
                {
                    Id = 5,
                    Name = "Damensolo",
                    IsSolo = true,
                    SoloParty = Party.Re,
                },
                new AnalogGameMode
                {
                    Id = 6,
                    Name = "Farbsolo",
                    IsSolo = true,
                    SoloParty = Party.Re,
                },
                new AnalogGameMode
                {
                    Id = 7,
                    Name = "Fleischloses",
                    IsSolo = true,
                    SoloParty = Party.Re,
                },
                new AnalogGameMode
                {
                    Id = 8,
                    Name = "Knochenloses",
                    IsSolo = true,
                    SoloParty = Party.Re,
                },
                new AnalogGameMode
                {
                    Id = 9,
                    Name = "Kontrasolo",
                    IsSolo = true,
                    SoloParty = Party.Kontra,
                },
                new AnalogGameMode
                {
                    Id = 10,
                    Name = "Schlanker Martin",
                    IsSolo = true,
                    SoloParty = Party.Re,
                },
                new AnalogGameMode
                {
                    Id = 11,
                    Name = "Schwarze Sau",
                    IsSolo = true,
                    SoloParty = Party.Re,
                },
                new AnalogGameMode
                {
                    Id = 12,
                    Name = "Stille Hochzeit",
                    IsSolo = true,
                    SoloParty = Party.Re,
                }
            );
        });

        modelBuilder.Entity<AnalogSpecialCard>(e =>
        {
            e.ToTable("special_card");
            e.Property(p => p.Name).HasMaxLength(100);
            e.HasData(
                new AnalogSpecialCard { Id = 1, Name = "Gegengenscherdamen" },
                new AnalogSpecialCard { Id = 2, Name = "Genscherdamen" },
                new AnalogSpecialCard { Id = 3, Name = "Heidfrau" },
                new AnalogSpecialCard { Id = 4, Name = "Heidmann" },
                new AnalogSpecialCard { Id = 5, Name = "Hyperschweinchen" },
                new AnalogSpecialCard { Id = 6, Name = "Kemmerich" },
                new AnalogSpecialCard { Id = 7, Name = "Linksdrehender Gehängter" },
                new AnalogSpecialCard { Id = 8, Name = "Schweinchen" },
                new AnalogSpecialCard { Id = 9, Name = "Superschweinchen" }
            );
        });

        modelBuilder.Entity<AnalogExtraPoint>(e =>
        {
            e.ToTable("extra_point");
            e.Property(p => p.Name).HasMaxLength(100);
            e.HasData(
                new AnalogExtraPoint { Id = 1, Name = "Agathe" },
                new AnalogExtraPoint { Id = 2, Name = "Doppelkopf" },
                new AnalogExtraPoint { Id = 3, Name = "Fischauge" },
                new AnalogExtraPoint { Id = 4, Name = "Fuchs gefangen" },
                new AnalogExtraPoint { Id = 5, Name = "Gans gefangen" },
                new AnalogExtraPoint { Id = 6, Name = "Kaffeekränzchen" },
                new AnalogExtraPoint { Id = 7, Name = "Karlchen" },
                new AnalogExtraPoint { Id = 8, Name = "Klabautermann gefangen" }
            );
        });

        modelBuilder.Entity<AnalogRound>(e =>
        {
            e.ToTable("round");
            e.Property(p => p.WinningParty).HasConversion<int>();
        });

        modelBuilder.Entity<AnalogTeam>(e =>
        {
            e.ToTable("team");
            e.Property(p => p.Name).HasMaxLength(100);
            e.Property(p => p.Party).HasConversion<int>();
        });

        modelBuilder.Entity<AnalogTeamMember>(e =>
        {
            e.ToTable("team_member");
            e.HasKey(m => new { m.TeamId, m.PlayerId });
        });

        modelBuilder.Entity<AnalogRoundSpecialCard>(e =>
        {
            e.ToTable("round_special_card");
            e.HasKey(r => new
            {
                r.RoundId,
                r.SpecialCardId,
                r.TeamId,
            });
        });

        modelBuilder.Entity<AnalogRoundExtraPoint>(e =>
        {
            e.ToTable("round_extra_point");
            e.HasKey(r => new
            {
                r.RoundId,
                r.ExtraPointId,
                r.TeamId,
            });
        });

        modelBuilder.Entity<AnalogComment>(e =>
        {
            e.ToTable("comment");
        });

        modelBuilder.Entity<PlayerStatsEntry>(e =>
        {
            e.HasNoKey();
            e.ToView("player_stats", "analytics");
            e.Property(p => p.PlayerId).HasColumnName("player_id");
            e.Property(p => p.Name).HasColumnName("name");
            e.Property(p => p.IsActive).HasColumnName("is_active");
            e.Property(p => p.TotalGames).HasColumnName("total_games");
            e.Property(p => p.TotalWins).HasColumnName("total_wins");
            e.Property(p => p.TotalWinRate).HasColumnName("total_win_rate");
            e.Property(p => p.TotalAvgGameValue).HasColumnName("total_avg_game_value");
            e.Property(p => p.TotalAvgPointsWonLost).HasColumnName("total_avg_points_won_lost");
            e.Property(p => p.TotalAvgPointsEarned).HasColumnName("total_avg_points_earned");
            e.Property(p => p.SoloGames).HasColumnName("solo_games");
            e.Property(p => p.SoloWins).HasColumnName("solo_wins");
            e.Property(p => p.SoloWinRate).HasColumnName("solo_win_rate");
            e.Property(p => p.SoloAvgGameValue).HasColumnName("solo_avg_game_value");
            e.Property(p => p.SoloAvgPointsWonLost).HasColumnName("solo_avg_points_won_lost");
            e.Property(p => p.AloneGames).HasColumnName("alone_games");
            e.Property(p => p.AloneWins).HasColumnName("alone_wins");
            e.Property(p => p.AloneWinRate).HasColumnName("alone_win_rate");
            e.Property(p => p.AloneAvgPointsEarned).HasColumnName("alone_avg_points_earned");
        });

        modelBuilder.Entity<PlayerGameModeStatsEntry>(e =>
        {
            e.HasNoKey();
            e.ToView("player_game_mode_stats", "analytics");
            e.Property(p => p.PlayerId).HasColumnName("player_id");
            e.Property(p => p.GameModeId).HasColumnName("game_mode_id");
            e.Property(p => p.GameModeName).HasColumnName("game_mode_name");
            e.Property(p => p.Party).HasColumnName("party");
            e.Property(p => p.Games).HasColumnName("games");
            e.Property(p => p.Wins).HasColumnName("wins");
            e.Property(p => p.WinRate).HasColumnName("win_rate");
            e.Property(p => p.AvgGameValue).HasColumnName("avg_game_value");
            e.Property(p => p.AvgPointsWonLost).HasColumnName("avg_points_won_lost");
            e.Property(p => p.AvgPointsEarned).HasColumnName("avg_points_earned");
        });

        modelBuilder.Entity<PlayerSpecialCardStatsEntry>(e =>
        {
            e.HasNoKey();
            e.ToView("player_special_card_stats", "analytics");
            e.Property(p => p.PlayerId).HasColumnName("player_id");
            e.Property(p => p.SpecialCardId).HasColumnName("special_card_id");
            e.Property(p => p.SpecialCardName).HasColumnName("special_card_name");
            e.Property(p => p.Party).HasColumnName("party");
            e.Property(p => p.Occurrences).HasColumnName("occurrences");
            e.Property(p => p.Wins).HasColumnName("wins");
            e.Property(p => p.WinRate).HasColumnName("win_rate");
            e.Property(p => p.AvgGameValue).HasColumnName("avg_game_value");
            e.Property(p => p.AvgPointsWonLost).HasColumnName("avg_points_won_lost");
        });

        modelBuilder.Entity<PlayerExtraPointStatsEntry>(e =>
        {
            e.HasNoKey();
            e.ToView("player_extra_point_stats", "analytics");
            e.Property(p => p.PlayerId).HasColumnName("player_id");
            e.Property(p => p.ExtraPointId).HasColumnName("extra_point_id");
            e.Property(p => p.ExtraPointName).HasColumnName("extra_point_name");
            e.Property(p => p.Party).HasColumnName("party");
            e.Property(p => p.Occurrences).HasColumnName("occurrences");
            e.Property(p => p.TotalCount).HasColumnName("total_count");
            e.Property(p => p.Wins).HasColumnName("wins");
            e.Property(p => p.WinRate).HasColumnName("win_rate");
            e.Property(p => p.AvgGameValue).HasColumnName("avg_game_value");
        });

        modelBuilder.Entity<PlayerPartnerStatsEntry>(e =>
        {
            e.HasNoKey();
            e.ToView("player_partner_stats", "analytics");
            e.Property(p => p.PlayerId).HasColumnName("player_id");
            e.Property(p => p.PartnerId).HasColumnName("partner_id");
            e.Property(p => p.PartnerName).HasColumnName("partner_name");
            e.Property(p => p.GamesTogether).HasColumnName("games_together");
            e.Property(p => p.WinsTogether).HasColumnName("wins_together");
            e.Property(p => p.WinRateTogether).HasColumnName("win_rate_together");
            e.Property(p => p.AvgPointsWonLost).HasColumnName("avg_points_won_lost");
        });

        modelBuilder.Entity<GameModeStatsEntry>(e =>
        {
            e.HasNoKey();
            e.ToView("game_mode_stats", "analytics");
            e.Property(p => p.GameModeId).HasColumnName("game_mode_id");
            e.Property(p => p.GameModeName).HasColumnName("game_mode_name");
            e.Property(p => p.TotalRounds).HasColumnName("total_rounds");
            e.Property(p => p.AvgGameValue).HasColumnName("avg_game_value");
            e.Property(p => p.ReWinRate).HasColumnName("re_win_rate");
            e.Property(p => p.ReAvgGameValue).HasColumnName("re_avg_game_value");
        });

        modelBuilder.Entity<SpecialCardStatsEntry>(e =>
        {
            e.HasNoKey();
            e.ToView("special_card_stats", "analytics");
            e.Property(p => p.SpecialCardId).HasColumnName("special_card_id");
            e.Property(p => p.Name).HasColumnName("name");
            e.Property(p => p.Party).HasColumnName("party");
            e.Property(p => p.Occurrences).HasColumnName("occurrences");
            e.Property(p => p.Wins).HasColumnName("wins");
            e.Property(p => p.WinRate).HasColumnName("win_rate");
            e.Property(p => p.AvgGameValue).HasColumnName("avg_game_value");
            e.Property(p => p.AvgPointsWonLost).HasColumnName("avg_points_won_lost");
        });

        modelBuilder.Entity<ExtraPointStatsEntry>(e =>
        {
            e.HasNoKey();
            e.ToView("extra_point_stats", "analytics");
            e.Property(p => p.ExtraPointId).HasColumnName("extra_point_id");
            e.Property(p => p.Name).HasColumnName("name");
            e.Property(p => p.Party).HasColumnName("party");
            e.Property(p => p.Occurrences).HasColumnName("occurrences");
            e.Property(p => p.TotalCount).HasColumnName("total_count");
            e.Property(p => p.Wins).HasColumnName("wins");
            e.Property(p => p.WinRate).HasColumnName("win_rate");
            e.Property(p => p.AvgGameValue).HasColumnName("avg_game_value");
            e.Property(p => p.AvgPointsWonLost).HasColumnName("avg_points_won_lost");
        });
    }
}
