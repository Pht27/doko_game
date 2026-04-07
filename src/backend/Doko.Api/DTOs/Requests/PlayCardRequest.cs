namespace Doko.Api.DTOs.Requests;

/// <param name="CardId">The physical card id (0–47) to play.</param>
/// <param name="ActivateSonderkarten">SonderkarteType names to activate alongside this card play.</param>
/// <param name="GenscherPartnerId">Required when activating Genscherdamen or Gegengenscherdamen.</param>
public record PlayCardRequest(
    int CardId,
    IReadOnlyList<string> ActivateSonderkarten,
    int? GenscherPartnerId
);
