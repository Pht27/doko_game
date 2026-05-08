import { apiFetch } from './client';
import type {
  PlayerListItem,
  PlayerDetail,
  RoundListResponse,
  RoundDetail,
  RoundRequest,
  StaticData,
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
  patch: { isActive: boolean; name?: string },
): Promise<{ id: number; name: string; isActive: boolean }> {
  return apiFetch(`/analog/players/${id}`, null, {
    method: 'PATCH',
    body: JSON.stringify({ isActive: patch.isActive, name: patch.name ?? null }),
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
