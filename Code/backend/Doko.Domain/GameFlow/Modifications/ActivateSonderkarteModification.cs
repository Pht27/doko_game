using Doko.Domain.Sonderkarten;

namespace Doko.Domain.GameFlow.Modifications;

public sealed record ActivateSonderkarteModification(SonderkarteType Type) : GameStateModification;
