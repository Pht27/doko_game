using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Domain.Announcements;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Events;
using Doko.Domain.GameFlow.Modifications;
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
    public Task<GameActionResult<Unit>> ExecuteAsync(
        MakeAnnouncementCommand command,
        CancellationToken ct = default
    ) =>
        GameCommandPipeline.RunAsync<Unit, PlayingState>(
            repository,
            publisher,
            command.GameId,
            execute: (PlayingState state) =>
            {
                if (state.Phase != GamePhase.Playing)
                    return (Fail<Unit>(GameError.InvalidPhase), [], state);

                if (!AnnouncementRules.CanAnnounce(command.Player, command.Type, state))
                    return (Fail<Unit>(GameError.AnnouncementNotAllowed), [], state);

                int trickNum = state.CompletedTricks.Count;
                int cardIdx = state.CurrentTrick?.Cards.Count ?? 0;
                bool isEffective = state.PartyResolver.IsAnnouncementEffective(
                    command.Player,
                    state
                );
                var announcement = new Announcement(command.Player, command.Type, trickNum, cardIdx)
                {
                    IsEffective = isEffective,
                };
                GameState nextState = state.Apply(new AddAnnouncementModification(announcement));

                return (
                    Ok(Unit.Value),
                    [
                        new AnnouncementMadeEvent(
                            nextState.Id,
                            command.Player,
                            command.Type,
                            trickNum,
                            cardIdx
                        ),
                    ],
                    nextState
                );
            },
            ct
        );
}
