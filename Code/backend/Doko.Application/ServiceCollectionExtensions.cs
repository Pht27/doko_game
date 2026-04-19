using Doko.Application.Abstractions;
using Doko.Application.Games;
using Doko.Application.Games.Handlers;
using Doko.Application.Lobbies.Handlers;
using Doko.Domain.Scoring;
using Microsoft.Extensions.DependencyInjection;

namespace Doko.Application;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all application-layer services.
    /// The Api layer must also register: <see cref="IGameRepository"/>, <see cref="IGameEventPublisher"/>,
    /// <see cref="IDeckShuffler"/>, <see cref="Lobbies.ILobbyRepository"/>.
    /// </summary>
    public static IServiceCollection AddDokoApplication(this IServiceCollection services)
    {
        services.AddScoped<IStartGameHandler, StartGameHandler>();
        services.AddScoped<IDealCardsHandler, DealCardsHandler>();
        services.AddScoped<IDeclareHealthStatusHandler, DeclareHealthStatusHandler>();
        services.AddScoped<IMakeReservationHandler, MakeReservationHandler>();
        services.AddScoped<IAcceptArmutHandler, AcceptArmutHandler>();
        services.AddScoped<IExchangeArmutCardsHandler, ExchangeArmutCardsHandler>();
        services.AddScoped<IPlayCardHandler, PlayCardHandler>();
        services.AddScoped<IMakeAnnouncementHandler, MakeAnnouncementHandler>();
        services.AddScoped<IGameQueryService, GameQueryService>();
        services.AddSingleton<IGameScorer, GameScorer>();

        services.AddScoped<ICreateLobbyHandler, CreateLobbyHandler>();
        services.AddScoped<IJoinSeatHandler, JoinSeatHandler>();
        services.AddScoped<ILeaveLobbyHandler, LeaveLobbyHandler>();
        services.AddScoped<ISwapSeatHandler, SwapSeatHandler>();
        services.AddScoped<IAddOpaHandler, AddOpaHandler>();
        services.AddScoped<IRemoveOpaHandler, RemoveOpaHandler>();

        return services;
    }
}
