import type { CardDto, SonderkarteInfoDto } from '../types/api';
import { cardSvgComponent } from '../api/cards';
import '../styles/HandDisplay.css';

// ─── Fan display tuning ───────────────────────────────────────────────────────
/** Maximum total spread of the fan in degrees. */
const FAN_SPREAD_DEG = 30;
/** Maximum rotation step between adjacent cards — limits spread when few cards remain. */
const MAX_CARD_ANGLE_DEG = 4;
/** Vertical drop (rem) at the outermost card. Controls arc curvature. */
const ARC_DEPTH_REM = 2;
/** How much (rem) a selected card lifts above the arc. */
const SELECTED_LIFT_REM = 1.25;
// Card overlap and card size are in HandDisplay.css as --card-overlap / .card height.
// ─────────────────────────────────────────────────────────────────────────────

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
