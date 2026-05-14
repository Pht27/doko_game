import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { usePlayerStats } from '@/hooks/usePlayerStats';
import { t } from '@/utils/translations';
import { BackButton } from '@/components/BackButton/BackButton';
import { StatusState } from '@/components/StatusState/StatusState';
import { SortableTable } from '@/components/SortableTable/SortableTable';
import type { Column } from '@/components/SortableTable/SortableTable';
import type {
  PlayerStats,
  PlayerGameModeStat,
  PlayerSpecialCardStat,
  PlayerExtraPointStat,
  PlayerPartnerStat,
  PlayerRound,
} from '@/types/analog';
import { colorForRate, colorForMean, colorForTotal, fmtRate, fmtMean, fmtInt } from '@/utils/statsUtils';
import './PlayerPage.css';

// ── Point type toggle ─────────────────────────────────────────────────────────

type PointType = 'value' | 'wonlost' | 'earned';
type TabId = 'gm' | 'sc' | 'ep' | 'pt' | 'al';

function PointTypeToggle({
  value,
  onChange,
}: {
  value: PointType;
  onChange: (v: PointType) => void;
}) {
  const opts: { key: PointType; label: string }[] = [
    { key: 'value', label: 'Wert' },
    { key: 'wonlost', label: 'Punkte' },
    { key: 'earned', label: 'Diff' },
  ];
  return (
    <div className="ps-pt-toggle">
      {opts.map((o) => (
        <button
          key={o.key}
          className={`ps-pt-btn${value === o.key ? ' ps-pt-active' : ''}`}
          onClick={() => onChange(o.key)}
        >
          {o.label}
        </button>
      ))}
    </div>
  );
}

// ── Game mode grouped table ───────────────────────────────────────────────────

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

type GmSortKey = 'name' | 'totalGames' | 'reWR' | 'totalWR';

function GameModeTable({
  rows,
  pointType,
}: {
  rows: PlayerGameModeStat[];
  pointType: PointType;
}) {
  const [sortKey, setSortKey] = useState<GmSortKey>('totalGames');
  const [sortDir, setSortDir] = useState<'asc' | 'desc'>('desc');

  const groups = groupByGameMode(rows);

  const totalGames = (g: GameModeGroup) => (g.re?.games ?? 0) + (g.kontra?.games ?? 0);
  const totalWins  = (g: GameModeGroup) => (g.re?.wins  ?? 0) + (g.kontra?.wins  ?? 0);
  const totalWR    = (g: GameModeGroup) => { const t = totalGames(g); return t > 0 ? totalWins(g) / t : 0; };

  const sortedGroups = [...groups].sort((a, b) => {
    let va: number | string;
    let vb: number | string;
    if (sortKey === 'name')       { va = a.gameModeName; vb = b.gameModeName; }
    else if (sortKey === 'reWR')  { va = a.re?.winRate ?? -1; vb = b.re?.winRate ?? -1; }
    else if (sortKey === 'totalGames') { va = totalGames(a); vb = totalGames(b); }
    else                          { va = totalWR(a); vb = totalWR(b); }
    const cmp = va < vb ? -1 : va > vb ? 1 : 0;
    return sortDir === 'asc' ? cmp : -cmp;
  });

  const handleSort = (key: GmSortKey) => {
    if (sortKey === key) setSortDir((d) => (d === 'asc' ? 'desc' : 'asc'));
    else { setSortKey(key); setSortDir('desc'); }
  };

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

  const SortBtn = ({ k, label, className }: { k: GmSortKey; label: string; className?: string }) => (
    <button className={`ps-gm-th${className ? ' ' + className : ''}`} onClick={() => handleSort(k)}>
      {label}{sortKey === k ? (sortDir === 'asc' ? ' ↑' : ' ↓') : ''}
    </button>
  );

  const SubRow = ({
    label,
    labelClass,
    stat,
    games,
    wr,
    avgValue,
  }: {
    label: string;
    labelClass: string;
    stat?: PlayerGameModeStat | null;
    games: number;
    wr: number;
    avgValue: number | null;
  }) => {
    void stat;
    const isEmpty = games === 0;
    return (
      <div className={`ps-gm-sub${isEmpty ? ' ps-gm-sub-empty' : ''}`}>
        <span className={`ps-gm-sub-label ${labelClass}`}>{label}</span>
        <span className="ps-gm-sub-val ps-gm-sub-games">
          {isEmpty ? '—' : fmtInt(games)}
        </span>
        <span className="ps-gm-sub-val ps-gm-sub-wr" style={{ color: isEmpty ? undefined : colorForRate(wr) }}>
          {isEmpty ? '—' : fmtRate(wr)}
        </span>
        <span className="ps-gm-sub-val ps-gm-sub-avg" style={{ color: avgValue != null ? colorForMean(avgValue) : undefined }}>
          {avgValue != null ? fmtMean(avgValue) : '—'}
        </span>
      </div>
    );
  };

  return (
    <div className="ps-gm-table">
      {/* Header */}
      <div className="ps-gm-header">
        <SortBtn k="name" label="Spielmodus" className="ps-gm-th-name" />
        <SortBtn k="totalGames" label="Sp." />
        <SortBtn k="totalWR" label="WR" />
        <SortBtn k="reWR" label="Ø" />
      </div>

      {sortedGroups.map((g) => {
        const tGames = totalGames(g);
        const tWR    = totalWR(g);
        const tAvg   = weightedAvg(g);
        return (
          <div key={g.gameModeId} className="ps-gm-group">
            <div className="ps-gm-group-name">{g.gameModeName}</div>
            <div className="ps-gm-subrows">
              <SubRow
                label="Re" labelClass="ps-gm-label-re"
                stat={g.re}
                games={g.re?.games ?? 0}
                wr={g.re?.winRate ?? 0} avgValue={g.re ? avg(g.re) : null}
              />
              <SubRow
                label="Ko." labelClass="ps-gm-label-ko"
                stat={g.kontra}
                games={g.kontra?.games ?? 0}
                wr={g.kontra?.winRate ?? 0} avgValue={g.kontra ? avg(g.kontra) : null}
              />
              <SubRow
                label="Ges." labelClass="ps-gm-label-total"
                games={tGames}
                wr={tWR} avgValue={tAvg}
              />
            </div>
          </div>
        );
      })}
    </div>
  );
}

