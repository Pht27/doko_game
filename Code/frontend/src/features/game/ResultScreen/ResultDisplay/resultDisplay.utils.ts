import type { AnnouncementRecordDto, GameResultDto } from '@/types/api';
import { t } from '@/utils/translations';

export type Party = 'Re' | 'Kontra';

const componentAnnouncementType: Record<string, string> = {
  'Gewonnen': 'Win',
  'Keine 90': 'Keine90',
  'Keine 60': 'Keine60',
  'Keine 30': 'Keine30',
  'Schwarz': 'Schwarz',
};

export function buildComponentRows(
  valueComponents: GameResultDto['valueComponents'],
  announcementRecords: AnnouncementRecordDto[],
  winner: string,
  feigheit: boolean,
): { label: string; value: number }[] {
  if (feigheit) return valueComponents.map(c => ({ label: c.label, value: c.value }));

  const matched = new Set<number>();

  const rows = valueComponents.map(c => {
    const aType = componentAnnouncementType[c.label];
    if (!aType) return { label: c.label, value: c.value };

    const matchingIndices = announcementRecords
      .map((r, i) => ({ r, i }))
      .filter(({ r, i }) =>
        r.type === aType &&
        !matched.has(i) &&
        (aType !== 'Win' || r.party === winner),
      )
      .map(({ i }) => i);

    matchingIndices.forEach(i => matched.add(i));
    const bonus = matchingIndices.length;
    return {
      label: bonus > 0 ? `${c.label} ${t.announcedSuffix}` : c.label,
      value: c.value + bonus,
    };
  });

  const unmatchedRows = announcementRecords
    .filter((_, i) => !matched.has(i))
    .map(r => ({
      label: `${t.announcementLabel(r.type === 'Win' ? r.party : r.type)} ${t.announcedSuffix}`,
      value: 1,
    }));

  return [...rows, ...unmatchedRows];
}

export function getSeatParty(seat: number, result: GameResultDto): Party | null {
  const pts = result.netPointsPerSeat[seat];
  if (pts === undefined || pts === 0) return null;
  return pts > 0 ? (result.winner as Party) : result.winner === 'Re' ? 'Kontra' : 'Re';
}

export function fmt(n: number): string {
  return n > 0 ? `+${n}` : `${n}`;
}
