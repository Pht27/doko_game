using Doko.Application.Abstractions;
using Doko.Application.Games;
using Doko.Application.Games.UseCases;
using Doko.Domain.Scoring;
using Microsoft.Extensions.DependencyInjection;

namespace Doko.Application;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all application-layer services.
    /// The Api layer must also register: <see cref="IGameRepository"/>, <see cref="IGameEventPublisher"/>,
    /// <see cref="IDeckShuffler"/>.
    /// </summary>
    public static IServiceCollection AddDokoApplication(this IServiceCollection services)
    {
        services.AddScoped<IStartGameUseCase, StartGameUseCase>();
        services.AddScoped<IDealCardsUseCase, DealCardsUseCase>();
        services.AddScoped<IDeclareHealthStatusUseCase, DeclareHealthStatusUseCase>();
        services.AddScoped<IMakeReservationUseCase, MakeReservationUseCase>();
        services.AddScoped<IAcceptArmutUseCase, AcceptArmutUseCase>();
        services.AddScoped<IExchangeArmutCardsUseCase, ExchangeArmutCardsUseCase>();
        services.AddScoped<IPlayCardUseCase, PlayCardUseCase>();
        services.AddScoped<IMakeAnnouncementUseCase, MakeAnnouncementUseCase>();
        services.AddScoped<IGameQueryService, GameQueryService>();
        services.AddSingleton<IGameScorer, GameScorer>();
        return services;
    }
}
