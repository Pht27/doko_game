using Doko.Application.Abstractions;

namespace Doko.Application.Scenarios;

public interface IScenarioShufflerFactory
{
    IDeckShuffler? TryCreate(string? scenarioName);
}
