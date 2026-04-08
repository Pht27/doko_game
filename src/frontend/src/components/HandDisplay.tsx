import type { CardDto, SonderkarteInfoDto } from '../types/api';
import { cardSvgComponent } from '../api/cards';
import '../styles/HandDisplay.css';

interface HandDisplayProps {
  cards: CardDto[];
  legalCardIds: Set<number>;
  isMyTurn: boolean;
  eligibleSonderkarten: Record<number, SonderkarteInfoDto[]>;
  onCardClick: (card: CardDto) => void;
  /** When true, all cards are selectable regardless of legalCardIds. */
  selectionMode?: boolean;
  selectedCardIds?: Set<number>;
  maxSelection?: number;
}

export function HandDisplay({
  cards,
  legalCardIds,
  isMyTurn,
  onCardClick,
  selectionMode = false,
  selectedCardIds,
  maxSelection,
}: HandDisplayProps) {
  return (
    <div className="hand">
      {cards.map((card, i) => {
        const isSelected = selectedCardIds?.has(card.id) ?? false;
        const canSelect = selectionMode && (isSelected || (selectedCardIds?.size ?? 0) < (maxSelection ?? Infinity));
        const clickable = selectionMode ? canSelect : (isMyTurn && legalCardIds.has(card.id));
        const CardSvg = cardSvgComponent(card.suit, card.rank);

        let cardClass = 'card ';
        if (selectionMode) {
          cardClass += isSelected ? 'card-selected' : (canSelect ? 'card-selectable' : 'card-unplayable');
        } else {
          cardClass += clickable ? 'card-playable' : 'card-unplayable';
        }

        return (
          <div
            key={card.id}
            style={{ marginLeft: i === 0 ? 0 : -20, zIndex: i }}
            className={`card-wrapper${isSelected ? ' card-wrapper-selected' : ''}`}
          >
            <CardSvg
              onClick={() => clickable && onCardClick(card)}
              className={cardClass}
            />
          </div>
        );
      })}
    </div>
  );
}
