import { ScoreRow } from './ScoreRow';

interface ValueComponentsTableProps {
  rows: { label: string; value: number }[];
  sign: number;
}

export function ValueComponentsTable({ rows, sign }: ValueComponentsTableProps) {
  return (
    <div className="rd-table">
      {rows.map((row, i) => (
        <ScoreRow key={i} label={row.label} value={sign * row.value} />
      ))}
    </div>
  );
}
