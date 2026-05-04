import { apiFetch } from './client';
import type { PlayerListItem, PlayerDetail } from '@/types/analog';

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
