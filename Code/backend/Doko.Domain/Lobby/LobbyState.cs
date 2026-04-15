using Doko.Domain.Players;

namespace Doko.Domain.Lobby;

public record LobbyPlayer(PlayerId Id, DateTimeOffset JoinedAt);

public class LobbyState
{
    private readonly List<LobbyPlayer> _players = [];

    public LobbyId Id { get; }
    public DateTimeOffset CreatedAt { get; }
    public IReadOnlyList<LobbyPlayer> Players => _players.AsReadOnly();
    public PlayerId HostId => _players[0].Id;
    public bool IsFull => _players.Count == 4;
    public bool IsStarted { get; private set; }

    private LobbyState(LobbyId id, DateTimeOffset createdAt)
    {
        Id = id;
        CreatedAt = createdAt;
    }

    /// <summary>Creates a new lobby with the first player (seat 0) as host.</summary>
    public static LobbyState Create()
    {
        var lobby = new LobbyState(LobbyId.New(), DateTimeOffset.UtcNow);
        lobby._players.Add(new LobbyPlayer(new PlayerId(0), DateTimeOffset.UtcNow));
        return lobby;
    }

    /// <summary>
    /// Tries to add the next player. Returns false if the lobby is already full.
    /// The assigned <paramref name="newPlayerId"/> is the seat index (1, 2, or 3).
    /// </summary>
    public bool TryAddPlayer(out PlayerId newPlayerId)
    {
        if (IsFull)
        {
            newPlayerId = default;
            return false;
        }

        newPlayerId = new PlayerId((byte)_players.Count);
        _players.Add(new LobbyPlayer(newPlayerId, DateTimeOffset.UtcNow));
        return true;
    }

    public void MarkStarted() => IsStarted = true;
}
