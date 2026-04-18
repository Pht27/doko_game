import { useEffect, useState } from 'react';
import { useHotSeat } from './hooks/useHotSeat';
import { useGameState } from './hooks/useGameState';
import { useTrickAnimation } from './hooks/useTrickAnimation';
import { useGameActions } from './hooks/useGameActions';
import { saveLobbySession, loadLobbySession } from './hooks/useLobby';
import { joinSeat, getLobby, voteNewGame, withdrawNewGame, voteNewGameGeschmissen } from './api/lobby';
import { GameBoard } from './components/GameBoard/GameBoard';
import { GameLoader } from './components/GameLoader/GameLoader';
import { LandingPage } from './components/LandingPage/LandingPage';
import { MultiplayerBrowserPage } from './components/MultiplayerBrowserPage/MultiplayerBrowserPage';
import { PortraitOverlay } from './components/PortraitOverlay/PortraitOverlay';
import { t } from './translations';
import type { LobbySession } from './hooks/useLobby';
import type { HotSeatSession } from './hooks/useHotSeat';

type AppView =
  | { kind: 'home' }
  | { kind: 'joining'; lobbyId: string }
  | { kind: 'multiplayer-browser'; selectedLobbyId?: string }
  | { kind: 'hot-seat' }
  | { kind: 'game'; tokens: string[]; gameId: string; myPlayerId: number; lobbySession?: LobbySession };

function detectInitialView(): AppView {
  const params = new URLSearchParams(window.location.search);
  const lobbyId = params.get('lobby');
  if (lobbyId) {
    const stored = loadLobbySession(lobbyId);
    if (stored) return { kind: 'multiplayer-browser', selectedLobbyId: lobbyId };
    return { kind: 'joining', lobbyId };
  }
  return { kind: 'home' };
}

