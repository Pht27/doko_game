import type { GameModeStat } from '@/types/analog';
import { colorForRate, colorForMean, fmtRate, fmtMean, fmtInt } from '@/utils/statsUtils';
import { t } from '@/utils/translations';
import './StatsHero.css';

export function StatsHero({ gameModes }: { gameModes: GameModeStat[] }) {
  if (gameModes.length === 0) return null;

  const totalRounds = gameModes.reduce((s, m) => s + m.totalRounds, 0);

  const modesWithWR = gameModes.filter((m) => m.reWinRate != null);
  const reWinRoundsSum = modesWithWR.reduce((s, m) => s + m.totalRounds, 0);
  const overallReWR =
    reWinRoundsSum > 0
      ? modesWithWR.reduce((s, m) => s + (m.reWinRate ?? 0) * m.totalRounds, 0) / reWinRoundsSum
      : null;
  const overallKoWR = overallReWR != null ? 1 - overallReWR : null;

  const totalRoundsForAvg = gameModes.reduce((s, m) => s + m.totalRounds, 0);
  const overallAvg =
    totalRoundsForAvg > 0
      ? gameModes.reduce((s, m) => s + m.avgGameValue * m.totalRounds, 0) / totalRoundsForAvg
      : null;

  const soloRounds = gameModes
    .filter((m) => m.gameModeName.toLowerCase().includes('solo') || m.gameModeName === 'Fleischloser')
    .reduce((s, m) => s + m.totalRounds, 0);

  return (
    <div className="sts-hero">
      <div className="sts-hero-headline">
        <span className="sts-hero-label">{t.statsHeroRoundsLabel}</span>
      </div>
      <div className="sts-hero-total">{fmtInt(totalRounds)}</div>

      {overallReWR != null && overallKoWR != null && (
        <div className="sts-hero-wr-section">
          <div className="sts-hero-wr-row">
            <div className="sts-hero-wr-side">
              <span className="sts-hero-wr-dot sts-hero-wr-dot-re" />
              <span className="sts-hero-wr-party sts-hero-wr-party-re">{t.reLabelShort}</span>
              <span className="sts-hero-wr-val" style={{ color: colorForRate(overallReWR) }}>
                {fmtRate(overallReWR)}
              </span>
            </div>
            <div className="sts-hero-wr-side sts-hero-wr-side-right">
              <span className="sts-hero-wr-val" style={{ color: colorForRate(overallKoWR) }}>
                {fmtRate(overallKoWR)}
              </span>
              <span className="sts-hero-wr-party sts-hero-wr-party-ko">{t.kontraLabelShort}</span>
              <span className="sts-hero-wr-dot sts-hero-wr-dot-ko" />
            </div>
          </div>
          <div className="sts-hero-wr-bar">
            <div className="sts-hero-wr-bar-fill" style={{ width: `${overallReWR * 100}%` }} />
          </div>
        </div>
      )}

      <div className="sts-hero-mini-row">
        <div className="sts-hero-mini-cell">
          <span className="sts-hero-mini-val">
            {overallAvg != null ? (
              <span style={{ color: colorForMean(overallAvg) }}>{fmtMean(overallAvg)}</span>
            ) : '—'}
          </span>
          <span className="sts-hero-mini-label">{t.statsHeroAvgValue}</span>
        </div>
        <div className="sts-hero-mini-divider" />
        <div className="sts-hero-mini-cell">
          <span className="sts-hero-mini-val">{fmtInt(soloRounds)}</span>
          <span className="sts-hero-mini-label">{t.statsHeroSolosPlayed}</span>
        </div>
      </div>
    </div>
  );
}
