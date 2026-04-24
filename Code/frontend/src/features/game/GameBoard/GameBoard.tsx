import { useState } from 'react';
import type { PlayerGameViewResponse, GameResultDto, TrickSummaryDto, SonderkarteNotification } from '@/types/api';
import type { AnimPhase } from '../TrickArea/TrickArea';
import type { GameActions } from '@/hooks/useGameActions';
import type { MultiplayerNewGameProps } from '../ResultScreen/ResultScreen';
import { ResultScreen } from '../ResultScreen/ResultScreen';
import { GameInfo } from '../shared/GameInfo';
import { PlayerLabel } from '../shared/PlayerLabel';
import { HandDisplay } from '../HandDisplay/HandDisplay';
import { AnnouncementButton } from '../AnnouncementButton/AnnouncementButton';
import { LastTrickOverlay } from '../LastTrickOverlay/LastTrickOverlay';
import { cardBackSvgPath } from '@/api/cards';
import { ArmutBanner } from './subcomponents/ArmutBanner';
import { CenterArea } from './subcomponents/CenterArea';
import { PlayerGrid } from './subcomponents/PlayerGrid';
import { GameOverlays } from './subcomponents/GameOverlays';
import { HealthCheckDialog } from '../dialogs/HealthCheckDialog';
import { ReservationDialog } from '../dialogs/ReservationDialog';
import { ArmutPartnerDialog } from '../dialogs/ArmutPartnerDialog';
import { ArmutReturnDialog } from '../dialogs/ArmutReturnDialog';
import { t } from '@/utils/translations';

interface GameBoardProps {
  view: PlayerGameViewResponse | null;
  activePlayer: number;
  animTrick: TrickSummaryDto | null;
  animPhase: AnimPhase;
  actions: GameActions;
  finishedResult: GameResultDto | null;
  sonderkarteNotification: SonderkarteNotification | null;
  viewLoading: boolean;
  viewError: string | null;
  allowPlayerSwitching: boolean;
  onPlayerSwitch: (player: number) => void;
  onNewGame: () => void;
  multiplayerNewGame?: MultiplayerNewGameProps;
  lastFinishedResult?: GameResultDto | null;
  onLeaveLobby?: () => Promise<void>;
}

/** Returns which compass direction a player sits relative to the active player. */
function seatOf(player: number, activePlayer: number): 'bottom' | 'left' | 'top' | 'right' {
  const seats = ['bottom', 'left', 'top', 'right'] as const;
  return seats[(player - activePlayer + 4) % 4];
}

