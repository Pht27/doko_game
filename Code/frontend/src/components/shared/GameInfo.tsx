import { t } from '../../translations';
import '../../styles/GameInfo.css';

interface GameInfoProps {
  phase: string;
  gameMode: string | null;
  trickNumber: number;
  completedTricks: number;
  onClick?: () => void;
}

export function GameInfo({ phase, gameMode, trickNumber, completedTricks, onClick }: GameInfoProps) {
  const modeLabel = phase === 'Playing'
    ? t.gameModeLabel(gameMode)
    : t.phaseLabel(phase);

  return (
    <div
      className={`game-info${onClick ? ' game-info-clickable' : ''}`}
      onClick={onClick}
      role={onClick ? 'button' : undefined}
    >
      <div className="game-info-phase">{modeLabel}</div>
      <div>{t.stichInfo(trickNumber, completedTricks)}</div>
    </div>
  );
}
