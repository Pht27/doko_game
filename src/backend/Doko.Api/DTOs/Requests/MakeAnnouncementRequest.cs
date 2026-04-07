namespace Doko.Api.DTOs.Requests;

/// <param name="Type">AnnouncementType enum name (e.g. "Re", "Kontra", "Keine90").</param>
public record MakeAnnouncementRequest(string Type);
