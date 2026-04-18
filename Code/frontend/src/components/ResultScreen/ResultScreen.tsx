import { useState } from 'react';
import { createPortal } from 'react-dom';
import type { GameResultDto } from '../../types/api';
import { t } from '../../translations';
import '../../styles/ResultScreen.css';

export interface MultiplayerNewGameProps {
  voteCount: number;
  mySeatIndex: number;
  onVote: () => Promise<void>;
  onWithdraw: () => Promise<void>;
}

interface ResultScreenProps {
  result: GameResultDto;
  onNewGame: () => void;
  multiplayerNewGame?: MultiplayerNewGameProps;
}

export function ResultScreen({ result, onNewGame, multiplayerNewGame }: ResultScreenProps) {
  const [hasVoted, setHasVoted] = useState(false);
  const [voting, setVoting] = useState(false);

  async function handleVote() {
    if (!multiplayerNewGame || voting) return;
    setVoting(true);
    try {
      await multiplayerNewGame.onVote();
      setHasVoted(true);
    } finally {
      setVoting(false);
    }
  }

  async function handleWithdraw() {
    if (!multiplayerNewGame || voting) return;
    setVoting(true);
    try {
      await multiplayerNewGame.onWithdraw();
      setHasVoted(false);
    } finally {
      setVoting(false);
    }
  }

  const mySeat = multiplayerNewGame?.mySeatIndex;
  const hasNetPoints = result.netPointsPerSeat?.length > 0;
  const hasStandings = result.lobbyStandings?.length > 0;

  return createPortal(
    <div className="result-overlay">
      <div className="result-screen">
        <h2 className="result-title">
          {t.winnerLabel(result.winner)}
        </h2>

        {/* Augen */}
        <div className="result-grid">
          <div className="result-label">{t.reAugen}</div>
          <div className="result-value">{result.reAugen}</div>
          <div className="result-label">{t.kontraAugen}</div>
          <div className="result-value">{result.kontraAugen}</div>
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

        {/* Gesamtergebnis — only shown when it differs from GameValue */}
        {result.totalScore !== result.gameValue && (
          <div>
            <div className="result-section-header">{t.gesamtergebnis}</div>
            <div className="result-breakdown">
              <div className="result-breakdown-row">
                <span className="result-label">{t.spielwert}</span>
                <span className="result-breakdown-value">{result.gameValue}</span>
              </div>
              {result.soloFactor > 1 && (
                <div className="result-breakdown-row">
                  <span className="result-label">{t.soloFaktor(result.soloFactor)}</span>
                  <span className="result-breakdown-value">{result.gameValue * result.soloFactor}</span>
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

        {/* Punkteänderung (net points this game) */}
        {hasNetPoints && (
          <div>
            <div className="result-section-header">{t.punkteAenderung}</div>
            <div className="result-breakdown">
              {result.netPointsPerSeat.map((pts, seat) => {
                const isMe = seat === mySeat;
                const ptsCls = pts > 0
                  ? 'result-points-positive'
                  : pts < 0
                    ? 'result-points-negative'
                    : 'result-points-neutral';
                return (
                  <div key={seat} className={isMe ? 'result-standings-row-me' : 'result-standings-row'}>
                    <span>{t.playerLabel(seat)}</span>
                    <span className={ptsCls}>{pts > 0 ? '+' : ''}{pts}</span>
                  </div>
                );
              })}
            </div>
          </div>
        )}

        {/* Gesamtstand (lobby standings) */}
        {hasStandings && (
          <div>
            <div className="result-section-header">{t.gesamtstand}</div>
            <div className="result-breakdown">
              {result.lobbyStandings.map((pts, seat) => {
                const isMe = seat === mySeat;
                return (
                  <div key={seat} className={isMe ? 'result-standings-row-me' : 'result-standings-row'}>
                    <span>{t.playerLabel(seat)}</span>
                    <span className="result-breakdown-value">{pts}</span>
                  </div>
                );
              })}
            </div>
          </div>
        )}

        {/* New game action */}
        {multiplayerNewGame ? (
          <div className="result-vote-area">
            <button
              onClick={hasVoted ? handleWithdraw : handleVote}
              className={hasVoted ? 'result-bereit-active-btn' : 'result-bereit-btn'}
              disabled={voting}
            >
              {hasVoted ? t.zurueckziehen : t.bereit}
            </button>
            <span className="result-vote-count">
              {t.bereitCount(multiplayerNewGame.voteCount)}
            </span>
          </div>
        ) : (
          <button onClick={onNewGame} className="result-new-game-btn">
            {t.neuesSpiel}
          </button>
        )}
      </div>
    </div>,
    document.body,
  );
}
