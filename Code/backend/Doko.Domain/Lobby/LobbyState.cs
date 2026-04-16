using Doko.Domain.Players;

namespace Doko.Domain.Lobby;

public record LobbyPlayer(PlayerId Id, DateTimeOffset JoinedAt);

public class LobbyState
{
    private readonly LobbyPlayer?[] _seats = new LobbyPlayer?[4];

    public LobbyId Id { get; }
    public DateTimeOffset CreatedAt { get; }

    /// <summary>All currently seated players (non-null seats).</summary>
    public IEnumerable<LobbyPlayer> Players => _seats.Where(s => s != null).Cast<LobbyPlayer>();

    /// <summary>Snapshot of the 4 seats; null means empty.</summary>
    public IReadOnlyList<LobbyPlayer?> Seats => _seats.AsReadOnly();

    public bool IsFull => _seats.All(s => s != null);
    public bool IsStarted { get; private set; }

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

    public void MarkStarted() => IsStarted = true;
}
