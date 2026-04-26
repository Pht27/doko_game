using Doko.Application.Abstractions;
using Doko.Application.Lobbies;
using Doko.Infrastructure.Repositories;
using Doko.Infrastructure.Shuffler;
using Microsoft.Extensions.DependencyInjection;

namespace Doko.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers infrastructure services: <see cref="IGameRepository"/> (in-memory) and
    /// <see cref="IDeckShuffler"/> (random). <see cref="IGameEventPublisher"/> is intentionally
    /// excluded — each presentation layer registers its own implementation.
    /// </summary>
    public static IServiceCollection AddDokoInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IGameRepository, InMemoryGameRepository>();
        services.AddSingleton<ILobbyRepository, InMemoryLobbyRepository>();
        services.AddSingleton<IDeckShuffler, RandomDeckShuffler>();
        return services;
    }
}
