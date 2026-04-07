using Doko.Api.Hubs;
using Doko.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Doko.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDokoApi(this IServiceCollection services)
    {
        services.AddSingleton<IGameEventPublisher, SignalRGameEventPublisher>();
        services.AddControllers();
        return services;
    }
}
