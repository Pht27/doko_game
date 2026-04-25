import type { GameResultDto } from '@/types/api';
import { t } from '@/utils/translations';
import type { Party } from './resultDisplay.utils';
import { getSeatParty } from './resultDisplay.utils';
import { ScoreRow } from './ScoreRow';

interface AwardsTableProps {
  awards: GameResultDto['allAwards'];
  myParty: Party | null;
  result: GameResultDto;
}

export function AwardsTable({ awards, myParty, result }: AwardsTableProps) {
  return (
    <div className="rd-table">
      {awards.map((award, i) => {
        const awardParty = getSeatParty(award.benefittingPlayer, result);
        const awardSign = myParty === null ? 1 : awardParty === myParty ? 1 : -1;
        return <ScoreRow
            key={i}
            label={t.extrapunktLabel(award.type)}
            awardee={{ name: t.seatShort(award.benefittingPlayer), party: awardParty }}
            value={awardSign * award.delta}
          />;
      })}
    </div>
  );
}
