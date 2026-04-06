using Doko.Api.DTOs.Responses;
using Doko.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Doko.Api.Extensions;

public static class GameActionResultExtensions
{
    public static IActionResult ToActionResult<T>(
        this GameActionResult<T> result,
        Func<T, IActionResult> onOk)
        => result switch
        {
            GameActionResult<T>.Ok ok => onOk(ok.Value),
            GameActionResult<T>.Failure f => MapError(f.Error),
            _ => new StatusCodeResult(500),
        };

    public static Task<IActionResult> ToActionResult<T>(
        this GameActionResult<T> result,
        Func<T, Task<IActionResult>> onOk)
        => result switch
        {
            GameActionResult<T>.Ok ok => onOk(ok.Value),
            GameActionResult<T>.Failure f => Task.FromResult<IActionResult>(MapError(f.Error)),
            _ => Task.FromResult<IActionResult>(new StatusCodeResult(500)),
        };

    private static IActionResult MapError(GameError error) => error switch
    {
        GameError.GameNotFound            => new NotFoundObjectResult(new ErrorResponse("game_not_found")),
        GameError.NotYourTurn             => new BadRequestObjectResult(new ErrorResponse("not_your_turn")),
        GameError.InvalidPhase            => new BadRequestObjectResult(new ErrorResponse("invalid_phase")),
        GameError.IllegalCard             => new BadRequestObjectResult(new ErrorResponse("illegal_card")),
        GameError.AnnouncementNotAllowed  => new BadRequestObjectResult(new ErrorResponse("announcement_not_allowed")),
        GameError.ReservationNotEligible  => new BadRequestObjectResult(new ErrorResponse("reservation_not_eligible")),
        GameError.SonderkarteNotEligible  => new BadRequestObjectResult(new ErrorResponse("sonderkarte_not_eligible")),
        GameError.AlreadyDeclared         => new BadRequestObjectResult(new ErrorResponse("already_declared")),
        GameError.GenscherPartnerRequired => new BadRequestObjectResult(new ErrorResponse("genscher_partner_required")),
        GameError.GenscherPartnerInvalid  => new BadRequestObjectResult(new ErrorResponse("genscher_partner_invalid")),
        _                                 => new BadRequestObjectResult(new ErrorResponse("unknown_error")),
    };
}
