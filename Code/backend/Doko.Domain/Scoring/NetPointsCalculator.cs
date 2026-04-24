using Doko.Domain.GameFlow;
using Doko.Domain.Parties;

namespace Doko.Domain.Scoring;

public static class NetPointsCalculator
{
    /// <summary>
    /// Returns net points per seat (index 0–3) and party per seat (null when unresolvable).
    /// Normal: winners +TotalScore, losers -TotalScore.
    /// Solo (soloFactor > 1): solo player ±TotalScore; each opponent ∓TotalScore/soloFactor.
    /// Kontrasolo: solo player is Kontra; all other solos: solo player is Re.
    /// </summary>
    public static (int[] NetPoints, Party?[] PartyPerSeat) Calculate(
        GameResult result,
        GameState state
    )
    {
        var soloParty =
            state.SilentMode?.Type == SilentGameModeType.KontraSolo ? Party.Kontra : Party.Re;

        var netPoints = new int[4];
        var partyPerSeat = new Party?[4];
        foreach (var player in state.Players)
        {
            int seat = (int)player.Seat;
            var party = state.PartyResolver.ResolveParty(player.Seat, state);
            partyPerSeat[seat] = party;
            if (party is null)
                continue; // Party unresolvable (e.g. undecided Hochzeit)
            bool isWinner = party == result.Winner;
            bool isSoloPlayer = result.SoloFactor > 1 && party == soloParty;

            int score = isSoloPlayer ? result.TotalScore : result.TotalScore / result.SoloFactor;

            netPoints[seat] = isWinner ? score : -score;
        }
        return (netPoints, partyPerSeat);
    }
}
