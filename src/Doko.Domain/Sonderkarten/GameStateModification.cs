using Doko.Domain.Announcements;
using Doko.Domain.Cards;
using Doko.Domain.Players;

namespace Doko.Domain.Sonderkarten;

public abstract record GameStateModification;

public sealed record ReverseDirectionModification : GameStateModification;

public sealed record WithdrawAnnouncementModification(
    PlayerId Player,
    AnnouncementType Type) : GameStateModification;

public sealed record TransferCardPointsModification(
    CardType From,
    CardType To) : GameStateModification;

public sealed record ActivateSonderkarteModification(
    SonderkarteType Type) : GameStateModification;
