import React from 'react';
import type { CSSProperties } from 'react';
import type { TrickSummaryDto } from '@/types/api';
import { Card } from '../Card/Card';
import {
  MAX_TILT_DEG,
  SEAT_OFFSET,
  SEAT_BASE_ROT,
  FLY_TRANSLATE,
  PILE_ROT,
} from './trickArea.constants';
import './TrickArea.css';

export type AnimPhase = 'appear' | 'winner' | 'flip' | 'stack' | 'fly' | null;

interface TrickAreaProps {
  trick: TrickSummaryDto | null;
  requestingPlayer: number;
  /** Seat layout: which compass direction a player sits relative to the requesting player */
  seatOf: (player: number) => 'bottom' | 'left' | 'top' | 'right';
  /** Current animation phase (null = normal display) */
  animPhase?: AnimPhase;
  /** Seat of the trick winner — used to determine fly-away direction */
  winnerSeat?: 'bottom' | 'left' | 'top' | 'right';
  /** Player who just triggered a sonderkarte — their card gets the special fly-in */
  sonderkartePlayer?: number;
}

const FLY_FROM: Record<'bottom' | 'left' | 'top' | 'right', { x: string; y: string }> = {
  bottom: { x: '0px',     y: '1rem'  },
  top:    { x: '0px',     y: '-1rem' },
  left:   { x: '-1rem',   y: '0px'   },
  right:  { x: '1rem',    y: '0px'   },
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

export function TrickArea({ trick, requestingPlayer, seatOf, animPhase = null, winnerSeat, sonderkartePlayer }: TrickAreaProps) {
  if (!trick || trick.cards.length === 0) {
    return <div className="trick-pile" />;
  }

  const isStacked = animPhase === 'stack' || animPhase === 'fly';
  const isFlipped  = animPhase === 'flip'  || isStacked;

  // ── Pile wrapper: drives rotation toward winner + fly-away ───────────────
  const pileRot = winnerSeat ? PILE_ROT[winnerSeat] : 0;
  let pileStyle: CSSProperties = {};
  if (animPhase === 'flip') {
    pileStyle = { transform: `rotate(${pileRot}deg)` };
  } else if (animPhase === 'stack') {
    pileStyle = {
      transform:  `rotate(${pileRot}deg)`,
      transition: 'transform 0.8s ease-in, opacity 0.8s ease-in',
    };
  } else if (animPhase === 'fly' && winnerSeat) {
    const { x, y } = FLY_TRANSLATE[winnerSeat];
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
      {trick.cards.map(({ player, card, faceDown }, index) => {
        const seat     = seatOf(player);
        const isWinner = player === trick.winner;
        const tilt     = cardTilt(player, card.id);
        const rotation = SEAT_BASE_ROT[seat] + tilt;
        const { ox, oy } = SEAT_OFFSET[seat];

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

        const posClass = [
          'trick-card-positioner',
          isStacked ? 'trick-card-positioner-stacked' : '',
        ].filter(Boolean).join(' ');

        const isSonderkarte = sonderkartePlayer != null && player === sonderkartePlayer && !isFlipped;

        const imgClass = [
          'trick-card',
          isSonderkarte ? 'trick-card-sonderkarte' : '',
          player === requestingPlayer && !isFlipped ? 'trick-card-mine' : '',
          animPhase === 'winner' && isWinner ? 'trick-card-winner-pulse' : '',
        ].filter(Boolean).join(' ');

        const zIndex = (animPhase === 'winner' && isWinner)
          ? trick.cards.length + 1
          : index + 1;

        const flyFrom = FLY_FROM[seat];

        return (
          <span
            key={card.id}
            className={posClass}
            style={{ zIndex, transform: posTransform }}
          >
            <Card
              card={card}
              faceDown={isFlipped || faceDown}
              className={imgClass}
              style={{ '--fly-from-x': flyFrom.x, '--fly-from-y': flyFrom.y } as React.CSSProperties}
            />
          </span>
        );
      })}
    </div>
  );
}
