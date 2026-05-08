using Doko.Analog.Entities;
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

        modelBuilder.Entity<AnalogPlayer>(e =>
        {
            e.ToTable("player");
            e.Property(p => p.Name).HasMaxLength(50);
            e.Property(p => p.StartingPoints).HasColumnType("decimal(10,1)");
            e.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
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
    }
}
