namespace Doko.Api.DTOs.Analog;

public record PatchPlayerRequest(bool IsActive, string? Name = null, string? HeroCard = null);
