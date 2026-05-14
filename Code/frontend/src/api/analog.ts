import { apiFetch } from './client';
import type {
  PlayerListItem,
  PlayerDetail,
  RoundListResponse,
  PlayerRoundListResponse,
  PlayerRoundListItem,
  RoundDetail,
  RoundRequest,
  StaticData,
  PlayerStats,
  PlayerGameModeStat,
  PlayerSpecialCardStat,
  PlayerExtraPointStat,
  PlayerPartnerStat,
  GameModeStat,
  SpecialCardStat,
  ExtraPointStat,
} from '@/types/analog';

export function getPlayers(): Promise<PlayerListItem[]> {
  return apiFetch('/analog/players', null);
}

export function getPlayer(id: number): Promise<PlayerDetail> {
  return apiFetch(`/analog/players/${id}`, null);
}

export function createPlayer(name: string, startingPoints = 0): Promise<PlayerListItem> {
  return apiFetch('/analog/players', null, {
    method: 'POST',
    body: JSON.stringify({ name, startingPoints }),
  });
}

export function patchPlayer(
  id: number,
  patch: { isActive: boolean; name?: string; heroCard?: string },
): Promise<{ id: number; name: string; isActive: boolean; heroCard?: string | null }> {
  return apiFetch(`/analog/players/${id}`, null, {
    method: 'PATCH',
    body: JSON.stringify({ isActive: patch.isActive, name: patch.name ?? null, heroCard: patch.heroCard ?? null }),
  });
}

export function getStaticData(): Promise<StaticData> {
  return apiFetch('/analog/static', null);
}

export function getRounds(page = 1, pageSize = 20): Promise<RoundListResponse> {
  return apiFetch(`/analog/rounds?page=${page}&pageSize=${pageSize}`, null);
}

export function getRound(id: number): Promise<RoundDetail> {
  return apiFetch(`/analog/rounds/${id}`, null);
}

export function createRound(body: RoundRequest): Promise<{ id: number }> {
  return apiFetch('/analog/rounds', null, {
    method: 'POST',
    body: JSON.stringify(body),
  });
}

export function updateRound(id: number, body: RoundRequest): Promise<{ id: number }> {
  return apiFetch(`/analog/rounds/${id}`, null, {
    method: 'PUT',
    body: JSON.stringify(body),
  });
}

export function deleteRound(id: number): Promise<void> {
  return apiFetch(`/analog/rounds/${id}`, null, { method: 'DELETE' });
}

// ── Stats ────────────────────────────────────────────────────────────────────

export function getPlayerStats(id: number): Promise<PlayerStats> {
  return apiFetch(`/analog/players/${id}/stats`, null);
}

export function getPlayerGameModeStats(id: number): Promise<PlayerGameModeStat[]> {
  return apiFetch(`/analog/players/${id}/stats/game-modes`, null);
}

export function getPlayerSpecialCardStats(id: number): Promise<PlayerSpecialCardStat[]> {
  return apiFetch(`/analog/players/${id}/stats/special-cards`, null);
}

export function getPlayerExtraPointStats(id: number): Promise<PlayerExtraPointStat[]> {
  return apiFetch(`/analog/players/${id}/stats/extra-points`, null);
}

export function getPlayerPartnerStats(id: number): Promise<PlayerPartnerStat[]> {
  return apiFetch(`/analog/players/${id}/stats/partners`, null);
}

export interface BestWorstRounds {
  best: PlayerRoundListItem | null;
  worst: PlayerRoundListItem | null;
}

export function getPlayerBestWorstRounds(id: number): Promise<BestWorstRounds> {
  return apiFetch(`/analog/players/${id}/stats/best-worst`, null);
}

export function getPlayerRounds(id: number, page = 1, pageSize = 20): Promise<PlayerRoundListResponse> {
  return apiFetch(`/analog/players/${id}/rounds?page=${page}&pageSize=${pageSize}`, null);
}

export function getGameModeStats(): Promise<GameModeStat[]> {
  return apiFetch('/analog/stats/game-modes', null);
}

export function getSpecialCardStats(): Promise<SpecialCardStat[]> {
  return apiFetch('/analog/stats/special-cards', null);
}

export function getExtraPointStats(): Promise<ExtraPointStat[]> {
  return apiFetch('/analog/stats/extra-points', null);
}
