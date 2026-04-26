import { fmt } from './resultDisplay.utils';
import type { Party } from './resultDisplay.utils';

interface ScoreRowProps {
  label: string;
  awardee?: { name: string, party: Party | null };
  value: number;
  labelClassName?: string;
}

export function ScoreRow({ label, awardee, value, labelClassName }: ScoreRowProps) {
  const valueClass = value >= 0 ? 'rd-row-value rd-value-pos' : 'rd-row-value rd-value-neg';

  if (awardee) {
    return (
      <div className="rd-award-row">
        <span className="rd-row-label">{label}</span>
        <span className={awardee.party === 'Re' ? 'rd-award-player rd-award-player-re' : 'rd-award-player rd-award-player-kontra'}>
          {awardee.name}
        </span>
        <span className={valueClass}>{fmt(value)}</span>
      </div>
    );
  }

  return (
    <div className="rd-row">
      <span className={labelClassName ?? 'rd-row-label'}>{label}</span>
      <span className={valueClass}>{fmt(value)}</span>
    </div>
  );
}
