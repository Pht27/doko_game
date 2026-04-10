import type { CardDto } from '../../types/api';
import { cardSvgComponent } from '../../api/cards';
import { FAN_SPREAD_DEG, MAX_CARD_ANGLE_DEG, ARC_DEPTH_REM, SELECTED_LIFT_REM } from './handDisplay.constants';
import '../../styles/HandDisplay.css';

interface HandDisplayProps {
  cards: CardDto[];
  legalCardIds: Set<number>;
  isMyTurn: boolean;
  onCardClick: (card: CardDto) => void;
  /** When true, all cards are selectable regardless of legalCardIds. */
  selectionMode?: boolean;
  selectedCardIds?: Set<number>;
  maxSelection?: number;
}

/** Rotation angle for card at `index` in a hand of `total` cards. */
function getFanAngle(index: number, total: number): number {
  if (total <= 1) return 0;
  const mid = (total - 1) / 2;
  const perCard = Math.min(FAN_SPREAD_DEG / (total - 1), MAX_CARD_ANGLE_DEG);
  return (index - mid) * perCard;
}

/**
 * Full CSS transform for a card.
 * Outer cards drop along a parabolic arc; selected cards lift above it.
 * Order: translateY first (page space), then rotate around card's bottom-center.
 */
function getCardTransform(index: number, total: number, isSelected: boolean): string {
  const angle = getFanAngle(index, total);
  const outerAngle = getFanAngle(total - 1, total); // positive max angle
  const arcDrop = outerAngle > 0
    ? ARC_DEPTH_REM * Math.pow(angle / outerAngle, 2)
    : 0;
  const totalY = arcDrop - (isSelected ? SELECTED_LIFT_REM : 0);
  return `translateY(${totalY.toFixed(3)}rem) rotate(${angle.toFixed(2)}deg)`;
}

function getCardClass(
  selectionMode: boolean,
  isSelected: boolean,
  canSelect: boolean,
  clickable: boolean,
): string {
  if (selectionMode) {
    if (isSelected) return 'card card-selected';
    if (canSelect) return 'card card-selectable';
    return 'card card-unplayable';
  }
  return `card ${clickable ? 'card-playable' : 'card-unplayable'}`;
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

        return (
          <div
            key={card.id}
            className="card-wrapper"
            style={{ zIndex: i, transform: getCardTransform(i, cards.length, isSelected) }}
          >
            <CardSvg
              onClick={() => clickable && onCardClick(card)}
              className={getCardClass(selectionMode, isSelected, canSelect, clickable)}
            />
          </div>
        );
      })}
    </div>
  );
}
