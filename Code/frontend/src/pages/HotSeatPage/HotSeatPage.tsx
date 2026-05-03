import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useHotSeat } from '@/hooks/useHotSeat';
import { useGameState } from '@/hooks/useGameState';
import { useTrickAnimation } from '@/hooks/useTrickAnimation';
import { useGameActions } from '@/hooks/useGameActions';
import { usePlayerNameResolver } from '@/context/PlayerNamesContext';
import { GameBoard } from '@/features/game/GameBoard/GameBoard';
import { GameLoader } from '@/features/game/GameLoader/GameLoader';
import type { GameResultDto } from '@/types/api';

export function HotSeatPage() {
  const navigate = useNavigate();
  const hotSeat = useHotSeat(true);
  const resolvePlayerName = usePlayerNameResolver();

  const gameSession = hotSeat.session ?? null;

  const {
    view: gameView,
    loading: viewLoading,
    error: viewError,
    activeSonderkarten,
    finishedResult,
    sonderkarteNotification,
    newGameVoteCount: _newGameVoteCount,
    newGameId: _newGameId,
    refetch,
  } = useGameState(
    gameSession?.tokens ?? [],
    gameSession?.gameId ?? null,
    hotSeat.activePlayer,
    resolvePlayerName,
  );

  const [lastFinishedResult, setLastFinishedResult] = useState<GameResultDto | null>(null);
  useEffect(() => {
    if (finishedResult) setLastFinishedResult(finishedResult);
  }, [finishedResult]);

  const actions = useGameActions(gameSession, hotSeat.activePlayer, gameView, refetch);
  const { animTrick, animPhase } = useTrickAnimation(gameView);

  if (!gameSession) {
    return (
      <GameLoader
        loading={hotSeat.loading}
        error={hotSeat.error}
        onRetry={hotSeat.restart}
      />
    );
  }

  return (
    <GameBoard
      view={gameView}
      activePlayer={hotSeat.activePlayer}
      activeSonderkarten={activeSonderkarten}
      animTrick={animTrick}
      animPhase={animPhase}
      actions={actions}
      finishedResult={finishedResult}
      sonderkarteNotification={sonderkarteNotification}
      viewLoading={viewLoading}
      viewError={viewError}
      allowPlayerSwitching={true}
      onPlayerSwitch={hotSeat.setActivePlayer}
      onNewGame={hotSeat.restart}
      lastFinishedResult={lastFinishedResult}
      onLeaveLobby={async () => navigate('/')}
    />
  );
}
