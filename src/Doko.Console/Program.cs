using Doko.Application;
using Doko.Application.Abstractions;
using Doko.Console;
using Doko.Console.Events;
using Doko.Console.Input;
using Doko.Console.Rendering;
using Doko.Console.Scenarios;
using Doko.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

var scenario = PromptScenario();

var services = new ServiceCollection();
services.AddDokoApplication();
services.AddDokoInfrastructure();
if (scenario is not null)
    services.AddSingleton<IDeckShuffler>(new ScenarioShuffler(scenario));
services.AddSingleton<IGameEventPublisher, ConsoleGameEventPublisher>();
services.AddTransient<GameRenderer>();
services.AddTransient<ConsoleInputReader>();
services.AddTransient<ConsoleGame>();

static ScenarioConfig? PromptScenario()
{
    var all = Doko.Console.Scenarios.Scenarios.All;

    System.Console.WriteLine("=== SCENARIO SETUP ===");
    System.Console.WriteLine("  [0] Random (normal shuffle)");
    for (int i = 0; i < all.Length; i++)
        System.Console.WriteLine($"  [{i + 1}] {all[i].Name}");
    System.Console.WriteLine();

    while (true)
    {
        System.Console.Write($"Choose [0-{all.Length}]: ");
        var input = System.Console.ReadLine()?.Trim();
        if (int.TryParse(input, out int choice) && choice >= 0 && choice <= all.Length)
        {
            System.Console.WriteLine();
            return choice == 0 ? null : all[choice - 1];
        }
        System.Console.WriteLine($"  Enter a number between 0 and {all.Length}.");
    }
}

await using var provider = services.BuildServiceProvider();
await using var scope = provider.CreateAsyncScope();

var game = scope.ServiceProvider.GetRequiredService<ConsoleGame>();
await game.RunAsync();
