import type { TrickSummaryDto } from '../types/api';
import { cardSvgPath } from '../api/cards';

interface TrickAreaProps {
  trick: TrickSummaryDto | null;
  requestingPlayer: number;
  /** Seat layout: which player sits at which compass direction relative to the requesting player */
  seatOf: (player: number) => 'bottom' | 'left' | 'top' | 'right';
}

export function TrickArea({ trick, requestingPlayer, seatOf }: TrickAreaProps) {
  if (!trick || trick.cards.length === 0) {
    return (
      <div className="flex items-center justify-center w-40 h-40 rounded-full border-2 border-white/10 text-white/20 text-sm">
        No trick
      </div>
    );
  }

  const positionClass: Record<string, string> = {
    top: 'row-start-1 col-start-2',
    left: 'row-start-2 col-start-1',
    bottom: 'row-start-3 col-start-2',
    right: 'row-start-2 col-start-3',
  };

  return (
    <div className="grid grid-cols-3 grid-rows-3 gap-1 w-44 h-44">
      {trick.cards.map(({ player, card }) => {
        const seat = seatOf(player);
        const isMe = player === requestingPlayer;
        return (
          <div
            key={card.id}
            className={`${positionClass[seat]} flex items-center justify-center`}
          >
            <img
              src={cardSvgPath(card.suit, card.rank)}
              alt={`${card.rank} of ${card.suit}`}
              className={`w-12 h-auto rounded shadow-lg drop-shadow ${isMe ? 'ring-2 ring-yellow-400' : ''}`}
            />
          </div>
        );
      })}
    </div>
  );
}
