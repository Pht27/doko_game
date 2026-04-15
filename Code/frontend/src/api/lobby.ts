import { apiFetch } from './client';

export interface LobbyJoinResponse {
  lobbyId: string;
  playerId: number;
  isHost: boolean;
  token: string;
  playerCount: number;
}

export interface LobbyViewResponse {
  lobbyId: string;
  playerCount: number;
  isFull: boolean;
  isStarted: boolean;
}

export function createLobby(): Promise<LobbyJoinResponse> {
  return apiFetch('/lobbies', null, { method: 'POST' });
}

export function joinLobby(lobbyId: string): Promise<LobbyJoinResponse> {
  return apiFetch(`/lobbies/${lobbyId}/join`, null, { method: 'POST' });
}

export function getLobby(lobbyId: string): Promise<LobbyViewResponse> {
  return apiFetch(`/lobbies/${lobbyId}`, null);
}

export function startLobbyGame(token: string, lobbyId: string): Promise<{ gameId: string }> {
  return apiFetch(`/lobbies/${lobbyId}/start`, token, { method: 'POST' });
}
