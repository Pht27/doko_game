using Doko.Api.DTOs.Responses;
using Doko.Api.Extensions;
using Doko.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Doko.Api.Tests;

public class GameActionResultExtensionsTests
{
    [Fact]
    public void Ok_InvokesOnOkDelegate()
    {
        var result = new GameActionResult<int>.Ok(42);
        var action = result.ToActionResult(v => new OkObjectResult(v));

        action.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(42);
    }

    [Theory]
    [InlineData(GameError.GameNotFound, 404)]
    [InlineData(GameError.NotYourTurn, 400)]
    [InlineData(GameError.InvalidPhase, 400)]
    [InlineData(GameError.IllegalCard, 400)]
    [InlineData(GameError.AnnouncementNotAllowed, 400)]
    [InlineData(GameError.ReservationNotEligible, 400)]
    [InlineData(GameError.SonderkarteNotEligible, 400)]
    [InlineData(GameError.AlreadyDeclared, 400)]
    [InlineData(GameError.GenscherPartnerRequired, 400)]
    [InlineData(GameError.GenscherPartnerInvalid, 400)]
    public void Failure_MapsToCorrectHttpStatus(GameError error, int expectedStatus)
    {
        var result = new GameActionResult<int>.Failure(error);
        var action = result.ToActionResult(v => new OkObjectResult(v));

        action.Should().BeAssignableTo<ObjectResult>()
            .Which.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [InlineData(GameError.GameNotFound, "game_not_found")]
    [InlineData(GameError.NotYourTurn, "not_your_turn")]
    [InlineData(GameError.InvalidPhase, "invalid_phase")]
    [InlineData(GameError.IllegalCard, "illegal_card")]
    [InlineData(GameError.AnnouncementNotAllowed, "announcement_not_allowed")]
    [InlineData(GameError.ReservationNotEligible, "reservation_not_eligible")]
    [InlineData(GameError.SonderkarteNotEligible, "sonderkarte_not_eligible")]
    [InlineData(GameError.AlreadyDeclared, "already_declared")]
    [InlineData(GameError.GenscherPartnerRequired, "genscher_partner_required")]
    [InlineData(GameError.GenscherPartnerInvalid, "genscher_partner_invalid")]
    public void Failure_IncludesErrorStringInBody(GameError error, string expectedError)
    {
        var result = new GameActionResult<int>.Failure(error);
        var action = result.ToActionResult(v => new OkObjectResult(v));

        action.Should().BeAssignableTo<ObjectResult>()
            .Which.Value.Should().BeOfType<ErrorResponse>()
            .Which.Error.Should().Be(expectedError);
    }
}
