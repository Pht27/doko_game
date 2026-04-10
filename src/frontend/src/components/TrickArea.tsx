import type { TrickSummaryDto } from '../types/api';
import { cardSvgPath } from '../api/cards';
import { t } from '../translations';
import '../styles/TrickArea.css';

// ── Display constants ──────────────────────────────────────────────────────────
/** Max random tilt angle in degrees (±). Must match --trick-max-tilt-deg in CSS. */
const MAX_TILT_DEG = 15;
/** Card offset from pile centre as a fraction of card width (horizontal) or height (vertical). */
const OFFSET_FACTOR = 0.40;

// Per-seat offsets as dimensionless factors (multiplied by card-w / card-h in CSS calc)
const SEAT_OFFSET: Record<string, { ox: number; oy: number }> = {
  top:    { ox: 0,             oy: -OFFSET_FACTOR },
  bottom: { ox: 0,             oy:  OFFSET_FACTOR },
  left:   { ox: -OFFSET_FACTOR, oy: 0 },
  right:  { ox:  OFFSET_FACTOR, oy: 0 },
};

/** Base rotation per seat so cards look like they came from that direction. */
const SEAT_BASE_ROT: Record<string, number> = {
  bottom:  0,
  top:   180,
  left:   90,
  right: -90,
};

/** Deterministic tilt so cards don't jump on re-render. */
function cardTilt(player: number, cardId: number): number {
  const hash = ((player * 31 + cardId * 17) % 200) - 100; // -100 … 99
  return (hash / 100) * MAX_TILT_DEG;
}

// ── Types ──────────────────────────────────────────────────────────────────────
interface TrickAreaProps {
  trick: TrickSummaryDto | null;
  requestingPlayer: number;
  /** Seat layout: which compass direction a player sits at relative to the requesting player */
  seatOf: (player: number) => 'bottom' | 'left' | 'top' | 'right';
}

// ── Component ──────────────────────────────────────────────────────────────────
export function TrickArea({ trick, requestingPlayer, seatOf }: TrickAreaProps) {
  if (!trick || trick.cards.length === 0) {
    return (
      <div className="trick-empty">
        {t.keinStich}
      </div>
    );
  }

  return (
    <div className="trick-pile">
      {trick.cards.map(({ player, card }, index) => {
        const seat = seatOf(player);
        const isMe = player === requestingPlayer;
        const tilt = cardTilt(player, card.id);
        const rotation = SEAT_BASE_ROT[seat] + tilt;
        const { ox, oy } = SEAT_OFFSET[seat];

        // Build CSS calc() expressions; skip multiply-by-0 for cleaner output
        const xExpr = ox === 0
          ? '-50%'
          : `calc(-50% + ${ox} * var(--trick-card-w))`;
        const yExpr = oy === 0
          ? '-50%'
          : `calc(-50% + ${oy} * var(--trick-card-h))`;

        return (
          <img
            key={card.id}
            src={cardSvgPath(card.suit, card.rank)}
            alt={t.cardAlt(card.rank, card.suit)}
            className={`trick-card${isMe ? ' trick-card-mine' : ''}`}
            style={{
              zIndex: index + 1,
              transform: `translate(${xExpr}, ${yExpr}) rotate(${rotation}deg)`,
            }}
          />
        );
      })}
    </div>
  );
}
