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
  opaSeats: number[];
  gameId: string | null;
  isStarted: boolean;
  lobbyClosed: boolean;
  startVoteCount: number;
  readySeats: number[];
  selectedScenario: string | null;
  error: string | null;
}

const SESSION_KEY = 'dokoLobbySession';

export function saveLobbySession(session: LobbySession): void {
  localStorage.setItem(SESSION_KEY, JSON.stringify(session));
}

export function loadLobbySession(lobbyId: string): LobbySession | null {
  try {
    const raw = localStorage.getItem(SESSION_KEY);
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
    const raw = localStorage.getItem(SESSION_KEY);
    return raw ? (JSON.parse(raw) as LobbySession) : null;
  } catch {
    return null;
  }
}

export function clearLobbySession(): void {
  localStorage.removeItem(SESSION_KEY);
}

export function useLobby(session: LobbySession | null, lobbyId: string): LobbyHookState {
  const [seats, setSeats] = useState<boolean[]>(Array(4).fill(false));
  const [opaSeats, setOpaSeats] = useState<number[]>([]);
  const [gameId, setGameId] = useState<string | null>(null);
  const [isStarted, setIsStarted] = useState(false);
  const [lobbyClosed, setLobbyClosed] = useState(false);
  const [startVoteCount, setStartVoteCount] = useState(0);
  const [readySeats, setReadySeats] = useState<number[]>([]);
  const [selectedScenario, setSelectedScenario] = useState<string | null>(null);
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
          setOpaSeats(view.opaSeats ?? []);
          setStartVoteCount(view.startVoteCount ?? 0);
          setReadySeats(view.readySeats ?? []);
          setIsStarted(view.isStarted);
          setSelectedScenario(view.selectedScenario ?? null);
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

      hub.on('playerJoined', (data: { seatIndex: number; isOpa?: boolean }) => {
        if (cancelled) return;
        setSeats((prev) => {
          const next = [...prev];
          next[data.seatIndex] = true;
          return next;
        });
        if (data.isOpa) {
          setOpaSeats((prev) => prev.includes(data.seatIndex) ? prev : [...prev, data.seatIndex]);
        }
      });

      hub.on('playerLeft', (data: { seatIndex: number }) => {
        if (cancelled) return;
        setSeats((prev) => {
          const next = [...prev];
          next[data.seatIndex] = false;
          return next;
        });
        setOpaSeats((prev) => prev.filter((s) => s !== data.seatIndex));
      });

      hub.on('gameStarted', (data: { gameId: string }) => {
        if (!cancelled) setGameId(data.gameId);
      });

      hub.on('lobbyReadyVoteChanged', (data: { count: number; seats: number[] }) => {
        if (!cancelled) {
          setStartVoteCount(data.count);
          setReadySeats(data.seats ?? []);
        }
      });

      hub.on('lobbyClosed', () => {
        if (!cancelled) setLobbyClosed(true);
      });

      hub.on('scenarioChanged', (data: { name: string | null }) => {
        if (!cancelled) setSelectedScenario(data.name ?? null);
      });

      hub.onreconnected(async () => {
        if (cancelled) return;
        try {
          await joinLobbyGroup(hub, session!.lobbyId);
          // Catch up on any event missed during the disconnect (e.g. gameStarted)
          const view = await getLobby(lobbyId);
          if (cancelled) return;
          setSeats(view.seats);
          setOpaSeats(view.opaSeats ?? []);
          setStartVoteCount(view.startVoteCount ?? 0);
          setReadySeats(view.readySeats ?? []);
          setIsStarted(view.isStarted);
          setSelectedScenario(view.selectedScenario ?? null);
          if (view.activeGameId) setGameId(view.activeGameId);
        } catch {
          // reconnect is best-effort
        }
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

  return { seats, opaSeats, gameId, isStarted, lobbyClosed, startVoteCount, readySeats, selectedScenario, error };
}
