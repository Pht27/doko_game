import { useState, useEffect, useRef } from 'react';
import { getLobby } from '../api/lobby';
import { createHubConnection, joinLobbyGroup } from '../api/signalr';
import type { HubConnection } from '@microsoft/signalr';

export interface LobbySession {
  lobbyId: string;
  token: string;
  playerId: number;
  seatIndex: number;
}

export interface LobbyHookState {
  seats: boolean[];
  gameId: string | null;
  lobbyClosed: boolean;
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
  const [seats, setSeats] = useState<boolean[]>(Array(4).fill(false));
  const [gameId, setGameId] = useState<string | null>(null);
  const [lobbyClosed, setLobbyClosed] = useState(false);
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
        setSeats(view.seats);
        if (view.isStarted) return;
      } catch {
        // Non-fatal: SignalR events will keep us updated
      }

      // Connect to SignalR and subscribe to lobby events
      const hub = createHubConnection(session.token);
      hubRef.current = hub;

      hub.on('playerJoined', (data: { seatIndex: number }) => {
        if (cancelled) return;
        setSeats((prev) => {
          const next = [...prev];
          next[data.seatIndex] = true;
          return next;
        });
      });

      hub.on('playerLeft', (data: { seatIndex: number }) => {
        if (cancelled) return;
        setSeats((prev) => {
          const next = [...prev];
          next[data.seatIndex] = false;
          return next;
        });
      });

      hub.on('gameStarted', (data: { gameId: string }) => {
        if (!cancelled) setGameId(data.gameId);
      });

      hub.on('lobbyClosed', () => {
        if (!cancelled) setLobbyClosed(true);
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

  return { seats, gameId, lobbyClosed, error };
}
