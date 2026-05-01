import { useEffect, useState } from 'react';
import { useHotSeat } from './hooks/useHotSeat';
import { useGameState } from './hooks/useGameState';
import { useTrickAnimation } from './hooks/useTrickAnimation';
import { useGameActions } from './hooks/useGameActions';
import { saveLobbySession, loadLobbySession, loadAnySession, clearLobbySession } from './hooks/useLobby';
import { usePlayerNameResolver } from './context/PlayerNamesContext';
import { joinSeat, getLobby, leaveLobby, voteNewGame, withdrawNewGame } from './api/lobby';
import { GameBoard } from '@/features/game/GameBoard/GameBoard';
import { GameLoader } from '@/features/game/GameLoader/GameLoader';
import { LandingPage } from '@/pages/LandingPage/LandingPage';
import { RulesPage } from '@/pages/RulesPage/RulesPage';
import { MultiplayerBrowserPage } from '@/features/lobby/MultiplayerBrowserPage/MultiplayerBrowserPage';
import { PortraitOverlay } from './components/PortraitOverlay/PortraitOverlay';
import { t } from '@/utils/translations';
import type { LobbySession } from './hooks/useLobby';
import type { HotSeatSession } from './hooks/useHotSeat';
import type { GameResultDto } from './types/api';

type AppView =
  | { kind: 'home' }
  | { kind: 'rules' }
  | { kind: 'joining'; lobbyId: string }
  | { kind: 'multiplayer-browser'; selectedLobbyId?: string }
  | { kind: 'hot-seat' }
  | { kind: 'game'; tokens: string[]; gameId: string; myPlayerId: number; lobbySession?: LobbySession };

function detectInitialView(): AppView {
  const params = new URLSearchParams(window.location.search);
  const lobbyId = params.get('lobby');
  if (lobbyId) {
    const stored = loadLobbySession(lobbyId);
    if (stored) {
      if (stored.activeGameId) {
        return {
          kind: 'game',
          tokens: Array<string>(4).fill(stored.token),
          gameId: stored.activeGameId,
          myPlayerId: stored.seatIndex,
          lobbySession: stored,
        };
      }
      return { kind: 'multiplayer-browser', selectedLobbyId: lobbyId };
    }
    return { kind: 'joining', lobbyId };
  }
  // No URL param — check for a stored game session (e.g. after reload while in-game)
  const anySession = loadAnySession();
  if (anySession?.activeGameId) {
    return {
      kind: 'game',
      tokens: Array<string>(4).fill(anySession.token),
      gameId: anySession.activeGameId,
      myPlayerId: anySession.seatIndex,
      lobbySession: anySession,
    };
  }
  return { kind: 'home' };
}

function orientationFor(view: AppView): 'portrait' | 'landscape' {
  return view.kind === 'home' || view.kind === 'rules' ? 'portrait' : 'landscape';
}

