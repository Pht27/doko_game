import type { TrickSummaryDto } from '../../types/api';
import { Card } from '../Card/Card';
import { SEAT_OFFSET, SEAT_BASE_ROT, MAX_TILT_DEG } from '../TrickArea/trickArea.constants';
import '../../styles/LastTrickOverlay.css';

interface LastTrickOverlayProps {
  trick: TrickSummaryDto;
  seatOf: (player: number) => 'bottom' | 'left' | 'top' | 'right';
  onClose: () => void;
}

function cardTilt(player: number, cardId: number): number {
  const hash = ((player * 31 + cardId * 17) % 200) - 100;
  return (hash / 100) * MAX_TILT_DEG;
}

export function LastTrickOverlay({ trick, seatOf, onClose }: LastTrickOverlayProps) {
  return (
    <div
      className="fixed inset-0 bg-black/75 flex items-center justify-center z-50"
      onClick={onClose}
    >
      <div
        className="last-trick-panel"
        onClick={(e) => e.stopPropagation()}
      >
        <p className="last-trick-title">Letzter Stich</p>
        <div className="last-trick-pile">
          {trick.cards.map(({ player, card, faceDown }, index) => {
            const seat = seatOf(player);
            const isWinner = player === trick.winner;
            const tilt = cardTilt(player, card.id);
            const rotation = SEAT_BASE_ROT[seat] + tilt;
            const { ox, oy } = SEAT_OFFSET[seat];

            const xExpr = ox === 0 ? '-50%' : `calc(-50% + ${ox} * var(--lt-card-w))`;
            const yExpr = oy === 0 ? '-50%' : `calc(-50% + ${oy} * var(--lt-card-h))`;
            const posTransform = `translate(${xExpr}, ${yExpr}) rotate(${rotation}deg)`;

            const zIndex = isWinner ? trick.cards.length + 1 : index + 1;

            return (
              <span
                key={card.id}
                className="last-trick-card-positioner"
                style={{ zIndex, transform: posTransform }}
              >
                <Card
                  card={card}
                  faceDown={faceDown}
                  className={`last-trick-card${isWinner ? ' last-trick-card-winner' : ''}`}
                />
              </span>
            );
          })}
        </div>
      </div>
    </div>
  );
}
