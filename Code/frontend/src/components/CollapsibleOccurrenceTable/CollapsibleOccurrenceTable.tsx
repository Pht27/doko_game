import { useState } from 'react';
import { colorForRate, colorForMean, fmtInt, fmtRate, fmtMean } from '@/utils/statsUtils';
import { t } from '@/utils/translations';
import './CollapsibleOccurrenceTable.css';

export interface OccurrencePartyStat {
  occurrences: number;
  wins: number;
  winRate: number;
  avgGameValue: number;
}

export interface OccurrenceGroup {
  id: number;
  name: string;
  re: OccurrencePartyStat | null;
  kontra: OccurrencePartyStat | null;
}

interface Props {
  groups: OccurrenceGroup[];
  nameHeader: string;
}

export function CollapsibleOccurrenceTable({ groups, nameHeader }: Props) {
  const [expanded, setExpanded] = useState<Set<number>>(new Set());

  const sorted = [...groups].sort(
    (a, b) =>
      (b.re?.occurrences ?? 0) + (b.kontra?.occurrences ?? 0) -
      ((a.re?.occurrences ?? 0) + (a.kontra?.occurrences ?? 0)),
  );

  const totalOcc  = (g: OccurrenceGroup) => (g.re?.occurrences ?? 0) + (g.kontra?.occurrences ?? 0);
  const totalWins = (g: OccurrenceGroup) => (g.re?.wins ?? 0) + (g.kontra?.wins ?? 0);
  const totalWR   = (g: OccurrenceGroup) => { const t = totalOcc(g); return t > 0 ? totalWins(g) / t : 0; };
  const weightedAvg = (g: OccurrenceGroup): number | null => {
    const total = totalOcc(g);
    if (total === 0) return null;
    return (
      (g.re ? g.re.avgGameValue * g.re.occurrences : 0) +
      (g.kontra ? g.kontra.avgGameValue * g.kontra.occurrences : 0)
    ) / total;
  };

  const toggle = (id: number) =>
    setExpanded((prev) => { const n = new Set(prev); n.has(id) ? n.delete(id) : n.add(id); return n; });

  return (
    <div className="ps-cgm-table">
      <div className="ps-cgm-header">
        <span className="ps-cgm-th ps-cgm-th-name">{nameHeader}</span>
        <span className="ps-cgm-th">{t.statsColOccurrences}</span>
        <span className="ps-cgm-th">{t.statsColWinRate}</span>
        <span className="ps-cgm-th">{t.statsColAvgShort}</span>
        <span className="ps-cgm-th ps-cgm-th-chevron" />
      </div>

      {sorted.map((g) => {
        const tOcc = totalOcc(g);
        const tWR  = totalWR(g);
        const tAvg = weightedAvg(g);
        const isExpanded = expanded.has(g.id);

        return (
          <div key={g.id} className="ps-cgm-group">
            <button
              className={`ps-cgm-summary${isExpanded ? ' ps-cgm-summary-open' : ''}`}
              onClick={() => toggle(g.id)}
            >
              <span className="ps-cgm-mode-name">{g.name}</span>
              <span className="ps-cgm-val">{tOcc > 0 ? fmtInt(tOcc) : '—'}</span>
              <span className="ps-cgm-val" style={{ color: tOcc > 0 ? colorForRate(tWR) : undefined }}>
                {tOcc > 0 ? fmtRate(tWR) : '—'}
              </span>
              <span className="ps-cgm-val" style={{ color: tAvg != null ? colorForMean(tAvg) : undefined }}>
                {tAvg != null ? fmtMean(tAvg) : '—'}
              </span>
              <span className="ps-cgm-chevron">{isExpanded ? '▴' : '▾'}</span>
            </button>

            {isExpanded && (
              <div className="ps-cgm-sub-rows">
                {([
                  { label: 'Re', labelClass: 'ps-cgm-label-re', stat: g.re },
                  { label: 'Ko', labelClass: 'ps-cgm-label-ko', stat: g.kontra },
                ] as const).map(({ label, labelClass, stat }) => {
                  const isEmpty = !stat || stat.occurrences === 0;
                  return (
                    <div key={label} className={`ps-cgm-sub${isEmpty ? ' ps-cgm-sub-empty' : ''}`}>
                      <span className={`ps-cgm-sub-label ${labelClass}`}>{label}</span>
                      <span className="ps-cgm-val ps-cgm-sub-val">{isEmpty ? '—' : fmtInt(stat!.occurrences)}</span>
                      <span className="ps-cgm-val ps-cgm-sub-val" style={{ color: !isEmpty ? colorForRate(stat!.winRate) : undefined }}>
                        {isEmpty ? '—' : fmtRate(stat!.winRate)}
                      </span>
                      <span className="ps-cgm-val ps-cgm-sub-val" style={{ color: !isEmpty ? colorForMean(stat!.avgGameValue) : undefined }}>
                        {isEmpty ? '—' : fmtMean(stat!.avgGameValue)}
                      </span>
                      <span className="ps-cgm-chevron" />
                    </div>
                  );
                })}
              </div>
            )}
          </div>
        );
      })}
    </div>
  );
}
