using Doko.Application.Games.Queries;
using Doko.Domain.Cards;
using Doko.Domain.Scoring;

namespace Doko.Console.Rendering;

public sealed class GameRenderer
{
    public void Render(PlayerGameView view)
    {
        var trickNum = view.CurrentTrick?.TrickNumber ?? view.CompletedTricks.Count;
        System.Console.WriteLine($"=== Player {view.RequestingPlayer} | Trick {trickNum + 1} | Phase: {view.Phase} ===");
        System.Console.WriteLine();

        RenderPlayerCircle(view);

        // Current trick
        if (view.CurrentTrick is { Cards.Count: > 0 } ct)
        {
            System.Console.WriteLine("Current trick:");
            foreach (var c in ct.Cards)
                System.Console.WriteLine($"  P{c.Player}: {FormatCard(c.Card)}");
            System.Console.WriteLine();
        }

        if (view.CompletedTricks.Count > 0)
            System.Console.WriteLine($"Completed tricks: {view.CompletedTricks.Count}");

        // Hand — use pre-sorted list, mark legal with *
        var legalIds = view.LegalCards.Select(c => c.Id).ToHashSet();
        var sorted   = view.HandSorted.Count > 0 ? view.HandSorted : view.Hand;
        System.Console.WriteLine($"\nYour hand ({sorted.Count} cards):  (* = legal to play)");
        for (int i = 0; i < sorted.Count; i++)
        {
            var card  = sorted[i];
            var legal = legalIds.Contains(card.Id) ? "*" : " ";
            System.Console.Write($"  {legal}[{i + 1,2}] {FormatCard(card),-5}");
            if ((i + 1) % 6 == 0) System.Console.WriteLine();
        }
        System.Console.WriteLine();

        // Legal cards numbered for input
        System.Console.WriteLine("Legal cards:");
        for (int i = 0; i < view.LegalCards.Count; i++)
            System.Console.Write($"  [{i + 1}] {FormatCard(view.LegalCards[i])}  ");
        System.Console.WriteLine();

        // Eligible sonderkarten per card
        var anySk = view.EligibleSonderkartenPerCard.Any(kv => kv.Value.Count > 0);
        if (anySk)
        {
            System.Console.WriteLine("\nSonderkarten when playing:");
            foreach (var (cardId, infos) in view.EligibleSonderkartenPerCard.Where(kv => kv.Value.Count > 0))
            {
                var card = view.Hand.FirstOrDefault(c => c.Id == cardId);
                if (card is null) continue;
                System.Console.WriteLine($"  {FormatCard(card)}: {string.Join(", ", infos.Select(s => s.Name))}");
            }
        }

        // Legal announcements
        if (view.LegalAnnouncements.Count > 0)
        {
            System.Console.Write("\nAnnounce: [0=skip");
            for (int i = 0; i < view.LegalAnnouncements.Count; i++)
                System.Console.Write($", {i + 1}={view.LegalAnnouncements[i]}");
            System.Console.WriteLine("]");
        }

        System.Console.WriteLine();
    }

    public void RenderHand(IReadOnlyList<Card> hand)
    {
        System.Console.WriteLine($"Your hand ({hand.Count} cards):");
        for (int i = 0; i < hand.Count; i++)
        {
            System.Console.Write($"  [{i + 1,2}] {FormatCard(hand[i]),-5}");
            if ((i + 1) % 6 == 0) System.Console.WriteLine();
        }
        System.Console.WriteLine();
    }

    public void RenderResult(GameResult result)
    {
        System.Console.WriteLine("╔══════════════════════════════╗");
        System.Console.WriteLine("║          GAME OVER           ║");
        System.Console.WriteLine("╚══════════════════════════════╝");
        System.Console.WriteLine();
        System.Console.WriteLine($"  Winner:        {result.Winner}");
        System.Console.WriteLine($"  Re points:     {result.RePoints}");
        System.Console.WriteLine($"  Kontra points: {result.KontraPoints}");
        System.Console.WriteLine($"  Game value:    {result.GameValue}");
        if (result.Feigheit)
            System.Console.WriteLine("  *** Feigheit! ***");
        if (result.AllAwards.Count > 0)
        {
            System.Console.WriteLine("\n  Extrapunkte:");
            foreach (var award in result.AllAwards)
                System.Console.WriteLine($"    {award.Type,-20} P{award.BenefittingPlayer}  ({award.Delta:+#;-#;0})");
        }
        System.Console.WriteLine();
    }

    public void RenderGeschmissen(byte schmeisserSeat)
    {
        System.Console.WriteLine("╔══════════════════════════════╗");
        System.Console.WriteLine("║         GESCHMISSEN!         ║");
        System.Console.WriteLine("╚══════════════════════════════╝");
        System.Console.WriteLine();
        System.Console.WriteLine($"  Player {schmeisserSeat} declared Schmeißen.");
        System.Console.WriteLine("  No points awarded. Game is void.");
        System.Console.WriteLine();
    }

    private static void RenderPlayerCircle(PlayerGameView view)
    {
        // Build a 4-slot array: slot 0 = P0, etc.
        // Layout:
        //        [P1]
        //   [P0]      [P2]
        //        [P3]

        var names = new string[4];
        for (byte i = 0; i < 4; i++)
        {
            bool isMe      = i == view.RequestingPlayer.Value;
            bool isCurrent = i == view.CurrentTurn.Value;
            var other      = view.OtherPlayers.FirstOrDefault(p => p.Id.Value == i);
            int cardCount  = isMe ? (view.Hand.Count) : (other?.HandCardCount ?? 0);
            var party      = isMe
                ? null
                : other?.KnownParty?.ToString();

            var tag   = isCurrent ? "◄" : " ";
            var me    = isMe ? "*" : " ";
            var pStr  = party is not null ? $"[{party[0]}]" : "   ";
            names[i]  = $"{me}P{i}{pStr}({cardCount}){tag}";
        }

        System.Console.WriteLine($"           {names[1]}");
        System.Console.WriteLine($"  {names[0]}           {names[2]}");
        System.Console.WriteLine($"           {names[3]}");
        System.Console.WriteLine();
    }

    public static string FormatCard(Card card)
    {
        var suit = card.Type.Suit switch
        {
            Suit.Kreuz => "♣",
            Suit.Pik   => "♠",
            Suit.Herz  => "♥",
            Suit.Karo  => "♦",
            _          => "?",
        };
        var rank = card.Type.Rank switch
        {
            Rank.Neun   => "9",
            Rank.Bube   => "J",
            Rank.Dame   => "Q",
            Rank.Koenig => "K",
            Rank.Zehn   => "10",
            Rank.Ass    => "A",
            _           => "?",
        };
        return $"{suit}{rank}";
    }
}
