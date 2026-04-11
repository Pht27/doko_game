import { useState, useEffect, useCallback, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { getGameView } from '../api/game';
import { createHubConnection, joinGameGroup } from '../api/signalr';
import type { PlayerGameViewResponse, GameResultDto } from '../types/api';

export interface GameStateResult {
  view: PlayerGameViewResponse | null;
  loading: boolean;
  error: string | null;
  /** Last finished result — set when GameFinished event arrives */
  finishedResult: GameResultDto | null;
  refetch: () => void;
}

export function useGameState(
  tokens: string[],
  gameId: string | null,
  activePlayer: number,
): GameStateResult {
  const [view, setView] = useState<PlayerGameViewResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [finishedResult, setFinishedResult] = useState<GameResultDto | null>(null);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  const token = tokens[activePlayer];

  const refetch = useCallback(async () => {
    if (!token || !gameId) return;
    setLoading(true);
    try {
      const data = await getGameView(token, gameId);
      setView(data);
      setError(null);
    } catch (e) {
      setError(e instanceof Error ? e.message : String(e));
    } finally {
      setLoading(false);
    }
  }, [token, gameId]);

  // Reset finished result when the game changes
  useEffect(() => {
    setFinishedResult(null);
  }, [gameId]);

  // Re-fetch whenever active player changes
  useEffect(() => {
    refetch();
  }, [refetch]);

  // Set up a single shared SignalR connection
  useEffect(() => {
    if (!tokens[0] || !gameId) return;
    const connection = createHubConnection(tokens[0]);
    connectionRef.current = connection;

    const handleEvent = (_payload: unknown) => {
      refetch();
    };

    const handleGameFinished = (payload: { result: GameResultDto }) => {
      setFinishedResult(payload.result);
      refetch();
    };

    connection.on('CardPlayed', handleEvent);
    connection.on('TrickCompleted', handleEvent);
    connection.on('AnnouncementMade', handleEvent);
    connection.on('ReservationMade', handleEvent);
    connection.on('SonderkarteTriggered', handleEvent);
    connection.on('GameFinished', handleGameFinished);

    connection
      .start()
      .then(() => joinGameGroup(connection, gameId!))
      .catch((e) => console.warn('SignalR connect failed', e));

    return () => {
      connection.stop();
    };
  }, [gameId, tokens]);

  return { view, loading, error, finishedResult, refetch };
}
