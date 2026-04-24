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
        <div className={isReWinner ? 'rd-party-players rd-party-re rd-party-winner' : 'rd-party-players rd-party-re rd-party-loser'}>
          {reSeats.map(s => t.seatShort(s)).join(', ')}
        </div>
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
        <div className={!isReWinner ? 'rd-party-players rd-party-kontra rd-party-winner' : 'rd-party-players rd-party-kontra rd-party-loser'}>
          {kontraSeats.map(s => t.seatShort(s)).join(', ')}
        </div>
      </div>
    </div>
  );
}
