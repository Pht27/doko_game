namespace Doko.Domain.Scoring;

public interface IGameScorer
{
    GameResult Score(CompletedGame game);
}
