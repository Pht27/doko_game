export interface PlayerRef {
  id: number;
  name: string;
}

export interface RoundListItem {
  id: number;
  playedAt: string;
  winningParty: 'Re' | 'Kontra';
  points: number;
  gameMode: string;
  rePlayers: PlayerRef[];
  kontraPlayers: PlayerRef[];
  comment: string | null;
}

export interface RoundListResponse {
  total: number;
  page: number;
  items: RoundListItem[];
}

export interface PlayerListItem {
  id: number;
  name: string;
  isActive: boolean;
  totalPoints: number;
  gamesPlayed: number;
  wins: number;
  losses: number;
}

export interface PlayerRound {
  roundId: number;
  playedAt: string;
  points: number;
  won: boolean;
  cumulativePoints: number;
}

export interface PlayerDetail {
  id: number;
  name: string;
  isActive: boolean;
  totalPoints: number;
  gamesPlayed: number;
  wins: number;
  losses: number;
  recentRounds: PlayerRound[];
}

// ── Static data ──────────────────────────────────────────────────────────────

export interface GameMode {
  id: number;
  name: string;
  isSolo: boolean;
}

export interface SpecialCard {
  id: number;
  name: string;
}

export interface ExtraPoint {
  id: number;
  name: string;
}

export interface StaticData {
  gameModes: GameMode[];
  specialCards: SpecialCard[];
  extraPoints: ExtraPoint[];
}

// ── Round detail (for edit prefill) ─────────────────────────────────────────

export interface RoundTeamDetail {
  party: 'Re' | 'Kontra';
  players: PlayerRef[];
  specialCards: { id: number; name: string }[];
  extraPoints: { id: number; name: string; count: number }[];
}

export interface RoundDetail {
  id: number;
  playedAt: string;
  winningParty: 'Re' | 'Kontra';
  points: number;
  gameMode: { id: number; name: string; isSolo: boolean };
  teams: RoundTeamDetail[];
  comment: string | null;
}

// ── Round request ────────────────────────────────────────────────────────────

export interface ExtraPointRequest {
  extraPointId: number;
  count: number;
}

export interface TeamRequest {
  party: 'Re' | 'Kontra';
  playerIds: number[];
  specialCardIds: number[];
  extraPoints: ExtraPointRequest[];
}

export interface RoundRequest {
  playedAt: string;
  winningParty: 'Re' | 'Kontra';
  points: number;
  gameModeId: number;
  teams: TeamRequest[];
  comment: string | null;
}
