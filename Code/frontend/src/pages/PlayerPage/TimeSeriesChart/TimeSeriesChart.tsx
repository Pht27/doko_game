import type { PlayerRound } from '@/types/analog';

export function TimeSeriesChart({ rounds }: { rounds: PlayerRound[] }) {
  if (rounds.length < 2) return null;
  const pts = rounds.map((r) => r.cumulativePoints);
  const w = 360, h = 100, pad = 4;
  const min = Math.min(0, ...pts);
  const max = Math.max(0, ...pts);
  const range = max - min || 1;
  const x = (i: number) => pad + (i / (pts.length - 1)) * (w - pad * 2);
  const y = (v: number) => pad + (1 - (v - min) / range) * (h - pad * 2);
  const linePath = pts.map((v, i) => `${i === 0 ? 'M' : 'L'} ${x(i).toFixed(1)} ${y(v).toFixed(1)}`).join(' ');
  const areaPath = `${linePath} L ${x(pts.length - 1).toFixed(1)} ${y(0).toFixed(1)} L ${x(0).toFixed(1)} ${y(0).toFixed(1)} Z`;
  const zeroY = y(0);
  const last = pts[pts.length - 1];
  const lastColor = last >= 0 ? 'var(--app-win)' : 'var(--app-loss)';

  return (
    <svg viewBox={`0 0 ${w} ${h}`} width="100%" height={h} style={{ display: 'block', overflow: 'visible' }}>
      <defs>
        <linearGradient id="ps-area-grad" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor={`rgb(from var(--app-re) r g b / 0.30)`} />
          <stop offset="100%" stopColor={`rgb(from var(--app-re) r g b / 0)`} />
        </linearGradient>
      </defs>
      <line x1={pad} x2={w - pad} y1={zeroY} y2={zeroY} stroke="var(--app-border-md)" strokeWidth="1" strokeDasharray="3 3" />
      <path d={areaPath} fill="url(#ps-area-grad)" />
      <path d={linePath} fill="none" stroke="var(--app-re)" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
      <circle cx={x(pts.length - 1)} cy={y(last)} r={3.5} fill={lastColor} stroke="var(--app-bg)" strokeWidth={1.5} />
      {(() => {
        const labelText = (last >= 0 ? '+' : '') + last.toFixed(0);
        const pillW = labelText.length * 6.6 + 10;
        const pillH = 15;
        const pillX = w - pad - pillW;
        const pillY = y(last) - 8 - pillH + 3;
        return (
          <>
            <rect x={pillX} y={pillY} width={pillW} height={pillH} rx={4} fill="var(--app-bg)" fillOpacity={0.88} />
            <text x={w - pad} y={y(last) - 8} textAnchor="end" fontSize={11} fontFamily="ui-monospace, monospace" fontWeight={700} fill={lastColor}>
              {labelText}
            </text>
          </>
        );
      })()}
    </svg>
  );
}
