import type { PlayerGameViewResponse, GameResultDto, TrickSummaryDto } from '../../types/api';
import type { AnimPhase } from '../TrickArea/TrickArea';
import type { GameActions } from '../../hooks/useGameActions';
import { GameInfo } from '../shared/GameInfo';
import { PlayerSwitcher } from '../shared/PlayerSwitcher';
import { PlayerLabel } from '../shared/PlayerLabel';
import { HandDisplay } from '../HandDisplay/HandDisplay';
import { AnnouncementButton } from '../AnnouncementButton/AnnouncementButton';
import { SonderkarteOverlay } from '../SonderkarteOverlay/SonderkarteOverlay';
import { ResultScreen } from '../ResultScreen/ResultScreen';
import { ArmutBanner } from './subcomponents/ArmutBanner';
import { CenterArea } from './subcomponents/CenterArea';
import { t } from '../../translations';

interface GameBoardProps {
  view: PlayerGameViewResponse | null;
  activePlayer: number;
  animTrick: TrickSummaryDto | null;
  animPhase: AnimPhase;
  actions: GameActions;
  finishedResult: GameResultDto | null;
  viewLoading: boolean;
  viewError: string | null;
  onPlayerSwitch: (player: number) => void;
  onNewGame: () => void;
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
  viewLoading,
  viewError,
  onPlayerSwitch,
  onNewGame,
}: GameBoardProps) {
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
      {/* Floating top-left: game info */}
      {view && (
        <div className="absolute top-2 left-2 z-10">
          <GameInfo
            phase={view.phase}
            trickNumber={(view.currentTrick?.trickNumber ?? 0) + 1}
            completedTricks={view.completedTricks.length}
          />
        </div>
      )}

      {/* Floating top-right: player switcher */}
      <div className="absolute top-2 right-2 z-10">
        <PlayerSwitcher activePlayer={activePlayer} onSwitch={onPlayerSwitch} />
      </div>

      {/* Armut exchange info banner */}
      {view?.armutExchangeCardCount != null && view.armutReturnedTrump != null && (
        <ArmutBanner
          exchangeCardCount={view.armutExchangeCardCount}
          returnedTrump={view.armutReturnedTrump}
        />
      )}

      {/* Main game area */}
      <div className="flex-1 relative flex flex-col items-center justify-between py-2">
        {/* Top opponent */}
        <div className="flex justify-center">
          {topOpponent && (
            <PlayerLabel
              player={topOpponent}
              isCurrentTurn={view?.currentTurn === topOpponent.id}
              orientation="top"
            />
          )}
        </div>

        {/* Middle row: left opponent | center | right opponent */}
        <div className="flex items-center justify-between w-full px-6">
          {leftOpponent ? (
            <PlayerLabel
              player={leftOpponent}
              isCurrentTurn={view?.currentTurn === leftOpponent.id}
              orientation="left"
            />
          ) : <div />}

          <CenterArea
            view={view}
            activePlayer={activePlayer}
            seatOf={seatOfPlayer}
            displayTrick={displayTrick}
            animPhase={animPhase}
            winnerSeat={winnerSeat}
            actions={actions}
          />

          {rightOpponent ? (
            <PlayerLabel
              player={rightOpponent}
              isCurrentTurn={view?.currentTurn === rightOpponent.id}
              orientation="right"
            />
          ) : <div />}
        </div>

        {/* Action error */}
        {actions.actionError && (
          <div className="bg-red-500/20 text-red-300 text-sm px-4 py-2 rounded-lg mx-4">
            {actions.actionError}
          </div>
        )}
      </div>

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

      {/* Overlays */}
      {actions.pendingCard && (
        <SonderkarteOverlay
          sonderkarten={actions.pendingCard.sonderkarten}
          onConfirm={(selected, genscherPartnerId) =>
            actions.submitPlayCard(actions.pendingCard!.card.id, selected, genscherPartnerId)
          }
          onCancel={() => actions.setPendingCard(null)}
        />
      )}

      {finishedResult && (
        <ResultScreen result={finishedResult} onNewGame={onNewGame} />
      )}
    </div>
  );
}
