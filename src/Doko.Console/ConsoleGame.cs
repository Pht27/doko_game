using Doko.Application.Abstractions;
using Doko.Application.Common;
using Doko.Application.Games.Commands;
using Doko.Application.Games.Results;
using Doko.Application.Games.UseCases;
using Doko.Console.Input;
using Doko.Console.Rendering;
using Doko.Domain.GameFlow;
using Doko.Domain.Players;
using Doko.Domain.Reservations;

namespace Doko.Console;

public sealed class ConsoleGame(
    IStartGameUseCase startGame,
    IDealCardsUseCase dealCards,
    IMakeReservationUseCase makeReservation,
    IPlayCardUseCase playCard,
    IMakeAnnouncementUseCase makeAnnouncement,
    IGameQueryService queryService,
    GameRenderer renderer,
    ConsoleInputReader inputReader)
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        // ── 1. Start ─────────────────��─────────────────────────��──────────────
        var players = Enumerable.Range(0, 4).Select(i => new PlayerId((byte)i)).ToList();
        var startResult = await startGame.ExecuteAsync(new StartGameCommand(players), ct);
        if (startResult is not GameActionResult<StartGameResult>.Ok { Value: var started })
            throw new InvalidOperationException("Failed to start game.");

        var gameId = started.GameId;
        System.Console.WriteLine($"Game started. ID: {gameId}\n");

        // ── 2. Deal ────────────────────────────────���──────────────────────────
        var dealResult = await dealCards.ExecuteAsync(new DealCardsCommand(gameId), ct);
        if (dealResult is GameActionResult<Unit>.Failure dealFail)
            throw new InvalidOperationException($"Failed to deal: {dealFail.Error}");

        System.Console.WriteLine("Cards dealt.\n");

        // ── 3. Reservations ───────────────────────────────���───────────────────
        System.Console.WriteLine("=== RESERVATION PHASE ===\n");
        for (byte i = 0; i < 4; i++)
        {
            var playerId = new PlayerId(i);
            var view = await queryService.GetPlayerViewAsync(gameId, playerId, ct)
                       ?? throw new InvalidOperationException($"No view for player {i}");

            System.Console.Clear();
            System.Console.WriteLine($"=== Player {i}'s Reservation ===\n");
            renderer.RenderHand(view.Hand);

            IReservation? reservation;
            while (true)
            {
                reservation = inputReader.PromptReservation(playerId, view);
                var result = await makeReservation.ExecuteAsync(
                    new MakeReservationCommand(gameId, playerId, reservation), ct);

                if (result is GameActionResult<MakeReservationResult>.Ok ok)
                {
                    if (ok.Value.Geschmissen)
                    {
                        System.Console.Clear();
                        renderer.RenderGeschmissen(playerId.Value);
                        Pause();
                        return;
                    }
                    if (ok.Value.AllDeclared)
                    {
                        var mode = ok.Value.WinningReservation is null
                            ? "Normal game"
                            : ok.Value.WinningReservation.GetType().Name.Replace("Reservation", "");
                        System.Console.WriteLine($"\nAll declared. Game mode: {mode}");
                        Pause();
                    }
                    break;
                }

                if (result is GameActionResult<MakeReservationResult>.Failure fail)
                    System.Console.WriteLine($"Not eligible: {fail.Error}. Try again.\n");
            }
        }

        // ── 4. Playing ────────────────────────────────────────────────────────
        System.Console.WriteLine("\n=== PLAYING PHASE ===");

        while (true)
        {
            // Determine current player by peeking at any view
            var peek = await queryService.GetPlayerViewAsync(gameId, new PlayerId(0), ct)
                       ?? throw new InvalidOperationException("Game disappeared.");

            var currentPlayer = peek.CurrentTurn;
            var view = await queryService.GetPlayerViewAsync(gameId, currentPlayer, ct)
                       ?? throw new InvalidOperationException($"No view for player {currentPlayer}.");

            System.Console.Clear();
            renderer.Render(view);

            // Optional announcement before playing
            if (view.LegalAnnouncements.Count > 0)
            {
                var annType = inputReader.PromptAnnouncement(view.LegalAnnouncements);
                if (annType.HasValue)
                {
                    var annResult = await makeAnnouncement.ExecuteAsync(
                        new MakeAnnouncementCommand(gameId, currentPlayer, annType.Value), ct);
                    if (annResult is GameActionResult<Unit>.Failure annFail)
                        System.Console.WriteLine($"  Announcement rejected: {annFail.Error}\n");
                }
            }

            // Play card — retry on failure
            while (true)
            {
                var (cardId, sonderkarten) = inputReader.PromptCard(view);
                var playResult = await playCard.ExecuteAsync(
                    new PlayCardCommand(gameId, currentPlayer, cardId, sonderkarten), ct);

                if (playResult is GameActionResult<PlayCardResult>.Failure playFail)
                {
                    System.Console.WriteLine($"  Illegal move: {playFail.Error}. Try again.\n");
                    continue;
                }

                if (playResult is GameActionResult<PlayCardResult>.Ok { Value: var pr })
                {
                    if (pr.TrickCompleted)
                    {
                        System.Console.WriteLine($"\n  Trick won by Player {pr.TrickWinner}!");
                        Pause();
                    }

                    if (pr.GameFinished)
                    {
                        System.Console.Clear();
                        renderer.RenderResult(pr.FinishedResult!.Result);
                        Pause();
                        return;
                    }
                }
                break;
            }
        }
    }

    private static void Pause()
    {
        System.Console.Write("\nPress Enter to continue...");
        System.Console.ReadLine();
    }
}
