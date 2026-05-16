import type { PlayerStats } from '@/types/analog';
import { colorForRate, colorForMean, fmtRate, fmtMean, fmtInt } from '@/utils/statsUtils';
import { t } from '@/utils/translations';
import './AloneStats.css';

export function AloneStats({ stats }: { stats: PlayerStats }) {
  const rows = [
    { k: t.playerAlonePlayed,  v: fmtInt(stats.aloneGames),             c: 'var(--app-text)' },
    { k: t.playerAloneWon,     v: fmtInt(stats.aloneWins),              c: 'var(--app-win)' },
    { k: t.playerAloneWinRate, v: fmtRate(stats.aloneWinRate),          c: colorForRate(stats.aloneWinRate) },
    { k: t.playerAloneAvgNet,  v: fmtMean(stats.aloneAvgPointsEarned), c: colorForMean(stats.aloneAvgPointsEarned) },
  ];

  return (
    <div className="ps-alone-stats">
      <div className="ps-alone-section-label">{t.playerAloneSectionLabel}</div>
      {rows.map((r, i) => (
        <div key={r.k} className={`ps-alone-row${i === rows.length - 1 ? ' ps-alone-row-last' : ''}`}>
          <span className="ps-alone-key">{r.k}</span>
          <span className="ps-alone-val" style={{ color: r.c }}>{r.v}</span>
        </div>
      ))}
    </div>
  );
}
