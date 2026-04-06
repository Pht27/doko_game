import type { CardDto, SonderkarteInfoDto } from '../types/api';
import { cardSvgComponent } from '../api/cards';
import './HandDisplay.css';

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
    <div className="hand">
      {cards.map((card, i) => {
        const clickable = isMyTurn && legalCardIds.has(card.id);
        const CardSvg = cardSvgComponent(card.suit, card.rank);

        return (
          <div
            key={card.id}
            style={{ marginLeft: i === 0 ? 0 : -20, zIndex: i }}
            className="card-wrapper"
          >
            <CardSvg
              onClick={() => clickable && onCardClick(card)}
              className={`card ${clickable ? 'card-playable' : 'card-unplayable'}`}
            />
          </div>
        );
      })}
    </div>
  );
}
