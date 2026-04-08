import type { PlayerPublicStateDto } from '../types/api';
import { t } from '../translations';
import '../styles/PlayerLabel.css';

interface PlayerLabelProps {
  player: PlayerPublicStateDto;
  isCurrentTurn: boolean;
  orientation: 'top' | 'left' | 'right';
}

export function PlayerLabel({ player, isCurrentTurn, orientation }: PlayerLabelProps) {
  const active = isCurrentTurn ? 'player-label-active' : 'player-label-inactive';

  const partyColor =
    player.knownParty === 'Re'
      ? 'player-party-re'
      : player.knownParty === 'Kontra'
        ? 'player-party-kontra'
        : 'player-party-unknown';

  const layout =
    orientation === 'top'
      ? 'player-label-top'
      : orientation === 'left'
        ? 'player-label-left'
        : 'player-label-right';

  return (
    <div className={`player-label ${active} ${layout}`}>
      <span className="player-label-name">{t.playerName(player.id)}</span>
      <span className={`player-party-dot ${partyColor}`} title={player.knownParty ?? t.unbekanntePartei} />
      <span>{t.kartenAnzahl(player.handCardCount)}</span>
    </div>
  );
}
