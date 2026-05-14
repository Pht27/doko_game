import { useState, useEffect } from 'react';
import {
  getPlayer,
  getPlayerStats,
  getPlayerGameModeStats,
  getPlayerSpecialCardStats,
  getPlayerExtraPointStats,
  getPlayerPartnerStats,
  getPlayerBestWorstRounds,
  getPlayers,
} from '@/api/analog';
import type {
  PlayerDetail,
  PlayerStats,
  PlayerGameModeStat,
  PlayerSpecialCardStat,
  PlayerExtraPointStat,
  PlayerPartnerStat,
  PlayerRoundListItem,
} from '@/types/analog';

export interface PlayerStatsPageData {
  detail: PlayerDetail | null;
  stats: PlayerStats | null;
  gameModes: PlayerGameModeStat[];
  specialCards: PlayerSpecialCardStat[];
  extraPoints: PlayerExtraPointStat[];
  partners: PlayerPartnerStat[];
  bestRound: PlayerRoundListItem | null;
  worstRound: PlayerRoundListItem | null;
  rank: number | null;
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
    bestRound: null,
    worstRound: null,
    rank: null,
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
      getPlayerBestWorstRounds(id),
      getPlayers(),
    ])
      .then(([detail, stats, gameModes, specialCards, extraPoints, partners, bestWorst, allPlayers]) => {
        if (cancelled) return;
        const activeSorted = allPlayers
          .filter((p) => p.isActive)
          .sort((a, b) => b.totalPoints - a.totalPoints);
        const rankIdx = activeSorted.findIndex((p) => p.id === id);
        const rank = rankIdx >= 0 ? rankIdx + 1 : null;
        setData({
          detail,
          stats,
          gameModes,
          specialCards,
          extraPoints,
          partners,
          bestRound: bestWorst.best,
          worstRound: bestWorst.worst,
          rank,
          loading: false,
          error: null,
        });
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
