using Doko.Domain.Lobby;
using Doko.Domain.Players;

namespace Doko.Application.Lobbies.Handlers;

public record CreateLobbyResult(LobbyId LobbyId, PlayerSeat Seat);

public interface ICreateLobbyHandler
{
    Task<LobbyActionResult<CreateLobbyResult>> ExecuteAsync(CancellationToken ct = default);
}

public sealed class CreateLobbyHandler(ILobbyRepository repository) : ICreateLobbyHandler
{
    public async Task<LobbyActionResult<CreateLobbyResult>> ExecuteAsync(
        CancellationToken ct = default
    )
    {
        var lobby = LobbyState.Create();
        await repository.SaveAsync(lobby, ct);
        return new LobbyActionResult<CreateLobbyResult>.Ok(
            new CreateLobbyResult(lobby.Id, PlayerSeat.First)
        );
    }
}
