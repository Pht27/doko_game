import { cardSvgPathById } from '@/api/cards';
import './HeroSection.css';

function HeroCardFallback() {
  const w = 64, h = 92, r = 7;
  const suit = '♦';
  const rank = 'A';
  const color = '#c0392b';
  return (
    <svg
      width={w}
      height={h}
      viewBox={`0 0 ${w} ${h}`}
      style={{ display: 'block', borderRadius: r, flexShrink: 0 }}
    >
      <rect x={0.5} y={0.5} width={w - 1} height={h - 1} rx={r} fill="#fafafa" stroke="rgba(0,0,0,0.18)" />
      <rect x={3} y={3} width={w - 6} height={h - 6} rx={r - 2} fill="none" stroke="rgba(0,0,0,0.05)" />
      <text x={w * 0.14} y={h * 0.17} fontFamily="Georgia, serif" fontWeight={700} fontSize={12} fill={color}>{rank}</text>
      <text x={w * 0.14} y={h * 0.27} fontFamily="sans-serif" fontSize={8} fill={color}>{suit}</text>
      <text x={w / 2} y={h / 2 + 16} textAnchor="middle" fontFamily="sans-serif" fontSize={38} fill={color}>{suit}</text>
      <g transform={`rotate(180 ${w * 0.86} ${h * 0.835})`}>
        <text x={w * 0.86} y={h * 0.835} fontFamily="Georgia, serif" fontWeight={700} fontSize={12} fill={color}>{rank}</text>
        <text x={w * 0.86} y={h * 0.92} fontFamily="sans-serif" fontSize={8} fill={color}>{suit}</text>
      </g>
    </svg>
  );
}

export function HeroCardDisplay({ heroCard }: { heroCard: string | null }) {
  const url = heroCard ? cardSvgPathById(heroCard) : null;
  return url ? (
    <img src={url} width={64} height={92} style={{ display: 'block', borderRadius: 7 }} alt={heroCard ?? ''} />
  ) : (
    <HeroCardFallback />
  );
}

export function RankBadge({ rank }: { rank: number | null }) {
  if (!rank) return null;
  const medals: Record<number, string> = { 1: '🥇', 2: '🥈', 3: '🥉' };
  const medal = medals[rank];
  return (
    <div className="ps-rank-badge">
      {medal ? <span className="ps-rank-medal">{medal}</span> : null}
      <span className="ps-rank-text">#{rank}</span>
    </div>
  );
}
