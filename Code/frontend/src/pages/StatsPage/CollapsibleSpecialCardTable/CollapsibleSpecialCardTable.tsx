import type { SpecialCardStat } from '@/types/analog';
import { CollapsibleOccurrenceTable } from '@/components/CollapsibleOccurrenceTable/CollapsibleOccurrenceTable';
import type { OccurrenceGroup } from '@/components/CollapsibleOccurrenceTable/CollapsibleOccurrenceTable';
import { t } from '@/utils/translations';

function toGroups(rows: SpecialCardStat[]): OccurrenceGroup[] {
  const map = new Map<number, OccurrenceGroup>();
  for (const row of rows) {
    if (!map.has(row.specialCardId))
      map.set(row.specialCardId, { id: row.specialCardId, name: row.name, re: null, kontra: null });
    const g = map.get(row.specialCardId)!;
    const stat = { occurrences: row.occurrences, wins: row.wins, winRate: row.winRate, avgGameValue: row.avgGameValue };
    if (row.party === 0) g.re = stat; else g.kontra = stat;
  }
  return Array.from(map.values());
}

export function CollapsibleSpecialCardTable({ rows }: { rows: SpecialCardStat[] }) {
  return <CollapsibleOccurrenceTable groups={toGroups(rows)} nameHeader={t.statsColCardName} />;
}
