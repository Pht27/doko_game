using Doko.Application.Abstractions;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Events;

namespace Doko.Console.Events;

public sealed class ConsoleGameEventPublisher : IGameEventPublisher
{
    public Task PublishAsync(GameId gameId, IReadOnlyList<IDomainEvent> events, CancellationToken ct = default)
    {
        foreach (var evt in events)
        {
            switch (evt)
            {
                case AnnouncementMadeEvent e:
                    System.Console.WriteLine($"  >> Player {e.Player} announces {e.Type}!");
                    break;
                case SonderkarteTriggeredEvent e:
                    System.Console.WriteLine($"  >> Player {e.Player} activates {e.Type}!");
                    break;
                case ReservationMadeEvent e:
                    var name = e.Reservation is null
                        ? "Keine Vorbehalt"
                        : e.Reservation.GetType().Name.Replace("Reservation", "");
                    System.Console.WriteLine($"  >> Player {e.Player} declares: {name}");
                    break;
                case PartyRevealedEvent e:
                    System.Console.WriteLine($"  >> Player {e.Player} is revealed as {e.Party}!");
                    break;
            }
        }
        return Task.CompletedTask;
    }
}
