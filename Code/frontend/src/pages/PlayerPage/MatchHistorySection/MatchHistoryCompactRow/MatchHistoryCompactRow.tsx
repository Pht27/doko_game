import type { PlayerRoundListItem } from '@/types/analog';
import { PlayerLink } from '@/components/PlayerLink/PlayerLink';
import { RoundCard } from '@/pages/HistoryPage/RoundCard/RoundCard';
import { t } from '@/utils/translations';
import './MatchHistoryCompactRow.css';

interface MatchHistoryCompactRowProps {
  round: PlayerRoundListItem;
  playerId: number;
  expanded: boolean;
  onToggle: () => void;
}

export function MatchHistoryCompactRow({ round, playerId, expanded, onToggle }: MatchHistoryCompactRowProps) {
  const date = new Date(round.playedAt);
  const isRe = round.rePlayers.some((p) => p.id === playerId);
  const won = round.pointDelta >= 0;
  const partners = round.teamPartners;

  const dateStr = date.toLocaleDateString('de-DE', { day: '2-digit', month: 'short' });
  const delta = round.pointDelta;
  const pts = (delta >= 0 ? '+' : '') + delta.toFixed(0);

  return (
    <div className={`ps-mh-row-wrap${expanded ? ' ps-mh-row-wrap-open' : ''}`}>
      <div
        className="ps-mh-compact-row"
        role="button"
        tabIndex={0}
        onClick={onToggle}
        onKeyDown={(e) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); onToggle(); } }}
      >
        <div className="ps-mh-left">
          <span className={`ps-mh-pill${won ? ' ps-mh-pill-win' : ' ps-mh-pill-loss'}`}>
            {won ? 'W' : 'L'}
          </span>
          <span className="ps-mh-date">{dateStr}</span>
        </div>

        <div className="ps-mh-center">
          <div className="ps-mh-mode-line">
            <span className="ps-mh-mode">{round.gameMode}</span>
          </div>
          {partners.length > 0 && (
            <div className="ps-mh-partner">
              {t.playerMatchHistoryWith}
              {partners.map((p, i) => (
                <span key={p.id}>
                  {i > 0 && ', '}
                  <PlayerLink player={p} className="ps-mh-partner-link" />
                </span>
              ))}
            </div>
          )}
        </div>

        <span className={`ps-mh-party-col${isRe ? ' ps-mh-party-re' : ' ps-mh-party-ko'}`}>
          {isRe ? 'Re' : 'Ko'}
        </span>

        <div className="ps-mh-right">
          <span className={`ps-mh-pts${won ? ' ps-mh-pts-win' : ' ps-mh-pts-loss'}`}>{pts}</span>
          <span className="ps-mh-chevron">{expanded ? '▴' : '▾'}</span>
        </div>
      </div>

      {expanded && (
        <div className="ps-mh-expanded">
          <RoundCard round={round} onDelete={() => {}} readOnly />
        </div>
      )}
    </div>
  );
}
