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
