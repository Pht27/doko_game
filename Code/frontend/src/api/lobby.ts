import { apiFetch } from './client';

export interface LobbyJoinResponse {
  lobbyId: string;
  playerId: number;
  token: string;
  seatIndex: number;
}

export interface LobbyListItemResponse {
  lobbyId: string;
  seats: boolean[];
  isStarted: boolean;
}

export interface LobbyViewResponse {
  lobbyId: string;
  seats: boolean[];
  isStarted: boolean;
}

export function createLobby(): Promise<LobbyJoinResponse> {
  return apiFetch('/lobbies', null, { method: 'POST' });
}

export function listLobbies(): Promise<LobbyListItemResponse[]> {
  return apiFetch('/lobbies', null);
}

export function joinSeat(lobbyId: string, seatIndex: number): Promise<LobbyJoinResponse> {
  return apiFetch(`/lobbies/${lobbyId}/seats/${seatIndex}/join`, null, { method: 'POST' });
}

export function leaveLobby(token: string, lobbyId: string): Promise<void> {
  return apiFetch(`/lobbies/${lobbyId}/leave`, token, { method: 'POST' });
}

export function getLobby(lobbyId: string): Promise<LobbyViewResponse> {
  return apiFetch(`/lobbies/${lobbyId}`, null);
}

export function startLobbyGame(token: string, lobbyId: string): Promise<{ gameId: string }> {
  return apiFetch(`/lobbies/${lobbyId}/start`, token, { method: 'POST' });
}

export function voteNewGame(token: string, lobbyId: string): Promise<{ voteCount: number }> {
  return apiFetch(`/lobbies/${lobbyId}/new-game/ready`, token, { method: 'POST' });
}

export function withdrawNewGame(token: string, lobbyId: string): Promise<{ voteCount: number }> {
  return apiFetch(`/lobbies/${lobbyId}/new-game/withdraw`, token, { method: 'POST' });
}

export function voteNewGameGeschmissen(token: string, lobbyId: string): Promise<{ voteCount: number }> {
  return apiFetch(`/lobbies/${lobbyId}/new-game/ready-geschmissen`, token, { method: 'POST' });
}
