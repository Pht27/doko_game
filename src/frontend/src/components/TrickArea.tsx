import type { CSSProperties } from 'react';
import type { TrickSummaryDto } from '../types/api';
import { cardSvgPath, cardBackSvgPath } from '../api/cards';
import { t } from '../translations';
import '../styles/TrickArea.css';

// ── Display constants ──────────────────────────────────────────────────────────
/** Max random tilt angle in degrees (±). Must match --trick-max-tilt-deg in CSS. */
const MAX_TILT_DEG = 15;
/** Card offset from pile centre as a fraction of card width (horizontal) or height (vertical). */
const OFFSET_FACTOR = 0.33;

// Per-seat offsets as dimensionless factors (multiplied by card-w / card-h in CSS calc)
const SEAT_OFFSET: Record<string, { ox: number; oy: number }> = {
  top:    { ox: 0,              oy: -OFFSET_FACTOR },
  bottom: { ox: 0,              oy:  OFFSET_FACTOR },
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

/**
 * Deterministic tilt — pure hash of player + cardId so it never changes on re-render.
 */
function cardTilt(player: number, cardId: number): number {
  const hash = ((player * 31 + cardId * 17) % 200) - 100; // -100 … 99
  return (hash / 100) * MAX_TILT_DEG;
}

/**
 * Deterministic stack offset — pure hash of index + cardId.
 * Small pixel nudges and rotation so the pile looks natural, never random-looking.
 */
function stackOffset(index: number, cardId: number): { ox: number; oy: number; rot: number } {
  const h1 = ((index * 53 + cardId * 7)  % 200) - 100; // -100…99
  const h2 = ((index * 37 + cardId * 13) % 200) - 100;
  const h3 = ((index * 19 + cardId * 29) % 200) - 100;
  return {
    ox:  (h1 / 100) * 5,   // ±5 px
    oy:  (h2 / 100) * 4,   // ±4 px
    rot: (h3 / 100) * 8,   // ±8 deg
  };
}

/** Direction the pile flies per winner's seat (in px, large enough to leave the viewport). */
const FLY_TRANSLATE: Record<string, { x: string; y: string }> = {
  bottom: { x: '0px',    y: '280px'  },
  top:    { x: '0px',    y: '-280px' },
  left:   { x: '-280px', y: '0px'    },
  right:  { x: '280px',  y: '0px'    },
};

/** Rotation of the stacked pile so it faces the winner's seat. */
const PILE_ROT: Record<string, number> = {
  bottom:   0,
  top:    180,
  left:    90,
  right:  -90,
};

// ── Types ──────────────────────────────────────────────────────────────────────
export type AnimPhase = 'winner' | 'flip' | 'stack' | 'fly' | null;

interface TrickAreaProps {
  trick: TrickSummaryDto | null;
  requestingPlayer: number;
  /** Seat layout: which compass direction a player sits relative to the requesting player */
  seatOf: (player: number) => 'bottom' | 'left' | 'top' | 'right';
  /** Current animation phase (null = normal display) */
  animPhase?: AnimPhase;
  /** Seat of the trick winner — used to determine fly-away direction */
  winnerSeat?: 'bottom' | 'left' | 'top' | 'right';
}

// ── Component ──────────────────────────────────────────────────────────────────
export function TrickArea({ trick, requestingPlayer, seatOf, animPhase = null, winnerSeat }: TrickAreaProps) {
  if (!trick || trick.cards.length === 0) {
    return (
      <div className="trick-empty">
        {t.keinStich}
      </div>
    );
  }

  const isStacked = animPhase === 'stack' || animPhase === 'fly';
  const isFlipped  = animPhase === 'flip'  || isStacked;

  // ── Pile wrapper: drives rotation toward winner + fly-away ───────────────
  const pileRot = winnerSeat ? PILE_ROT[winnerSeat] : 0;
  let pileStyle: CSSProperties = {};
  if (animPhase === 'flip') {
    // Snap pile to face the winner simultaneously with the card flip — no transition needed.
    pileStyle = { transform: `rotate(${pileRot}deg)` };
  } else if (animPhase === 'stack') {
    // Hold the rotation; pre-arm transition so fly can animate immediately.
    pileStyle = {
      transform:  `rotate(${pileRot}deg)`,
      transition: 'transform 0.8s ease-in, opacity 0.8s ease-in',
    };
  } else if (animPhase === 'fly' && winnerSeat) {
    const { x, y } = FLY_TRANSLATE[winnerSeat];
    // translate() before rotate() so the translation is in screen coordinates.
    pileStyle = {
      transform:  `translate(${x}, ${y}) rotate(${pileRot}deg)`,
      opacity:    0,
      transition: 'transform 0.8s ease-in, opacity 0.8s ease-in',
    };
  }

  const pileClass = ['trick-pile', animPhase ? `trick-phase-${animPhase}` : '']
    .filter(Boolean).join(' ');

  return (
    <div className={pileClass} style={pileStyle}>
      {trick.cards.map(({ player, card }, index) => {
        const seat     = seatOf(player);
        const isWinner = player === trick.winner;
        const tilt     = cardTilt(player, card.id);
        const rotation = SEAT_BASE_ROT[seat] + tilt;
        const { ox, oy } = SEAT_OFFSET[seat];

        // ── Positioner transform (wrapper — handles seat offset) ─────────────
        // During stacked phases: tiny deterministic nudge to look like a real pile.
        // During normal/winner: full seat-based spread.
        let posTransform: string;
        if (isStacked) {
          const { ox: sox, oy: soy, rot } = stackOffset(index, card.id);
          posTransform = `translate(calc(-50% + ${sox}px), calc(-50% + ${soy}px)) rotate(${rot}deg)`;
        } else {
          const xExpr = ox === 0
            ? '-50%'
            : `calc(-50% + ${ox} * var(--trick-card-w))`;
          const yExpr = oy === 0
            ? '-50%'
            : `calc(-50% + ${oy} * var(--trick-card-h))`;
          posTransform = `translate(${xExpr}, ${yExpr}) rotate(${rotation}deg)`;
        }

        // ── CSS classes ──────────────────────────────────────────────────────
        const posClass = [
          'trick-card-positioner',
          isStacked ? 'trick-card-positioner-stacked' : '',
        ].filter(Boolean).join(' ');

        const imgClass = [
          'trick-card',
          // Hide yellow ring once cards are face-down
          player === requestingPlayer && !isFlipped ? 'trick-card-mine' : '',
          animPhase === 'winner' && isWinner ? 'trick-card-winner-pulse' : '',
        ].filter(Boolean).join(' ');

        // Winner card sits on top of all others during the highlight phase
        const zIndex = (animPhase === 'winner' && isWinner)
          ? trick.cards.length + 1
          : index + 1;

        const src = isFlipped ? cardBackSvgPath : cardSvgPath(card.suit, card.rank);

        return (
          // Outer span: handles absolute positioning & stacking slide
          <span
            key={card.id}
            className={posClass}
            style={{ zIndex, transform: posTransform }}
          >
            {/* Inner img: handles winner scale-pulse & card face/back */}
            <img
              src={src}
              alt={isFlipped ? 'Kartenrücken' : t.cardAlt(card.rank, card.suit)}
              className={imgClass}
            />
          </span>
        );
      })}
    </div>
  );
}
