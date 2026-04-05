using Doko.Application;
using Doko.Application.Abstractions;
using Doko.Console;
using Doko.Console.Events;
using Doko.Console.Input;
using Doko.Console.Rendering;
using Doko.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddDokoApplication();
services.AddDokoInfrastructure();
services.AddSingleton<IGameEventPublisher, ConsoleGameEventPublisher>();
services.AddTransient<GameRenderer>();
services.AddTransient<ConsoleInputReader>();
services.AddTransient<ConsoleGame>();

await using var provider = services.BuildServiceProvider();
await using var scope = provider.CreateAsyncScope();

var game = scope.ServiceProvider.GetRequiredService<ConsoleGame>();
await game.RunAsync();
