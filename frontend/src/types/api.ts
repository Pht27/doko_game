// ── Shared ────────────────────────────────────────────────────────────────────

export interface CardDto {
  id: number;
  suit: string;
  rank: string;
}

export interface SonderkarteInfoDto {
  type: string;
  name: string;
  description: string;
}

// ── Game view ─────────────────────────────────────────────────────────────────

export interface PlayerPublicStateDto {
  id: number;
  seat: string;
  knownParty: string | null;
  handCardCount: number;
}

export interface TrickCardDto {
  player: number;
  card: CardDto;
}

export interface TrickSummaryDto {
  trickNumber: number;
  cards: TrickCardDto[];
  winner: number | null;
}

export interface PlayerGameViewResponse {
  gameId: string;
  phase: string;
  requestingPlayer: number;
  hand: CardDto[];
  handSorted: CardDto[];
  legalCards: CardDto[];
  legalAnnouncements: string[];
  eligibleSonderkartenPerCard: Record<number, SonderkarteInfoDto[]>;
  otherPlayers: PlayerPublicStateDto[];
  currentTrick: TrickSummaryDto | null;
  completedTricks: TrickSummaryDto[];
  currentTurn: number;
  isMyTurn: boolean;
  eligibleReservations: string[];
}

// ── Results ───────────────────────────────────────────────────────────────────

export interface ExtrapunktAwardDto {
  type: string;
  benefittingPlayer: number;
  delta: number;
}

export interface GameResultDto {
  winner: string;
  rePoints: number;
  kontraPoints: number;
  gameValue: number;
  allAwards: ExtrapunktAwardDto[];
  feigheit: boolean;
}

// ── Responses ─────────────────────────────────────────────────────────────────

export interface StartGameResponse {
  gameId: string;
}

export interface MakeReservationResponse {
  allDeclared: boolean;
  winningReservation: string | null;
  geschmissen: boolean;
}

export interface PlayCardResponse {
  trickCompleted: boolean;
  trickWinner: number | null;
  gameFinished: boolean;
  finishedResult: GameResultDto | null;
}

export interface AuthTokenResponse {
  token: string;
}

// ── Requests ──────────────────────────────────────────────────────────────────

export interface PlayCardRequest {
  cardId: number;
  activateSonderkarten: string[];
  genscherPartnerId: number | null;
}

export interface MakeReservationRequest {
  reservation: string | null;
  hochzeitCondition: string | null;
  armutPartner: number | null;
}

export interface MakeAnnouncementRequest {
  type: string;
}

// ── SignalR events ─────────────────────────────────────────────────────────────

export type SignalREvent =
  | 'CardPlayed'
  | 'TrickCompleted'
  | 'AnnouncementMade'
  | 'ReservationMade'
  | 'GameFinished'
  | 'SonderkarteTriggered';
