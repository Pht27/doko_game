using Doko.Analog.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Doko.Analog;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDokoAnalog(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<AnalogDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Analog"))
        );

        services.AddScoped<AnalogPlayersService>();

        return services;
    }
}
