import { useState, useEffect, useCallback, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { getGameView } from '../api/game';
import { createHubConnection, joinGameGroup } from '../api/signalr';
import type { PlayerGameViewResponse, GameResultDto, SonderkarteNotification } from '../types/api';

export interface GameStateResult {
  view: PlayerGameViewResponse | null;
  loading: boolean;
  error: string | null;
  /** Last finished result — set when GameFinished event arrives */
  finishedResult: GameResultDto | null;
  /** Brief notification when a sonderkarte was just triggered; auto-clears after 3 s */
  sonderkarteNotification: SonderkarteNotification | null;
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
  const [sonderkarteNotification, setSonderkarteNotification] =
    useState<SonderkarteNotification | null>(null);
  const notifTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const refetchRef = useRef<(() => void) | null>(null);

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

  // Keep ref in sync so SignalR handlers always call the latest refetch
  useEffect(() => {
    refetchRef.current = refetch;
  }, [refetch]);

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
      refetchRef.current?.();
    };

    const handleGameFinished = (payload: { result: GameResultDto }) => {
      setFinishedResult(payload.result);
      refetchRef.current?.();
    };

    const handleSonderkarteTriggered = (payload: SonderkarteNotification) => {
      if (notifTimerRef.current) clearTimeout(notifTimerRef.current);
      setSonderkarteNotification(payload);
      notifTimerRef.current = setTimeout(() => setSonderkarteNotification(null), 3000);
      refetchRef.current?.();
    };

    connection.on('HealthDeclared', handleEvent);
    connection.on('CardPlayed', handleEvent);
    connection.on('TrickCompleted', handleEvent);
    connection.on('AnnouncementMade', handleEvent);
    connection.on('ReservationMade', handleEvent);
    connection.on('ArmutResponse', handleEvent);
    connection.on('ArmutCardsExchanged', handleEvent);
    connection.on('PartyRevealed', handleEvent);
    connection.on('SonderkarteTriggered', handleSonderkarteTriggered);
    connection.on('GameFinished', handleGameFinished);

    connection
      .start()
      .then(() => joinGameGroup(connection, gameId!))
      .catch((e) => console.warn('SignalR connect failed', e));

    return () => {
      connection.stop();
    };
  }, [gameId, tokens]);

  return { view, loading, error, finishedResult, sonderkarteNotification, refetch };
}
