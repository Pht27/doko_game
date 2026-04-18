using Doko.Domain.GameFlow;
using Doko.Domain.Players;

namespace Doko.Domain.Lobby;

public record LobbyPlayer(PlayerId Id, DateTimeOffset JoinedAt);

public class LobbyState
{
    private readonly LobbyPlayer?[] _seats = new LobbyPlayer?[4];
    private readonly int[] _standings = new int[4];
    private readonly HashSet<byte> _newGameVoters = [];

    public LobbyId Id { get; }
    public DateTimeOffset CreatedAt { get; }

    /// <summary>All currently seated players (non-null seats).</summary>
    public IEnumerable<LobbyPlayer> Players => _seats.Where(s => s != null).Cast<LobbyPlayer>();

    /// <summary>Snapshot of the 4 seats; null means empty.</summary>
    public IReadOnlyList<LobbyPlayer?> Seats => _seats.AsReadOnly();

    public bool IsFull => _seats.All(s => s != null);
    public bool IsStarted { get; private set; }

    /// <summary>The game that is currently running in this lobby, if any.</summary>
    public GameId? ActiveGameId { get; private set; }

    /// <summary>Cumulative lobby standings per seat (index 0–3).</summary>
    public IReadOnlyList<int> Standings => Array.AsReadOnly(_standings);

    /// <summary>Number of players who have voted to start a new game.</summary>
    public int NewGameVoteCount => _newGameVoters.Count;

    private LobbyState(LobbyId id, DateTimeOffset createdAt)
    {
        Id = id;
        CreatedAt = createdAt;
    }

    /// <summary>Creates a new lobby; the creator automatically occupies seat 0.</summary>
    public static LobbyState Create()
    {
        var lobby = new LobbyState(LobbyId.New(), DateTimeOffset.UtcNow);
        lobby._seats[0] = new LobbyPlayer(new PlayerId(0), DateTimeOffset.UtcNow);
        return lobby;
    }

    /// <summary>
    /// Tries to occupy a specific seat. Returns false if the index is out of range,
    /// the seat is already taken, or the lobby is full/started.
    /// </summary>
    public bool TryOccupySeat(int seatIndex, out PlayerId playerId)
    {
        playerId = default;
        if (seatIndex < 0 || seatIndex >= 4) return false;
        if (IsStarted) return false;
        if (_seats[seatIndex] != null) return false;

        playerId = new PlayerId((byte)seatIndex);
        _seats[seatIndex] = new LobbyPlayer(playerId, DateTimeOffset.UtcNow);
        return true;
    }

    /// <summary>
    /// Removes the player from their seat. Returns true if the lobby is now completely empty.
    /// </summary>
    public bool TryRemovePlayer(PlayerId playerId)
    {
        _seats[playerId.Value] = null;
        return _seats.All(s => s == null);
    }

    /// <summary>Returns true if the given player currently holds a seat.</summary>
    public bool HasPlayer(PlayerId playerId) =>
        playerId.Value < 4 && _seats[playerId.Value] != null;

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
    public bool AddNewGameVote(PlayerId playerId)
    {
        _newGameVoters.Add(playerId.Value);
        return _newGameVoters.Count >= 4;
    }

    /// <summary>Withdraws a player's vote to start a new game.</summary>
    public void RemoveNewGameVote(PlayerId playerId) =>
        _newGameVoters.Remove(playerId.Value);

    /// <summary>Clears all new-game votes (called when the new game actually starts).</summary>
    public void ResetNewGameVotes() => _newGameVoters.Clear();

    /// <summary>Applies per-seat net point deltas to the running lobby standings.</summary>
    public void UpdateStandings(int[] netPointsPerSeat)
    {
        for (int i = 0; i < 4 && i < netPointsPerSeat.Length; i++)
            _standings[i] += netPointsPerSeat[i];
    }
}
