using Doko.Domain.GameFlow;
using Doko.Domain.Parties;
using Doko.Domain.Players;
using Doko.Domain.Scoring;

namespace Doko.Domain.Lobby;

public record LobbyPlayer(PlayerSeat Seat, DateTimeOffset JoinedAt);

public class LobbyState
{
    private readonly LobbyPlayer?[] _seats = new LobbyPlayer?[4];
    private readonly int[] _standings = new int[4];
    private readonly string?[] _playerNames = new string?[4];
    private readonly HashSet<PlayerSeat> _newGameVoters = [];
    private readonly HashSet<PlayerSeat> _lobbyStartVoters = [];
    private readonly List<(
        GameResult Result,
        string? GameMode,
        int[] NetPoints,
        Party?[] PartyPerSeat
    )> _gameHistory = [];
    private readonly HashSet<int> _opaSeats = [];
    private bool _advanceRauskommer = true;

    public LobbyId Id { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset LastActivityAt { get; private set; }

    /// <summary>All currently seated players (non-null seats).</summary>
    public IEnumerable<LobbyPlayer> Players => _seats.Where(s => s != null).Cast<LobbyPlayer>();

    /// <summary>Snapshot of the 4 seats; null means empty.</summary>
    public IReadOnlyList<LobbyPlayer?> Seats => _seats.AsReadOnly();

    public bool IsFull => _seats.All(s => s != null);
    public bool IsStarted { get; private set; }

    /// <summary>
    /// Seat index (0–3) of the player who leads the next reservation-check phase.
    /// Advances counter-clockwise (+1 mod 4) after each completed game.
    /// Stays the same after Schmeißen.
    /// </summary>
    public int VorbehaltRauskommer { get; private set; }

    /// <summary>The game that is currently running in this lobby, if any.</summary>
    public GameId? ActiveGameId { get; private set; }

    /// <summary>Name of the scenario to use when dealing cards, or null for random.</summary>
    public string? SelectedScenario { get; private set; }

    /// <summary>Cumulative lobby standings per seat (index 0–3).</summary>
    public IReadOnlyList<int> Standings => Array.AsReadOnly(_standings);

    /// <summary>Player-chosen display names per seat (index 0–3); null means no custom name.</summary>
    public IReadOnlyList<string?> PlayerNames => Array.AsReadOnly(_playerNames);

    /// <summary>Sets a custom display name for the given seat. Pass null to clear.</summary>
    public void SetPlayerName(int seatIndex, string? name)
    {
        if (seatIndex < 0 || seatIndex >= 4)
            return;
        _playerNames[seatIndex] = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
    }

    /// <summary>Ordered list of completed game results with their net points and party per seat.</summary>
    public IReadOnlyList<(
        GameResult Result,
        string? GameMode,
        int[] NetPoints,
        Party?[] PartyPerSeat
    )> GameHistory => _gameHistory.AsReadOnly();

    /// <summary>Number of players who have voted to start a new game.</summary>
    public int NewGameVoteCount => _newGameVoters.Count;

    /// <summary>Number of players who have voted ready to start the initial lobby game.</summary>
    public int LobbyStartVoteCount => _lobbyStartVoters.Count;

    /// <summary>Seat indices (0–3) of players who have voted ready to start the initial lobby game.</summary>
    public IReadOnlySet<int> LobbyStartVoterSeats =>
        _lobbyStartVoters.Select(s => (int)s).ToHashSet();

    private LobbyState(LobbyId id, DateTimeOffset createdAt)
    {
        Id = id;
        CreatedAt = createdAt;
        LastActivityAt = createdAt;
    }

    /// <summary>Creates a new lobby; the creator automatically occupies seat 0.</summary>
    public static LobbyState Create()
    {
        var lobby = new LobbyState(LobbyId.New(), DateTimeOffset.UtcNow);
        lobby._seats[0] = new LobbyPlayer(PlayerSeat.First, DateTimeOffset.UtcNow);
        return lobby;
    }

    public void RecordActivity() => LastActivityAt = DateTimeOffset.UtcNow;

    /// <summary>Set of seat indices (0–3) currently occupied by Opa (the computer player).</summary>
    public IReadOnlySet<int> OpaSeats => _opaSeats;

    /// <summary>Returns true if the given seat index is occupied by Opa.</summary>
    public bool IsOpaSeat(int seatIndex) => _opaSeats.Contains(seatIndex);

    /// <summary>Returns true if at least one non-Opa player is in the lobby.</summary>
    public bool HasHumanPlayers => Players.Any(p => !_opaSeats.Contains((int)p.Seat));

    /// <summary>
    /// Tries to occupy a specific seat. Returns false if the index is out of range
    /// or the seat is already taken.
    /// </summary>
    public bool TryOccupySeat(int seatIndex, out PlayerSeat seat)
    {
        seat = default;
        if (seatIndex < 0 || seatIndex >= 4)
            return false;
        if (_seats[seatIndex] != null)
            return false;

        seat = (PlayerSeat)seatIndex;
        _seats[seatIndex] = new LobbyPlayer(seat, DateTimeOffset.UtcNow);
        return true;
    }

    /// <summary>
    /// Occupies a seat as Opa (the computer player). Returns false if out of range or taken.
    /// </summary>
    public bool TryOccupySeatAsOpa(int seatIndex, out PlayerSeat seat)
    {
        if (!TryOccupySeat(seatIndex, out seat))
            return false;
        _opaSeats.Add(seatIndex);
        return true;
    }

    /// <summary>
    /// Removes Opa from the given seat. Returns false if the seat is not an Opa seat.
    /// </summary>
    public bool TryRemoveOpa(int seatIndex, out PlayerSeat seat)
    {
        seat = default;
        if (!_opaSeats.Contains(seatIndex))
            return false;
        seat = (PlayerSeat)seatIndex;
        _opaSeats.Remove(seatIndex);
        _seats[seatIndex] = null;
        return true;
    }

    /// <summary>Removes the player from their seat.</summary>
    public void RemovePlayer(PlayerSeat seat)
    {
        _opaSeats.Remove((int)seat);
        _seats[(int)seat] = null;
    }

    /// <summary>Returns true if the given seat is currently occupied.</summary>
    public bool HasPlayer(PlayerSeat seat) => (int)seat < 4 && _seats[(int)seat] != null;

    public void SetScenario(string? scenarioName) => SelectedScenario = scenarioName;

    public void MarkStarted(GameId gameId)
    {
        IsStarted = true;
        ActiveGameId = gameId;
    }

    /// <summary>Resets the lobby so a new game can be started with the same players.</summary>
    public void MarkGameFinished()
    {
        IsStarted = false;
        ActiveGameId = null;
    }

    /// <summary>
    /// Adds a vote to start a new game. Returns true when all 4 seated players have voted.
    /// </summary>
    public bool AddNewGameVote(PlayerSeat seat)
    {
        _newGameVoters.Add(seat);
        return _newGameVoters.Count >= 4;
    }

    /// <summary>Withdraws a player's vote to start a new game.</summary>
    public void RemoveNewGameVote(PlayerSeat seat) => _newGameVoters.Remove(seat);

    /// <summary>Clears all new-game votes (called when the new game actually starts).</summary>
    public void ResetNewGameVotes() => _newGameVoters.Clear();

    /// <summary>Adds a vote to start the initial lobby game. Returns true when all 4 have voted.</summary>
    public bool AddLobbyStartVote(PlayerSeat seat)
    {
        _lobbyStartVoters.Add(seat);
        return _lobbyStartVoters.Count >= 4;
    }

    /// <summary>Withdraws a player's lobby-start vote.</summary>
    public void RemoveLobbyStartVote(PlayerSeat seat) => _lobbyStartVoters.Remove(seat);

    /// <summary>Clears all lobby-start votes (called when the game actually starts).</summary>
    public void ResetLobbyStartVotes() => _lobbyStartVoters.Clear();

    /// <summary>
    /// Records whether the VorbehaltRauskommer should advance after the current game.
    /// Call this when a game finishes, before VoteNewGame triggers the advance.
    /// </summary>
    public void SetAdvanceRauskommer(bool advance) => _advanceRauskommer = advance;

    /// <summary>
    /// Rotates the VorbehaltRauskommer one seat counter-clockwise, but only if
    /// the last completed game was a Normal or Hochzeit game.
    /// Soli, Armut, and Schmeißen keep the same leader.
    /// </summary>
    public void AdvanceRauskommerIfRequired()
    {
        if (_advanceRauskommer)
            VorbehaltRauskommer = (VorbehaltRauskommer + 1) % 4;
        _advanceRauskommer = true; // reset for next game
    }

    /// <summary>Applies per-seat net point deltas to the running lobby standings.</summary>
    public void UpdateStandings(int[] netPointsPerSeat)
    {
        for (int i = 0; i < 4 && i < netPointsPerSeat.Length; i++)
            _standings[i] += netPointsPerSeat[i];
    }

    /// <summary>Records a completed game result in the match history.</summary>
    public void AddGameRecord(
        GameResult result,
        string? gameMode,
        int[] netPointsPerSeat,
        Party?[] partyPerSeat
    ) => _gameHistory.Add((result, gameMode, netPointsPerSeat, partyPerSeat));
}
