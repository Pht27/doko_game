import { apiFetch } from './client';
import type { PlayerListItem, PlayerDetail, RoundListResponse } from '@/types/analog';

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

export function getRounds(page = 1, pageSize = 20): Promise<RoundListResponse> {
  return apiFetch(`/analog/rounds?page=${page}&pageSize=${pageSize}`, null);
}

export function deleteRound(id: number): Promise<void> {
  return apiFetch(`/analog/rounds/${id}`, null, { method: 'DELETE' });
}
