import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useGameState } from '@/hooks/useGameState';
import { useTrickAnimation } from '@/hooks/useTrickAnimation';
import { useGameActions } from '@/hooks/useGameActions';
import { loadAnySession, saveLobbySession, clearLobbySession } from '@/hooks/useLobby';
import { usePlayerNameResolver } from '@/context/PlayerNamesContext';
import { leaveLobby, voteNewGame, withdrawNewGame } from '@/api/lobby';
import { GameBoard } from '@/features/game/GameBoard/GameBoard';
import { GameLoader } from '@/features/game/GameLoader/GameLoader';
import type { LobbySession } from '@/hooks/useLobby';
import type { GameResultDto } from '@/types/api';

export function GamePage() {
  const { id: gameId } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const session = loadAnySession();
  const token = session?.token ?? '';
  const myPlayerId = session?.seatIndex ?? 0;
  const tokens = token ? Array<string>(4).fill(token) : [];

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
  } = useGameState(tokens, gameId ?? null, myPlayerId, resolvePlayerName);

  const [currentSession, setCurrentSession] = useState<LobbySession | null>(session);
  const [lastFinishedResult, setLastFinishedResult] = useState<GameResultDto | null>(null);

  // Track the active game ID (may change when backend starts a new game)
  const [activeGameId, setActiveGameId] = useState(gameId ?? '');

  useEffect(() => {
    if (finishedResult) setLastFinishedResult(finishedResult);
  }, [finishedResult]);

  // Backend started a new round — update session and navigate to new game
  useEffect(() => {
    if (!newGameId) return;
    const updatedSession = currentSession
      ? { ...currentSession, activeGameId: newGameId }
      : null;
    if (updatedSession) saveLobbySession(updatedSession);
    setCurrentSession(updatedSession);
    setActiveGameId(newGameId);
    navigate(`/game/${newGameId}`, { replace: true });
  }, [newGameId]);

  // 404 — game gone (e.g. backend restart), go home
  useEffect(() => {
    if (!viewError?.startsWith('HTTP 404')) return;
    clearLobbySession();
    navigate('/', { replace: true });
  }, [viewError]);

  async function handleLeaveLobby() {
    if (!currentSession) return;
    const { token: t, lobbyId } = currentSession;
    try {
      await leaveLobby(t, lobbyId);
    } catch {
      // best-effort
    }
    clearLobbySession();
    navigate('/lobby', {
      state: { lastFinishedResult, fromLobbyId: lobbyId },
    });
  }

  const gameSession = tokens.length > 0 ? { tokens, gameId: activeGameId } : null;
  const actions = useGameActions(gameSession, myPlayerId, gameView, refetch);
  const { animTrick, animPhase } = useTrickAnimation(gameView);

  if (!gameSession) {
    return <GameLoader loading={false} error="Keine Spielsitzung gefunden." onRetry={() => navigate('/')} />;
  }

  return (
    <GameBoard
      view={gameView}
      activePlayer={myPlayerId}
      activeSonderkarten={activeSonderkarten}
      animTrick={animTrick}
      animPhase={animPhase}
      actions={actions}
      finishedResult={finishedResult}
      sonderkarteNotification={sonderkarteNotification}
      viewLoading={viewLoading}
      viewError={viewError}
      allowPlayerSwitching={false}
      onPlayerSwitch={() => {}}
      onNewGame={() => navigate('/')}
      lastFinishedResult={lastFinishedResult}
      multiplayerNewGame={
        currentSession
          ? {
              voteCount: newGameVoteCount,
              mySeatIndex: currentSession.seatIndex,
              onVote: () => voteNewGame(currentSession.token, currentSession.lobbyId),
              onWithdraw: () => withdrawNewGame(currentSession.token, currentSession.lobbyId),
            }
          : undefined
      }
      onLeaveLobby={currentSession ? handleLeaveLobby : undefined}
    />
  );
}
