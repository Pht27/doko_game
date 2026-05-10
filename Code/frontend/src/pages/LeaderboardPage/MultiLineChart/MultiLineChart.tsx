import { t } from '@/utils/translations';

export const CHART_H = 150;
export const CHART_PAD = { top: 8, right: 8, bottom: 20, left: 32 };
export const MAX_ROUNDS = 50;

export interface ChartSeries { name: string; color: string; points: number[]; }

export function MultiLineChart({ series }: { series: ChartSeries[] }) {
  const allPoints = series.flatMap((s) => s.points);
  if (allPoints.length === 0) return null;

  let minY = Math.min(...allPoints);
  let maxY = Math.max(...allPoints);
  if (minY > 0) minY = 0;
  if (maxY < 0) maxY = 0;
  const rangeY = maxY - minY || 1;

  const W = 372;
  const ticks = 4;
  const gridVals = Array.from({ length: ticks + 1 }, (_, k) => minY + (rangeY * k) / ticks);

  const toX = (i: number, len: number) => {
    const slot = MAX_ROUNDS - 1;
    const offset = slot - (len - 1);
    return CHART_PAD.left + ((offset + i) / slot) * (W - CHART_PAD.left - CHART_PAD.right);
  };
  const toY = (v: number) =>
    CHART_PAD.top + (1 - (v - minY) / rangeY) * (CHART_H - CHART_PAD.top - CHART_PAD.bottom);

  return (
    <div className="alb-chart-wrap">
      <svg className="alb-chart" viewBox={`0 0 ${W} ${CHART_H}`} preserveAspectRatio="none">
        {gridVals.map((g, i) => {
          const yy = toY(g);
          const isZero = Math.abs(g) < 1e-6;
          return (
            <g key={i}>
              <line x1={CHART_PAD.left} x2={W - CHART_PAD.right} y1={yy} y2={yy}
                stroke={isZero ? 'rgba(255,255,255,0.18)' : 'rgba(255,255,255,0.05)'}
                strokeWidth={1} />
              <text x={CHART_PAD.left - 5} y={yy + 3}
                textAnchor="end" fontSize={9} fill="rgba(255,255,255,0.3)"
                fontFamily="ui-monospace, monospace">
                {Math.round(g)}
              </text>
            </g>
          );
        })}
        {series.map((s) => {
          if (s.points.length < 2) return null;
          const d = s.points.map((p, i) =>
            `${i === 0 ? 'M' : 'L'}${toX(i, s.points.length).toFixed(1)},${toY(p).toFixed(1)}`
          ).join(' ');
          const lx = toX(s.points.length - 1, s.points.length);
          const ly = toY(s.points[s.points.length - 1]);
          return (
            <g key={s.name}>
              <path d={d} stroke={s.color} fill="none" strokeWidth={1.8}
                strokeLinecap="round" strokeLinejoin="round" />
              <circle cx={lx} cy={ly} r={2.5} fill={s.color} />
            </g>
          );
        })}
        <text x={CHART_PAD.left} y={CHART_H - 4} fontSize={9}
          fill="rgba(255,255,255,0.3)" fontFamily="ui-monospace, monospace">
          {t.analogLeaderboardChartPast}
        </text>
        <text x={W - CHART_PAD.right} y={CHART_H - 4} fontSize={9}
          fill="rgba(255,255,255,0.3)" fontFamily="ui-monospace, monospace"
          textAnchor="end">
          {t.analogLeaderboardChartToday}
        </text>
      </svg>
    </div>
  );
}
