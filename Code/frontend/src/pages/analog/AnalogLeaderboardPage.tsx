import { useState, useEffect, useCallback, useRef } from 'react';
import { Link } from 'react-router-dom';
import { getPlayers, getPlayer } from '@/api/analog';
import { t } from '@/utils/translations';
import type { PlayerListItem, PlayerDetail } from '@/types/analog';
import { PageHeader } from '@/components/PageHeader/PageHeader';
import { ToggleSwitch } from '@/components/ToggleSwitch/ToggleSwitch';
import { StatusState } from '@/components/StatusState/StatusState';
import './AnalogLeaderboardPage.css';

const PLAYER_COLORS = [
  '#818cf8', '#f472b6', '#34d399', '#fbbf24',
  '#60a5fa', '#fb923c', '#a78bfa', '#4ade80',
];

const CHART_H = 150;
const CHART_PAD = { top: 8, right: 8, bottom: 20, left: 32 };
const MAX_ROUNDS = 24;

type SortKey = 'totalPoints' | 'winRate' | 'gamesPlayed';
type DetailState = PlayerDetail | 'loading' | 'error';

// ── Custom toggle switch ────────────────────────────────────────────────────

// ── SVG multi-line chart ────────────────────────────────────────────────────

interface ChartSeries { name: string; color: string; points: number[]; }

function MultiLineChart({ series }: { series: ChartSeries[] }) {
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
          vor {MAX_ROUNDS} Spielen
        </text>
        <text x={W - CHART_PAD.right} y={CHART_H - 4} fontSize={9}
          fill="rgba(255,255,255,0.3)" fontFamily="ui-monospace, monospace"
          textAnchor="end">
          heute
        </text>
      </svg>
    </div>
  );
}

// ── Expandable row ──────────────────────────────────────────────────────────

