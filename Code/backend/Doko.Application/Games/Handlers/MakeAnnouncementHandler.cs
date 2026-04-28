using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Domain.Announcements;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Events;
using Doko.Domain.Sonderkarten;
using static Doko.Application.Common.GameActionResultExtensions;

namespace Doko.Application.Games.Handlers;

public interface IMakeAnnouncementHandler
{
    Task<GameActionResult<Unit>> ExecuteAsync(
        MakeAnnouncementCommand command,
        CancellationToken ct = default
    );
}

public sealed class MakeAnnouncementHandler(
    IGameRepository repository,
    IGameEventPublisher publisher
) : IMakeAnnouncementHandler
{
    public async Task<GameActionResult<Unit>> ExecuteAsync(
        MakeAnnouncementCommand command,
        CancellationToken ct = default
    )
    {
        var loaded = await repository.LoadOrFailAsync<Unit>(command.GameId, ct);
        if (loaded.Failure is not null)
            return loaded.Failure;
        var state = loaded.State!;

        if (state.Phase != GamePhase.Playing)
            return Fail<Unit>(GameError.InvalidPhase);

        if (!AnnouncementRules.CanAnnounce(command.Player, command.Type, state))
            return Fail<Unit>(GameError.AnnouncementNotAllowed);

        int trickNum = state.CompletedTricks.Count;
        int cardIdx = state.CurrentTrick?.Cards.Count ?? 0;

        bool isEffective = state.PartyResolver.IsAnnouncementEffective(command.Player, state);
        var announcement = new Announcement(command.Player, command.Type, trickNum, cardIdx)
        {
            IsEffective = isEffective,
        };
        state.Apply(new AddAnnouncementModification(announcement));

        await repository.SaveAsync(state, ct);
        await publisher.PublishAsync(
            state.Id,
            [new AnnouncementMadeEvent(state.Id, command.Player, command.Type, trickNum, cardIdx)],
            ct
        );

        return Ok(Unit.Value);
    }
}