// ── Hero card SVG ─────────────────────────────────────────────────────────────

function CardFace() {
  const w = 72, h = 104, r = 8;
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
      <text x={w * 0.14} y={h * 0.17} fontFamily="Georgia, serif" fontWeight={700} fontSize={13} fill={color}>{rank}</text>
      <text x={w * 0.14} y={h * 0.27} fontFamily="sans-serif" fontSize={9} fill={color}>{suit}</text>
      <text x={w / 2} y={h / 2 + 18} textAnchor="middle" fontFamily="sans-serif" fontSize={44} fill={color}>{suit}</text>
      <g transform={`rotate(180 ${w * 0.86} ${h * 0.835})`}>
        <text x={w * 0.86} y={h * 0.835} fontFamily="Georgia, serif" fontWeight={700} fontSize={13} fill={color}>{rank}</text>
        <text x={w * 0.86} y={h * 0.92} fontFamily="sans-serif" fontSize={9} fill={color}>{suit}</text>
      </g>
    </svg>
  );
}

function HeroCard() {
  return (
    <div className="ps-hero-mount">
      <CardFace />
    </div>
  );
}

// ── Time series chart ─────────────────────────────────────────────────────────

function TimeSeriesChart({ rounds }: { rounds: PlayerRound[] }) {
  if (rounds.length < 2) return null;
  const pts = rounds.map((r) => r.cumulativePoints);
  const w = 360, h = 120, pad = 4;
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
      <text x={w - pad} y={y(last) - 8} textAnchor="end" fontSize={11} fontFamily="ui-monospace, monospace" fontWeight={700} fill={lastColor}>
        {(last >= 0 ? '+' : '') + last.toFixed(0)}
      </text>
    </svg>
  );
}

// ── Alone / Solo stats key-value list ─────────────────────────────────────────

