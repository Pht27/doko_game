import { useState, useEffect, useRef } from 'react';
import type { PlayerGameViewResponse, GameResultDto, TrickSummaryDto, SonderkarteNotification } from '@/types/api';
import type { AnimPhase } from '../TrickArea/TrickArea';
import type { GameActions } from '@/hooks/useGameActions';
import type { MultiplayerNewGameProps } from '../ResultScreen/ResultScreen';
import { ResultScreen } from '../ResultScreen/ResultScreen';
import { GameModeBadge } from '../GameModeBadge/GameModeBadge';
import { BurgerMenu } from '../BurgerMenu/BurgerMenu';
import { TitleCard } from '../TitleCard/TitleCard';
import { PlayerLabel } from '../shared/PlayerLabel';
import { SelfPlayerLabel } from '../shared/SelfPlayerLabel';
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
import { SchwarzesSauSoloDialog } from '../dialogs/SchwarzesSauSoloDialog';
import { t } from '@/utils/translations';

interface GameBoardProps {
  view: PlayerGameViewResponse | null;
  activePlayer: number;
  animTrick: TrickSummaryDto | null;
  animPhase: AnimPhase;
  actions: GameActions;
  finishedResult: GameResultDto | null;
  sonderkarteNotification: SonderkarteNotification | null;
  activeSonderkarten: SonderkarteNotification[];
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
  const seats = ['bottom', 'right', 'top', 'left'] as const;
  return seats[(player - activePlayer + 4) % 4];
}

/** Find the Re partner seat: the Re player who is NOT the declarer. */
function getPartnerSeat(view: PlayerGameViewResponse): number | null {
  const declarerSeat = view.gameModePlayerSeat ?? null;
  if (declarerSeat === null) return null;
  const reSeats: number[] = [];
  if (view.ownParty === 'Re') reSeats.push(view.requestingPlayer);
  for (const p of view.otherPlayers) {
    if (p.knownParty === 'Re') reSeats.push(p.id);
  }
  return reSeats.find(s => s !== declarerSeat) ?? null;
}

export function GameBoard({
  view,
  activePlayer,
  animTrick,
  animPhase,
  actions,
  finishedResult,
  sonderkarteNotification,
  activeSonderkarten,
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

  // Title card: shown when phase first transitions to Playing
  const [titleCardKey, setTitleCardKey] = useState<number | null>(null);
  const [titleCardData, setTitleCardData] = useState<{
    gameMode: string | null;
    declarerSeat: number | null;
    partnerSeat: number | null;
  } | null>(null);
  const prevPhaseRef = useRef<string | undefined>(undefined);

  useEffect(() => {
    if (!view) return;
    if (
      view.phase === 'Playing' &&
      prevPhaseRef.current !== undefined &&
      prevPhaseRef.current !== 'Playing'
    ) {
      setTitleCardData({
        gameMode: view.activeGameMode,
        declarerSeat: view.gameModePlayerSeat ?? null,
        partnerSeat: getPartnerSeat(view),
      });
      setTitleCardKey(Date.now());
    }
    prevPhaseRef.current = view.phase;
  }, [view?.phase]); // eslint-disable-line react-hooks/exhaustive-deps

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

  const showHealthStatus = view?.phase !== undefined && view.phase !== 'Playing' && view.phase !== 'Scoring' && view.phase !== 'Finished' && view.phase !== 'Geschmissen' && view.phase !== 'Dealing';

  const legalCardIds = new Set(view?.legalCards.map((c) => c.id) ?? []);

  const trickCountByPlayer = (view?.completedTricks ?? []).reduce<Record<number, number>>((acc, trick) => {
    if (trick.winner != null) acc[trick.winner] = (acc[trick.winner] ?? 0) + 1;
    return acc;
  }, {});

  return (
    <div className="w-full h-full relative flex flex-col bg-[#1a1a2e] select-none overflow-hidden">
      {/* Burger menu — top-left, opens match history overlay */}
      {view && (
        <BurgerMenu onClick={() => setShowInfoOverlay(true)} />
      )}

      {/* Game mode badge — top-right, pill with mode/players/tricks/sonderkarten */}
      {view && (
        <div className="absolute top-2 right-2 z-10">
          <GameModeBadge
            gameMode={view.activeGameMode}
            declarerSeat={view.gameModePlayerSeat ?? null}
            partnerSeat={view.phase === 'Playing' ? getPartnerSeat(view) : null}
            trickNumber={(view.currentTrick?.trickNumber ?? 0) + 1}
            totalTricks={view.handSorted.length + view.completedTricks.length}
            activeSonderkarten={activeSonderkarten}
            isSchwarzesSau={view.isSchwarzesSau}
            phase={view.phase}
          />
        </div>
      )}

      {/* Title card: animated center announcement when game mode is determined */}
      {titleCardKey !== null && titleCardData && (
        <TitleCard
          key={titleCardKey}
          gameMode={titleCardData.gameMode}
          declarerSeat={titleCardData.declarerSeat}
          partnerSeat={titleCardData.partnerSeat}
          onDone={() => setTitleCardKey(null)}
        />
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
            trickCount={trickCountByPlayer[topOpponent.id] ?? 0}
            showHealthStatus={showHealthStatus}
            onClick={allowPlayerSwitching ? () => onPlayerSwitch(topOpponent.id) : undefined}
          />
        )}
        left={leftOpponent ? (
          <PlayerLabel
            player={leftOpponent}
            isCurrentTurn={view?.currentTurn === leftOpponent.id}
            orientation="left"
            sonderkarteNotif={sonderkarteNotification?.player === leftOpponent.id ? sonderkarteNotification.type : null}
            trickCount={trickCountByPlayer[leftOpponent.id] ?? 0}
            showHealthStatus={showHealthStatus}
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
            trickCount={trickCountByPlayer[rightOpponent.id] ?? 0}
            showHealthStatus={showHealthStatus}
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

      {/* Own player label — bar glued to the bottom edge of the screen */}
      {view && (
        <div className="absolute bottom-0 left-0 right-0 z-10 flex justify-center">
          <SelfPlayerLabel
            playerId={activePlayer}
            trickCount={trickCountByPlayer[activePlayer] ?? 0}
            ownParty={view.ownParty}
            highestAnnouncement={view.ownHighestAnnouncement}
            isCurrentTurn={view.isMyTurn}
          />
        </div>
      )}

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
            sonderkarteCardIds={new Set(Object.keys(view.eligibleSonderkartenPerCard).map(Number))}
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

      {view?.shouldChooseSchwarzesSauSolo && view.eligibleSchwarzesSauSolos.length > 0 && (
        <div className="absolute left-1/2 -translate-x-1/2 top-[20%] z-20 w-[calc(100%-2rem)] max-w-sm">
          <SchwarzesSauSoloDialog
            playerId={activePlayer}
            eligibleSolos={view.eligibleSchwarzesSauSolos}
            onChoose={actions.handleSchwarzesSauSolo}
          />
        </div>
      )}

      {/* Game info overlay: match history (opened via burger menu) */}
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
