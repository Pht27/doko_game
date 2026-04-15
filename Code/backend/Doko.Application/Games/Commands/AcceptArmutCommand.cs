using Doko.Domain.GameFlow;
using Doko.Domain.Players;

namespace Doko.Application.Games.Commands;

/// <summary>
/// A player responds to an Armut partner request during <see cref="GamePhase.ArmutPartnerFinding"/>.
/// </summary>
public record AcceptArmutCommand(GameId GameId, PlayerId Player, bool Accepts);