function ExpandableRow({
  player, rank, color, detail, expanded, onToggle,
}: {
  player: PlayerListItem;
  rank: number;
  color: string;
  detail: DetailState | undefined;
  expanded: boolean;
  onToggle: () => void;
}) {
  const positive = player.totalPoints >= 0;
  const winPct = Math.round((player.winRate ?? 0) * 100);
  const avgSign = player.avgPointsPerGame >= 0 ? '+' : '';
  const avgLabel = `${avgSign}${player.avgPointsPerGame.toFixed(2)}`;
  const pointsLabel = `${positive ? '+' : ''}${player.totalPoints.toFixed(1)}`;

  const rounds = detail && detail !== 'loading' && detail !== 'error' ? detail.recentRounds : null;
  const cumulativePts = rounds?.map((r) => r.cumulativePoints) ?? [];
  const deltaPts = rounds?.map((r) => r.points) ?? [];
  const maxDelta = deltaPts.length ? Math.max(...deltaPts) : null;
  const minDelta = deltaPts.length ? Math.min(...deltaPts) : null;

  // Use cumulative trajectory as single-player "chart" data
  const chartSeries: ChartSeries[] = rounds && cumulativePts.length >= 2
    ? [{ name: player.name, color, points: cumulativePts.slice(-MAX_ROUNDS) }]
    : [];

  const handleHeadClick = (e: React.MouseEvent) => {
    if ((e.target as HTMLElement).closest('a')) return;
    onToggle();
  };

  return (
    <div style={{ borderBottom: '1px solid rgba(255,255,255,0.08)' }}>
      <div
        className={`alb-row-head${expanded ? ' alb-row-head--open' : ''}`}
        onClick={handleHeadClick}
        role="button"
        aria-expanded={expanded}
        tabIndex={0}
        onKeyDown={(e) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); onToggle(); } }}
      >
        <div className="alb-row-rank-wrap">
          <span className="alb-rank-dot" style={{ background: color }} />
          <span className={`alb-row-rank${rank <= 3 ? ' alb-row-rank--top' : ''}`}>{rank}</span>
        </div>

        <div className="alb-row-info">
          <Link
            to={`/analog/players/${player.id}`}
            className={`alb-row-name${!player.isActive ? ' alb-row-name--inactive' : ''}`}
          >
            {player.name}
          </Link>
          <div className="alb-row-stats">
            {winPct.toFixed(1)}% WR · {player.gamesPlayed} Spiele
          </div>
        </div>

        <div className="alb-row-right">
          <span className={`alb-row-points${positive ? ' alb-row-points--pos' : ' alb-row-points--neg'}`}>
            {pointsLabel}
          </span>
          {player.gamesPlayed > 0 && (
            <span className="alb-row-avg">Ø {avgLabel}</span>
          )}
        </div>

        <span className={`alb-row-chevron${expanded ? ' alb-row-chevron--open' : ''}`}>›</span>
      </div>

      <div className={`alb-row-body${expanded ? ' alb-row-body--open' : ''}`}>
        <div className="alb-row-body-inner">
          <div className="alb-row-detail">
            {detail === 'loading' && <StatusState type="loading" className="alb-row-detail-loading" />}
            {detail === 'error' && <StatusState type="error" message="Fehler beim Laden" className="alb-row-detail-loading" />}
            {rounds && (
              <>
                {chartSeries.length >= 1 && (
                  <MultiLineChart series={chartSeries} />
                )}
                <div className="alb-mini-grid" style={{ borderTop: `2px solid ${color}` }}>
                  <div className="alb-mini-cell">
                    <span className="alb-mini-val alb-mini-val--pos">
                      {maxDelta !== null ? `+${maxDelta.toFixed(1)}` : '—'}
                    </span>
                    <span className="alb-mini-lbl">{t.analogLeaderboardHigh}</span>
                  </div>
                  <div className="alb-mini-cell">
                    <span className={`alb-mini-val${minDelta !== null && minDelta < 0 ? ' alb-mini-val--neg' : ''}`}>
                      {minDelta !== null ? `${minDelta > 0 ? '+' : ''}${minDelta.toFixed(1)}` : '—'}
                    </span>
                    <span className="alb-mini-lbl">{t.analogLeaderboardLow}</span>
                  </div>
                  <div className="alb-mini-cell">
                    <span className="alb-mini-val">{player.wins}S · {player.losses}N</span>
                    <span className="alb-mini-lbl">Bilanz</span>
                  </div>
                </div>
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

// ── Main page ───────────────────────────────────────────────────────────────

export function AnalogLeaderboardPage() {
  const [players, setPlayers] = useState<PlayerListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [sortBy, setSortBy] = useState<SortKey>('totalPoints');
  const [showInactive, setShowInactive] = useState(false);
  const [topN, setTopN] = useState<3 | 5 | 8>(5);
  const [expandedId, setExpandedId] = useState<number | null>(null);

  const [details, setDetails] = useState<Record<number, DetailState>>({});
  const fetchedIds = useRef(new Set<number>());

  const fetchDetail = useCallback((id: number) => {
    if (fetchedIds.current.has(id)) return;
    fetchedIds.current.add(id);
    setDetails((prev) => ({ ...prev, [id]: 'loading' }));
    getPlayer(id)
      .then((d) => setDetails((prev) => ({ ...prev, [id]: d })))
      .catch(() => setDetails((prev) => ({ ...prev, [id]: 'error' })));
  }, []);

  useEffect(() => {
    getPlayers()
      .then((data) => { setPlayers(data); setLoading(false); })
      .catch((err: unknown) => { setError(err instanceof Error ? err.message : 'Fehler'); setLoading(false); });
  }, []);

  useEffect(() => {
    if (players.length === 0) return;
    [...players]
      .filter((p) => p.isActive)
      .sort((a, b) => b.totalPoints - a.totalPoints)
      .slice(0, 5)
      .forEach((p) => fetchDetail(p.id));
  }, [players, fetchDetail]);

  const colorMap = new Map<number, string>(
    [...players]
      .filter((p) => p.isActive)
      .sort((a, b) => b.totalPoints - a.totalPoints)
      .map((p, i) => [p.id, PLAYER_COLORS[i % PLAYER_COLORS.length]])
  );

  const sorted = [...players]
    .filter((p) => showInactive || p.isActive)
    .sort((a, b) => {
      if (sortBy === 'totalPoints') return b.totalPoints - a.totalPoints;
      if (sortBy === 'winRate') return b.winRate - a.winRate;
      return b.gamesPlayed - a.gamesPlayed;
    });

  const heroSeries: ChartSeries[] = [...players]
    .filter((p) => p.isActive)
    .sort((a, b) => b.totalPoints - a.totalPoints)
    .slice(0, topN)
    .flatMap((p) => {
      const d = details[p.id];
      if (!d || d === 'loading' || d === 'error') return [];
      const points = d.recentRounds.slice(-MAX_ROUNDS).map((r) => r.cumulativePoints);
      if (points.length < 2) return [];
      return [{ name: p.name, color: colorMap.get(p.id) ?? PLAYER_COLORS[0], points }];
    });

  const handleToggle = (id: number) => {
    if (expandedId === id) { setExpandedId(null); return; }
    setExpandedId(id);
    fetchDetail(id);
  };

  return (
    <div className="alb-page">
      <PageHeader title={t.analogLeaderboardTitle} backTo="/" />

      <div className="alb-body">
        {/* Hero chart */}
        <div className="alb-hero">
          <div className="alb-hero-head">
            <div className="alb-topn-row">
              {([3, 5, 8] as const).map((n) => (
                <button key={n}
                  className={`alb-topn-btn${topN === n ? ' alb-topn-btn--active' : ''}`}
                  onClick={() => setTopN(n)}>
                  Top {n}
                </button>
              ))}
            </div>
          </div>

          {loading ? (
            <StatusState type="loading" />
          ) : heroSeries.length === 0 ? (
            <div className="alb-chart-empty">Noch nicht genug Daten</div>
          ) : (
            <MultiLineChart series={heroSeries} />
          )}

        </div>

        {/* Toolbar */}
        <div className="alb-toolbar">
          <div className="alb-select-wrap">
            <select
              className="alb-select"
              value={sortBy}
              onChange={(e) => setSortBy(e.target.value as SortKey)}
            >
              <option value="totalPoints">{t.analogLeaderboardSortPoints}</option>
              <option value="winRate">{t.analogLeaderboardSortWinRate}</option>
              <option value="gamesPlayed">{t.analogLeaderboardSortGames}</option>
            </select>
            <span className="alb-select-arrow" aria-hidden>▾</span>
          </div>

          <div className="alb-inactive-row">
            <span className="alb-inactive-label">{t.analogLeaderboardShowInactive}</span>
            <ToggleSwitch on={showInactive} onChange={() => setShowInactive((v) => !v)} size="sm" />
          </div>
        </div>

        {/* List */}
        <div className="alb-list">
          {loading && <StatusState type="loading" />}
          {error && <StatusState type="error" message={error} />}
          {!loading && !error && sorted.length === 0 && (
            <StatusState type="empty" message={t.analogLeaderboardNoPlayers} />
          )}
          {sorted.map((player, idx) => (
            <ExpandableRow
              key={player.id}
              player={player}
              rank={idx + 1}
              color={colorMap.get(player.id) ?? PLAYER_COLORS[idx % PLAYER_COLORS.length]}
              detail={details[player.id]}
              expanded={expandedId === player.id}
              onToggle={() => handleToggle(player.id)}
            />
          ))}
        </div>
      </div>
    </div>
  );
}