function AloneStats({ stats }: { stats: PlayerStats }) {
  const sections = [
    {
      label: 'Solo',
      rows: [
        { k: 'Gespielt', v: fmtInt(stats.soloGames), c: 'var(--app-text)' },
        { k: 'Gewonnen', v: fmtInt(stats.soloWins), c: 'var(--app-win)' },
        { k: 'Winrate', v: fmtRate(stats.soloWinRate), c: colorForRate(stats.soloWinRate) },
        { k: 'Ø Spielwert', v: fmtMean(stats.soloAvgGameValue), c: colorForMean(stats.soloAvgGameValue) },
        { k: 'Ø Punkte', v: fmtMean(stats.soloAvgPointsWonLost), c: colorForMean(stats.soloAvgPointsWonLost) },
        { k: 'Ø Diff', v: fmtMean(stats.aloneAvgPointsEarned), c: colorForMean(stats.aloneAvgPointsEarned) },
      ],
    },
  ];

  return (
    <div className="ps-alone-stats">
      {sections.map((s) => (
        <div key={s.label}>
          <div className="ps-alone-section-label">{s.label}</div>
          {s.rows.map((r, i) => (
            <div key={r.k} className={`ps-alone-row${i === s.rows.length - 1 ? ' ps-alone-row-last' : ''}`}>
              <span className="ps-alone-key">{r.k}</span>
              <span className="ps-alone-val" style={{ color: r.c }}>
                {r.v}
              </span>
            </div>
          ))}
        </div>
      ))}
    </div>
  );
}

// ── Main page ─────────────────────────────────────────────────────────────────

const TABS: { id: TabId; label: string }[] = [
  { id: 'gm', label: 'Spielmodi' },
  { id: 'sc', label: 'Sonderkarten' },
  { id: 'ep', label: 'Extrapunkte' },
  { id: 'pt', label: 'Teamstatistik' },
  { id: 'al', label: 'Solo' },
];

export function PlayerPage() {
  const { id } = useParams<{ id: string }>();
  const data = usePlayerStats(Number(id));
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

  const { stats, detail, gameModes, specialCards, extraPoints, partners } = data;

  // ── Column definitions ─────────────────────────────────────────────────────

  const scAvg = (r: PlayerSpecialCardStat) =>
    pointType === 'wonlost' || pointType === 'earned' ? r.avgPointsWonLost : r.avgGameValue;

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
  ];

  const totalPts = detail?.totalPoints ?? 0;
  const avgVal =
    pointType === 'earned'
      ? stats.totalAvgPointsEarned
      : pointType === 'wonlost'
        ? stats.totalAvgPointsWonLost
        : stats.totalAvgGameValue;

  const showPointToggle = activeTab !== 'pt' && activeTab !== 'al';

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
        {/* Hero */}
        <div className="ps-hero">
          <HeroCard />
          <div className="ps-hero-right">
            <span className="ps-hero-name">{stats.name}</span>
            <div className="ps-hero-total" style={{ color: colorForTotal(totalPts) }}>
              {totalPts >= 0 ? '+' : ''}{totalPts}
            </div>
          </div>
        </div>

        {/* Hero stats mini-grid */}
        <div className="ps-hero-grid">
          <div className="ps-hero-cell">
            <span className="ps-hero-cell-val">{fmtInt(stats.totalGames)}</span>
            <span className="ps-hero-cell-label">Spiele</span>
          </div>
          <div className="ps-hero-cell">
            <span className="ps-hero-cell-val" style={{ color: colorForRate(stats.totalWinRate) }}>
              {fmtRate(stats.totalWinRate)}
            </span>
            <span className="ps-hero-cell-label">Winrate</span>
          </div>
          <div className="ps-hero-cell">
            <span className="ps-hero-cell-val" style={{ color: colorForMean(avgVal) }}>
              {fmtMean(avgVal)}
            </span>
            <span className="ps-hero-cell-label">
              Ø {pointType === 'earned' ? 'Diff' : pointType === 'wonlost' ? 'Pkt.' : 'Wert'}
            </span>
          </div>
        </div>

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

        {/* Tabs row — full width, scrollable */}
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
        </div>

        {/* Point type toggle — own row, right-aligned */}
        {showPointToggle && (
          <div className="ps-pt-row">
            <PointTypeToggle value={pointType} onChange={setPointType} />
          </div>
        )}

        {/* Tab content */}
        <div className="ps-tab-body">
          {activeTab === 'gm' && (
            <GameModeTable rows={gameModes} pointType={pointType} />
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

        <div style={{ height: 24 }} />
      </div>
    </div>
  );
}
