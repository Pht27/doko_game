import { useState, useEffect, useCallback } from 'react';
import { getPlayers, createPlayer as apiCreatePlayer } from '@/api/analog';
import type { PlayerListItem } from '@/types/analog';

const byPointsDesc = (a: PlayerListItem, b: PlayerListItem) => b.totalPoints - a.totalPoints;

export function useAnalogPlayers() {
  const [players, setPlayers] = useState<PlayerListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    getPlayers()
      .then((data) => {
        if (cancelled) return;
        setPlayers(data.sort(byPointsDesc));
        setLoading(false);
      })
      .catch((err: unknown) => {
        if (cancelled) return;
        setError(err instanceof Error ? err.message : 'Fehler');
        setLoading(false);
      });
    return () => { cancelled = true; };
  }, []);

  const createPlayer = useCallback(async (name: string, startingPoints = 0): Promise<PlayerListItem> => {
    try {
      const player = await apiCreatePlayer(name, startingPoints);
      setPlayers((prev) => [...prev, player].sort(byPointsDesc));
      return player;
    } catch (err) {
      const msg = err instanceof Error ? err.message : '';
      if (msg.includes('409') || msg.includes('name_taken')) throw new Error('name_taken');
      throw err;
    }
  }, []);

  return { players, loading, error, createPlayer };
}
