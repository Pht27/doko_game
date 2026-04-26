import type { GameResultDto } from '@/types/api';
import { t } from '@/utils/translations';

interface LobbyHistoryProps {
  result: GameResultDto;
  mySeat?: number;
  selectedGame: number;
  onSelectGame: (index: number) => void;
}

export function LobbyHistory({ result, mySeat, selectedGame, onSelectGame }: LobbyHistoryProps) {
  const history = result.matchHistory ?? [];
  const totalGames = history.length + (result.isGeschmissen ? 0 : 1);
  const standings = result.lobbyStandings;
  const seatCount = 4;

  function getNetPoints(gameIndex: number, seat: number): number {
    if (gameIndex < history.length) return history[gameIndex].netPointsPerSeat[seat] ?? 0;
    return result.netPointsPerSeat[seat] ?? 0;
  }

  function formatPoints(pts: number): string {
    if (pts === 0) return '0';
    return pts > 0 ? `+${pts}` : `${pts}`;
  }

  if (totalGames === 0 && standings.length === 0) return null;

  return (
    <div className="rh-container">
      {/* Title */}
      <div className="rh-title">{t.matchHistory}</div>

      {/* Header: seat labels — fixed at top */}
      <div className="rh-row rh-header-row">
        <div className="rh-cell rh-game-cell" />
        {Array.from({ length: seatCount }, (_, seat) => (
          <div
            key={seat}
            className={seat === mySeat ? 'rh-cell rh-seat-header rh-seat-me' : 'rh-cell rh-seat-header'}
          >
            {t.seatShort(seat)}
          </div>
        ))}
      </div>

      {/* Game rows — scrollable, fills available space */}
      <div className="rh-rows">
        {Array.from({ length: totalGames }, (_, i) => {
          const isSelected = i === selectedGame;
          return (
            <div
              key={i}
              className={isSelected ? 'rh-row rh-game-row rh-row-selected' : 'rh-row rh-game-row rh-row-clickable'}
              onClick={() => onSelectGame(i)}
            >
              <div className="rh-cell rh-game-cell rh-game-number">{i + 1}</div>
              {Array.from({ length: seatCount }, (_, seat) => {
                const pts = getNetPoints(i, seat);
                const cls =
                  pts > 0
                    ? 'rh-cell rh-points rh-points-pos'
                    : pts < 0
                      ? 'rh-cell rh-points rh-points-neg'
                      : 'rh-cell rh-points rh-points-zero';
                return (
                  <div key={seat} className={cls}>
                    {formatPoints(pts)}
                  </div>
                );
              })}
            </div>
          );
        })}
      </div>

      {/* Standings — fixed at bottom */}
      {standings.length > 0 && (
        <div className="rh-row rh-standings-row">
          <div className="rh-cell rh-game-cell" />
          {Array.from({ length: seatCount }, (_, seat) => {
            const pts = standings[seat] ?? 0;
            const cls =
              pts > 0
                ? 'rh-cell rh-standing rh-standing-pos'
                : pts < 0
                  ? 'rh-cell rh-standing rh-standing-neg'
                  : 'rh-cell rh-standing';
            return (
              <div key={seat} className={cls}>
                {pts}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
