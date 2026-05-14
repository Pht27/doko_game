import { useState, useEffect } from 'react';
import {
  getPlayer,
  getPlayerStats,
  getPlayerGameModeStats,
  getPlayerSpecialCardStats,
  getPlayerExtraPointStats,
  getPlayerPartnerStats,
} from '@/api/analog';
import type {
  PlayerDetail,
  PlayerStats,
  PlayerGameModeStat,
  PlayerSpecialCardStat,
  PlayerExtraPointStat,
  PlayerPartnerStat,
} from '@/types/analog';

export interface PlayerStatsPageData {
  detail: PlayerDetail | null;
  stats: PlayerStats | null;
  gameModes: PlayerGameModeStat[];
  specialCards: PlayerSpecialCardStat[];
  extraPoints: PlayerExtraPointStat[];
  partners: PlayerPartnerStat[];
  loading: boolean;
  error: string | null;
}

export function usePlayerStats(id: number): PlayerStatsPageData {
  const [data, setData] = useState<PlayerStatsPageData>({
    detail: null,
    stats: null,
    gameModes: [],
    specialCards: [],
    extraPoints: [],
    partners: [],
    loading: true,
    error: null,
  });

  useEffect(() => {
    let cancelled = false;
    setData((d) => ({ ...d, loading: true, error: null }));
    Promise.all([
      getPlayer(id),
      getPlayerStats(id),
      getPlayerGameModeStats(id),
      getPlayerSpecialCardStats(id),
      getPlayerExtraPointStats(id),
      getPlayerPartnerStats(id),
    ])
      .then(([detail, stats, gameModes, specialCards, extraPoints, partners]) => {
        if (cancelled) return;
        setData({ detail, stats, gameModes, specialCards, extraPoints, partners, loading: false, error: null });
      })
      .catch((err: unknown) => {
        if (cancelled) return;
        setData((d) => ({ ...d, loading: false, error: err instanceof Error ? err.message : 'Fehler' }));
      });
    return () => {
      cancelled = true;
    };
  }, [id]);

  return data;
}
