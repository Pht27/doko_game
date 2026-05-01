import { t } from '@/utils/translations';
import { usePlayerName } from '@/context/PlayerNamesContext';
import './SonderkarteFlash.css';

interface SonderkarteFlashProps {
  player: number;
  type: string;
}

export function SonderkarteFlash({ player, type }: SonderkarteFlashProps) {
  const playerName = usePlayerName(player);
  return (
    <div className="sk-flash" aria-hidden="true">
      <div className="sk-flash-badge">{t.sonderkarteBadge}</div>
      <div className="sk-flash-name">{t.sonderkarteName(type)}</div>
      <div className="sk-flash-player">{playerName}</div>
    </div>
  );
}
