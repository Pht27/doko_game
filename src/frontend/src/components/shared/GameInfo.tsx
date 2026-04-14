import { t } from '../../translations';
import '../../styles/GameInfo.css';

interface GameInfoProps {
  phase: string;
  gameMode: string | null;
  trickNumber: number;
  completedTricks: number;
}

export function GameInfo({ phase, gameMode, trickNumber, completedTricks }: GameInfoProps) {
  const modeLabel = phase === 'Playing'
    ? t.gameModeLabel(gameMode)
    : t.phaseLabel(phase);

  return (
    <div className="game-info">
      <div className="game-info-phase">{modeLabel}</div>
      <div>{t.stichInfo(trickNumber, completedTricks)}</div>
    </div>
  );
}
