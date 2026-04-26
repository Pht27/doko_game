using Doko.Api.Mapping;
using Doko.Application.Abstractions;
using Doko.Domain.GameFlow;
using Doko.Domain.GameFlow.Events;
using Microsoft.AspNetCore.SignalR;

namespace Doko.Api.Hubs;

public sealed class SignalRGameEventPublisher(IHubContext<GameHub> hubContext) : IGameEventPublisher
{
    public async Task PublishAsync(
        GameId gameId,
        IReadOnlyList<IDomainEvent> events,
        CancellationToken ct = default
    )
    {
        var group = hubContext.Clients.Group(gameId.ToString());

        foreach (var evt in events)
        {
            var task = evt switch
            {
                CardPlayedEvent e => group.SendAsync(
                    "cardPlayed",
                    new
                    {
                        player = (int)e.Player,
                        card = DtoMapper.ToDto(e.Card),
                        trickNumber = e.TrickNumber,
                    },
                    ct
                ),

                TrickCompletedEvent e => group.SendAsync(
                    "trickCompleted",
                    new
                    {
                        trickNumber = e.TrickNumber,
                        winner = (int)e.Winner,
                        awards = e.Result.Awards.Select(DtoMapper.ToDto).ToArray(),
                    },
                    ct
                ),

                AnnouncementMadeEvent e => group.SendAsync(
                    "announcementMade",
                    new
                    {
                        player = (int)e.Player,
                        type = e.Type.ToString(),
                        trickNumber = e.TrickNumber,
                    },
                    ct
                ),

                HealthDeclaredEvent e => group.SendAsync(
                    "healthDeclared",
                    new { player = (int)e.Player },
                    ct
                ),

                ReservationMadeEvent e => group.SendAsync(
                    "reservationMade",
                    new
                    {
                        player = (int)e.Player,
                        reservation = e.Reservation?.Priority.ToString(),
                    },
                    ct
                ),

                SonderkarteTriggeredEvent e => group.SendAsync(
                    "sonderkarteTriggered",
                    new { player = (int)e.Player, type = e.Type.ToString() },
                    ct
                ),

                PartyRevealedEvent e => group.SendAsync(
                    "partyRevealed",
                    new { player = (int)e.Player, party = e.Party.ToString() },
                    ct
                ),

                ArmutResponseEvent e => group.SendAsync(
                    "armutResponse",
                    new { player = (int)e.Player, accepted = e.Accepted },
                    ct
                ),

                ArmutCardsExchangedEvent e => group.SendAsync(
                    "armutCardsExchanged",
                    new
                    {
                        richPlayer = (int)e.RichPlayer,
                        cardCount = e.CardCount,
                        includedTrump = e.IncludedTrump,
                    },
                    ct
                ),

                _ => Task.CompletedTask,
            };

            await task;
        }
    }
}
