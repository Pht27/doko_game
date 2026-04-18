using Doko.Domain.GameFlow;
using Doko.Domain.Parties;

namespace Doko.Domain.Scoring;

public static class NetPointsCalculator
{
    /// <summary>
    /// Returns net points per seat (index 0–3).
    /// Normal: winners +TotalScore, losers -TotalScore.
    /// Solo (soloFactor > 1): solo player (Re) ±TotalScore; each opponent ∓TotalScore/soloFactor.
    /// </summary>
    public static int[] Calculate(GameResult result, GameState state)
    {
        var netPoints = new int[4];
        foreach (var player in state.Players)
        {
            int seat = (int)player.Seat;
            var party = state.PartyResolver.ResolveParty(player.Id, state);
            if (party is null) continue; // Party unresolvable (e.g. undecided Hochzeit)
            bool isWinner = party == result.Winner;
            bool isSoloPlayer = result.SoloFactor > 1 && party == Party.Re;

            int score = isSoloPlayer
                ? result.TotalScore
                : result.TotalScore / result.SoloFactor;

            netPoints[seat] = isWinner ? score : -score;
        }
        return netPoints;
    }
}
