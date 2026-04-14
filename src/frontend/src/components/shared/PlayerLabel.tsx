import type { PlayerPublicStateDto } from '../../types/api';
import { t } from '../../translations';
import '../../styles/PlayerLabel.css';

interface PlayerLabelProps {
  player: PlayerPublicStateDto;
  isCurrentTurn: boolean;
  orientation: 'top' | 'left' | 'right';
  sonderkarteNotif?: string | null;
  onClick?: () => void;
}

export function PlayerLabel({ player, isCurrentTurn, orientation, sonderkarteNotif, onClick }: PlayerLabelProps) {
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

  const clickable = onClick ? 'player-label-clickable' : '';

  const annColor =
    player.highestAnnouncement === 'Re'
      ? 'player-ann-re'
      : player.highestAnnouncement === 'Kontra'
        ? 'player-ann-kontra'
        : 'player-ann-other';

  const inner = (
    <>
      <span className="player-label-name">{t.playerName(player.id)}</span>
      <span className={`player-party-dot ${partyColor}`} title={player.knownParty ?? t.unbekanntePartei} />
      {player.highestAnnouncement && (
        <span className={`player-ann-badge ${annColor}`}>
          {t.announcementLabel(player.highestAnnouncement)}
        </span>
      )}
      {sonderkarteNotif && (
        <span className="player-sonderkarte-notif">{t.sonderkarteName(sonderkarteNotif)}</span>
      )}
    </>
  );

  if (onClick) {
    return (
      <button className={`player-label ${active} ${layout} ${clickable}`} onClick={onClick}>
        {inner}
      </button>
    );
  }

  return (
    <div className={`player-label ${active} ${layout}`}>
      {inner}
    </div>
  );
}
