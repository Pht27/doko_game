import type { CardDto, SonderkarteInfoDto } from '../types/api';
import { cardSvgPath } from '../api/cards';

interface HandDisplayProps {
  cards: CardDto[];
  legalCardIds: Set<number>;
  isMyTurn: boolean;
  eligibleSonderkarten: Record<number, SonderkarteInfoDto[]>;
  onCardClick: (card: CardDto) => void;
}

export function HandDisplay({
  cards,
  legalCardIds,
  isMyTurn,
  onCardClick,
}: HandDisplayProps) {
  return (
    <div className="flex items-end justify-center gap-[-12px] px-4 pb-2 overflow-visible">
      {cards.map((card, i) => {
        const isLegal = legalCardIds.has(card.id);
        const clickable = isMyTurn && isLegal;

        return (
          <div
            key={card.id}
            style={{ marginLeft: i === 0 ? 0 : -20, zIndex: i }}
            className="relative transition-transform duration-100"
          >
            <img
              src={cardSvgPath(card.suit, card.rank)}
              alt={`${card.rank} of ${card.suit}`}
              onClick={() => clickable && onCardClick(card)}
              className={[
                'h-28 w-auto rounded-md shadow-lg select-none',
                clickable
                  ? 'cursor-pointer hover:-translate-y-3 ring-2 ring-indigo-400 transition-transform'
                  : 'opacity-40 cursor-default',
                'transition-transform duration-100',
              ].join(' ')}
            />
          </div>
        );
      })}
    </div>
  );
}
