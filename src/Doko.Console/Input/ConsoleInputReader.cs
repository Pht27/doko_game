using Doko.Application.Games.Queries;
using Doko.Console.Rendering;
using Doko.Domain.Announcements;
using Doko.Domain.Cards;
using Doko.Domain.Parties;
using Doko.Domain.Players;
using Doko.Domain.Reservations;
using Doko.Domain.Sonderkarten;

namespace Doko.Console.Input;

public sealed class ConsoleInputReader
{
    /// <summary>
    /// Prompts the player to pick a legal card and any sonderkarten to activate with it.
    /// </summary>
    public (CardId CardId, IReadOnlyList<SonderkarteType> Sonderkarten) PromptCard(PlayerGameView view)
    {
        while (true)
        {
            System.Console.Write($"Play card [1-{view.LegalCards.Count}]: ");
            var input = System.Console.ReadLine()?.Trim();
            if (int.TryParse(input, out int choice) && choice >= 1 && choice <= view.LegalCards.Count)
            {
                var card        = view.LegalCards[choice - 1];
                var sonderkarten = PromptSonderkarten(card, view);
                return (card.Id, sonderkarten);
            }
            System.Console.WriteLine($"  Enter a number between 1 and {view.LegalCards.Count}.");
        }
    }

    private static IReadOnlyList<SonderkarteType> PromptSonderkarten(Card card, PlayerGameView view)
    {
        if (!view.EligibleSonderkartenPerCard.TryGetValue(card.Id, out var eligible) || eligible.Count == 0)
            return [];

        var activated = new List<SonderkarteType>();
        foreach (var info in eligible)
        {
            System.Console.Write($"  Activate {info.Name}? [y/N]: ");
            var input = System.Console.ReadLine()?.Trim().ToLower();
            if (input is "y" or "yes")
                activated.Add(info.Type);
        }
        return activated;
    }

    /// <summary>
    /// Asks if the player wants to make an announcement. Returns null if they skip.
    /// </summary>
    public AnnouncementType? PromptAnnouncement(IReadOnlyList<AnnouncementType> legal)
    {
        System.Console.Write("Announce? [0=skip");
        for (int i = 0; i < legal.Count; i++)
            System.Console.Write($", {i + 1}={legal[i]}");
        System.Console.Write("]: ");

        var input = System.Console.ReadLine()?.Trim();
        if (int.TryParse(input, out int choice) && choice >= 1 && choice <= legal.Count)
            return legal[choice - 1];
        return null;
    }

    /// <summary>
    /// Prompts the player to declare a reservation, showing only eligible options.
    /// </summary>
    public IReservation? PromptReservation(PlayerId playerId, PlayerGameView view)
    {
        var eligible = view.EligibleReservations;

        System.Console.WriteLine("Declare a reservation:");
        System.Console.WriteLine("  [0] Keine Vorbehalt");
        for (int i = 0; i < eligible.Count; i++)
            System.Console.WriteLine($"  [{i + 1}] {FormatReservationKind(eligible[i])}");

        while (true)
        {
            System.Console.Write($"Your choice [0-{eligible.Count}]: ");
            var input = System.Console.ReadLine()?.Trim();
            if (!int.TryParse(input, out int choice) || choice < 0 || choice > eligible.Count)
            {
                System.Console.WriteLine($"  Enter a number 0–{eligible.Count}.");
                continue;
            }

            if (choice == 0) return null;
            return BuildReservation(eligible[choice - 1], playerId);
        }
    }

    private static IReservation BuildReservation(ReservationKind kind, PlayerId playerId)
        => kind switch
        {
            ReservationKind.Hochzeit      => BuildHochzeit(playerId),
            ReservationKind.Armut         => BuildArmut(playerId),
            ReservationKind.Schmeissen    => new SchmeissenReservation(),
            ReservationKind.Damensolo     => new DamensoloReservation(playerId),
            ReservationKind.Bubensolo     => new BubensoloReservation(playerId),
            ReservationKind.Fleischloses  => new FleischlosesReservation(playerId),
            ReservationKind.Knochenloses  => new KnochenlosesReservation(playerId),
            ReservationKind.SchlankerMartin => new SchlankerMartinReservation(playerId),
            ReservationKind.KaroSolo      => new FarbsoloReservation(Suit.Karo,  playerId),
            ReservationKind.KreuzSolo     => new FarbsoloReservation(Suit.Kreuz, playerId),
            ReservationKind.PikSolo       => new FarbsoloReservation(Suit.Pik,   playerId),
            ReservationKind.HerzSolo      => new FarbsoloReservation(Suit.Herz,  playerId),
            _                             => null!,
        };

    private static HochzeitReservation BuildHochzeit(PlayerId playerId)
    {
        System.Console.WriteLine("  Hochzeit condition:");
        System.Console.WriteLine("    [1] First trick");
        System.Console.WriteLine("    [2] First Fehl trick");
        System.Console.WriteLine("    [3] First trump trick");
        while (true)
        {
            System.Console.Write("  Condition [1-3]: ");
            var input = System.Console.ReadLine()?.Trim();
            if (int.TryParse(input, out int c) && c >= 1 && c <= 3)
            {
                var condition = c switch
                {
                    1 => HochzeitCondition.FirstTrick,
                    2 => HochzeitCondition.FirstFehlTrick,
                    _ => HochzeitCondition.FirstTrumpTrick,
                };
                return new HochzeitReservation(playerId, condition);
            }
            System.Console.WriteLine("  Enter 1, 2 or 3.");
        }
    }

    private static ArmutReservation BuildArmut(PlayerId poorPlayer)
    {
        System.Console.WriteLine("  Which player accepts your Armut? (0–3, not yourself)");
        while (true)
        {
            System.Console.Write($"  Rich player (not {poorPlayer}): ");
            var input = System.Console.ReadLine()?.Trim();
            if (byte.TryParse(input, out byte id) && id <= 3 && id != poorPlayer.Value)
                return new ArmutReservation(poorPlayer, new PlayerId(id));
            System.Console.WriteLine($"  Enter 0–3 (not {poorPlayer}).");
        }
    }

    private static string FormatReservationKind(ReservationKind kind) => kind switch
    {
        ReservationKind.Hochzeit       => "Hochzeit",
        ReservationKind.Armut          => "Armut",
        ReservationKind.Schmeissen     => "Schmeißen",
        ReservationKind.Damensolo      => "Damensolo",
        ReservationKind.Bubensolo      => "Bubensolo",
        ReservationKind.Fleischloses   => "Fleischloses",
        ReservationKind.Knochenloses   => "Knochenloses",
        ReservationKind.SchlankerMartin => "Schlanker Martin",
        ReservationKind.KaroSolo       => "Karo-Solo",
        ReservationKind.KreuzSolo      => "Kreuz-Solo",
        ReservationKind.PikSolo        => "Pik-Solo",
        ReservationKind.HerzSolo       => "Herz-Solo",
        _                              => kind.ToString(),
    };
}
