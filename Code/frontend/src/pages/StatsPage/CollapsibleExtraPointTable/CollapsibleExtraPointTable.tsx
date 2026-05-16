import type { ExtraPointStat } from '@/types/analog';
import { CollapsibleOccurrenceTable } from '@/components/CollapsibleOccurrenceTable/CollapsibleOccurrenceTable';
import type { OccurrenceGroup } from '@/components/CollapsibleOccurrenceTable/CollapsibleOccurrenceTable';

function toGroups(rows: ExtraPointStat[]): OccurrenceGroup[] {
  const map = new Map<number, OccurrenceGroup>();
  for (const row of rows) {
    if (!map.has(row.extraPointId))
      map.set(row.extraPointId, { id: row.extraPointId, name: row.name, re: null, kontra: null });
    const g = map.get(row.extraPointId)!;
    const stat = { occurrences: row.occurrences, wins: row.wins, winRate: row.winRate, avgGameValue: row.avgGameValue };
    if (row.party === 0) g.re = stat; else g.kontra = stat;
  }
  return Array.from(map.values());
}

export function CollapsibleExtraPointTable({ rows }: { rows: ExtraPointStat[] }) {
  return <CollapsibleOccurrenceTable groups={toGroups(rows)} nameHeader={t.statsColExtraPointName} />;
}
