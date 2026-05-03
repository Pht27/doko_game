using System.ComponentModel.DataAnnotations;

namespace Doko.Api.DTOs.Analog;

public record CreatePlayerRequest(
    [Required, MaxLength(50)] string Name,
    decimal StartingPoints = 0
);
