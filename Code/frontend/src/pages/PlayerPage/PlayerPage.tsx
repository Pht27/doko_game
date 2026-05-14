import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { usePlayerStats } from '@/hooks/usePlayerStats';
import { usePlayerRounds } from '@/hooks/usePlayerRounds';
import { t } from '@/utils/translations';
import { BackButton } from '@/components/BackButton/BackButton';
import { StatusState } from '@/components/StatusState/StatusState';
import { SortableTable } from '@/components/SortableTable/SortableTable';
import { BottomSheet } from '@/components/BottomSheet/BottomSheet';
import { RoundCard } from '@/pages/HistoryPage/RoundCard/RoundCard';
import type { Column } from '@/components/SortableTable/SortableTable';
import type {
  PlayerStats,
  PlayerGameModeStat,
  PlayerSpecialCardStat,
  PlayerExtraPointStat,
  PlayerPartnerStat,
  PlayerRound,
  PlayerRoundListItem,
} from '@/types/analog';
import { colorForRate, colorForMean, colorForTotal, fmtRate, fmtMean, fmtInt } from '@/utils/statsUtils';
import './PlayerPage.css';

// ── Point type toggle ─────────────────────────────────────────────────────────

type PointType = 'value' | 'wonlost' | 'earned';
type TabId = 'gm' | 'sc' | 'ep' | 'pt' | 'al';

function PointTypeInfoSheet({ onClose }: { onClose: () => void }) {
  return (
    <BottomSheet title="Punktetypen" onClose={onClose}>
      <div className="ps-pt-info">
        <div className="ps-pt-info-item">
          <span className="ps-pt-info-name">Spielwert</span>
          <span className="ps-pt-info-desc">Der reine Spielwert ohne Solofaktor und Teamfaktor. Vergleichbar unabhängig von Spielkonstellation.</span>
        </div>
        <div className="ps-pt-info-item">
          <span className="ps-pt-info-name">Bruttopunkte</span>
          <span className="ps-pt-info-desc">Spielwert × Solofaktor (z. B. ×3 bei Solo). Zeigt, was die Partei insgesamt gewonnen oder verloren hat.</span>
        </div>
        <div className="ps-pt-info-item">
          <span className="ps-pt-info-name">Nettopunkte</span>
          <span className="ps-pt-info-desc">Bruttopunkte ÷ Teamgröße. Der persönliche Anteil – was tatsächlich auf dem Konto landet.</span>
        </div>
      </div>
    </BottomSheet>
  );
}

function PointTypeToggle({
  value,
  onChange,
  disabledOptions = [],
}: {
  value: PointType;
  onChange: (v: PointType) => void;
  disabledOptions?: PointType[];
}) {
  const [showInfo, setShowInfo] = useState(false);
  const opts: { key: PointType; label: string }[] = [
    { key: 'value', label: 'Spielwert' },
    { key: 'wonlost', label: 'Brutto' },
    { key: 'earned', label: 'Netto' },
  ];
  return (
    <>
      <div className="ps-pt-row">
        <button className="ps-pt-info-btn" onClick={() => setShowInfo(true)} aria-label="Punktetypen erklären">?</button>
        <span className="ps-pt-label">Ø zeigt</span>
        <div className="ps-pt-toggle">
          {opts.map((o) => {
            const isDisabled = disabledOptions.includes(o.key);
            return (
              <button
                key={o.key}
                className={`ps-pt-btn${value === o.key ? ' ps-pt-active' : ''}${isDisabled ? ' ps-pt-locked' : ''}`}
                disabled={isDisabled}
                onClick={() => onChange(o.key)}
              >
                {o.label}
              </button>
            );
          })}
        </div>
      </div>
      {showInfo && <PointTypeInfoSheet onClose={() => setShowInfo(false)} />}
    </>
  );
}

// ── Game mode collapsible table ───────────────────────────────────────────────

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
      map.set(row.gameModeId, {
        gameModeId: row.gameModeId,
        gameModeName: row.gameModeName,
        re: null,
        kontra: null,
      });
    }
    const g = map.get(row.gameModeId)!;
    if (row.party === 0) g.re = row;
    else g.kontra = row;
  }
  return Array.from(map.values());
}

