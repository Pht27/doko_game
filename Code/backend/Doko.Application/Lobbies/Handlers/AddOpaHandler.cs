using Doko.Domain.Lobby;
using Doko.Domain.Players;

namespace Doko.Application.Lobbies.Handlers;

public record AddOpaCommand(LobbyId LobbyId, int SeatIndex);

public record AddOpaResult(PlayerSeat Seat, bool IsNowFull);

public interface IAddOpaHandler
{
    Task<LobbyActionResult<AddOpaResult>> ExecuteAsync(
        AddOpaCommand command,
        CancellationToken ct = default
    );
}

public sealed class AddOpaHandler(ILobbyRepository repository) : IAddOpaHandler
{
    public async Task<LobbyActionResult<AddOpaResult>> ExecuteAsync(
        AddOpaCommand command,
        CancellationToken ct = default
    )
    {
        var lobby = await repository.GetAsync(command.LobbyId, ct);
        if (lobby is null)
            return new LobbyActionResult<AddOpaResult>.Failure(LobbyError.LobbyNotFound);

        if (!lobby.TryOccupySeatAsOpa(command.SeatIndex, out var seat))
            return new LobbyActionResult<AddOpaResult>.Failure(LobbyError.SeatOccupied);

        // Opa is always ready to start
        lobby.AddLobbyStartVote(seat);

        await repository.SaveAsync(lobby, ct);
        return new LobbyActionResult<AddOpaResult>.Ok(new AddOpaResult(seat, lobby.IsFull));
    }
}
