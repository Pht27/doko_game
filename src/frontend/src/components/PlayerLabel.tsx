import type { PlayerPublicStateDto } from '../types/api';
import { t } from '../translations';
import './PlayerLabel.css';

interface PlayerLabelProps {
  player: PlayerPublicStateDto;
  isCurrentTurn: boolean;
  orientation: 'top' | 'left' | 'right';
}

export function PlayerLabel({ player, isCurrentTurn, orientation }: PlayerLabelProps) {
  const base = 'flex flex-col items-center gap-1 px-3 py-2 rounded-xl text-xs transition-all';
  const active = isCurrentTurn
    ? 'ring-2 ring-yellow-400 bg-yellow-400/10 text-yellow-300'
    : 'text-white/60';

  const partyColor =
    player.knownParty === 'Re'
      ? 'bg-blue-500'
      : player.knownParty === 'Kontra'
        ? 'bg-red-500'
        : 'bg-white/20';

  const layout =
    orientation === 'top'
      ? 'flex-col'
      : orientation === 'left'
        ? 'flex-row'
        : 'flex-row-reverse';

  return (
    <div className={`${base} ${active} ${layout}`}>
      <span className="font-semibold">{t.playerName(player.id)}</span>
      <span className={`w-2 h-2 rounded-full ${partyColor}`} title={player.knownParty ?? t.unbekanntePartei} />
      <span>{t.kartenAnzahl(player.handCardCount)}</span>
    </div>
  );
}
