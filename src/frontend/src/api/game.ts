import { apiFetch } from './client';
import type {
  AuthTokenResponse,
  StartGameResponse,
  PlayerGameViewResponse,
  DeclareHealthRequest,
  DeclareHealthResponse,
  MakeReservationRequest,
  MakeReservationResponse,
  AcceptArmutRequest,
  AcceptArmutResponse,
  ExchangeArmutCardsRequest,
  ExchangeArmutCardsResponse,
  PlayCardRequest,
  PlayCardResponse,
  MakeAnnouncementRequest,
} from '../types/api';

export function fetchToken(playerId: number): Promise<AuthTokenResponse> {
  return apiFetch('/auth/token', null, {
    method: 'POST',
    body: JSON.stringify({ playerId }),
  });
}

export function startGame(
  token: string,
  playerIds: number[],
): Promise<StartGameResponse> {
  return apiFetch('/games', token, {
    method: 'POST',
    body: JSON.stringify({ playerIds }),
  });
}

export function dealCards(token: string, gameId: string): Promise<void> {
  return apiFetch(`/games/${gameId}/deal`, token, { method: 'POST' });
}

export function getGameView(
  token: string,
  gameId: string,
): Promise<PlayerGameViewResponse> {
  return apiFetch(`/games/${gameId}`, token);
}

export function declareHealth(
  token: string,
  gameId: string,
  body: DeclareHealthRequest,
): Promise<DeclareHealthResponse> {
  return apiFetch(`/games/${gameId}/health`, token, {
    method: 'POST',
    body: JSON.stringify(body),
  });
}

export function respondToArmut(
  token: string,
  gameId: string,
  body: AcceptArmutRequest,
): Promise<AcceptArmutResponse> {
  return apiFetch(`/games/${gameId}/armut-response`, token, {
    method: 'POST',
    body: JSON.stringify(body),
  });
}

export function exchangeArmutCards(
  token: string,
  gameId: string,
  body: ExchangeArmutCardsRequest,
): Promise<ExchangeArmutCardsResponse> {
  return apiFetch(`/games/${gameId}/armut-exchange`, token, {
    method: 'POST',
    body: JSON.stringify(body),
  });
}

export function makeReservation(
  token: string,
  gameId: string,
  body: MakeReservationRequest,
): Promise<MakeReservationResponse> {
  return apiFetch(`/games/${gameId}/reservations`, token, {
    method: 'POST',
    body: JSON.stringify(body),
  });
}

export function playCard(
  token: string,
  gameId: string,
  body: PlayCardRequest,
): Promise<PlayCardResponse> {
  return apiFetch(`/games/${gameId}/cards`, token, {
    method: 'POST',
    body: JSON.stringify(body),
  });
}

export function makeAnnouncement(
  token: string,
  gameId: string,
  body: MakeAnnouncementRequest,
): Promise<void> {
  return apiFetch(`/games/${gameId}/announcements`, token, {
    method: 'POST',
    body: JSON.stringify(body),
  });
}