function CollapsibleGameModeTable({
  rows,
  pointType,
}: {
  rows: PlayerGameModeStat[];
  pointType: PointType;
}) {
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
    const reContrib = re ? avg(re) * re.games : 0;
    const koContrib = ko ? avg(ko) * ko.games : 0;
    return (reContrib + koContrib) / total;
  };

  const toggle = (id: number) => {
    setExpanded((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  return (
    <div className="ps-cgm-table">
      {/* Header */}
      <div className="ps-cgm-header">
        <span className="ps-cgm-th ps-cgm-th-name">Spielmodus</span>
        <span className="ps-cgm-th">Sp.</span>
        <span className="ps-cgm-th">WR</span>
        <span className="ps-cgm-th">Ø</span>
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
              <span
                className="ps-cgm-val"
                style={{ color: tGames > 0 ? colorForRate(tWR) : undefined }}
              >
                {tGames > 0 ? fmtRate(tWR) : '—'}
              </span>
              <span
                className="ps-cgm-val"
                style={{ color: tAvg != null ? colorForMean(tAvg) : undefined }}
              >
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
                    <div
                      key={label}
                      className={`ps-cgm-sub${isEmpty ? ' ps-cgm-sub-empty' : ''}`}
                    >
                      <span className={`ps-cgm-sub-label ${labelClass}`}>{label}</span>
                      <span className="ps-cgm-val ps-cgm-sub-val">
                        {isEmpty ? '—' : fmtInt(stat!.games)}
                      </span>
                      <span
                        className="ps-cgm-val ps-cgm-sub-val"
                        style={{ color: !isEmpty ? colorForRate(stat!.winRate) : undefined }}
                      >
                        {isEmpty ? '—' : fmtRate(stat!.winRate)}
                      </span>
                      <span
                        className="ps-cgm-val ps-cgm-sub-val"
                        style={{ color: sAvg != null ? colorForMean(sAvg) : undefined }}
                      >
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

// ── Hero card SVG ─────────────────────────────────────────────────────────────

function CardFace() {
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

function RankBadge({ rank }: { rank: number | null }) {
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

// ── Best / worst round cards ──────────────────────────────────────────────────

function BestWorstCards({
  best,
  worst,
}: {
  best: PlayerRoundListItem | null;
  worst: PlayerRoundListItem | null;
}) {
  if (!best && !worst) return null;

  function MiniCard({
    round,
    label,
    labelClass,
  }: {
    round: PlayerRoundListItem | null;
    label: string;
    labelClass: string;
  }) {
    if (!round) return null;
    const won = round.pointDelta >= 0;
    const date = new Date(round.playedAt).toLocaleDateString('de-DE', { day: '2-digit', month: 'short', year: '2-digit' });
    const delta = round.pointDelta;
    const pts = (delta >= 0 ? '+' : '') + delta.toFixed(0);
    return (
      <div className={`ps-bw-card ${labelClass}`}>
        <div className="ps-bw-card-label">{label}</div>
        <div className={`ps-bw-card-pts${won ? ' ps-bw-pts-win' : ' ps-bw-pts-loss'}`}>{pts}</div>
        <div className="ps-bw-card-mode">{round.gameMode}</div>
        <div className="ps-bw-card-date">{date}</div>
      </div>
    );
  }

  return (
    <div className="ps-bw-row">
        <MiniCard round={best} label="Bestes Spiel" labelClass="ps-bw-best" />
        <MiniCard round={worst} label="Schlechtestes" labelClass="ps-bw-worst" />
    </div>
  );
}

// ── Time series chart ─────────────────────────────────────────────────────────

function TimeSeriesChart({ rounds }: { rounds: PlayerRound[] }) {
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
          <stop offset="0%" stopColor={`rgba(var(--app-re-raw), 0.30)`} />
          <stop offset="100%" stopColor={`rgba(var(--app-re-raw), 0)`} />
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

// ── Alone stats ───────────────────────────────────────────────────────────────

function AloneStats({ stats }: { stats: PlayerStats }) {
  const rows = [
    { k: 'Gespielt',  v: fmtInt(stats.aloneGames),           c: 'var(--app-text)' },
    { k: 'Gewonnen',  v: fmtInt(stats.aloneWins),            c: 'var(--app-win)' },
    { k: 'Winrate',   v: fmtRate(stats.aloneWinRate),        c: colorForRate(stats.aloneWinRate) },
    { k: 'Ø Punktediff', v: fmtMean(stats.aloneAvgPointsEarned), c: colorForMean(stats.aloneAvgPointsEarned) },
  ];

  return (
    <div className="ps-alone-stats">
      <div className="ps-alone-section-label">Alleine gespielt</div>
      {rows.map((r, i) => (
        <div key={r.k} className={`ps-alone-row${i === rows.length - 1 ? ' ps-alone-row-last' : ''}`}>
          <span className="ps-alone-key">{r.k}</span>
          <span className="ps-alone-val" style={{ color: r.c }}>{r.v}</span>
        </div>
      ))}
    </div>
  );
}

function getPageNumbers(current: number, total: number): (number | 'ellipsis')[] {
  if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);
  const pages: (number | 'ellipsis')[] = [1];
  if (current > 3) pages.push('ellipsis');
  for (let p = Math.max(2, current - 1); p <= Math.min(total - 1, current + 1); p++) {
    pages.push(p);
  }
  if (current < total - 2) pages.push('ellipsis');
  pages.push(total);
  return pages;
}

// ── Match history section ─────────────────────────────────────────────────────

function MatchHistoryCompactRow({
  round,
  playerId,
  expanded,
  onToggle,
}: {
  round: PlayerRoundListItem;
  playerId: number;
  expanded: boolean;
  onToggle: () => void;
}) {
  const date = new Date(round.playedAt);
  const isRe = round.rePlayers.some((p) => p.id === playerId);
  const won = round.pointDelta >= 0;
  const partners = round.teamPartners;

  const dateStr = date.toLocaleDateString('de-DE', { day: '2-digit', month: 'short' });
  const delta = round.pointDelta;
  const pts = (delta >= 0 ? '+' : '') + delta.toFixed(0);

  return (
    <div className={`ps-mh-row-wrap${expanded ? ' ps-mh-row-wrap-open' : ''}`}>
      <button className="ps-mh-compact-row" onClick={onToggle}>
        {/* Left: W/L pill + date */}
        <div className="ps-mh-left">
          <span className={`ps-mh-pill${won ? ' ps-mh-pill-win' : ' ps-mh-pill-loss'}`}>
            {won ? 'W' : 'L'}
          </span>
          <span className="ps-mh-date">{dateStr}</span>
        </div>

        {/* Center: mode on top, team partner below */}
        <div className="ps-mh-center">
          <div className="ps-mh-mode-line">
            <span className="ps-mh-mode">{round.gameMode}</span>
          </div>
          {partners.length > 0 && (
            <div className="ps-mh-partner">mit {partners.map((p) => p.name).join(', ')}</div>
          )}
        </div>

        {/* Party column */}
        <span className={`ps-mh-party-col${isRe ? ' ps-mh-party-re' : ' ps-mh-party-ko'}`}>
          {isRe ? 'Re' : 'Ko'}
        </span>

        {/* Right: points + chevron */}
        <div className="ps-mh-right">
          <span className={`ps-mh-pts${won ? ' ps-mh-pts-win' : ' ps-mh-pts-loss'}`}>{pts}</span>
          <span className="ps-mh-chevron">{expanded ? '▴' : '▾'}</span>
        </div>
      </button>

      {expanded && (
        <div className="ps-mh-expanded">
          <RoundCard round={round} onDelete={() => {}} readOnly />
        </div>
      )}
    </div>
  );
}

function MatchHistorySection({ playerId }: { playerId: number }) {
  const { rounds, total, page, totalPages, loading, error, goToPage } = usePlayerRounds(playerId);
  const [expandedIds, setExpandedIds] = useState<Set<number>>(new Set());

  const toggle = (id: number) => {
    setExpandedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  return (
    <div className="ps-mh-section">
      <div className="ps-mh-header">
        <span className="ps-section-label">Spielverlauf</span>
        {total > 0 && <span className="ps-mh-count">{total} Spiele</span>}
      </div>

      {loading && <StatusState type="loading" />}
      {error && <StatusState type="error" message={error} />}
      {!loading && !error && rounds.length === 0 && (
        <div className="ps-mh-empty">Keine Spiele vorhanden.</div>
      )}

      {!loading && rounds.map((round) => (
        <MatchHistoryCompactRow
          key={round.id}
          round={round}
          playerId={playerId}
          expanded={expandedIds.has(round.id)}
          onToggle={() => toggle(round.id)}
        />
      ))}

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="ps-mh-pagination">
          <button
            className="ps-mh-page-btn ps-mh-page-arrow"
            disabled={page <= 1 || loading}
            onClick={() => goToPage(page - 1)}
          >
            ‹
          </button>
          {getPageNumbers(page, totalPages).map((p, i) =>
            p === 'ellipsis' ? (
              <span key={`ellipsis-${i}`} className="ps-mh-page-ellipsis">…</span>
            ) : (
              <button
                key={p}
                className={`ps-mh-page-btn${p === page ? ' ps-mh-page-active' : ''}`}
                disabled={p === page || loading}
                onClick={() => goToPage(p)}
              >
                {p}
              </button>
            )
          )}
          <button
            className="ps-mh-page-btn ps-mh-page-arrow"
            disabled={page >= totalPages || loading}
            onClick={() => goToPage(page + 1)}
          >
            ›
          </button>
        </div>
      )}
    </div>
  );
}

// ── Main page ─────────────────────────────────────────────────────────────────

const TABS: { id: TabId; label: string }[] = [
  { id: 'gm', label: 'Spielmodi' },
  { id: 'sc', label: 'Sonderkarten' },
  { id: 'ep', label: 'Extrapunkte' },
  { id: 'pt', label: 'Teamstatistik' },
  { id: 'al', label: 'Alleine' },
];

export function PlayerPage() {
  const { id } = useParams<{ id: string }>();
  const playerId = Number(id);
  const data = usePlayerStats(playerId);
  const [activeTab, setActiveTab] = useState<TabId>('gm');
  const [pointType, setPointType] = useState<PointType>('earned');

  if (data.loading) {
    return (
      <div className="ps-page">
        <StatusState type="loading" />
      </div>
    );
  }

  if (data.error || !data.stats) {
    return (
      <div className="ps-page">
        <StatusState type="error" message={data.error ?? t.analogPlayerNotFound} />
      </div>
    );
  }

  const { stats, detail, gameModes, specialCards, extraPoints, partners, bestRound, worstRound, rank } = data;

  const scAvg = (r: PlayerSpecialCardStat) =>
    effectivePointType === 'wonlost' || effectivePointType === 'earned' ? r.avgPointsWonLost : r.avgGameValue;

  const specialCardColumns: Column<PlayerSpecialCardStat>[] = [
    { key: 'specialCardName', label: 'Karte' },
    {
      key: 'occurrences',
      label: 'Anz.',
      sortValue: (r) => r.occurrences,
      render: (r) => fmtInt(r.occurrences),
    },
    {
      key: 'winRate',
      label: 'WR',
      sortValue: (r) => r.winRate,
      render: (r) => <span style={{ color: colorForRate(r.winRate) }}>{fmtRate(r.winRate)}</span>,
    },
    {
      key: 'avg',
      label: 'Ø',
      sortValue: (r) => scAvg(r),
      render: (r) => <span style={{ color: colorForMean(scAvg(r)) }}>{fmtMean(scAvg(r))}</span>,
    },
  ];

  const extraPointColumns: Column<PlayerExtraPointStat>[] = [
    { key: 'extraPointName', label: 'Extrapunkt' },
    {
      key: 'occurrences',
      label: 'Anz.',
      sortValue: (r) => r.occurrences,
      render: (r) => fmtInt(r.occurrences),
    },
    {
      key: 'winRate',
      label: 'WR',
      sortValue: (r) => r.winRate,
      render: (r) => <span style={{ color: colorForRate(r.winRate) }}>{fmtRate(r.winRate)}</span>,
    },
    {
      key: 'avgGameValue',
      label: 'Ø Wert',
      sortValue: (r) => r.avgGameValue,
      render: (r) => (
        <span style={{ color: colorForMean(r.avgGameValue) }}>{fmtMean(r.avgGameValue)}</span>
      ),
    },
  ];

  const partnerColumns: Column<PlayerPartnerStat>[] = [
    { key: 'partnerName', label: 'Partner' },
    {
      key: 'gamesTogether',
      label: 'Sp.',
      sortValue: (r) => r.gamesTogether,
      render: (r) => fmtInt(r.gamesTogether),
    },
    {
      key: 'winRateTogether',
      label: 'WR',
      sortValue: (r) => r.winRateTogether,
      render: (r) => (
        <span style={{ color: colorForRate(r.winRateTogether) }}>{fmtRate(r.winRateTogether)}</span>
      ),
    },
    {
      key: 'avgPointsWonLost',
      label: 'Ø',
      sortValue: (r) => r.avgPointsWonLost,
      render: (r) => (
        <span style={{ color: colorForMean(r.avgPointsWonLost) }}>{fmtMean(r.avgPointsWonLost)}</span>
      ),
    },
  ];

  const totalPts = detail?.totalPoints ?? 0;
  const avgVal =
    pointType === 'earned'
      ? stats.totalAvgPointsEarned
      : pointType === 'wonlost'
        ? stats.totalAvgPointsWonLost
        : stats.totalAvgGameValue;

  const showPointToggle = activeTab === 'gm' || activeTab === 'sc' || activeTab === 'ep';
  const toggleDisabledOptions: PointType[] =
    activeTab === 'ep' || activeTab === 'sc' ? ['wonlost', 'earned'] : [];
  // Force effective type when current selection is locked
  const effectivePointType: PointType =
    activeTab === 'ep' || activeTab === 'sc' ? 'value' : pointType;

  return (
    <div className="ps-page">
      {/* Top bar */}
      <div className="ps-topbar">
        <BackButton to={-1 as never} />
        <div className="ps-topbar-text">
          <span className="ps-topbar-title">Profil</span>
          <span className="ps-topbar-sub">{stats.name}</span>
        </div>
        {!stats.isActive && <span className="ps-inactive-badge">{t.analogInactive}</span>}
      </div>

      <div className="ps-scroll">
        {/* Hero: card + name/points + inline stats */}
        <div className="ps-hero">
          <div className="ps-hero-mount">
            <CardFace />
          </div>
          <div className="ps-hero-right">
            <div className="ps-hero-name-row">
              <span className="ps-hero-name">{stats.name}</span>
              <RankBadge rank={rank} />
            </div>
            <div className="ps-hero-total" style={{ color: colorForTotal(totalPts) }}>
              {totalPts >= 0 ? '+' : ''}{totalPts}
            </div>
            {/* Inline stats below total */}
            <div className="ps-hero-inline-stats">
              <div className="ps-hero-stat">
                <span className="ps-hero-stat-val">{fmtInt(stats.totalGames)}</span>
                <span className="ps-hero-stat-label">Spiele</span>
              </div>
              <div className="ps-hero-stat-sep" />
              <div className="ps-hero-stat">
                <span className="ps-hero-stat-val" style={{ color: colorForRate(stats.totalWinRate) }}>
                  {fmtRate(stats.totalWinRate)}
                </span>
                <span className="ps-hero-stat-label">Winrate</span>
              </div>
              <div className="ps-hero-stat-sep" />
              <div className="ps-hero-stat">
                <span className="ps-hero-stat-val" style={{ color: colorForMean(avgVal) }}>
                  {fmtMean(avgVal)}
                </span>
                <span className="ps-hero-stat-label">
                  Ø {effectivePointType === 'earned' ? 'Netto' : effectivePointType === 'wonlost' ? 'Brutto' : 'Wert'}
                </span>
              </div>
            </div>
          </div>
        </div>

        {/* Best / worst round */}
        <BestWorstCards best={bestRound} worst={worstRound} />

        {/* Time series */}
        {detail && detail.recentRounds.length >= 2 && (
          <div className="ps-timeseries">
            <div className="ps-timeseries-header">
              <span className="ps-section-label">Punkte über die Zeit</span>
              <span className="ps-timeseries-count">{detail.recentRounds.length} Runden</span>
            </div>
            <TimeSeriesChart rounds={detail.recentRounds} />
          </div>
        )}

        {/* Tabs row */}
        <div className="ps-tabs-row">
          <div className="ps-tabs">
            {TABS.map((tab) => (
              <button
                key={tab.id}
                className={`ps-tab${activeTab === tab.id ? ' ps-tab-active' : ''}`}
                onClick={() => setActiveTab(tab.id)}
              >
                {tab.label}
              </button>
            ))}
          </div>
          <div className="ps-tabs-fade" aria-hidden />
        </div>

        {/* Point type toggle */}
        {showPointToggle && (
          <PointTypeToggle
            value={effectivePointType}
            onChange={setPointType}
            disabledOptions={toggleDisabledOptions}
          />
        )}

        {/* Tab content */}
        <div className="ps-tab-body">
          {activeTab === 'gm' && (
            <CollapsibleGameModeTable rows={gameModes} pointType={effectivePointType} />
          )}
          {activeTab === 'sc' && (
            <SortableTable
              columns={specialCardColumns}
              rows={specialCards}
              initialSort="occurrences"
              rowKey={(r) => r.specialCardId}
            />
          )}
          {activeTab === 'ep' && (
            <SortableTable
              columns={extraPointColumns}
              rows={extraPoints}
              initialSort="occurrences"
              rowKey={(r) => r.extraPointId}
            />
          )}
          {activeTab === 'pt' && (
            <SortableTable
              columns={partnerColumns}
              rows={partners}
              initialSort="gamesTogether"
              rowKey={(r) => r.partnerId}
            />
          )}
          {activeTab === 'al' && <AloneStats stats={stats} />}
        </div>

        {/* Match history */}
        <MatchHistorySection playerId={playerId} />

        <div style={{ height: 24 }} />
      </div>
    </div>
  );
}
