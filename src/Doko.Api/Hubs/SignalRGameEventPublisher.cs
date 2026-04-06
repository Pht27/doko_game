using Doko.Api.Mapping;
using Doko.Application.Abstractions;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Events;
using Microsoft.AspNetCore.SignalR;

namespace Doko.Api.Hubs;

public sealed class SignalRGameEventPublisher(IHubContext<GameHub> hubContext) : IGameEventPublisher
{
    public async Task PublishAsync(GameId gameId, IReadOnlyList<IDomainEvent> events, CancellationToken ct = default)
    {
        var group = hubContext.Clients.Group(gameId.ToString());

        foreach (var evt in events)
        {
            var task = evt switch
            {
                CardPlayedEvent e => group.SendAsync("cardPlayed", new
                {
                    player = e.Player.Value,
                    card = DtoMapper.ToDto(e.Card),
                    trickNumber = e.TrickNumber,
                }, ct),

                TrickCompletedEvent e => group.SendAsync("trickCompleted", new
                {
                    trickNumber = e.TrickNumber,
                    winner = e.Winner.Value,
                    awards = e.Result.Awards.Select(DtoMapper.ToDto).ToArray(),
                }, ct),

                AnnouncementMadeEvent e => group.SendAsync("announcementMade", new
                {
                    player = e.Player.Value,
                    type = e.Type.ToString(),
                    trickNumber = e.TrickNumber,
                }, ct),

                ReservationMadeEvent e => group.SendAsync("reservationMade", new
                {
                    player = e.Player.Value,
                    reservation = e.Reservation?.Priority.ToString(),
                }, ct),

                SonderkarteTriggeredEvent e => group.SendAsync("sonderkarteTriggered", new
                {
                    player = e.Player.Value,
                    type = e.Type.ToString(),
                }, ct),

                PartyRevealedEvent e => group.SendAsync("partyRevealed", new
                {
                    player = e.Player.Value,
                    party = e.Party.ToString(),
                }, ct),

                _ => Task.CompletedTask,
            };

            await task;
        }
    }
}
