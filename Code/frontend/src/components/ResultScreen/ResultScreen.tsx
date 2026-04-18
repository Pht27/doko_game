import { useState } from 'react';
import { createPortal } from 'react-dom';
import type { GameResultDto } from '../../types/api';
import { t } from '../../translations';
import { GeschmissenDisplay } from './GeschmissenDisplay';
import { ResultDisplay } from './ResultDisplay';
import '../../styles/ResultScreen.css';

export interface MultiplayerNewGameProps {
  voteCount: number;
  mySeatIndex: number;
  onVote: () => Promise<unknown>;
  onWithdraw: () => Promise<unknown>;
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

  return createPortal(
    <div className="result-overlay">
      <div className="result-screen">
        {result.isGeschmissen ? (
          <GeschmissenDisplay result={result} mySeat={mySeat} />
        ) : (
          <ResultDisplay result={result} mySeat={mySeat} />
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
