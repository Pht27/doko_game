import type { GameResultDto } from '@/types/api';
import { t } from '@/utils/translations';
import { type Party, getSeatParty } from './resultDisplay.utils';

interface AugenRowProps {
  reAugen: number;
  reStiche?: number | null;
  kontraAugen: number;
  kontraStiche?: number | null;
  isReWinner: boolean;
  result: GameResultDto;
}

export function AugenRow({ reAugen, reStiche, kontraAugen, kontraStiche, isReWinner, result }: AugenRowProps) {
  const reSeats: number[] = [];
  const kontraSeats: number[] = [];
  for (let seat = 0; seat < result.netPointsPerSeat.length; seat++) {
    const party: Party | null = getSeatParty(seat, result);
    if (party === 'Re') reSeats.push(seat);
    else if (party === 'Kontra') kontraSeats.push(seat);
  }

  const rowCount = Math.max(reSeats.length, kontraSeats.length);

  return (
    <div className="rd-augen-section">
      <div className="rd-augen-left">
        <div className="rd-augen-score-row">
          <span className={isReWinner ? 'rd-augen-party rd-augen-winner' : 'rd-augen-party'}>Re</span>
          <span className={isReWinner ? 'rd-augen-score rd-augen-winner' : 'rd-augen-score'}>
            {reAugen}
            {reStiche != null && <span className="rd-augen-stiche"> ({reStiche})</span>}
          </span>
        </div>
        {Array.from({ length: rowCount }, (_, i) => {
          const seat = reSeats[i];
          return seat !== undefined ? (
            <div key={seat} className={isReWinner ? 'rd-player-row rd-party-re rd-party-winner' : 'rd-player-row rd-party-re rd-party-loser'}>
              <span className="rd-player-name">{t.seatShort(seat)}</span>
              <span className="rd-player-score">
                {result.augenPerSeat[seat]}
                <span className="rd-augen-stiche"> ({result.stichePerSeat[seat]})</span>
              </span>
            </div>
          ) : (
            <div key={`empty-re-${i}`} className="rd-player-row" />
          );
        })}
      </div>

      <div className="rd-augen-vert-divider" />

      <div className="rd-augen-right">
        <div className="rd-augen-score-row">
          <span className={!isReWinner ? 'rd-augen-score rd-augen-winner' : 'rd-augen-score'}>
            {kontraAugen}
            {kontraStiche != null && <span className="rd-augen-stiche"> ({kontraStiche})</span>}
          </span>
          <span className={!isReWinner ? 'rd-augen-party rd-augen-winner' : 'rd-augen-party'}>Kontra</span>
        </div>
        {Array.from({ length: rowCount }, (_, i) => {
          const seat = kontraSeats[i];
          return seat !== undefined ? (
            <div key={seat} className={!isReWinner ? 'rd-player-row rd-player-row-right rd-party-kontra rd-party-winner' : 'rd-player-row rd-player-row-right rd-party-kontra rd-party-loser'}>
              <span className="rd-player-score">
                {result.augenPerSeat[seat]}
                <span className="rd-augen-stiche"> ({result.stichePerSeat[seat]})</span>
              </span>
              <span className="rd-player-name">{t.seatShort(seat)}</span>
            </div>
          ) : (
            <div key={`empty-kontra-${i}`} className="rd-player-row rd-player-row-right" />
          );
        })}
      </div>
    </div>
  );
}
