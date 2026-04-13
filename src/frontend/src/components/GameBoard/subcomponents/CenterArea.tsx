import type { PlayerGameViewResponse, TrickSummaryDto } from '../../../types/api';
import type { AnimPhase } from '../../TrickArea/TrickArea';
import type { GameActions } from '../../../hooks/useGameActions';
import { HealthCheckDialog } from '../../dialogs/HealthCheckDialog';
import { ReservationDialog } from '../../dialogs/ReservationDialog';
import { ArmutPartnerDialog } from '../../dialogs/ArmutPartnerDialog';
import { ArmutReturnDialog } from '../../dialogs/ArmutReturnDialog';
import { TrickArea } from '../../TrickArea/TrickArea';

interface CenterAreaProps {
  view: PlayerGameViewResponse | null;
  activePlayer: number;
  seatOf: (player: number) => 'bottom' | 'left' | 'top' | 'right';
  displayTrick: TrickSummaryDto | null;
  animPhase: AnimPhase;
  winnerSeat?: 'bottom' | 'left' | 'top' | 'right';
  actions: GameActions;
}

/**
 * Renders the correct dialog for the current game phase, or the trick area during play.
 */
export function CenterArea({ view, activePlayer, seatOf, displayTrick, animPhase, winnerSeat, actions }: CenterAreaProps) {
  if (view?.shouldDeclareHealth) {
    return (
      <HealthCheckDialog
        playerId={activePlayer}
        onDeclare={actions.handleHealthCheck}
      />
    );
  }

  if (view?.shouldDeclareReservation) {
    return (
      <ReservationDialog
        playerId={activePlayer}
        eligibleReservations={view.eligibleReservations}
        mustDeclare={view.mustDeclareReservation}
        onDeclare={actions.handleReservation}
      />
    );
  }

  if (view?.shouldRespondToArmut) {
    return (
      <ArmutPartnerDialog
        playerId={activePlayer}
        onRespond={actions.handleArmutResponse}
      />
    );
  }

  if (view?.shouldReturnArmutCards && view.armutCardReturnCount !== null) {
    return (
      <ArmutReturnDialog
        playerId={activePlayer}
        cardReturnCount={view.armutCardReturnCount}
        selectedCount={actions.armutReturnSelected.size}
        onConfirm={() => actions.handleArmutExchange(Array.from(actions.armutReturnSelected))}
      />
    );
  }

  return (
    <TrickArea
      trick={displayTrick}
      requestingPlayer={activePlayer}
      seatOf={seatOf}
      animPhase={animPhase}
      winnerSeat={winnerSeat}
    />
  );
}