export default function App() {
  const [view, setView] = useState<AppView>(detectInitialView);
  const [joinError, setJoinError] = useState<string | null>(null);

  const orientation = orientationFor(view);

  // Only show the portrait overlay when the lock actually fails (e.g. iOS).
  // While locking is in-flight or already succeeded, suppress it to avoid the
  // green flash that appeared during the async lock transition.
  const [landscapeLockFailed, setLandscapeLockFailed] = useState(false);

  useEffect(() => {
    if (orientation === 'portrait') {
      setLandscapeLockFailed(false);
      screen.orientation?.lock?.('portrait')?.catch(() => {});
      return;
    }

    // Landscape required. If the lock API is absent (iOS Safari), fall back to
    // showing the overlay immediately so the user knows to rotate manually.
    if (!screen.orientation?.lock) {
      setLandscapeLockFailed(true);
      return;
    }

    // Optimistically hide the overlay while the lock is being applied.
    setLandscapeLockFailed(false);
    screen.orientation.lock('landscape')
      .then(() => setLandscapeLockFailed(false))
      .catch(() => setLandscapeLockFailed(true));
  }, [orientation]);

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

  // Re-apply the orientation lock after fullscreen transitions.
  // On the first visit to the lobby, requestFullscreen() fires on the same click
  // that changes the view, and the fullscreen transition resets the orientation
  // lock that was just applied. This effect catches that and re-locks.
  useEffect(() => {
    function reapplyLock() {
      if (!document.fullscreenElement) return;
      const target = orientation === 'portrait' ? 'portrait' : 'landscape';
      screen.orientation?.lock?.(target)
        ?.then(() => { if (target === 'landscape') setLandscapeLockFailed(false); })
        ?.catch(() => { if (target === 'landscape') setLandscapeLockFailed(true); });
    }
    document.addEventListener('fullscreenchange', reapplyLock);
    return () => document.removeEventListener('fullscreenchange', reapplyLock);
  }, [orientation]);

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
    const { token, seatIndex } = session;
    const tokens = Array<string>(4).fill(token);
    const gameSession: LobbySession = { ...session, activeGameId: gameId };
    saveLobbySession(gameSession);
    window.history.replaceState({}, '', window.location.pathname);
    setView({ kind: 'game', tokens, gameId, myPlayerId: seatIndex, lobbySession: gameSession });
  }

  async function handleLeaveLobby() {
    if (view.kind !== 'game' || !view.lobbySession) return;
    const { token, lobbyId } = view.lobbySession;
    try {
      await leaveLobby(token, lobbyId);
    } catch {
      // best-effort: lobby may already be gone
    }
    clearLobbySession();
    setView({ kind: 'multiplayer-browser', selectedLobbyId: lobbyId });
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

  const resolvePlayerName = usePlayerNameResolver();

  const {
    view: gameView,
    loading: viewLoading,
    error: viewError,
    activeSonderkarten,
    finishedResult,
    sonderkarteNotification,
    newGameVoteCount,
    newGameId,
    refetch,
  } = useGameState(gameSession?.tokens ?? [], gameSession?.gameId ?? null, activePlayer, resolvePlayerName);

  // If the game is gone (backend restart), clear stale session and go home
  useEffect(() => {
    if (!viewError || view.kind !== 'game') return;
    if (!viewError.startsWith('HTTP 404')) return;
    clearLobbySession();
    setView({ kind: 'home' });
  }, [viewError, view.kind]);

  // When the backend auto-starts a new game, update the active game ID
  useEffect(() => {
    if (!newGameId) return;
    setView(prev => {
      if (prev.kind !== 'game') return prev;
      const updatedSession = prev.lobbySession
        ? { ...prev.lobbySession, activeGameId: newGameId }
        : undefined;
      if (updatedSession) saveLobbySession(updatedSession);
      return { ...prev, gameId: newGameId, lobbySession: updatedSession };
    });
  }, [newGameId]);

  const [lastFinishedResult, setLastFinishedResult] = useState<GameResultDto | null>(null);
  const [lastFinishedResultLobbyId, setLastFinishedResultLobbyId] = useState<string | null>(null);
  useEffect(() => {
    if (finishedResult) {
      setLastFinishedResult(finishedResult);
      if (view.kind === 'game' && view.lobbySession) {
        setLastFinishedResultLobbyId(view.lobbySession.lobbyId);
      }
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [finishedResult]);

  const { animTrick, animPhase } = useTrickAnimation(gameView);

  const actions = useGameActions(gameSession, activePlayer, gameView, refetch);

  // ── Render ─────────────────────────────────────────────────────────────────

  if (view.kind === 'home') {
    return (
      <>
        <PortraitOverlay requireLandscape={false} />
        <LandingPage
          onMultiplayer={() => setView({ kind: 'multiplayer-browser' })}
          onTestGame={() => setView({ kind: 'hot-seat' })}
          onRules={() => setView({ kind: 'rules' })}
        />
      </>
    );
  }

  if (view.kind === 'rules') {
    return (
      <>
        <PortraitOverlay requireLandscape={false} />
        <RulesPage onBack={() => setView({ kind: 'home' })} />
      </>
    );
  }

  if (view.kind === 'joining') {
    return (
      <>
        <PortraitOverlay requireLandscape={landscapeLockFailed} />
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
        <PortraitOverlay requireLandscape={landscapeLockFailed} />
        <MultiplayerBrowserPage
          selectedLobbyId={view.selectedLobbyId}
          onBack={() => setView({ kind: 'home' })}
          onSelectLobby={(lobbyId) => setView({ kind: 'multiplayer-browser', selectedLobbyId: lobbyId })}
          onGameStarted={handleGameStarted}
          lastFinishedResult={lastFinishedResultLobbyId === view.selectedLobbyId ? lastFinishedResult : null}
        />
      </>
    );
  }

  // hot-seat or game: show loader until session is ready
  if (!gameSession) {
    return (
      <>
        <PortraitOverlay requireLandscape={landscapeLockFailed} />
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
      <PortraitOverlay requireLandscape={landscapeLockFailed} />
      <GameBoard
        view={gameView}
        activePlayer={activePlayer}
        activeSonderkarten={activeSonderkarten}
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
        lastFinishedResult={lastFinishedResult}
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
        onLeaveLobby={isGame && view.lobbySession ? handleLeaveLobby : undefined}
      />
    </>
  );
}
