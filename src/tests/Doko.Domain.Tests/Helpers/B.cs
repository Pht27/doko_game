namespace Doko.Domain.Tests.Helpers;

/// <summary>Short-hand builder helpers used across all test classes.</summary>
internal static class B
{
    // ── Players ───────────────────────────────────────────────────────────────
    public static readonly PlayerId P0 = new(0);
    public static readonly PlayerId P1 = new(1);
    public static readonly PlayerId P2 = new(2);
    public static readonly PlayerId P3 = new(3);

    /// <summary>Four players seated First–Fourth, all with empty hands.</summary>
    public static IReadOnlyList<PlayerState> FourPlayers(
        Hand? p0 = null,
        Hand? p1 = null,
        Hand? p2 = null,
        Hand? p3 = null
    ) =>
        [
            new(P0, PlayerSeat.First, p0 ?? Hand.Empty, null),
            new(P1, PlayerSeat.Second, p1 ?? Hand.Empty, null),
            new(P2, PlayerSeat.Third, p2 ?? Hand.Empty, null),
            new(P3, PlayerSeat.Fourth, p3 ?? Hand.Empty, null),
        ];

    // ── Cards ─────────────────────────────────────────────────────────────────
    public static Card Card(byte id, Suit suit, Rank rank) =>
        new(new CardId(id), new CardType(suit, rank));

    public static Hand HandOf(params Card[] cards) => new(cards);

    // ── Tricks ────────────────────────────────────────────────────────────────
    /// <summary>Builds a complete 4-card trick from (cardId, suit, rank, player) tuples.</summary>
    public static Trick Trick(params (byte id, Suit suit, Rank rank, PlayerId player)[] cards)
    {
        var trick = new Trick();
        foreach (var (id, suit, rank, player) in cards)
            trick.Add(new TrickCard(Card(id, suit, rank), player));
        return trick;
    }

    /// <summary>A TrickResult with no extrapunkt awards.</summary>
    public static TrickResult Result(Trick trick, PlayerId winner) => new(trick, winner, []);

    /// <summary>4 Asses (44 Augen), all attributed to <paramref name="winner"/>.</summary>
    public static TrickResult HighValueTrick(PlayerId winner, byte startId = 0)
    {
        var trick = new Trick();
        trick.Add(new TrickCard(Card(startId, Suit.Kreuz, Rank.Ass), winner));
        trick.Add(new TrickCard(Card((byte)(startId + 1), Suit.Pik, Rank.Ass), new PlayerId(1)));
        trick.Add(new TrickCard(Card((byte)(startId + 2), Suit.Herz, Rank.Ass), new PlayerId(2)));
        trick.Add(new TrickCard(Card((byte)(startId + 3), Suit.Karo, Rank.Ass), new PlayerId(3)));
        return new TrickResult(trick, winner, []);
    }

    /// <summary>4 Nines (0 Augen), all attributed to <paramref name="winner"/>.</summary>
    public static TrickResult ZeroValueTrick(PlayerId winner, byte startId = 0)
    {
        var trick = new Trick();
        trick.Add(new TrickCard(Card(startId, Suit.Kreuz, Rank.Neun), winner));
        trick.Add(new TrickCard(Card((byte)(startId + 1), Suit.Pik, Rank.Neun), new PlayerId(1)));
        trick.Add(new TrickCard(Card((byte)(startId + 2), Suit.Herz, Rank.Neun), new PlayerId(2)));
        trick.Add(new TrickCard(Card((byte)(startId + 3), Suit.Karo, Rank.Neun), new PlayerId(3)));
        return new TrickResult(trick, winner, []);
    }

    // ── Announcements ─────────────────────────────────────────────────────────
    public static Announcement Ann(
        PlayerId player,
        AnnouncementType type,
        bool isMandatory = false,
        int trickNum = 0,
        int cardIdx = 0
    ) => new(player, type, trickNum, cardIdx) { IsMandatory = isMandatory };

    // ── Party resolvers ───────────────────────────────────────────────────────
    /// <summary>P0 = Re, P1/P2/P3 = Kontra.</summary>
    public static IPartyResolver SoloResolver() => new SoloPartyResolver(P0);

    /// <summary>
    /// NormalPartyResolver backed by initial hands where P0 and P2 hold ♣Q (Re),
    /// P1 and P3 do not (Kontra).
    /// </summary>
    public static (
        IPartyResolver resolver,
        IReadOnlyDictionary<PlayerId, Hand> initialHands
    ) NormalResolver()
    {
        var kreuzDame = Card(0, Suit.Kreuz, Rank.Dame);
        var kreuzDame2 = Card(1, Suit.Kreuz, Rank.Dame);
        var hands = new Dictionary<PlayerId, Hand>
        {
            [P0] = HandOf(kreuzDame),
            [P1] = HandOf(Card(2, Suit.Pik, Rank.Ass)),
            [P2] = HandOf(kreuzDame2),
            [P3] = HandOf(Card(3, Suit.Herz, Rank.Ass)),
        };
        return (NormalPartyResolver.Instance, hands);
    }

    // ── GameState helpers ────────────────────────────────────────────────────
    /// <summary>
    /// Minimal 4-player state ready for play. P0 = Re (SoloPartyResolver).
    /// </summary>
    public static GameState BasicState(
        RuleSet? rules = null,
        IReadOnlyList<Announcement>? announcements = null,
        IReadOnlyList<Trick>? completedTricks = null,
        IReadOnlyList<SonderkarteType>? activeSonderkarten = null,
        IPartyResolver? partyResolver = null,
        IReadOnlyDictionary<PlayerId, Hand>? initialHands = null
    ) =>
        GameState.Create(
            rules: rules,
            players: FourPlayers(),
            currentTurn: P0,
            partyResolver: partyResolver ?? SoloResolver(),
            announcements: announcements,
            completedTricks: completedTricks,
            activeSonderkarten: activeSonderkarten,
            initialHands: initialHands,
            phase: GamePhase.Playing
        );
}
