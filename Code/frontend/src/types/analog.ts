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