export function GameBoard({
  view,
  activePlayer,
  animTrick,
  animPhase,
  actions,
  finishedResult,
  sonderkarteNotification,
  viewLoading,
  viewError,
  allowPlayerSwitching,
  onPlayerSwitch,
  onNewGame,
  multiplayerNewGame,
  lastFinishedResult,
  onLeaveLobby,
}: GameBoardProps) {
  const [showInfoOverlay, setShowInfoOverlay] = useState(false);
  const [showLeaveConfirm, setShowLeaveConfirm] = useState(false);
  const [showLastTrick, setShowLastTrick] = useState(false);
  const seatOfPlayer = (player: number) => seatOf(player, activePlayer);

  function triggerLeave() {
    setShowInfoOverlay(false);
    setShowLeaveConfirm(true);
  }

  async function confirmLeave() {
    setShowLeaveConfirm(false);
    await onLeaveLobby?.();
  }

  const displayTrick = animTrick ?? (view?.currentTrick ?? null);
  const winnerSeat = animTrick?.winner != null ? seatOfPlayer(animTrick.winner) : undefined;

  const opponents = view?.otherPlayers ?? [];
  const topOpponent = opponents.find((p) => seatOfPlayer(p.id) === 'top');
  const leftOpponent = opponents.find((p) => seatOfPlayer(p.id) === 'left');
  const rightOpponent = opponents.find((p) => seatOfPlayer(p.id) === 'right');

  const legalCardIds = new Set(view?.legalCards.map((c) => c.id) ?? []);

  return (
    <div className="w-full h-full relative flex flex-col bg-[#1a1a2e] select-none overflow-hidden">
      {/* Floating top-right: game info (clickable when lobby standings available) */}
      {view && (
        <div className="absolute top-2 right-2 z-10">
          <GameInfo
            phase={view.phase}
            gameMode={view.activeGameMode}
            trickNumber={(view.currentTrick?.trickNumber ?? 0) + 1}
            completedTricks={view.completedTricks.length}
            onClick={() => setShowInfoOverlay(true)}
          />
        </div>
      )}

      {/* Armut exchange info banner */}
      {view?.armutExchangeCardCount != null && view.armutReturnedTrump != null && (
        <ArmutBanner
          exchangeCardCount={view.armutExchangeCardCount}
          returnedTrump={view.armutReturnedTrump}
        />
      )}

      {/* Main game area: compass layout with opponents + center */}
      <PlayerGrid
        top={topOpponent && (
          <PlayerLabel
            player={topOpponent}
            isCurrentTurn={view?.currentTurn === topOpponent.id}
            orientation="top"
            sonderkarteNotif={sonderkarteNotification?.player === topOpponent.id ? sonderkarteNotification.type : null}
            onClick={allowPlayerSwitching ? () => onPlayerSwitch(topOpponent.id) : undefined}
          />
        )}
        left={leftOpponent ? (
          <PlayerLabel
            player={leftOpponent}
            isCurrentTurn={view?.currentTurn === leftOpponent.id}
            orientation="left"
            sonderkarteNotif={sonderkarteNotification?.player === leftOpponent.id ? sonderkarteNotification.type : null}
            onClick={allowPlayerSwitching ? () => onPlayerSwitch(leftOpponent.id) : undefined}
          />
        ) : <div />}
        center={
          <CenterArea
            requestingPlayer={activePlayer}
            seatOf={seatOfPlayer}
            displayTrick={displayTrick}
            animPhase={animPhase}
            winnerSeat={winnerSeat}
          />
        }
        right={rightOpponent ? (
          <PlayerLabel
            player={rightOpponent}
            isCurrentTurn={view?.currentTurn === rightOpponent.id}
            orientation="right"
            sonderkarteNotif={sonderkarteNotification?.player === rightOpponent.id ? sonderkarteNotification.type : null}
            onClick={allowPlayerSwitching ? () => onPlayerSwitch(rightOpponent.id) : undefined}
          />
        ) : <div />}
        bottom={actions.actionError && (
          <div className="bg-red-500/20 text-red-300 text-sm px-4 py-2 rounded-lg mx-4">
            {actions.actionError}
          </div>
        )}
      />

      {/* Last trick button — bottom-left, shifted toward center to avoid player label */}
      {view && view.completedTricks.length > 0 && (
        <div className="absolute bottom-[20%] left-[10%] z-10">
          <button
            onClick={() => setShowLastTrick(true)}
            className="rounded-xl bg-gray-700/80 hover:bg-gray-600/90 active:bg-gray-800 shadow-lg transition-colors p-1.5"
            title="Letzter Stich"
          >
            <img src={cardBackSvgPath} alt="" className="w-7 h-auto block" />
          </button>
        </div>
      )}

      {/* Floating announcement button — bottom-right at ~20% from bottom */}
      <div className="absolute bottom-[20%] right-4 z-10">
        <AnnouncementButton
          legalAnnouncements={view?.legalAnnouncements ?? []}
          ownParty={view?.ownParty ?? null}
          onAnnounce={actions.handleAnnouncement}
        />
      </div>

      {/* Loading / error feedback above the hand */}
      {viewLoading && !view && <div className="text-center text-white/40 text-xs py-1">{t.loading}</div>}
      {viewError && <div className="text-center text-red-400 text-xs py-1">{viewError}</div>}

      {/* Hand — .hand-container clips the bottom half of the cards so they
           appear to rise from below the table edge (see HandDisplay.css) */}
      <div className="hand-container">
        {view && (
          <HandDisplay
            cards={view.handSorted}
            legalCardIds={legalCardIds}
            isMyTurn={view.isMyTurn}
            onCardClick={actions.handleCardClick}
            selectionMode={view.shouldReturnArmutCards}
            selectedCardIds={actions.armutReturnSelected}
            maxSelection={view.armutCardReturnCount ?? undefined}
            playingCardId={actions.playingCardId}
          />
        )}
      </div>

      {/* Dialog overlays: centered, top-anchored, hover over hand */}
      {view?.shouldDeclareHealth && (
        <div className="absolute left-1/2 -translate-x-1/2 top-[20%] z-20">
          <HealthCheckDialog
            playerId={activePlayer}
            onDeclare={actions.handleHealthCheck}
          />
        </div>
      )}

      {view?.shouldDeclareReservation && (
        <div className="absolute left-1/2 -translate-x-1/2 top-[20%] z-20 w-[calc(100%-2rem)] max-w-sm">
          <ReservationDialog
            playerId={activePlayer}
            eligibleReservations={view.eligibleReservations}
            mustDeclare={view.mustDeclareReservation}
            onDeclare={actions.handleReservation}
          />
        </div>
      )}

      {view?.shouldRespondToArmut && (
        <div className="absolute left-1/2 -translate-x-1/2 top-[20%] z-20">
          <ArmutPartnerDialog
            playerId={activePlayer}
            onRespond={actions.handleArmutResponse}
          />
        </div>
      )}

      {view?.shouldReturnArmutCards && view.armutCardReturnCount !== null && (
        <div className="absolute left-1/2 -translate-x-1/2 top-[20%] z-20">
          <ArmutReturnDialog
            playerId={activePlayer}
            cardReturnCount={view.armutCardReturnCount}
            selectedCount={actions.armutReturnSelected.size}
            onConfirm={() => actions.handleArmutExchange(Array.from(actions.armutReturnSelected))}
          />
        </div>
      )}

      {/* Game info overlay: match history if available, otherwise empty state */}
      {showInfoOverlay && (
        <ResultScreen
          result={lastFinishedResult ?? undefined}
          onNewGame={() => setShowInfoOverlay(false)}
          viewOnly
          onLeaveLobby={onLeaveLobby ? triggerLeave : undefined}
        />
      )}

      {/* Leave confirmation dialog */}
      {showLeaveConfirm && (
        <div className="fixed inset-0 bg-black/80 flex items-center justify-center z-60 p-4">
          <div className="bg-gray-800 rounded-2xl p-6 w-full max-w-xs shadow-2xl flex flex-col gap-4 text-white text-center">
            <p className="font-semibold text-base">Willst du die Lobby wirklich verlassen?</p>
            <p className="text-white/50 text-sm">Dein Platz wird frei und kann von anderen besetzt werden.</p>
            <div className="flex flex-col gap-2">
              <button
                onClick={confirmLeave}
                className="w-full py-2.5 rounded-xl bg-red-700 hover:bg-red-600 active:bg-red-800 text-white font-semibold transition-colors"
              >
                Ja, verlassen
              </button>
              <button
                onClick={() => setShowLeaveConfirm(false)}
                className="w-full py-2.5 rounded-xl bg-gray-600 hover:bg-gray-500 active:bg-gray-700 text-white font-semibold transition-colors"
              >
                Nein, weiterspielen
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Last trick overlay */}
      {showLastTrick && view && view.completedTricks.length > 0 && (
        <LastTrickOverlay
          trick={view.completedTricks[view.completedTricks.length - 1]}
          seatOf={seatOfPlayer}
          onClose={() => setShowLastTrick(false)}
        />
      )}

      {/* Full-screen overlays: Sonderkarte confirmation + result screen */}
      <GameOverlays
        pendingCard={actions.pendingCard}
        activePlayer={activePlayer}
        finishedResult={finishedResult}
        onSubmitPlayCard={actions.submitPlayCard}
        onCancelPendingCard={() => actions.setPendingCard(null)}
        onNewGame={onNewGame}
        multiplayerNewGame={multiplayerNewGame}
        onLeaveLobby={onLeaveLobby ? triggerLeave : undefined}
      />
    </div>
  );
}
