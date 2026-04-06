import { useState } from 'react';
import { useHotSeat } from './hooks/useHotSeat';
import { useGameState } from './hooks/useGameState';
import { playCard, makeReservation, makeAnnouncement } from './api/game';
import { PlayerSwitcher } from './components/PlayerSwitcher';
import { GameInfo } from './components/GameInfo';
import { PlayerLabel } from './components/PlayerLabel';
import { TrickArea } from './components/TrickArea';
import { HandDisplay } from './components/HandDisplay';
import { ReservationDialog } from './components/ReservationDialog';
import { SonderkarteOverlay } from './components/SonderkarteOverlay';
import { AnnouncementButton } from './components/AnnouncementButton';
import { ResultScreen } from './components/ResultScreen';
import type { CardDto, SonderkarteInfoDto } from './types/api';

function App() {
  const { session, activePlayer, error: initError, loading: initLoading, setActivePlayer, restart } = useHotSeat();

  const { view, loading: viewLoading, error: viewError, finishedResult, refetch } = useGameState(
    session?.tokens ?? [],
    session?.gameId ?? null,
    activePlayer,
  );

  // Pending card play — waiting for sonderkarte selection
  const [pendingCard, setPendingCard] = useState<{ card: CardDto; sonderkarten: SonderkarteInfoDto[] } | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);

  if (!session) {
    return (
      <div className="w-full h-full flex items-center justify-center">
        {initLoading && <p className="text-white/60 text-lg">Starting game…</p>}
        {initError && (
          <div className="text-center">
            <p className="text-red-400 mb-4">{initError}</p>
            <button onClick={restart} className="bg-indigo-500 text-white px-6 py-2 rounded-lg">Retry</button>
          </div>
        )}
      </div>
    );
  }

  const { tokens, gameId } = session;
  const token = tokens[activePlayer];

  async function handleCardClick(card: CardDto) {
    if (!view) return;
    const eligibleSk = view.eligibleSonderkartenPerCard[card.id] ?? [];
    if (eligibleSk.length > 0) {
      setPendingCard({ card, sonderkarten: eligibleSk });
      return;
    }
    await submitPlayCard(card.id, [], null);
  }

  async function submitPlayCard(cardId: number, activateSonderkarten: string[], genscherPartnerId: number | null) {
    setPendingCard(null);
    setActionError(null);
    try {
      await playCard(token, gameId, { cardId, activateSonderkarten, genscherPartnerId });
      refetch();
    } catch (e) {
      setActionError(e instanceof Error ? e.message : String(e));
    }
  }

  async function handleReservation(reservation: string | null, hochzeitCondition: string | null, armutPartner: number | null) {
    setActionError(null);
    try {
      await makeReservation(token, gameId, { reservation, hochzeitCondition, armutPartner });
      // Auto-advance to next player who hasn't declared yet
      const next = (activePlayer + 1) % 4;
      setActivePlayer(next);
      refetch();
    } catch (e) {
      setActionError(e instanceof Error ? e.message : String(e));
    }
  }

  async function handleAnnouncement(type: string) {
    setActionError(null);
    try {
      await makeAnnouncement(token, gameId, { type });
      refetch();
    } catch (e) {
      setActionError(e instanceof Error ? e.message : String(e));
    }
  }

  // Compass seat helper: given a player ID, what direction relative to active player?
  function seatOf(player: number): 'bottom' | 'left' | 'top' | 'right' {
    const seats = ['bottom', 'left', 'top', 'right'] as const;
    const offset = (player - activePlayer + 4) % 4;
    return seats[offset];
  }

  // Find opponents from view (excludes requesting player)
  const opponents = view?.otherPlayers ?? [];
  const topOpponent = opponents.find((p) => seatOf(p.id) === 'top');
  const leftOpponent = opponents.find((p) => seatOf(p.id) === 'left');
  const rightOpponent = opponents.find((p) => seatOf(p.id) === 'right');

  const legalCardIds = new Set(view?.legalCards.map((c) => c.id) ?? []);

  return (
    <div className="w-full h-full flex flex-col bg-[#1a1a2e] select-none overflow-hidden">
      {/* Top bar */}
      <div className="flex items-center justify-between px-4 py-2 bg-black/30">
        {view ? (
          <GameInfo
            phase={view.phase}
            trickNumber={(view.currentTrick?.trickNumber ?? 0) + 1}
            completedTricks={view.completedTricks.length}
          />
        ) : (
          <div />
        )}
        <PlayerSwitcher activePlayer={activePlayer} onSwitch={setActivePlayer} />
      </div>

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

        {/* Middle row: left opponent | trick | right opponent */}
        <div className="flex items-center justify-between w-full px-6">
          {leftOpponent ? (
            <PlayerLabel
              player={leftOpponent}
              isCurrentTurn={view?.currentTurn === leftOpponent.id}
              orientation="left"
            />
          ) : <div />}

          <TrickArea
            trick={view?.currentTrick ?? null}
            requestingPlayer={activePlayer}
            seatOf={seatOf}
          />

          {rightOpponent ? (
            <PlayerLabel
              player={rightOpponent}
              isCurrentTurn={view?.currentTurn === rightOpponent.id}
              orientation="right"
            />
          ) : <div />}
        </div>

        {/* Announcement button */}
        {view?.isMyTurn && (
          <AnnouncementButton
            legalAnnouncements={view.legalAnnouncements}
            onAnnounce={handleAnnouncement}
          />
        )}

        {/* Action error */}
        {actionError && (
          <div className="bg-red-500/20 text-red-300 text-sm px-4 py-2 rounded-lg mx-4">
            {actionError}
          </div>
        )}
      </div>

      {/* Hand */}
      <div className="pb-2">
        {view && (
          <HandDisplay
            cards={view.handSorted}
            legalCardIds={legalCardIds}
            isMyTurn={view.isMyTurn}
            eligibleSonderkarten={view.eligibleSonderkartenPerCard}
            onCardClick={handleCardClick}
          />
        )}
        {viewLoading && <div className="text-center text-white/40 text-xs py-1">Loading…</div>}
        {viewError && <div className="text-center text-red-400 text-xs py-1">{viewError}</div>}
      </div>

      {/* Overlays */}
      {view?.phase === 'Reservations' && view.isMyTurn && (
        <ReservationDialog
          playerId={activePlayer}
          eligibleReservations={view.eligibleReservations}
          onDeclare={handleReservation}
        />
      )}

      {pendingCard && (
        <SonderkarteOverlay
          sonderkarten={pendingCard.sonderkarten}
          onConfirm={(selected, genscherPartnerId) =>
            submitPlayCard(pendingCard.card.id, selected, genscherPartnerId)
          }
          onCancel={() => setPendingCard(null)}
        />
      )}

      {finishedResult && (
        <ResultScreen result={finishedResult} onNewGame={restart} />
      )}
    </div>
  );
}

export default App;
