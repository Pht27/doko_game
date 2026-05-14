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

export interface PlayerRoundListItem extends RoundListItem {
  teamPartners: PlayerRef[];
  pointDelta: number;
}

export interface PlayerRoundListResponse {
  total: number;
  page: number;
  items: PlayerRoundListItem[];
}

export interface PlayerListItem {
  id: number;
  name: string;
  isActive: boolean;
  totalPoints: number;
  gamesPlayed: number;
  wins: number;
  losses: number;
  winRate: number;
  avgPointsPerGame: number;
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

// ── Stats ────────────────────────────────────────────────────────────────────

export interface PlayerStats {
  playerId: number;
  name: string;
  isActive: boolean;
  totalGames: number;
  totalWins: number;
  totalWinRate: number;
  totalAvgGameValue: number;
  totalAvgPointsWonLost: number;
  totalAvgPointsEarned: number;
  soloGames: number;
  soloWins: number;
  soloWinRate: number;
  soloAvgGameValue: number;
  soloAvgPointsWonLost: number;
  aloneGames: number;
  aloneWins: number;
  aloneWinRate: number;
  aloneAvgPointsEarned: number;
}

export interface PlayerGameModeStat {
  gameModeId: number;
  gameModeName: string;
  party: number;
  games: number;
  wins: number;
  winRate: number;
  avgGameValue: number;
  avgPointsWonLost: number;
  avgPointsEarned: number;
}

export interface PlayerSpecialCardStat {
  specialCardId: number;
  specialCardName: string;
  occurrences: number;
  wins: number;
  winRate: number;
  avgGameValue: number;
  avgPointsWonLost: number;
}

export interface PlayerExtraPointStat {
  extraPointId: number;
  extraPointName: string;
  occurrences: number;
  totalCount: number;
  wins: number;
  winRate: number;
  avgGameValue: number;
}

export interface PlayerPartnerStat {
  partnerId: number;
  partnerName: string;
  gamesTogether: number;
  winsTogether: number;
  winRateTogether: number;
  avgPointsWonLost: number;
}

export interface GameModeStat {
  gameModeId: number;
  gameModeName: string;
  totalRounds: number;
  avgGameValue: number;
  reWinRate: number | null;
  reAvgGameValue: number | null;
}

export interface SpecialCardStat {
  specialCardId: number;
  name: string;
  occurrences: number;
  wins: number;
  winRate: number;
  avgGameValue: number;
  avgPointsWonLost: number;
}

export interface ExtraPointStat {
  extraPointId: number;
  name: string;
  occurrences: number;
  totalCount: number;
  wins: number;
  winRate: number;
  avgGameValue: number;
  avgPointsWonLost: number;
}
