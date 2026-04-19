import { useState, useEffect, useRef } from 'react';
import { getLobby } from '../api/lobby';
import { createHubConnection, joinLobbyGroup } from '../api/signalr';
import type { HubConnection } from '@microsoft/signalr';

export interface LobbySession {
  lobbyId: string;
  token: string;
  seatIndex: number;
  activeGameId?: string;
}

export interface LobbyHookState {
  seats: boolean[];
  gameId: string | null;
  isStarted: boolean;
  lobbyClosed: boolean;
  startVoteCount: number;
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

/** Returns whatever session is stored, regardless of which lobby it belongs to. */
export function loadAnySession(): LobbySession | null {
  try {
    const raw = sessionStorage.getItem(SESSION_KEY);
    return raw ? (JSON.parse(raw) as LobbySession) : null;
  } catch {
    return null;
  }
}

export function clearLobbySession(): void {
  sessionStorage.removeItem(SESSION_KEY);
}

export function useLobby(session: LobbySession | null, lobbyId: string): LobbyHookState {
  const [seats, setSeats] = useState<boolean[]>(Array(4).fill(false));
  const [gameId, setGameId] = useState<string | null>(null);
  const [isStarted, setIsStarted] = useState(false);
  const [lobbyClosed, setLobbyClosed] = useState(false);
  const [startVoteCount, setStartVoteCount] = useState(0);
  const [error, setError] = useState<string | null>(null);
  const hubRef = useRef<HubConnection | null>(null);

  // Fetch current seat state — getLobby is unauthenticated, so this runs even
  // before the user joins. Re-fetches on join (session?.token changes) to get
  // a fresh snapshot before SignalR takes over.
  useEffect(() => {
    let cancelled = false;
    getLobby(lobbyId)
      .then((view) => {
        if (!cancelled) {
          setSeats(view.seats);
          setStartVoteCount(view.startVoteCount ?? 0);
          setIsStarted(view.isStarted);
          if (view.activeGameId) setGameId(view.activeGameId);
        }
      })
      .catch(() => {});
    return () => { cancelled = true; };
  }, [lobbyId, session?.token]);

  // Connect to SignalR for live updates — requires an authenticated session.
  useEffect(() => {
    if (!session) return;

    let cancelled = false;

    async function setup() {
      const hub = createHubConnection(session!.token);
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

      hub.on('lobbyReadyVoteChanged', (data: { count: number }) => {
        if (!cancelled) setStartVoteCount(data.count);
      });

      hub.on('lobbyClosed', () => {
        if (!cancelled) setLobbyClosed(true);
      });

      try {
        await hub.start();
        if (cancelled) { await hub.stop(); return; }
        await joinLobbyGroup(hub, session!.lobbyId);
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

  return { seats, gameId, isStarted, lobbyClosed, startVoteCount, error };
}
