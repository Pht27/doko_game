import { useState, useEffect } from 'react';
import { getPlayer } from '@/api/analog';
import type { PlayerDetail } from '@/types/analog';

export function useAnalogPlayer(id: number) {
  const [player, setPlayer] = useState<PlayerDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);
    getPlayer(id)
      .then((data) => {
        if (cancelled) return;
        setPlayer(data);
        setLoading(false);
      })
      .catch((err: unknown) => {
        if (cancelled) return;
        setError(err instanceof Error ? err.message : 'Fehler');
        setLoading(false);
      });
    return () => { cancelled = true; };
  }, [id]);

  return { player, loading, error };
}
