import { useState, useEffect, useCallback } from 'react';
import { getRounds, deleteRound as apiDeleteRound } from '@/api/analog';
import type { RoundListItem } from '@/types/analog';

const PAGE_SIZE = 20;

export function useAnalogRounds() {
  const [rounds, setRounds] = useState<RoundListItem[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);
    getRounds(1, PAGE_SIZE)
      .then((data) => {
        if (cancelled) return;
        setRounds(data.items);
        setTotal(data.total);
        setPage(1);
        setLoading(false);
      })
      .catch((err: unknown) => {
        if (cancelled) return;
        setError(err instanceof Error ? err.message : 'Fehler');
        setLoading(false);
      });
    return () => { cancelled = true; };
  }, []);

  const loadMore = useCallback(async () => {
    const nextPage = page + 1;
    setLoadingMore(true);
    try {
      const data = await getRounds(nextPage, PAGE_SIZE);
      setRounds((prev) => [...prev, ...data.items]);
      setPage(nextPage);
    } finally {
      setLoadingMore(false);
    }
  }, [page]);

  const deleteRound = useCallback(async (id: number) => {
    await apiDeleteRound(id);
    setRounds((prev) => prev.filter((r) => r.id !== id));
    setTotal((prev) => prev - 1);
  }, []);

  const hasMore = rounds.length < total;

  return { rounds, total, loading, loadingMore, error, hasMore, loadMore, deleteRound };
}
