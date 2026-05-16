import { useState } from 'react';
import type { PlayerGameModeStat } from '@/types/analog';
import { colorForRate, colorForMean, fmtInt, fmtRate, fmtMean } from '@/utils/statsUtils';
import { t } from '@/utils/translations';
import type { PointType } from '../PointTypeToggle/PointTypeToggle';
import '@/components/CollapsibleOccurrenceTable/CollapsibleOccurrenceTable.css';

type GameModeGroup = {
  gameModeId: number;
  gameModeName: string;
  re: PlayerGameModeStat | null;
  kontra: PlayerGameModeStat | null;
};

function groupByGameMode(rows: PlayerGameModeStat[]): GameModeGroup[] {
  const map = new Map<number, GameModeGroup>();
  for (const row of rows) {
    if (!map.has(row.gameModeId)) {
      map.set(row.gameModeId, { gameModeId: row.gameModeId, gameModeName: row.gameModeName, re: null, kontra: null });
    }
    const g = map.get(row.gameModeId)!;
    if (row.party === 0) g.re = row;
    else g.kontra = row;
  }
  return Array.from(map.values());
}

export function CollapsibleGameModeTable({ rows, pointType }: { rows: PlayerGameModeStat[]; pointType: PointType }) {
  const [expanded, setExpanded] = useState<Set<number>>(new Set());

  const groups = groupByGameMode(rows).sort(
    (a, b) =>
      (b.re?.games ?? 0) + (b.kontra?.games ?? 0) - ((a.re?.games ?? 0) + (a.kontra?.games ?? 0)),
  );

  const totalGames = (g: GameModeGroup) => (g.re?.games ?? 0) + (g.kontra?.games ?? 0);
  const totalWins  = (g: GameModeGroup) => (g.re?.wins ?? 0) + (g.kontra?.wins ?? 0);
  const totalWR    = (g: GameModeGroup) => { const t = totalGames(g); return t > 0 ? totalWins(g) / t : 0; };

  const avg = (r: PlayerGameModeStat) =>
    pointType === 'earned' ? r.avgPointsEarned
    : pointType === 'wonlost' ? r.avgPointsWonLost
    : r.avgGameValue;

  const weightedAvg = (g: GameModeGroup): number | null => {
    const re = g.re, ko = g.kontra;
    const total = (re?.games ?? 0) + (ko?.games ?? 0);
    if (total === 0) return null;
    return ((re ? avg(re) * re.games : 0) + (ko ? avg(ko) * ko.games : 0)) / total;
  };

  const toggle = (id: number) =>
    setExpanded((prev) => { const n = new Set(prev); n.has(id) ? n.delete(id) : n.add(id); return n; });

  return (
    <div className="ps-cgm-table">
      <div className="ps-cgm-header">
        <span className="ps-cgm-th ps-cgm-th-name">{t.statsColGameModeName}</span>
        <span className="ps-cgm-th">{t.statsColGamesShort}</span>
        <span className="ps-cgm-th">{t.statsColWinRate}</span>
        <span className="ps-cgm-th">{t.statsColAvgShort}</span>
        <span className="ps-cgm-th ps-cgm-th-chevron" />
      </div>

      {groups.map((g) => {
        const tGames = totalGames(g);
        const tWR = totalWR(g);
        const tAvg = weightedAvg(g);
        const isExpanded = expanded.has(g.gameModeId);

        return (
          <div key={g.gameModeId} className="ps-cgm-group">
            <button
              className={`ps-cgm-summary${isExpanded ? ' ps-cgm-summary-open' : ''}`}
              onClick={() => toggle(g.gameModeId)}
            >
              <span className="ps-cgm-mode-name">{g.gameModeName}</span>
              <span className="ps-cgm-val">{tGames > 0 ? fmtInt(tGames) : '—'}</span>
              <span className="ps-cgm-val" style={{ color: tGames > 0 ? colorForRate(tWR) : undefined }}>
                {tGames > 0 ? fmtRate(tWR) : '—'}
              </span>
              <span className="ps-cgm-val" style={{ color: tAvg != null ? colorForMean(tAvg) : undefined }}>
                {tAvg != null ? fmtMean(tAvg) : '—'}
              </span>
              <span className="ps-cgm-chevron">{isExpanded ? '▴' : '▾'}</span>
            </button>

            {isExpanded && (
              <div className="ps-cgm-sub-rows">
                {[
                  { label: 'Re', labelClass: 'ps-cgm-label-re', stat: g.re },
                  { label: 'Ko', labelClass: 'ps-cgm-label-ko', stat: g.kontra },
                ].map(({ label, labelClass, stat }) => {
                  const isEmpty = !stat || stat.games === 0;
                  const sAvg = stat ? avg(stat) : null;
                  return (
                    <div key={label} className={`ps-cgm-sub${isEmpty ? ' ps-cgm-sub-empty' : ''}`}>
                      <span className={`ps-cgm-sub-label ${labelClass}`}>{label}</span>
                      <span className="ps-cgm-val ps-cgm-sub-val">{isEmpty ? '—' : fmtInt(stat!.games)}</span>
                      <span className="ps-cgm-val ps-cgm-sub-val" style={{ color: !isEmpty ? colorForRate(stat!.winRate) : undefined }}>
                        {isEmpty ? '—' : fmtRate(stat!.winRate)}
                      </span>
                      <span className="ps-cgm-val ps-cgm-sub-val" style={{ color: sAvg != null ? colorForMean(sAvg) : undefined }}>
                        {sAvg != null ? fmtMean(sAvg) : '—'}
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
