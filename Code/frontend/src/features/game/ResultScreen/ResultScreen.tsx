import { useState } from 'react';
import { createPortal } from 'react-dom';
import type { GameResultDto } from '@/types/api';
import { t } from '@/utils/translations';
import { GeschmissenDisplay } from './GeschmissenDisplay';
import { ResultDisplay } from './ResultDisplay';
import { LobbyHistory } from './LobbyHistory';
import { ReadyVoteButton } from '../shared/ReadyVoteButton';
import './ResultScreen.css';

export interface MultiplayerNewGameProps {
  voteCount: number;
  mySeatIndex: number;
  onVote: () => Promise<unknown>;
  onWithdraw: () => Promise<unknown>;
}

interface ResultScreenProps {
  result?: GameResultDto;
  onNewGame: () => void;
  multiplayerNewGame?: MultiplayerNewGameProps;
  viewOnly?: boolean;
  onLeaveLobby?: () => void;
}

export function ResultScreen({ result, onNewGame, multiplayerNewGame, viewOnly, onLeaveLobby }: ResultScreenProps) {
  const [hasVoted, setHasVoted] = useState(false);
  const [voting, setVoting] = useState(false);

  const history = result?.matchHistory ?? [];
  const currentGameIndex = history.length;
  const [selectedGame, setSelectedGame] = useState(currentGameIndex);

  const mySeat = multiplayerNewGame?.mySeatIndex;
  const hasHistory = result != null && (history.length > 0 || !result.isGeschmissen);

  function getDisplayResult(): GameResultDto | null {
    if (!result) return null;
    if (selectedGame < history.length) return history[selectedGame];
    return result;
  }

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

  const voteCount = multiplayerNewGame?.voteCount ?? 0;
  const displayResult = getDisplayResult();

  return createPortal(
    <div className="result-overlay">
      <div className="result-screen-wide">
        <div className="result-main-columns">
          {/* ── Left column: match history table ── */}
          {hasHistory && result && (
            <div className="result-left-col">
              <LobbyHistory
                result={result}
                mySeat={mySeat}
                selectedGame={selectedGame}
                onSelectGame={setSelectedGame}
              />
            </div>
          )}

          {/* Vertical divider */}
          {hasHistory && <div className="result-divider" />}

          {/* ── Right column: detail + action ── */}
          <div className="result-right-col">
            <div className="result-detail-area">
              {displayResult == null ? (
                <div className="result-no-games">
                  <span>{t.nochKeineErgebnisse}</span>
                </div>
              ) : displayResult.isGeschmissen ? (
                <GeschmissenDisplay />
              ) : (
                <ResultDisplay result={displayResult} mySeat={mySeat} />
              )}
            </div>

            {/* Action button(s) */}
            <div className="result-action-area">
              {onLeaveLobby && (
                <button onClick={onLeaveLobby} className="result-leave-btn">
                  Lobby verlassen
                </button>
              )}
              {multiplayerNewGame ? (
                <ReadyVoteButton
                  hasVoted={hasVoted}
                  voteCount={voteCount}
                  disabled={voting}
                  onClick={hasVoted ? handleWithdraw : handleVote}
                />
              ) : viewOnly ? (
                <button onClick={onNewGame} className="result-new-game-btn">
                  {t.schliessen}
                </button>
              ) : (
                <button onClick={onNewGame} className="result-new-game-btn">
                  {t.neuesSpiel}
                </button>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>,
    document.body,
  );
}
