import { t } from '@/utils/translations';
import './SelfPlayerLabel.css';

interface SelfPlayerLabelProps {
  playerId: number;
  trickCount: number;
  ownParty: string | null;
  highestAnnouncement: string | null;
  isCurrentTurn: boolean;
}

export function SelfPlayerLabel({
  playerId,
  trickCount,
  ownParty,
  highestAnnouncement,
  isCurrentTurn,
}: SelfPlayerLabelProps) {
  const bgClass =
    ownParty === 'Re'
      ? 'self-bg-re'
      : ownParty === 'Kontra'
        ? 'self-bg-kontra'
        : '';

  return (
    <div className={`self-player-label ${bgClass} ${isCurrentTurn ? 'self-label-active' : ''}`}>
      <span className="self-label-name">{t.playerName(playerId)}</span>
      {trickCount > 0 && (
        <>
          <span className="self-label-dot">·</span>
          <span className="self-label-tricks">{trickCount}</span>
        </>
      )}
      {highestAnnouncement && (
        <>
          <span className="self-label-dot">·</span>
          <span className="self-label-ann">{t.announcementLabel(highestAnnouncement)}</span>
        </>
      )}
    </div>
  );
}
