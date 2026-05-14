import { useState, useEffect, useCallback } from 'react';
import { getPlayerRounds } from '@/api/analog';
import type { PlayerRoundListItem } from '@/types/analog';

export const ROUNDS_PAGE_SIZE = 10;

export interface PlayerRoundsData {
  rounds: PlayerRoundListItem[];
  total: number;
  page: number;
  totalPages: number;
  loading: boolean;
  error: string | null;
  goToPage: (p: number) => void;
}

export function usePlayerRounds(playerId: number): PlayerRoundsData {
  const [rounds, setRounds] = useState<PlayerRoundListItem[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetch = useCallback(
    (p: number) => {
      let cancelled = false;
      setLoading(true);
      setError(null);
      getPlayerRounds(playerId, p, ROUNDS_PAGE_SIZE)
        .then((res) => {
          if (cancelled) return;
          setRounds(res.items);
          setTotal(res.total);
          setPage(p);
          setLoading(false);
        })
        .catch((err: unknown) => {
          if (cancelled) return;
          setError(err instanceof Error ? err.message : 'Fehler');
          setLoading(false);
        });
      return () => {
        cancelled = true;
      };
    },
    [playerId],
  );

  useEffect(() => {
    return fetch(1);
  }, [fetch]);

  const goToPage = useCallback(
    (p: number) => {
      fetch(p);
    },
    [fetch],
  );

  const totalPages = Math.max(1, Math.ceil(total / ROUNDS_PAGE_SIZE));

  return { rounds, total, page, totalPages, loading, error, goToPage };
}
