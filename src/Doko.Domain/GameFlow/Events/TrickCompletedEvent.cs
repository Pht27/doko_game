using Doko.Domain.Players;
using Doko.Domain.Scoring;

namespace Doko.Domain.GameFlow.Events;

public sealed record TrickCompletedEvent(
    GameId GameId,
    int TrickNumber,
    PlayerId Winner,
    TrickResult Result) : IDomainEvent;
