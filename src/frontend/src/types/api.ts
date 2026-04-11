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
  ownParty: string | null;
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
  mustDeclareReservation: boolean;
  shouldDeclareHealth: boolean;
  shouldRespondToArmut: boolean;
  shouldReturnArmutCards: boolean;
  armutCardReturnCount: number | null;
  armutExchangeCardCount: number | null;
  armutReturnedTrump: boolean | null;
}

// ── Results ───────────────────────────────────────────────────────────────────

export interface ExtrapunktAwardDto {
  type: string;
  benefittingPlayer: number;
  delta: number;
}

export interface GameValueComponentDto {
  label: string;
  value: number;
}

export interface GameResultDto {
  winner: string;
  rePoints: number;
  kontraPoints: number;
  gameValue: number;
  allAwards: ExtrapunktAwardDto[];
  feigheit: boolean;
  valueComponents: GameValueComponentDto[];
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

export interface DeclareHealthRequest {
  hasVorbehalt: boolean;
}

export interface DeclareHealthResponse {
  allDeclared: boolean;
}

export interface AcceptArmutRequest {
  accepts: boolean;
}

export interface AcceptArmutResponse {
  accepted: boolean;
  schwarzesSau: boolean;
}

export interface ExchangeArmutCardsRequest {
  cardIds: number[];
}

export interface ExchangeArmutCardsResponse {
  returnedTrumpCount: number;
}

// ── SignalR events ─────────────────────────────────────────────────────────────

export type SignalREvent =
  | 'CardPlayed'
  | 'TrickCompleted'
  | 'AnnouncementMade'
  | 'ReservationMade'
  | 'GameFinished'
  | 'SonderkarteTriggered'
  | 'ArmutCardsExchanged';
