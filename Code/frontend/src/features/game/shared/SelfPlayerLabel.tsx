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
  const partyClass =
    ownParty === 'Re'
      ? 'self-label-re'
      : ownParty === 'Kontra'
        ? 'self-label-kontra'
        : '';

  const annPartyClass =
    ownParty === 'Re'
      ? 'self-ann-re'
      : ownParty === 'Kontra'
        ? 'self-ann-kontra'
        : 'self-ann-other';

  return (
    <div className={`self-player-label ${isCurrentTurn ? 'self-label-active' : ''}`}>
      <span className={`self-label-name ${partyClass}`}>{t.playerName(playerId)}</span>
      {trickCount > 0 && (
        <span className="self-label-tricks">{trickCount}</span>
      )}
      {highestAnnouncement && (
        <span className={`self-label-ann ${annPartyClass}`}>
          {t.announcementLabel(highestAnnouncement)}
        </span>
      )}
    </div>
  );
}
