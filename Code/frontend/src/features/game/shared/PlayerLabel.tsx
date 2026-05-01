import type { PlayerPublicStateDto } from '@/types/api';
import { t } from '@/utils/translations';
import './PlayerLabel.css';

interface PlayerLabelProps {
  player: PlayerPublicStateDto;
  isCurrentTurn: boolean;
  orientation: 'top' | 'left' | 'right';
  sonderkarteNotif?: string | null;
  trickCount?: number;
  showHealthStatus?: boolean;
  onClick?: () => void;
}

export function PlayerLabel({ player, isCurrentTurn, orientation, sonderkarteNotif, trickCount, showHealthStatus, onClick }: PlayerLabelProps) {
  const active = isCurrentTurn ? 'player-label-active' : 'player-label-inactive';

  const nameColor =
    player.knownParty === 'Re'
      ? 'player-name-re'
      : player.knownParty === 'Kontra'
        ? 'player-name-kontra'
        : '';

  const layout =
    orientation === 'top'
      ? 'player-label-top'
      : orientation === 'left'
        ? 'player-label-left'
        : 'player-label-right';

  const clickable = onClick ? 'player-label-clickable' : '';

  const annColor =
    player.knownParty === 'Re'
      ? 'player-ann-re'
      : player.knownParty === 'Kontra'
        ? 'player-ann-kontra'
        : 'player-ann-other';

  const healthChipClass =
    player.healthStatus === 'Gesund'    ? 'player-health-gesund'
    : player.healthStatus === 'Vorbehalt' ? 'player-health-vorbehalt'
    : 'player-health-pending';

  const inner = (
    <>
      <div className="player-label-row">
        <span className={`player-label-name ${nameColor}`}>{t.playerName(player.id)}</span>
        {trickCount !== undefined && trickCount > 0 && (
          <span className="player-trick-count">{trickCount}</span>
        )}
      </div>
      {showHealthStatus && (
        <span className={`player-health-chip ${healthChipClass}`}>
          {player.healthStatus ?? '···'}
        </span>
      )}
      {!showHealthStatus && player.highestAnnouncement && (
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
