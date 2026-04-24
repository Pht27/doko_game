import { t } from '@/utils/translations';
import { fmt } from './resultDisplay.utils';

interface TotalRowProps {
  myNetPoints: number | null;
  totalScore: number;
}

export function TotalRow({ myNetPoints, totalScore }: TotalRowProps) {
  const display = myNetPoints !== null ? myNetPoints : totalScore;
  return (
    <div className="rd-total">
      <span>{t.insgesamt}</span>
      <span className={display >= 0 ? 'rd-total-value rd-value-pos' : 'rd-total-value rd-value-neg'}>
        {fmt(display)}
      </span>
    </div>
  );
}
