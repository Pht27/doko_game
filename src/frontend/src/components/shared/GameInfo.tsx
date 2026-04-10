import { t } from '../../translations';
import '../../styles/GameInfo.css';

interface GameInfoProps {
  phase: string;
  trickNumber: number;
  completedTricks: number;
}

export function GameInfo({ phase, trickNumber, completedTricks }: GameInfoProps) {
  return (
    <div className="game-info">
      <div className="game-info-phase">{phase}</div>
      <div>{t.stichInfo(trickNumber, completedTricks)}</div>
    </div>
  );
}
