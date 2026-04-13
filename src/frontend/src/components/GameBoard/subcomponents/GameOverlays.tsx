import type { CardDto, GameResultDto, SonderkarteInfoDto } from '../../../types/api';
import { SonderkarteOverlay } from '../../SonderkarteOverlay/SonderkarteOverlay';
import { ResultScreen } from '../../ResultScreen/ResultScreen';

interface GameOverlaysProps {
  pendingCard: { card: CardDto; sonderkarten: SonderkarteInfoDto[] } | null;
  finishedResult: GameResultDto | null;
  onSubmitPlayCard: (cardId: number, activateSonderkarten: string[], genscherPartnerId: number | null) => Promise<void>;
  onCancelPendingCard: () => void;
  onNewGame: () => void;
}

/**
 * Renders all full-screen overlays: the Sonderkarte confirmation dialog and the result screen.
 */
export function GameOverlays({ pendingCard, finishedResult, onSubmitPlayCard, onCancelPendingCard, onNewGame }: GameOverlaysProps) {
  return (
    <>
      {pendingCard && (
        <SonderkarteOverlay
          sonderkarten={pendingCard.sonderkarten}
          onConfirm={(selected, genscherPartnerId) =>
            onSubmitPlayCard(pendingCard.card.id, selected, genscherPartnerId)
          }
          onCancel={onCancelPendingCard}
        />
      )}
      {finishedResult && (
        <ResultScreen result={finishedResult} onNewGame={onNewGame} />
      )}
    </>
  );
}
