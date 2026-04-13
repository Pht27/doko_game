import { useHotSeat } from './hooks/useHotSeat';
import { useGameState } from './hooks/useGameState';
import { useTrickAnimation } from './hooks/useTrickAnimation';
import { useGameActions } from './hooks/useGameActions';
import { GameBoard } from './components/GameBoard/GameBoard';
import { GameLoader } from './components/GameLoader/GameLoader';

const PLAYER_SWITCHING_ENABLED = true;

function App() {
  const { session, activePlayer, error: initError, loading: initLoading, setActivePlayer, restart } = useHotSeat();

  const { view, loading: viewLoading, error: viewError, finishedResult, sonderkarteNotification, refetch } = useGameState(
    session?.tokens ?? [],
    session?.gameId ?? null,
    activePlayer,
  );

  const { animTrick, animPhase } = useTrickAnimation(view);

  const actions = useGameActions(session, activePlayer, view, refetch);

  if (!session) {
    return <GameLoader loading={initLoading} error={initError} onRetry={restart} />;
  }

  return (
    <GameBoard
      view={view}
      activePlayer={activePlayer}
      animTrick={animTrick}
      animPhase={animPhase}
      actions={actions}
      finishedResult={finishedResult}
      sonderkarteNotification={sonderkarteNotification}
      viewLoading={viewLoading}
      viewError={viewError}
      allowPlayerSwitching={PLAYER_SWITCHING_ENABLED}
      onPlayerSwitch={setActivePlayer}
      onNewGame={restart}
    />
  );
}

export default App;
