import { useState } from 'react';
import type { PlayerGameViewResponse, GameResultDto, TrickSummaryDto, SonderkarteNotification } from '../../types/api';
import type { AnimPhase } from '../TrickArea/TrickArea';
import type { GameActions } from '../../hooks/useGameActions';
import type { MultiplayerNewGameProps } from '../ResultScreen/ResultScreen';
import { GameInfo } from '../shared/GameInfo';
import { GameInfoOverlay } from '../shared/GameInfoOverlay';
import { GeschmissenResultScreen } from '../GeschmissenResultScreen/GeschmissenResultScreen';
import { PlayerLabel } from '../shared/PlayerLabel';
import { HandDisplay } from '../HandDisplay/HandDisplay';
import { AnnouncementButton } from '../AnnouncementButton/AnnouncementButton';
import { ArmutBanner } from './subcomponents/ArmutBanner';
import { CenterArea } from './subcomponents/CenterArea';
import { PlayerGrid } from './subcomponents/PlayerGrid';
import { GameOverlays } from './subcomponents/GameOverlays';
import { HealthCheckDialog } from '../dialogs/HealthCheckDialog';
import { ReservationDialog } from '../dialogs/ReservationDialog';
import { ArmutPartnerDialog } from '../dialogs/ArmutPartnerDialog';
import { ArmutReturnDialog } from '../dialogs/ArmutReturnDialog';
import { t } from '../../translations';

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
  multiplayerGeschmissenNewGame?: MultiplayerNewGameProps;
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
  multiplayerGeschmissenNewGame,
}: GameBoardProps) {
  const [showInfoOverlay, setShowInfoOverlay] = useState(false);
  const seatOfPlayer = (player: number) => seatOf(player, activePlayer);

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

      {/* Floating announcement button — bottom-left at ~20% from bottom */}
      <div className="absolute bottom-[20%] left-4 z-10">
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

      {/* Schmeißen result screen */}
      {view?.phase === 'Geschmissen' && (
        <GeschmissenResultScreen
          lobbyStandings={view.lobbyStandings}
          activePlayer={activePlayer}
          onNewGame={onNewGame}
          multiplayerNewGame={multiplayerGeschmissenNewGame}
        />
      )}

      {/* Game info overlay: game details + lobby standings */}
      {showInfoOverlay && view && (
        <GameInfoOverlay
          phase={view.phase}
          gameMode={view.activeGameMode}
          trickNumber={(view.currentTrick?.trickNumber ?? 0) + 1}
          completedTricks={view.completedTricks.length}
          lobbyStandings={view.lobbyStandings}
          activePlayer={activePlayer}
          onClose={() => setShowInfoOverlay(false)}
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
      />
    </div>
  );
}
