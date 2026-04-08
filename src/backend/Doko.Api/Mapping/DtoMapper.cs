using Doko.Api.DTOs.Requests;
using Doko.Api.DTOs.Responses;
using Doko.Application.Games.Queries;
using Doko.Application.Games.Results;
using Doko.Domain.Cards;
using Doko.Domain.Extrapunkte;
using Doko.Domain.Parties;
using Doko.Domain.Players;
using Doko.Domain.Reservations;
using Doko.Domain.Scoring;

namespace Doko.Api.Mapping;

public static class DtoMapper
{
    public static PlayerGameViewResponse ToResponse(PlayerGameView view) =>
        new(
            GameId: view.GameId.ToString(),
            Phase: view.Phase.ToString(),
            RequestingPlayer: view.RequestingPlayer.Value,
            Hand: view.Hand.Select(ToDto).ToList(),
            HandSorted: view.HandSorted.Select(ToDto).ToList(),
            LegalCards: view.LegalCards.Select(ToDto).ToList(),
            LegalAnnouncements: view.LegalAnnouncements.Select(a => a.ToString()).ToList(),
            EligibleSonderkartenPerCard: view.EligibleSonderkartenPerCard.ToDictionary(
                kvp => (int)kvp.Key.Value,
                kvp => (IReadOnlyList<SonderkarteInfoDto>)kvp.Value.Select(ToDto).ToList()
            ),
            OtherPlayers: view.OtherPlayers.Select(ToDto).ToList(),
            CurrentTrick: view.CurrentTrick is { } ct ? ToDto(ct) : null,
            CompletedTricks: view.CompletedTricks.Select(ToDto).ToList(),
            CurrentTurn: view.CurrentTurn.Value,
            IsMyTurn: view.IsMyTurn,
            EligibleReservations: view.EligibleReservations.Select(r => r.ToString()).ToList()
        )
        {
            ShouldDeclareHealth = view.ShouldDeclareHealth,
            MustDeclareReservation = view.MustDeclareReservation,
            ShouldRespondToArmut = view.ShouldRespondToArmut,
            ShouldReturnArmutCards = view.ShouldReturnArmutCards,
            ArmutCardReturnCount = view.ArmutCardReturnCount,
            ArmutExchangeCardCount = view.ArmutExchangeCardCount,
            ArmutReturnedTrump = view.ArmutReturnedTrump,
        };

    public static CardDto ToDto(Card card) =>
        new(card.Id.Value, card.Type.Suit.ToString(), card.Type.Rank.ToString());

    public static SonderkarteInfoDto ToDto(SonderkarteInfo info) =>
        new(info.Type.ToString(), info.Name, info.Description);

    public static PlayerPublicStateDto ToDto(PlayerPublicState p) =>
        new(p.Id.Value, p.Seat.ToString(), p.KnownParty?.ToString(), p.HandCardCount);

    public static TrickSummaryDto ToDto(TrickSummary t) =>
        new(
            t.TrickNumber,
            t.Cards.Select(c => new TrickCardDto(c.Player.Value, ToDto(c.Card))).ToList(),
            t.Winner?.Value
        );

    public static GameResultDto ToDto(GameResult r) =>
        new(
            r.Winner.ToString(),
            r.RePoints,
            r.KontraPoints,
            r.GameValue,
            r.AllAwards.Select(ToDto).ToList(),
            r.Feigheit
        );

    public static ExtrapunktAwardDto ToDto(ExtrapunktAward a) =>
        new(a.Type.ToString(), a.BenefittingPlayer.Value, a.Delta);

    public static MakeReservationResponse ToResponse(MakeReservationResult r) =>
        new(r.AllDeclared, r.WinningReservation?.Priority.ToString(), r.Geschmissen);

    public static PlayCardResponse ToResponse(PlayCardResult r) =>
        new(
            r.TrickCompleted,
            r.TrickWinner?.Value,
            r.GameFinished,
            r.FinishedResult is { } fr ? ToDto(fr.Result) : null
        );

    /// <summary>
    /// Constructs an <see cref="IReservation"/> from the API request, mirroring
    /// <c>ConsoleInputReader.BuildReservation</c>.
    /// Returns null for keine Vorbehalt (Reservation is null or unrecognized).
    /// </summary>
    public static IReservation? BuildReservation(MakeReservationRequest req, PlayerId player)
    {
        if (req.Reservation is null)
            return null;

        if (
            !Enum.TryParse<ReservationPriority>(req.Reservation, ignoreCase: true, out var priority)
        )
            return null;

        return priority switch
        {
            ReservationPriority.Hochzeit => BuildHochzeit(req, player),
            ReservationPriority.Armut => BuildArmut(req, player),
            ReservationPriority.Schmeissen => new SchmeissenReservation(),
            ReservationPriority.Damensolo => new DamensoloReservation(player),
            ReservationPriority.Bubensolo => new BubensoloReservation(player),
            ReservationPriority.Fleischloses => new FleischlosesReservation(player),
            ReservationPriority.Knochenloses => new KnochenlosesReservation(player),
            ReservationPriority.SchlankerMartin => new SchlankerMartinReservation(player),
            ReservationPriority.KaroSolo => new FarbsoloReservation(Suit.Karo, player),
            ReservationPriority.KreuzSolo => new FarbsoloReservation(Suit.Kreuz, player),
            ReservationPriority.PikSolo => new FarbsoloReservation(Suit.Pik, player),
            ReservationPriority.HerzSolo => new FarbsoloReservation(Suit.Herz, player),
            _ => null,
        };
    }

    private static HochzeitReservation BuildHochzeit(MakeReservationRequest req, PlayerId player)
    {
        var condition = Enum.TryParse<HochzeitCondition>(
            req.HochzeitCondition,
            ignoreCase: true,
            out var c
        )
            ? c
            : HochzeitCondition.FirstTrick;
        return new HochzeitReservation(player, condition);
    }

    private static ArmutReservation BuildArmut(MakeReservationRequest req, PlayerId player)
    {
        // Rich player is not known at declaration time — it is determined during ArmutPartnerFinding.
        // Use the declaring player as a placeholder; the real reservation is constructed in AcceptArmutHandler.
        _ = req; // ArmutPartner field intentionally ignored
        return new ArmutReservation(player, player);
    }
}
