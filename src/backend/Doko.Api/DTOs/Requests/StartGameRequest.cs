namespace Doko.Api.DTOs.Requests;

public record StartGameRequest(IReadOnlyList<int> PlayerIds);
