using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Results;
using Doko.Application.Games.UseCases;
using Doko.Domain.GameFlow;

namespace Doko.Api.IntegrationTests.Stubs;

public class StubStartGameUseCase : IStartGameUseCase
{
    public Func<StartGameCommand, GameActionResult<StartGameResult>>? Handler { get; set; }

    public Task<GameActionResult<StartGameResult>> ExecuteAsync(
        StartGameCommand command,
        CancellationToken ct = default
    )
    {
        if (Handler is not null)
            return Task.FromResult(Handler(command));
        return Task.FromResult<GameActionResult<StartGameResult>>(
            new GameActionResult<StartGameResult>.Ok(new StartGameResult(GameId.New()))
        );
    }
}

public class StubDealCardsUseCase : IDealCardsUseCase
{
    public Func<DealCardsCommand, GameActionResult<Unit>>? Handler { get; set; }

    public Task<GameActionResult<Unit>> ExecuteAsync(
        DealCardsCommand command,
        CancellationToken ct = default
    )
    {
        if (Handler is not null)
            return Task.FromResult(Handler(command));
        return Task.FromResult<GameActionResult<Unit>>(new GameActionResult<Unit>.Ok(Unit.Value));
    }
}

public class StubMakeReservationUseCase : IMakeReservationUseCase
{
    public Func<
        MakeReservationCommand,
        GameActionResult<MakeReservationResult>
    >? Handler { get; set; }

    public Task<GameActionResult<MakeReservationResult>> ExecuteAsync(
        MakeReservationCommand command,
        CancellationToken ct = default
    )
    {
        if (Handler is not null)
            return Task.FromResult(Handler(command));
        return Task.FromResult<GameActionResult<MakeReservationResult>>(
            new GameActionResult<MakeReservationResult>.Ok(
                new MakeReservationResult(AllDeclared: false, WinningReservation: null)
            )
        );
    }
}

public class StubPlayCardUseCase : IPlayCardUseCase
{
    public Func<PlayCardCommand, GameActionResult<PlayCardResult>>? Handler { get; set; }

    public Task<GameActionResult<PlayCardResult>> ExecuteAsync(
        PlayCardCommand command,
        CancellationToken ct = default
    )
    {
        if (Handler is not null)
            return Task.FromResult(Handler(command));
        return Task.FromResult<GameActionResult<PlayCardResult>>(
            new GameActionResult<PlayCardResult>.Ok(
                new PlayCardResult(
                    TrickCompleted: false,
                    TrickWinner: null,
                    GameFinished: false,
                    FinishedResult: null
                )
            )
        );
    }
}

public class StubMakeAnnouncementUseCase : IMakeAnnouncementUseCase
{
    public Func<MakeAnnouncementCommand, GameActionResult<Unit>>? Handler { get; set; }

    public Task<GameActionResult<Unit>> ExecuteAsync(
        MakeAnnouncementCommand command,
        CancellationToken ct = default
    )
    {
        if (Handler is not null)
            return Task.FromResult(Handler(command));
        return Task.FromResult<GameActionResult<Unit>>(new GameActionResult<Unit>.Ok(Unit.Value));
    }
}
