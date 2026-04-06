import { useState, useEffect, useCallback } from 'react';
import { fetchToken, startGame, dealCards } from '../api/game';

export interface HotSeatSession {
  tokens: string[];
  gameId: string;
}

export interface HotSeatState {
  session: HotSeatSession | null;
  activePlayer: number;
  error: string | null;
  loading: boolean;
  setActivePlayer: (player: number) => void;
  restart: () => void;
}

export function useHotSeat(): HotSeatState {
  const [session, setSession] = useState<HotSeatSession | null>(null);
  const [activePlayer, setActivePlayer] = useState(0);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [generation, setGeneration] = useState(0);

  useEffect(() => {
    let cancelled = false;

    async function init() {
      setLoading(true);
      setError(null);
      setSession(null);
      setActivePlayer(0);

      try {
        const tokenResults = await Promise.all([0, 1, 2, 3].map(fetchToken));
        if (cancelled) return;
        const tokens = tokenResults.map((r) => r.token);

        const game = await startGame(tokens[0], [0, 1, 2, 3]);
        if (cancelled) return;

        await dealCards(tokens[0], game.gameId);
        if (cancelled) return;

        setSession({ tokens, gameId: game.gameId });
      } catch (e) {
        if (!cancelled) setError(e instanceof Error ? e.message : String(e));
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    init();
    return () => { cancelled = true; };
  }, [generation]);

  const restart = useCallback(() => {
    setGeneration((g) => g + 1);
  }, []);

  return { session, activePlayer, error, loading, setActivePlayer, restart };
}
