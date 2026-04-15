import { useEffect, useState } from 'react';
import { useHotSeat } from './hooks/useHotSeat';
import { useGameState } from './hooks/useGameState';
import { useTrickAnimation } from './hooks/useTrickAnimation';
import { useGameActions } from './hooks/useGameActions';
import { useLobby, saveLobbySession, loadLobbySession, clearLobbySession } from './hooks/useLobby';
import { createLobby, joinLobby } from './api/lobby';
import { GameBoard } from './components/GameBoard/GameBoard';
import { GameLoader } from './components/GameLoader/GameLoader';
import { LandingPage } from './components/LandingPage/LandingPage';
import { LobbyPage } from './components/LobbyPage/LobbyPage';
import { PortraitOverlay } from './components/PortraitOverlay/PortraitOverlay';
import { t } from './translations';
import type { LobbySession } from './hooks/useLobby';
import type { HotSeatSession } from './hooks/useHotSeat';

type AppView =
  | { kind: 'home' }
  | { kind: 'joining'; lobbyId: string }
  | { kind: 'lobby'; session: LobbySession }
  | { kind: 'hot-seat' }
  | { kind: 'game'; tokens: string[]; gameId: string; myPlayerId: number };

function detectInitialView(): AppView {
  const params = new URLSearchParams(window.location.search);
  const lobbyId = params.get('lobby');
  if (lobbyId) {
    const stored = loadLobbySession(lobbyId);
    if (stored) return { kind: 'lobby', session: stored };
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

  const [view, setView] = useState<AppView>(detectInitialView);
  const [joinError, setJoinError] = useState<string | null>(null);

  // Auto-join when arriving via invite URL without an existing session
  const joiningLobbyId = view.kind === 'joining' ? view.lobbyId : null;
  useEffect(() => {
    if (!joiningLobbyId) return;
    let cancelled = false;
    setJoinError(null);

    joinLobby(joiningLobbyId)
      .then((res) => {
        if (cancelled) return;
        const session: LobbySession = {
          lobbyId: res.lobbyId,
          token: res.token,
          playerId: res.playerId,
          isHost: false,
        };
        saveLobbySession(session);
        window.history.replaceState({}, '', `?lobby=${res.lobbyId}`);
        setView({ kind: 'lobby', session });
      })
      .catch((e: unknown) => {
        if (!cancelled) setJoinError(e instanceof Error ? e.message : String(e));
      });

    return () => { cancelled = true; };
  }, [joiningLobbyId]);

  async function handleCreateLobby() {
    try {
      const res = await createLobby();
      const session: LobbySession = {
        lobbyId: res.lobbyId,
        token: res.token,
        playerId: res.playerId,
        isHost: true,
      };
      saveLobbySession(session);
      window.history.pushState({}, '', `?lobby=${res.lobbyId}`);
      setView({ kind: 'lobby', session });
    } catch (e) {
      console.error('Failed to create lobby', e);
    }
  }

  function handleGameStarted(gameId: string) {
    if (view.kind !== 'lobby') return;
    const { token, playerId } = view.session;
    // Each slot holds the player's own token; activePlayer is locked to myPlayerId.
    const tokens = Array<string>(4).fill(token);
    clearLobbySession();
    window.history.replaceState({}, '', window.location.pathname);
    setView({ kind: 'game', tokens, gameId, myPlayerId: playerId });
  }

  // Lobby SignalR subscription (only active when in lobby view)
  const lobbySession = view.kind === 'lobby' ? view.session : null;
  const { playerCount, gameId: lobbyGameId } = useLobby(lobbySession);

  // Transition to game when backend broadcasts gameStarted
  useEffect(() => {
    if (lobbyGameId && view.kind === 'lobby') {
      handleGameStarted(lobbyGameId);
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [lobbyGameId]);

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
    refetch,
  } = useGameState(gameSession?.tokens ?? [], gameSession?.gameId ?? null, activePlayer);

  const { animTrick, animPhase } = useTrickAnimation(gameView);

  const actions = useGameActions(gameSession, activePlayer, gameView, refetch);

  // ── Render ─────────────────────────────────────────────────────────────────

  if (view.kind === 'home') {
    return (
      <>
        <PortraitOverlay />
        <LandingPage
          onCreateLobby={handleCreateLobby}
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

  if (view.kind === 'lobby') {
    return (
      <>
        <PortraitOverlay />
        <LobbyPage
          session={view.session}
          playerCount={playerCount}
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
      />
    </>
  );
}
