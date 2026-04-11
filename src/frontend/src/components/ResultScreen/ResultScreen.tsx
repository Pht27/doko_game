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

        {/* Augen */}
        <div className="result-grid">
          <div className="result-label">{t.reAugen}</div>
          <div className="result-value">{result.rePoints}</div>
          <div className="result-label">{t.kontraAugen}</div>
          <div className="result-value">{result.kontraPoints}</div>
        </div>

        {/* Spielwert breakdown */}
        <div>
          <div className="result-section-header">{t.spielwertBerechnung}</div>
          <div className="result-breakdown">
            {result.valueComponents.map((c, i) => (
              <div key={i} className="result-breakdown-row">
                <span className="result-label">{c.label}</span>
                <span className="result-breakdown-value">{c.value > 0 ? `+${c.value}` : c.value}</span>
              </div>
            ))}
            <div className="result-breakdown-total">
              <span>{t.spielwert}</span>
              <span>{result.gameValue}</span>
            </div>
          </div>
        </div>

        {result.feigheit && (
          <div className="result-feigheit-banner">{t.feigheit}</div>
        )}

        {/* Zusatzpunkte */}
        {result.allAwards.length > 0 && (
          <div>
            <div className="result-section-header">{t.zusatzpunkte}</div>
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
