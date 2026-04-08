import { t } from '../translations';
import '../styles/GameInfo.css';

interface GameInfoProps {
  phase: string;
  trickNumber: number;
  completedTricks: number;
}

export function GameInfo({ phase, trickNumber, completedTricks }: GameInfoProps) {
  return (
    <div className="text-xs text-white/60 leading-tight">
      <div className="font-semibold text-white/80">{phase}</div>
      <div>{t.stichInfo(trickNumber, completedTricks)}</div>
    </div>
  );
}
