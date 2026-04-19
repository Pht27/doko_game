using Doko.Api.DTOs.Requests;
using Doko.Api.Mapping;
using Doko.Domain.Cards;
using Doko.Domain.Parties;
using Doko.Domain.Players;
using Doko.Domain.Reservations;

namespace Doko.Api.Tests;

public class DtoMapperTests
{
    private static readonly PlayerSeat Player0 = PlayerSeat.First;

    [Fact]
    public void BuildReservation_NullInput_ReturnsNull()
    {
        var req = new MakeReservationRequest(
            Reservation: null,
            HochzeitCondition: null,
            ArmutPartner: null
        );
        DtoMapper.BuildReservation(req, Player0).Should().BeNull();
    }

    [Theory]
    [InlineData("Schmeissen", typeof(SchmeissenReservation))]
    [InlineData("Damensolo", typeof(DamensoloReservation))]
    [InlineData("Bubensolo", typeof(BubensoloReservation))]
    [InlineData("Fleischloses", typeof(FleischlosesReservation))]
    [InlineData("Knochenloses", typeof(KnochenlosesReservation))]
    [InlineData("SchlankerMartin", typeof(SchlankerMartinReservation))]
    [InlineData("KaroSolo", typeof(FarbsoloReservation))]
    [InlineData("KreuzSolo", typeof(FarbsoloReservation))]
    [InlineData("PikSolo", typeof(FarbsoloReservation))]
    [InlineData("HerzSolo", typeof(FarbsoloReservation))]
    public void BuildReservation_KnownPriority_ReturnsCorrectType(
        string priority,
        Type expectedType
    )
    {
        var req = new MakeReservationRequest(priority, null, null);
        var reservation = DtoMapper.BuildReservation(req, Player0);
        reservation.Should().BeOfType(expectedType);
    }

    [Theory]
    [InlineData("karosolo", typeof(FarbsoloReservation))]
    [InlineData("DAMENSOLO", typeof(DamensoloReservation))]
    public void BuildReservation_CaseInsensitive(string priority, Type expectedType)
    {
        var req = new MakeReservationRequest(priority, null, null);
        DtoMapper.BuildReservation(req, Player0).Should().BeOfType(expectedType);
    }

    [Fact]
    public void BuildReservation_UnrecognizedPriority_ReturnsNull()
    {
        var req = new MakeReservationRequest("NotARealReservation", null, null);
        DtoMapper.BuildReservation(req, Player0).Should().BeNull();
    }

    [Theory]
    [InlineData("FirstTrick")]
    [InlineData("FirstFehlTrick")]
    [InlineData("FirstTrumpTrick")]
    public void BuildReservation_Hochzeit_MapsCondition(string conditionStr)
    {
        var req = new MakeReservationRequest("Hochzeit", conditionStr, null);
        var reservation = DtoMapper.BuildReservation(req, Player0);
        reservation
            .Should()
            .BeOfType<HochzeitReservation>()
            .Which.Priority.Should()
            .Be(ReservationPriority.Hochzeit);
    }

    [Fact]
    public void BuildReservation_Hochzeit_DefaultsToFirstTrickWhenConditionMissing()
    {
        var req = new MakeReservationRequest(
            "Hochzeit",
            HochzeitCondition: null,
            ArmutPartner: null
        );
        var reservation = DtoMapper.BuildReservation(req, Player0);
        reservation.Should().BeOfType<HochzeitReservation>();
    }

    [Fact]
    public void BuildReservation_Armut_WiresRichPlayer()
    {
        var req = new MakeReservationRequest("Armut", null, ArmutPartner: 2);
        var reservation = DtoMapper.BuildReservation(req, Player0);
        reservation.Should().BeOfType<ArmutReservation>();
        reservation!.Priority.Should().Be(ReservationPriority.Armut);
    }

    [Fact]
    public void ToDto_Card_MapsFields()
    {
        var card = new Card(new CardId(5), new CardType(Suit.Herz, Rank.Ass));
        var dto = DtoMapper.ToDto(card);

        dto.Id.Should().Be(5);
        dto.Suit.Should().Be("Herz");
        dto.Rank.Should().Be("Ass");
    }
}
