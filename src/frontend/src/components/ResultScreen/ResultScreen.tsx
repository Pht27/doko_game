import type { GameResultDto } from '../../types/api';
import { t } from '../../translations';
import '../../styles/ResultScreen.css';

interface ResultScreenProps {
  result: GameResultDto;
  onNewGame: () => void;
}

export function ResultScreen({ result, onNewGame }: ResultScreenProps) {
  return (
    <div className="result-overlay">
      <div className="result-screen">
        <h2 className="result-title">
          {t.winnerLabel(result.winner)}
        </h2>

        <div className="result-grid">
          <div className="result-label">{t.rePunkte}</div>
          <div className="result-value">{result.rePoints}</div>
          <div className="result-label">{t.kontraPunkte}</div>
          <div className="result-value">{result.kontraPoints}</div>
          <div className="result-label">{t.spielwert}</div>
          <div className="result-value">{result.gameValue}</div>
          {result.feigheit && (
            <>
              <div className="result-label">{t.hinweis}</div>
              <div className="result-feigheit">{t.feigheit}</div>
            </>
          )}
        </div>

        {result.allAwards.length > 0 && (
          <div>
            <div className="result-awards-header">{t.zusatzpunkte}</div>
            <ul className="result-awards-list">
              {result.allAwards.map((award, i) => (
                <li key={i} className="result-award-item">
                  <span>{t.awardLabel(award.type, award.benefittingPlayer)}</span>
                  <span className="result-award-delta">{award.delta > 0 ? '+' : ''}{award.delta}</span>
                </li>
              ))}
            </ul>
          </div>
        )}

        <button
          onClick={onNewGame}
          className="result-new-game-btn"
        >
          {t.neuesSpiel}
        </button>
      </div>
    </div>
  );
}
