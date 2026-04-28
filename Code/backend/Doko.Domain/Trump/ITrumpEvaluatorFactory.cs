using Doko.Domain.GameFlow;
using Doko.Domain.Reservations;
using Doko.Domain.Rules;
using Doko.Domain.Sonderkarten;

namespace Doko.Domain.Trump;

public interface ITrumpEvaluatorFactory
{
    ITrumpEvaluator Build(
        IReservation? activeReservation,
        SilentGameMode? silentMode,
        IReadOnlyCollection<SonderkarteType> activeSonderkarten,
        RuleSet rules
    );
}
