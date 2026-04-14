import type { TrickSummaryDto } from '../../../types/api';
import type { AnimPhase } from '../../TrickArea/TrickArea';
import { TrickArea } from '../../TrickArea/TrickArea';

interface CenterAreaProps {
  requestingPlayer: number;
  seatOf: (player: number) => 'bottom' | 'left' | 'top' | 'right';
  displayTrick: TrickSummaryDto | null;
  animPhase: AnimPhase;
  winnerSeat?: 'bottom' | 'left' | 'top' | 'right';
}

/**
 * Renders the trick area in the center of the board.
 * Dialog overlays (reservation, health check, armut) are rendered
 * as absolute overlays in GameBoard.tsx.
 */
export function CenterArea({ requestingPlayer, seatOf, displayTrick, animPhase, winnerSeat }: CenterAreaProps) {
  return (
    <TrickArea
      trick={displayTrick}
      requestingPlayer={requestingPlayer}
      seatOf={seatOf}
      animPhase={animPhase}
      winnerSeat={winnerSeat}
    />
  );
}
