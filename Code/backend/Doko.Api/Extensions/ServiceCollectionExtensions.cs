using System.Text.Json.Serialization;
using Doko.Api.Hubs;
using Doko.Api.Services;
using Doko.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Doko.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDokoApi(this IServiceCollection services)
    {
        services.AddSingleton<IGameEventPublisher, SignalRGameEventPublisher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<IOpaService, OpaService>();
        services
            .AddControllers()
            .AddJsonOptions(opts =>
                opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
            );
        return services;
    }
}
