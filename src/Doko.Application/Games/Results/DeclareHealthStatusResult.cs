namespace Doko.Application.Games.Results;

/// <param name="AllDeclared">True when all four players have now declared their health status.</param>
public record DeclareHealthStatusResult(bool AllDeclared);
