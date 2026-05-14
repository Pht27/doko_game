import { useState, useEffect } from 'react';
import { getGameModeStats, getSpecialCardStats, getExtraPointStats } from '@/api/analog';
import type { GameModeStat, SpecialCardStat, ExtraPointStat } from '@/types/analog';

export interface OverallStatsData {
  gameModes: GameModeStat[];
  specialCards: SpecialCardStat[];
  extraPoints: ExtraPointStat[];
  loading: boolean;
  error: string | null;
}

export function useOverallStats(): OverallStatsData {
  const [data, setData] = useState<OverallStatsData>({
    gameModes: [],
    specialCards: [],
    extraPoints: [],
    loading: true,
    error: null,
  });

  useEffect(() => {
    let cancelled = false;
    Promise.all([getGameModeStats(), getSpecialCardStats(), getExtraPointStats()])
      .then(([gameModes, specialCards, extraPoints]) => {
        if (cancelled) return;
        setData({ gameModes, specialCards, extraPoints, loading: false, error: null });
      })
      .catch((err: unknown) => {
        if (cancelled) return;
        setData((d) => ({ ...d, loading: false, error: err instanceof Error ? err.message : 'Fehler' }));
      });
    return () => {
      cancelled = true;
    };
  }, []);

  return data;
}
