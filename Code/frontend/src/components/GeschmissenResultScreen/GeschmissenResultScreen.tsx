import { useState } from 'react';
import { createPortal } from 'react-dom';
import type { MultiplayerNewGameProps } from '../ResultScreen/ResultScreen';
import { t } from '../../translations';
import '../../styles/ResultScreen.css';

interface GeschmissenResultScreenProps {
  lobbyStandings: number[];
  activePlayer: number;
  onNewGame: () => void;
  multiplayerNewGame?: MultiplayerNewGameProps;
}

export function GeschmissenResultScreen({
  lobbyStandings,
  activePlayer,
  onNewGame,
  multiplayerNewGame,
}: GeschmissenResultScreenProps) {
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
  const hasStandings = lobbyStandings.length === 4;

  return createPortal(
    <div className="result-overlay">
      <div className="result-screen">
        <h2 className="result-title">{t.geschmissenTitle}</h2>
        <p className="text-white/60 text-sm text-center">{t.geschmissenSubtitle}</p>

        {hasStandings && (
          <div>
            <div className="result-section-header">{t.gesamtstand}</div>
            <div className="result-breakdown">
              {lobbyStandings.map((pts, seat) => {
                const isMe = seat === mySeat || (mySeat == null && seat === activePlayer);
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
