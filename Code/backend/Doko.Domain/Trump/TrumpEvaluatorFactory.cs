using Doko.Domain.GameFlow;
using Doko.Domain.Reservations;
using Doko.Domain.Rules;
using Doko.Domain.Sonderkarten;

namespace Doko.Domain.Trump;

public sealed class TrumpEvaluatorFactory : ITrumpEvaluatorFactory
{
    public static readonly TrumpEvaluatorFactory Instance = new();

    private TrumpEvaluatorFactory() { }

    public ITrumpEvaluator Build(
        IReservation? activeReservation,
        SilentGameMode? silentMode,
        IReadOnlyCollection<SonderkarteType> activeSonderkarten,
        RuleSet rules
    )
    {
        var baseEvaluator =
            activeReservation?.Apply().TrumpEvaluator
            ?? (
                silentMode?.Type == SilentGameModeType.KontraSolo
                    ? (ITrumpEvaluator)KontraSoloTrumpEvaluator.Instance
                    : NormalTrumpEvaluator.Instance
            );

        var activeSet = activeSonderkarten.ToHashSet();
        var suppressed = SonderkarteRegistry
            .GetEnabled(rules)
            .Where(s => activeSet.Contains(s.Type) && s.Suppresses.HasValue)
            .Select(s => s.Suppresses!.Value)
            .ToHashSet();

        var modifiers = SonderkarteRegistry
            .GetEnabled(rules)
            .Where(s =>
                activeSet.Contains(s.Type)
                && !suppressed.Contains(s.Type)
                && s.RankingModifier is not null
            )
            .Select(s => s.RankingModifier!)
            .ToList();

        return modifiers.Count > 0
            ? new StandardSonderkarteDecorator(baseEvaluator, modifiers)
            : baseEvaluator;
    }
}
