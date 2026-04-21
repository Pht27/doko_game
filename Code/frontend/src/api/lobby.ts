import { apiFetch } from './client';

export interface LobbyJoinResponse {
  lobbyId: string;
  token: string;
  seatIndex: number;
  activeGameId?: string | null;
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
  standings: number[];
  startVoteCount: number;
  activeGameId?: string | null;
  opaSeats?: number[] | null;
  selectedScenario?: string | null;
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

export function swapSeat(token: string, lobbyId: string, targetSeatIndex: number): Promise<LobbyJoinResponse> {
  return apiFetch(`/lobbies/${lobbyId}/seats/${targetSeatIndex}/swap`, token, { method: 'POST' });
}

export function getLobby(lobbyId: string): Promise<LobbyViewResponse> {
  return apiFetch(`/lobbies/${lobbyId}`, null);
}

export function startLobbyGame(token: string, lobbyId: string): Promise<{ gameId: string }> {
  return apiFetch(`/lobbies/${lobbyId}/start`, token, { method: 'POST' });
}

export function voteReady(token: string, lobbyId: string): Promise<{ voteCount: number }> {
  return apiFetch(`/lobbies/${lobbyId}/ready`, token, { method: 'POST' });
}

export function withdrawReady(token: string, lobbyId: string): Promise<{ voteCount: number }> {
  return apiFetch(`/lobbies/${lobbyId}/ready/withdraw`, token, { method: 'POST' });
}

export function voteNewGame(token: string, lobbyId: string): Promise<{ voteCount: number }> {
  return apiFetch(`/lobbies/${lobbyId}/new-game/ready`, token, { method: 'POST' });
}

export function withdrawNewGame(token: string, lobbyId: string): Promise<{ voteCount: number }> {
  return apiFetch(`/lobbies/${lobbyId}/new-game/withdraw`, token, { method: 'POST' });
}

export function getScenarios(): Promise<{ scenarios: string[] }> {
  return apiFetch('/lobbies/scenarios', null);
}

export function setScenario(token: string, lobbyId: string, name: string | null): Promise<{ name: string | null }> {
  return apiFetch(`/lobbies/${lobbyId}/scenario`, token, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ name }),
  });
}

export function addOpa(token: string, lobbyId: string, seatIndex: number): Promise<{ seatIndex: number }> {
  return apiFetch(`/lobbies/${lobbyId}/seats/${seatIndex}/opa`, token, { method: 'POST' });
}

export function removeOpa(token: string, lobbyId: string, seatIndex: number): Promise<void> {
  return apiFetch(`/lobbies/${lobbyId}/seats/${seatIndex}/opa`, token, { method: 'DELETE' });
}