export default function App() {
  useEffect(() => {
    screen.orientation?.lock('landscape').catch(() => {
      // Not supported on iOS Safari — PortraitOverlay handles that case
    });
  }, []);

  useEffect(() => {
    const isPwa =
      window.matchMedia('(display-mode: standalone)').matches ||
      window.matchMedia('(display-mode: fullscreen)').matches ||
      (window.navigator as { standalone?: boolean }).standalone === true;

    if (!isPwa) return;

    function enterFullscreen() {
      if (!document.fullscreenElement) {
        document.documentElement.requestFullscreen().catch(() => {});
      }
    }

    window.addEventListener('click', enterFullscreen, { once: true });
    window.addEventListener('touchstart', enterFullscreen, { once: true });
    return () => {
      window.removeEventListener('click', enterFullscreen);
      window.removeEventListener('touchstart', enterFullscreen);
    };
  }, []);

  const [view, setView] = useState<AppView>(detectInitialView);
  const [joinError, setJoinError] = useState<string | null>(null);

  // Auto-join when arriving via invite URL without an existing session
  const joiningLobbyId = view.kind === 'joining' ? view.lobbyId : null;
  useEffect(() => {
    if (!joiningLobbyId) return;
    let cancelled = false;
    setJoinError(null);

    async function autoJoin() {
      try {
        // Find first available seat
        const lobbyView = await getLobby(joiningLobbyId!);
        if (cancelled) return;

        const seatIndex = lobbyView.seats.findIndex((occupied) => !occupied);
        if (seatIndex === -1) {
          if (!cancelled) setJoinError(t.lobbyFull);
          return;
        }

        const res = await joinSeat(joiningLobbyId!, seatIndex);
        if (cancelled) return;

        const session: LobbySession = {
          lobbyId: res.lobbyId,
          token: res.token,
          playerId: res.playerId,
          seatIndex: res.seatIndex,
        };
        saveLobbySession(session);
        window.history.replaceState({}, '', `?lobby=${res.lobbyId}`);
        setView({ kind: 'multiplayer-browser', selectedLobbyId: res.lobbyId });
      } catch (e: unknown) {
        if (!cancelled) setJoinError(e instanceof Error ? e.message : String(e));
      }
    }

    autoJoin();
    return () => { cancelled = true; };
  }, [joiningLobbyId]);

  function handleGameStarted(gameId: string, session: LobbySession) {
    const { token, playerId } = session;
    const tokens = Array<string>(4).fill(token);
    window.history.replaceState({}, '', window.location.pathname);
    setView({ kind: 'game', tokens, gameId, myPlayerId: playerId, lobbySession: session });
  }

  // ── Game wiring (hot-seat and multiplayer share the same hooks) ─────────────

  const isHotSeat = view.kind === 'hot-seat';
  const hotSeat = useHotSeat(isHotSeat);
  const isGame = view.kind === 'game';

  const gameSession: HotSeatSession | null =
    isGame
      ? { tokens: view.tokens, gameId: view.gameId }
      : isHotSeat
        ? (hotSeat.session ?? null)
        : null;

  const activePlayer = isGame ? view.myPlayerId : isHotSeat ? hotSeat.activePlayer : 0;

  const {
    view: gameView,
    loading: viewLoading,
    error: viewError,
    finishedResult,
    sonderkarteNotification,
    newGameVoteCount,
    newGameId,
    refetch,
  } = useGameState(gameSession?.tokens ?? [], gameSession?.gameId ?? null, activePlayer);

  // When the backend auto-starts a new game, update the active game ID
  useEffect(() => {
    if (!newGameId) return;
    setView(prev => prev.kind === 'game' ? { ...prev, gameId: newGameId } : prev);
  }, [newGameId]);

  const { animTrick, animPhase } = useTrickAnimation(gameView);

  const actions = useGameActions(gameSession, activePlayer, gameView, refetch);

  // ── Render ─────────────────────────────────────────────────────────────────

  if (view.kind === 'home') {
    return (
      <>
        <PortraitOverlay />
        <LandingPage
          onMultiplayer={() => setView({ kind: 'multiplayer-browser' })}
          onTestGame={() => setView({ kind: 'hot-seat' })}
        />
      </>
    );
  }

  if (view.kind === 'joining') {
    return (
      <>
        <PortraitOverlay />
        <div className="w-full h-full flex items-center justify-center">
          {joinError
            ? <p className="text-red-400 text-lg">{joinError}</p>
            : <p className="text-white/60 text-lg">{t.joiningLobby}</p>}
        </div>
      </>
    );
  }

  if (view.kind === 'multiplayer-browser') {
    return (
      <>
        <PortraitOverlay />
        <MultiplayerBrowserPage
          selectedLobbyId={view.selectedLobbyId}
          onBack={() => setView({ kind: 'home' })}
          onSelectLobby={(lobbyId) => setView({ kind: 'multiplayer-browser', selectedLobbyId: lobbyId })}
          onGameStarted={handleGameStarted}
        />
      </>
    );
  }

  // hot-seat or game: show loader until session is ready
  if (!gameSession) {
    return (
      <>
        <PortraitOverlay />
        <GameLoader
          loading={isHotSeat ? hotSeat.loading : false}
          error={isHotSeat ? hotSeat.error : null}
          onRetry={isHotSeat ? hotSeat.restart : () => setView({ kind: 'home' })}
        />
      </>
    );
  }

  return (
    <>
      <PortraitOverlay />
      <GameBoard
        view={gameView}
        activePlayer={activePlayer}
        animTrick={animTrick}
        animPhase={animPhase}
        actions={actions}
        finishedResult={finishedResult}
        sonderkarteNotification={sonderkarteNotification}
        viewLoading={viewLoading}
        viewError={viewError}
        allowPlayerSwitching={isHotSeat}
        onPlayerSwitch={isHotSeat ? hotSeat.setActivePlayer : () => {}}
        onNewGame={isHotSeat ? hotSeat.restart : () => setView({ kind: 'home' })}
        multiplayerNewGame={
          isGame && view.lobbySession
            ? {
                voteCount: newGameVoteCount,
                mySeatIndex: view.lobbySession.seatIndex,
                onVote: () => voteNewGame(view.lobbySession!.token, view.lobbySession!.lobbyId),
                onWithdraw: () => withdrawNewGame(view.lobbySession!.token, view.lobbySession!.lobbyId),
              }
            : undefined
        }
        multiplayerGeschmissenNewGame={
          isGame && view.lobbySession
            ? {
                voteCount: newGameVoteCount,
                mySeatIndex: view.lobbySession.seatIndex,
                onVote: () => voteNewGameGeschmissen(view.lobbySession!.token, view.lobbySession!.lobbyId),
                onWithdraw: () => withdrawNewGame(view.lobbySession!.token, view.lobbySession!.lobbyId),
              }
            : undefined
        }
      />
    </>
  );
}
