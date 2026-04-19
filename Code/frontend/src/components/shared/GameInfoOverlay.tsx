import { useEffect, useState } from 'react';
import { getLobby } from '../../api/lobby';
import { t } from '../../translations';
import '../../styles/GameInfo.css';

interface GameInfoOverlayProps {
  phase: string;
  gameMode: string | null;
  trickNumber: number;
  completedTricks: number;
  lobbyId?: string;
  activePlayer: number;
  onClose: () => void;
  onLeaveLobby?: () => void;
}

export function GameInfoOverlay({
  phase,
  gameMode,
  trickNumber,
  completedTricks,
  lobbyId,
  activePlayer,
  onClose,
  onLeaveLobby,
}: GameInfoOverlayProps) {
  const [standings, setStandings] = useState<number[] | null>(null);

  useEffect(() => {
    if (!lobbyId) return;
    getLobby(lobbyId)
      .then((lobby) => setStandings(lobby.standings))
      .catch(() => {});
  }, [lobbyId]);

  const modeLabel = phase === 'Playing'
    ? t.gameModeLabel(gameMode)
    : t.phaseLabel(phase);

  return (
    <div className="game-info-overlay" onClick={onClose}>
      <div className="game-info-panel" onClick={(e) => e.stopPropagation()}>
        <div className="game-info-panel-title">{t.spielInfo}</div>

        <div>
          <div className="game-info-section-header">{t.spielInfo}</div>
          <div className="game-info-detail-row">
            <span className="game-info-detail-label">Spielmodus</span>
            <span className="game-info-detail-value">{modeLabel}</span>
          </div>
          <div className="game-info-detail-row">
            <span className="game-info-detail-label">{t.stichInfo(trickNumber, completedTricks)}</span>
          </div>
        </div>

        {standings && standings.length === 4 && (
          <div>
            <div className="game-info-section-header">{t.gesamtstand}</div>
            <div className="flex flex-col gap-1">
              {standings.map((pts, seat) => {
                const isMe = seat === activePlayer;
                return (
                  <div key={seat} className={isMe ? 'game-info-standings-row-me' : 'game-info-standings-row'}>
                    <span>{t.playerLabel(seat)}{isMe ? ` ${t.youSuffix.trim()}` : ''}</span>
                    <span className={pts > 0 ? 'result-points-positive' : pts < 0 ? 'result-points-negative' : 'result-points-neutral'}>
                      {pts > 0 ? `+${pts}` : pts}
                    </span>
                  </div>
                );
              })}
            </div>
          </div>
        )}
        {onLeaveLobby && (
          <button className="game-info-leave-btn" onClick={onLeaveLobby}>
            Lobby verlassen123
          </button>
        )}
        <button className="game-info-close-btn" onClick={onClose}>
          {12312312}
        </button>
      </div>
    </div>
  );
}
