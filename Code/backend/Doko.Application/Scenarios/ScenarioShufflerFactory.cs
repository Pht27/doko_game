using Doko.Application.Abstractions;

namespace Doko.Application.Scenarios;

public sealed class ScenarioShufflerFactory : IScenarioShufflerFactory
{
    public IDeckShuffler? TryCreate(string? scenarioName)
    {
        if (scenarioName is null)
            return null;
        var config = Scenarios.All.FirstOrDefault(s => s.Name == scenarioName);
        return config is not null ? new ScenarioShuffler(config) : null;
    }
}
