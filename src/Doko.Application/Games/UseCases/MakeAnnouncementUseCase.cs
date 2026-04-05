using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Domain.Announcements;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Events;
using Doko.Domain.Sonderkarten;

namespace Doko.Application.Games.UseCases;

public interface IMakeAnnouncementUseCase
{
    Task<GameActionResult<Unit>> ExecuteAsync(MakeAnnouncementCommand command, CancellationToken ct = default);
}

public sealed class MakeAnnouncementUseCase(IGameRepository repository, IGameEventPublisher publisher) : IMakeAnnouncementUseCase
{
    public async Task<GameActionResult<Unit>> ExecuteAsync(MakeAnnouncementCommand command, CancellationToken ct = default)
    {
        var state = await repository.GetAsync(command.GameId, ct);
        if (state is null)
            return new GameActionResult<Unit>.Failure(GameError.GameNotFound);

        if (state.Phase != GamePhase.Playing)
            return new GameActionResult<Unit>.Failure(GameError.InvalidPhase);

        if (!AnnouncementRules.CanAnnounce(command.Player, command.Type, state))
            return new GameActionResult<Unit>.Failure(GameError.AnnouncementNotAllowed);

        int trickNum  = state.CompletedTricks.Count;
        int cardIdx   = state.CurrentTrick?.Cards.Count ?? 0;

        var announcement = new Announcement(command.Player, command.Type, trickNum, cardIdx);
        state.Apply(new AddAnnouncementModification(announcement));

        await repository.SaveAsync(state, ct);
        await publisher.PublishAsync(state.Id,
            [new AnnouncementMadeEvent(state.Id, command.Player, command.Type, trickNum, cardIdx)], ct);

        return new GameActionResult<Unit>.Ok(Unit.Value);
    }
}
