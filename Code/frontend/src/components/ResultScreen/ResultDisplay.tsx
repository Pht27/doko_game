import type { GameResultDto } from '../../types/api';
import { t } from '../../translations';

interface ResultDisplayProps {
  result: GameResultDto;
  mySeat?: number;
}

export function ResultDisplay({ result, mySeat }: ResultDisplayProps) {
  const hasNetPoints = result.netPointsPerSeat?.length > 0;
  const hasStandings = result.lobbyStandings?.length > 0;

  return (
    <div className="result-columns">
      {/* ── Left column: lobby standings + this-game delta ── */}
      <div className="result-col-standings">
        <div className="result-section-header">{t.gesamtstand}</div>
        {hasStandings && hasNetPoints && (
          <div className="result-breakdown">
            {result.lobbyStandings.map((pts, seat) => {
              const delta = result.netPointsPerSeat[seat] ?? 0;
              const isMe = seat === mySeat;
              const deltaCls =
                delta > 0
                  ? 'result-points-positive'
                  : delta < 0
                    ? 'result-points-negative'
                    : 'result-points-neutral';
              return (
                <div
                  key={seat}
                  className={isMe ? 'result-standings-row-me' : 'result-standings-row'}
                >
                  <span>{t.playerLabel(seat)}</span>
                  <span className="result-col-standings-values">
                    <span className={deltaCls}>
                      {delta > 0 ? '+' : ''}
                      {delta}
                    </span>
                    <span className="result-breakdown-value">{pts}</span>
                  </span>
                </div>
              );
            })}
          </div>
        )}
      </div>

      {/* ── Right column: game details ── */}
      <div className="result-col-details">
        <div className="result-section-header">{t.winnerLabel(result.winner)}</div>

        {/* Augen */}
        <div className="result-grid result-grid-compact">
          <div className="result-label">{t.reAugen}</div>
          <div className="result-value">{result.reAugen}</div>
          <div className="result-label">{t.kontraAugen}</div>
          <div className="result-value">{result.kontraAugen}</div>
        </div>

        {/* Spielwert breakdown */}
        <div>
          <div className="result-section-header result-section-header-sm">{t.spielwertBerechnung}</div>
          <div className="result-breakdown">
            {result.valueComponents.map((c, i) => (
              <div key={i} className="result-breakdown-row">
                <span className="result-label">{c.label}</span>
                <span className="result-breakdown-value">
                  {c.value > 0 ? `+${c.value}` : c.value}
                </span>
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

        {/* Gesamtergebnis — only when it differs from GameValue */}
        {result.totalScore !== result.gameValue && (
          <div>
            <div className="result-section-header result-section-header-sm">{t.gesamtergebnis}</div>
            <div className="result-breakdown">
              <div className="result-breakdown-row">
                <span className="result-label">{t.spielwert}</span>
                <span className="result-breakdown-value">{result.gameValue}</span>
              </div>
              {result.soloFactor > 1 && (
                <div className="result-breakdown-row">
                  <span className="result-label">{t.soloFaktor(result.soloFactor)}</span>
                  <span className="result-breakdown-value">
                    {result.gameValue * result.soloFactor}
                  </span>
                </div>
              )}
              {result.totalScore !== result.gameValue * result.soloFactor && (
                <div className="result-breakdown-row">
                  <span className="result-label">{t.extrapunkteNetto}</span>
                  <span className="result-breakdown-value">
                    {result.totalScore - result.gameValue * result.soloFactor > 0 ? '+' : ''}
                    {result.totalScore - result.gameValue * result.soloFactor}
                  </span>
                </div>
              )}
              <div className="result-breakdown-total">
                <span>{t.gesamtergebnis}</span>
                <span>{result.totalScore}</span>
              </div>
            </div>
          </div>
        )}

        {/* Zusatzpunkte */}
        {result.allAwards.length > 0 && (
          <div>
            <div className="result-section-header result-section-header-sm">{t.zusatzpunkte}</div>
            <ul className="result-awards-list">
              {result.allAwards.map((award, i) => (
                <li key={i} className="result-award-item">
                  <span>{t.awardLabel(award.type, award.benefittingPlayer)}</span>
                  <span className="result-award-delta">
                    {award.delta > 0 ? '+' : ''}
                    {award.delta}
                  </span>
                </li>
              ))}
            </ul>
          </div>
        )}
      </div>
    </div>
  );
}
