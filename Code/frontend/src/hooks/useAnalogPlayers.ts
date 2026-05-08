import { useState, useEffect, useCallback } from 'react';
import { getPlayers, createPlayer as apiCreatePlayer, patchPlayer as apiPatchPlayer } from '@/api/analog';
import type { PlayerListItem } from '@/types/analog';

const byNameAsc = (a: PlayerListItem, b: PlayerListItem) => a.name.localeCompare(b.name, 'de');

export function useAnalogPlayers() {
  const [players, setPlayers] = useState<PlayerListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    getPlayers()
      .then((data) => {
        if (cancelled) return;
        setPlayers(data.sort(byNameAsc));
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
      setPlayers((prev) => [...prev, player].sort(byNameAsc));
      return player;
    } catch (err) {
      const msg = err instanceof Error ? err.message : '';
      if (msg.includes('409') || msg.includes('name_taken')) throw new Error('name_taken');
      throw err;
    }
  }, []);

  const toggleActive = useCallback(async (player: PlayerListItem): Promise<void> => {
    const updated = await apiPatchPlayer(player.id, { isActive: !player.isActive });
    setPlayers((prev) =>
      prev.map((p) => (p.id === updated.id ? { ...p, isActive: updated.isActive } : p)),
    );
  }, []);

  const renamePlayer = useCallback(async (player: PlayerListItem, newName: string): Promise<void> => {
    try {
      const updated = await apiPatchPlayer(player.id, { isActive: player.isActive, name: newName });
      setPlayers((prev) =>
        prev
          .map((p) => (p.id === updated.id ? { ...p, name: updated.name } : p))
          .sort(byNameAsc),
      );
    } catch (err) {
      const msg = err instanceof Error ? err.message : '';
      if (msg.includes('409') || msg.includes('name_taken')) throw new Error('name_taken');
      throw err;
    }
  }, []);

  return { players, loading, error, createPlayer, toggleActive, renamePlayer };
}
