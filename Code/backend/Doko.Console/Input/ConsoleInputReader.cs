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
    private static readonly HashSet<SonderkarteType> GenscherTypes =
    [
        SonderkarteType.Genscherdamen,
        SonderkarteType.Gegengenscherdamen,
    ];

    /// <summary>
    /// Prompts the player to pick a legal card and any sonderkarten to activate with it.
    /// Returns the card, the activated sonderkarten, and an optional Genscher partner.
    /// </summary>
    public (
        CardId CardId,
        IReadOnlyList<SonderkarteType> Sonderkarten,
        PlayerSeat? GenscherPartner
    ) PromptCard(PlayerGameView view)
    {
        while (true)
        {
            System.Console.Write($"Play card [1-{view.LegalCards.Count}]: ");
            var input = System.Console.ReadLine()?.Trim();
            if (
                int.TryParse(input, out int choice)
                && choice >= 1
                && choice <= view.LegalCards.Count
            )
            {
                var card = view.LegalCards[choice - 1];
                var (sonderkarten, partner) = PromptSonderkarten(card, view);
                return (card.Id, sonderkarten, partner);
            }
            System.Console.WriteLine($"  Enter a number between 1 and {view.LegalCards.Count}.");
        }
    }

    private (
        IReadOnlyList<SonderkarteType> Activated,
        PlayerSeat? GenscherPartner
    ) PromptSonderkarten(Card card, PlayerGameView view)
    {
        if (
            !view.EligibleSonderkartenPerCard.TryGetValue(card.Id, out var eligible)
            || eligible.Count == 0
        )
            return ([], null);

        var activated = new List<SonderkarteType>();
        PlayerSeat? genscherPartner = null;

        foreach (var info in eligible)
        {
            System.Console.Write($"  Activate {info.Name}? [y/N]: ");
            var input = System.Console.ReadLine()?.Trim().ToLower();
            if (input is not ("y" or "yes"))
                continue;

            activated.Add(info.Type);

            if (GenscherTypes.Contains(info.Type))
                genscherPartner = PromptGenscherPartner(view.RequestingPlayer, view.OtherPlayers);
        }
        return (activated, genscherPartner);
    }

    private static PlayerSeat PromptGenscherPartner(
        PlayerSeat genscher,
        IReadOnlyList<PlayerPublicState> others
    )
    {
        System.Console.WriteLine("  Choose your new partner:");
        foreach (var p in others)
            System.Console.WriteLine($"    [{(int)p.Seat}] Player {(int)p.Seat}");

        while (true)
        {
            System.Console.Write($"  Partner (not {(int)genscher}): ");
            var input = System.Console.ReadLine()?.Trim();
            if (
                byte.TryParse(input, out byte id)
                && id != (int)genscher
                && others.Any(p => (int)p.Seat == id)
            )
                return (PlayerSeat)id;
            System.Console.WriteLine("  Invalid choice. Enter a player number from the list.");
        }
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
    public IReservation? PromptReservation(PlayerSeat playerId, PlayerGameView view)
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

            if (choice == 0)
                return null;
            return BuildReservation(eligible[choice - 1], playerId);
        }
    }

    private static IReservation BuildReservation(ReservationPriority kind, PlayerSeat playerId) =>
        kind switch
        {
            ReservationPriority.Hochzeit => BuildHochzeit(playerId),
            ReservationPriority.Armut => BuildArmut(playerId),
            ReservationPriority.Schmeissen => new SchmeissenReservation(),
            ReservationPriority.Damensolo => new DamensoloReservation(playerId),
            ReservationPriority.Bubensolo => new BubensoloReservation(playerId),
            ReservationPriority.Fleischloses => new FleischlosesReservation(playerId),
            ReservationPriority.Knochenloses => new KnochenlosesReservation(playerId),
            ReservationPriority.SchlankerMartin => new SchlankerMartinReservation(playerId),
            ReservationPriority.KaroSolo => new FarbsoloReservation(Suit.Karo, playerId),
            ReservationPriority.KreuzSolo => new FarbsoloReservation(Suit.Kreuz, playerId),
            ReservationPriority.PikSolo => new FarbsoloReservation(Suit.Pik, playerId),
            ReservationPriority.HerzSolo => new FarbsoloReservation(Suit.Herz, playerId),
            _ => null!,
        };

    private static HochzeitReservation BuildHochzeit(PlayerSeat playerId)
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

    private static ArmutReservation BuildArmut(PlayerSeat poorPlayer)
    {
        System.Console.WriteLine("  Which player accepts your Armut? (0–3, not yourself)");
        while (true)
        {
            System.Console.Write($"  Rich player (not {poorPlayer}): ");
            var input = System.Console.ReadLine()?.Trim();
            if (byte.TryParse(input, out byte id) && id <= 3 && id != (int)poorPlayer)
                return new ArmutReservation(poorPlayer, (PlayerSeat)id);
            System.Console.WriteLine($"  Enter 0–3 (not {poorPlayer}).");
        }
    }

    private static string FormatReservationKind(ReservationPriority kind) =>
        kind switch
        {
            ReservationPriority.Hochzeit => "Hochzeit",
            ReservationPriority.Armut => "Armut",
            ReservationPriority.Schmeissen => "Schmeißen",
            ReservationPriority.Damensolo => "Damensolo",
            ReservationPriority.Bubensolo => "Bubensolo",
            ReservationPriority.Fleischloses => "Fleischloses",
            ReservationPriority.Knochenloses => "Knochenloses",
            ReservationPriority.SchlankerMartin => "Schlanker Martin",
            ReservationPriority.KaroSolo => "Karo-Solo",
            ReservationPriority.KreuzSolo => "Kreuz-Solo",
            ReservationPriority.PikSolo => "Pik-Solo",
            ReservationPriority.HerzSolo => "Herz-Solo",
            _ => kind.ToString(),
        };
}
