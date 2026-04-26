import { useState, useEffect } from 'react';
import type { CardDto } from '@/types/api';
import { cardSvgComponent } from '@/api/cards';
import {
  FAN_SPREAD_DEG, MAX_CARD_ANGLE_DEG, ARC_DEPTH_REM, SELECTED_LIFT_REM,
  MOBILE_CARD_STEP_REM, TABLET_CARD_STEP_REM, TABLET_BREAKPOINT_PX,
  COMPACT_HAND_THRESHOLD, MOBILE_COMPACT_CARD_STEP_REM, TABLET_COMPACT_CARD_STEP_REM,
} from './handDisplay.constants';
import './HandDisplay.css';

interface HandDisplayProps {
  cards: CardDto[];
  legalCardIds: Set<number>;
  isMyTurn: boolean;
  onCardClick: (card: CardDto) => void;
  /** When true, all cards are selectable regardless of legalCardIds. */
  selectionMode?: boolean;
  selectedCardIds?: Set<number>;
  maxSelection?: number;
  /** Card currently being played (shows fly-out animation). */
  playingCardId?: number | null;
  /** Cards that have at least one activatable sonderkarte effect. */
  sonderkarteCardIds?: Set<number>;
}

/** Returns the horizontal step (rem) between adjacent card centers for the current viewport and card count. */
function useCardStep(cardCount: number): number {
  const [isTablet, setIsTablet] = useState(window.innerWidth >= TABLET_BREAKPOINT_PX);
  useEffect(() => {
    const mq = window.matchMedia(`(min-width: ${TABLET_BREAKPOINT_PX}px)`);
    const handler = (e: MediaQueryListEvent) => setIsTablet(e.matches);
    mq.addEventListener('change', handler);
    return () => mq.removeEventListener('change', handler);
  }, []);
  if (cardCount >= COMPACT_HAND_THRESHOLD) {
    return isTablet ? TABLET_COMPACT_CARD_STEP_REM : MOBILE_COMPACT_CARD_STEP_REM;
  }
  return isTablet ? TABLET_CARD_STEP_REM : MOBILE_CARD_STEP_REM;
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
 * All positioning (X fan offset, parabolic arc drop, rotation) is encoded here
 * so that every change animates via the single `transition: transform` on .card-wrapper.
 *
 * Assuming .card-wrapper is `position: absolute; left: 50%`:
 *   translateX(xOffset - 50%)  →  positions card center at `xOffset` from the hand's midpoint
 *   translateY(arcDrop)        →  drops outer cards along the parabolic arc
 *   rotate(angle)              →  fans the card around its bottom-center (transform-origin)
 */
function getCardTransform(index: number, total: number, isSelected: boolean, cardStep: number): string {
  const angle = getFanAngle(index, total);
  const outerAngle = getFanAngle(total - 1, total); // positive max angle
  const arcDrop = outerAngle > 0
    ? ARC_DEPTH_REM * Math.pow(angle / outerAngle, 2) + (Math.abs(angle / outerAngle) * (SELECTED_LIFT_REM / 2))
    : 0;
  const totalY = arcDrop - (isSelected ? SELECTED_LIFT_REM : 0);
  const xOffset = (index - (total - 1) / 2) * cardStep;
  // calc() mixes rem (fan position) and % (card's own half-width) to center the card on xOffset.
  return `translateX(calc(${xOffset.toFixed(3)}rem - 50%)) translateY(${totalY.toFixed(3)}rem) rotate(${angle.toFixed(2)}deg)`;
}

function getCardClass(
  selectionMode: boolean,
  isSelected: boolean,
  canSelect: boolean,
  clickable: boolean,
  isPlaying: boolean,
): string {
  const base = selectionMode
    ? (isSelected ? 'card card-selected' : canSelect ? 'card card-selectable' : 'card card-unplayable')
    : `card ${clickable ? 'card-playable' : 'card-unplayable'}`;
  return isPlaying ? `${base} card-playing` : base;
}

export function HandDisplay({
  cards,
  legalCardIds,
  isMyTurn,
  onCardClick,
  selectionMode = false,
  selectedCardIds,
  maxSelection,
  playingCardId,
  sonderkarteCardIds,
}: HandDisplayProps) {
  const cardStep = useCardStep(cards.length);

  return (
    <div className="hand">
      {cards.map((card, i) => {
        const isSelected = selectedCardIds?.has(card.id) ?? false;
        const canSelect = selectionMode && (isSelected || (selectedCardIds?.size ?? 0) < (maxSelection ?? Infinity));
        const clickable = selectionMode ? canSelect : (isMyTurn && legalCardIds.has(card.id));
        const isPlaying = playingCardId === card.id;
        const hasSonderkarte = sonderkarteCardIds?.has(card.id) ?? false;
        const CardSvg = cardSvgComponent(card.suit, card.rank);

        return (
          <div
            key={card.id}
            className={`card-wrapper${hasSonderkarte ? ' card-wrapper-sonderkarte' : ''}`}
            style={{ zIndex: i, transform: getCardTransform(i, cards.length, isSelected, cardStep) }}
          >
            <CardSvg
              onClick={() => clickable && !isPlaying && onCardClick(card)}
              className={getCardClass(selectionMode, isSelected, canSelect, clickable, isPlaying)}
            />
          </div>
        );
      })}
    </div>
  );
}
