import { useState, useEffect, useRef } from 'react';
import { getLobby } from '../api/lobby';
import { createHubConnection, joinLobbyGroup } from '../api/signalr';
import type { HubConnection } from '@microsoft/signalr';

export interface LobbySession {
  lobbyId: string;
  token: string;
  playerId: number;
  isHost: boolean;
}

export interface LobbyHookState {
  playerCount: number;
  gameId: string | null;
  error: string | null;
}

const SESSION_KEY = 'dokoLobbySession';

export function saveLobbySession(session: LobbySession): void {
  sessionStorage.setItem(SESSION_KEY, JSON.stringify(session));
}

export function loadLobbySession(lobbyId: string): LobbySession | null {
  try {
    const raw = sessionStorage.getItem(SESSION_KEY);
    if (!raw) return null;
    const parsed = JSON.parse(raw) as LobbySession;
    return parsed.lobbyId === lobbyId ? parsed : null;
  } catch {
    return null;
  }
}

export function clearLobbySession(): void {
  sessionStorage.removeItem(SESSION_KEY);
}

export function useLobby(session: LobbySession | null): LobbyHookState {
  const [playerCount, setPlayerCount] = useState(1);
  const [gameId, setGameId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const hubRef = useRef<HubConnection | null>(null);

  useEffect(() => {
    if (!session) return;

    let cancelled = false;

    async function setup() {
      if (!session) return;

      // Fetch initial lobby state
      try {
        const view = await getLobby(session.lobbyId);
        if (cancelled) return;
        setPlayerCount(view.playerCount);
        if (view.isStarted) {
          // Lobby already started before we connected
          return;
        }
      } catch {
        // Non-fatal: SignalR events will keep us updated
      }

      // Connect to SignalR and subscribe to lobby events
      const hub = createHubConnection(session.token);
      hubRef.current = hub;

      hub.on('playerJoined', (data: { playerCount: number }) => {
        if (!cancelled) setPlayerCount(data.playerCount);
      });

      hub.on('gameStarted', (data: { gameId: string }) => {
        if (!cancelled) setGameId(data.gameId);
      });

      try {
        await hub.start();
        if (cancelled) { await hub.stop(); return; }
        await joinLobbyGroup(hub, session.lobbyId);
      } catch (e) {
        if (!cancelled) setError(e instanceof Error ? e.message : String(e));
      }
    }

    setup();

    return () => {
      cancelled = true;
      hubRef.current?.stop();
      hubRef.current = null;
    };
  }, [session?.lobbyId, session?.token]);

  return { playerCount, gameId, error };
}
