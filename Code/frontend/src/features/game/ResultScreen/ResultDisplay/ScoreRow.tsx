import type { ReactNode } from 'react';
import { fmt } from './resultDisplay.utils';

interface ScoreRowProps {
  label: ReactNode;
  value: number;
  labelClassName?: string;
}

export function ScoreRow({ label, value, labelClassName }: ScoreRowProps) {
  return (
    <div className="rd-row">
      <span className={labelClassName ?? 'rd-row-label'}>{label}</span>
      <span className={value >= 0 ? 'rd-row-value rd-value-pos' : 'rd-row-value rd-value-neg'}>
        {fmt(value)}
      </span>
    </div>
  );
}
