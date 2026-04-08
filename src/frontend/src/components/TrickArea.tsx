import type { TrickSummaryDto } from '../types/api';
import { cardSvgPath } from '../api/cards';
import { t } from '../translations';
import '../styles/TrickArea.css';

interface TrickAreaProps {
  trick: TrickSummaryDto | null;
  requestingPlayer: number;
  /** Seat layout: which player sits at which compass direction relative to the requesting player */
  seatOf: (player: number) => 'bottom' | 'left' | 'top' | 'right';
}

export function TrickArea({ trick, requestingPlayer, seatOf }: TrickAreaProps) {
  if (!trick || trick.cards.length === 0) {
    return (
      <div className="trick-empty">
        {t.keinStich}
      </div>
    );
  }

  const positionClass: Record<string, string> = {
    top: 'trick-card-top',
    left: 'trick-card-left',
    bottom: 'trick-card-bottom',
    right: 'trick-card-right',
  };

  return (
    <div className="trick-grid">
      {trick.cards.map(({ player, card }) => {
        const seat = seatOf(player);
        const isMe = player === requestingPlayer;
        return (
          <div
            key={card.id}
            className={`${positionClass[seat]} trick-card-cell`}
          >
            <img
              src={cardSvgPath(card.suit, card.rank)}
              alt={t.cardAlt(card.rank, card.suit)}
              className={`trick-card${isMe ? ' trick-card-mine' : ''}`}
            />
          </div>
        );
      })}
    </div>
  );
}
