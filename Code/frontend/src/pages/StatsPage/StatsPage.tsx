import { useState } from 'react';
import { useOverallStats } from '@/hooks/useOverallStats';
import { t } from '@/utils/translations';
import { PageHeader } from '@/components/PageHeader/PageHeader';
import { StatusState } from '@/components/StatusState/StatusState';
import { SortableTable } from '@/components/SortableTable/SortableTable';
import type { Column } from '@/components/SortableTable/SortableTable';
import type { GameModeStat, SpecialCardStat, ExtraPointStat } from '@/types/analog';
import { colorForRate, colorForMean, fmtRate, fmtMean, fmtInt } from '@/utils/statsUtils';
import './StatsPage.css';

type TabId = 'gm' | 'sc' | 'ep';

const TABS: { id: TabId; label: string }[] = [
  { id: 'gm', label: 'Spielmodi' },
  { id: 'sc', label: 'Sonderkarten' },
  { id: 'ep', label: 'Extrapunkte' },
];

// ── Hero banner ──────────────────────────────────────────────────────────────

function StatsHero({ gameModes }: { gameModes: GameModeStat[] }) {
  if (gameModes.length === 0) return null;

  const totalRounds = gameModes.reduce((s, m) => s + m.totalRounds, 0);

  // Weighted Re win rate (only modes that have it)
  const modesWithWR = gameModes.filter((m) => m.reWinRate != null);
  const reWinRoundsSum = modesWithWR.reduce((s, m) => s + m.totalRounds, 0);
  const overallReWR =
    reWinRoundsSum > 0
      ? modesWithWR.reduce((s, m) => s + (m.reWinRate ?? 0) * m.totalRounds, 0) / reWinRoundsSum
      : null;
  const overallKoWR = overallReWR != null ? 1 - overallReWR : null;

  // Weighted avg game value
  const totalRoundsForAvg = gameModes.reduce((s, m) => s + m.totalRounds, 0);
  const overallAvg =
    totalRoundsForAvg > 0
      ? gameModes.reduce((s, m) => s + m.avgGameValue * m.totalRounds, 0) / totalRoundsForAvg
      : null;

  // Solo rounds (modes whose name contains "Solo" or "Fleischloser")
  const soloRounds = gameModes
    .filter((m) => m.gameModeName.toLowerCase().includes('solo') || m.gameModeName === 'Fleischloser')
    .reduce((s, m) => s + m.totalRounds, 0);

  return (
    <div className="sts-hero">
      {/* Total rounds */}
      <div className="sts-hero-headline">
        <span className="sts-hero-label">Gespielte Runden</span>
      </div>
      <div className="sts-hero-total">{fmtInt(totalRounds)}</div>

      {/* Re / Ko win rate bar */}
      {overallReWR != null && overallKoWR != null && (
        <div className="sts-hero-wr-section">
          <div className="sts-hero-wr-row">
            <div className="sts-hero-wr-side">
              <span className="sts-hero-wr-dot sts-hero-wr-dot-re" />
              <span className="sts-hero-wr-party sts-hero-wr-party-re">RE</span>
              <span className="sts-hero-wr-val" style={{ color: colorForRate(overallReWR) }}>
                {fmtRate(overallReWR)}
              </span>
            </div>
            <div className="sts-hero-wr-side sts-hero-wr-side-right">
              <span className="sts-hero-wr-val" style={{ color: colorForRate(overallKoWR) }}>
                {fmtRate(overallKoWR)}
              </span>
              <span className="sts-hero-wr-party sts-hero-wr-party-ko">KO</span>
              <span className="sts-hero-wr-dot sts-hero-wr-dot-ko" />
            </div>
          </div>
          <div className="sts-hero-wr-bar">
            <div
              className="sts-hero-wr-bar-fill"
              style={{ width: `${overallReWR * 100}%` }}
            />
          </div>
        </div>
      )}

      {/* Mini stats row */}
      <div className="sts-hero-mini-row">
        <div className="sts-hero-mini-cell">
          <span className="sts-hero-mini-val">
            {overallAvg != null ? (
              <span style={{ color: colorForMean(overallAvg) }}>{fmtMean(overallAvg)}</span>
            ) : '—'}
          </span>
          <span className="sts-hero-mini-label">Ø Spielwert</span>
        </div>
        <div className="sts-hero-mini-divider" />
        <div className="sts-hero-mini-cell">
          <span className="sts-hero-mini-val">{fmtInt(soloRounds)}</span>
          <span className="sts-hero-mini-label">Solos gespielt</span>
        </div>
      </div>
    </div>
  );
}

// ── Special card collapsible table ────────────────────────────────────────────

type ScGroup = { specialCardId: number; name: string; re: SpecialCardStat | null; kontra: SpecialCardStat | null };

function groupSpecialCards(rows: SpecialCardStat[]): ScGroup[] {
  const map = new Map<number, ScGroup>();
  for (const row of rows) {
    if (!map.has(row.specialCardId))
      map.set(row.specialCardId, { specialCardId: row.specialCardId, name: row.name, re: null, kontra: null });
    const g = map.get(row.specialCardId)!;
    if (row.party === 0) g.re = row;
    else g.kontra = row;
  }
  return Array.from(map.values());
}

function CollapsibleSpecialCardTable({ rows }: { rows: SpecialCardStat[] }) {
  const [expanded, setExpanded] = useState<Set<number>>(new Set());

  const groups = groupSpecialCards(rows).sort(
    (a, b) => (b.re?.occurrences ?? 0) + (b.kontra?.occurrences ?? 0) - ((a.re?.occurrences ?? 0) + (a.kontra?.occurrences ?? 0)),
  );

  const totalOcc = (g: ScGroup) => (g.re?.occurrences ?? 0) + (g.kontra?.occurrences ?? 0);
  const totalWins = (g: ScGroup) => (g.re?.wins ?? 0) + (g.kontra?.wins ?? 0);
  const totalWR = (g: ScGroup) => { const t = totalOcc(g); return t > 0 ? totalWins(g) / t : 0; };
  const weightedAvg = (g: ScGroup) => {
    const total = totalOcc(g);
    if (total === 0) return null;
    return ((g.re ? g.re.avgGameValue * g.re.occurrences : 0) + (g.kontra ? g.kontra.avgGameValue * g.kontra.occurrences : 0)) / total;
  };

  const toggle = (id: number) =>
    setExpanded((prev) => { const n = new Set(prev); n.has(id) ? n.delete(id) : n.add(id); return n; });

  return (
    <div className="ps-cgm-table">
      <div className="ps-cgm-header">
        <span className="ps-cgm-th ps-cgm-th-name">Karte</span>
        <span className="ps-cgm-th">Anz.</span>
        <span className="ps-cgm-th">WR</span>
        <span className="ps-cgm-th">Ø</span>
        <span className="ps-cgm-th ps-cgm-th-chevron" />
      </div>

      {groups.map((g) => {
        const tOcc = totalOcc(g);
        const tWR = totalWR(g);
        const tAvg = weightedAvg(g);
        const isExpanded = expanded.has(g.specialCardId);
        return (
          <div key={g.specialCardId} className="ps-cgm-group">
            <button
              className={`ps-cgm-summary${isExpanded ? ' ps-cgm-summary-open' : ''}`}
              onClick={() => toggle(g.specialCardId)}
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
                {([{ label: 'Re', labelClass: 'ps-cgm-label-re', stat: g.re }, { label: 'Ko', labelClass: 'ps-cgm-label-ko', stat: g.kontra }] as const).map(({ label, labelClass, stat }) => {
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

// ── Extra point collapsible table ─────────────────────────────────────────────

type EpGroup = { extraPointId: number; name: string; re: ExtraPointStat | null; kontra: ExtraPointStat | null };

function groupExtraPoints(rows: ExtraPointStat[]): EpGroup[] {
  const map = new Map<number, EpGroup>();
  for (const row of rows) {
    if (!map.has(row.extraPointId))
      map.set(row.extraPointId, { extraPointId: row.extraPointId, name: row.name, re: null, kontra: null });
    const g = map.get(row.extraPointId)!;
    if (row.party === 0) g.re = row;
    else g.kontra = row;
  }
  return Array.from(map.values());
}

function CollapsibleExtraPointTable({ rows }: { rows: ExtraPointStat[] }) {
  const [expanded, setExpanded] = useState<Set<number>>(new Set());

  const groups = groupExtraPoints(rows).sort(
    (a, b) => (b.re?.occurrences ?? 0) + (b.kontra?.occurrences ?? 0) - ((a.re?.occurrences ?? 0) + (a.kontra?.occurrences ?? 0)),
  );

  const totalOcc = (g: EpGroup) => (g.re?.occurrences ?? 0) + (g.kontra?.occurrences ?? 0);
  const totalWins = (g: EpGroup) => (g.re?.wins ?? 0) + (g.kontra?.wins ?? 0);
  const totalWR = (g: EpGroup) => { const t = totalOcc(g); return t > 0 ? totalWins(g) / t : 0; };
  const weightedAvg = (g: EpGroup) => {
    const total = totalOcc(g);
    if (total === 0) return null;
    return ((g.re ? g.re.avgGameValue * g.re.occurrences : 0) + (g.kontra ? g.kontra.avgGameValue * g.kontra.occurrences : 0)) / total;
  };

  const toggle = (id: number) =>
    setExpanded((prev) => { const n = new Set(prev); n.has(id) ? n.delete(id) : n.add(id); return n; });

  return (
    <div className="ps-cgm-table">
      <div className="ps-cgm-header">
        <span className="ps-cgm-th ps-cgm-th-name">Extrapunkt</span>
        <span className="ps-cgm-th">Anz.</span>
        <span className="ps-cgm-th">WR</span>
        <span className="ps-cgm-th">Ø</span>
        <span className="ps-cgm-th ps-cgm-th-chevron" />
      </div>

      {groups.map((g) => {
        const tOcc = totalOcc(g);
        const tWR = totalWR(g);
        const tAvg = weightedAvg(g);
        const isExpanded = expanded.has(g.extraPointId);
        return (
          <div key={g.extraPointId} className="ps-cgm-group">
            <button
              className={`ps-cgm-summary${isExpanded ? ' ps-cgm-summary-open' : ''}`}
              onClick={() => toggle(g.extraPointId)}
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
                {([{ label: 'Re', labelClass: 'ps-cgm-label-re', stat: g.re }, { label: 'Ko', labelClass: 'ps-cgm-label-ko', stat: g.kontra }] as const).map(({ label, labelClass, stat }) => {
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

// ── Page ─────────────────────────────────────────────────────────────────────

export function StatsPage() {
  const { gameModes, specialCards, extraPoints, loading, error } = useOverallStats();
  const [activeTab, setActiveTab] = useState<TabId>('gm');

  if (loading) {
    return (
      <div className="sts-page">
        <PageHeader title={t.statsTitle} backTo={-1 as never} />
        <StatusState type="loading" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="sts-page">
        <PageHeader title={t.statsTitle} backTo={-1 as never} />
        <StatusState type="error" message={error} />
      </div>
    );
  }

  const gameModeColumns: Column<GameModeStat>[] = [
    { key: 'gameModeName', label: 'Spielmodus' },
    {
      key: 'totalRounds',
      label: 'Runden',
      sortValue: (r) => r.totalRounds,
      render: (r) => fmtInt(r.totalRounds),
    },
    {
      key: 'reWinRate',
      label: 'Re WR',
      sortValue: (r) => r.reWinRate,
      render: (r) =>
        r.reWinRate != null ? (
          <span style={{ color: colorForRate(r.reWinRate) }}>{fmtRate(r.reWinRate)}</span>
        ) : (
          <span style={{ color: 'var(--app-text-muted)' }}>—</span>
        ),
    },
    {
      key: 'reAvgGameValue',
      label: 'Re Ø Wert',
      sortValue: (r) => r.reAvgGameValue,
      render: (r) =>
        r.reAvgGameValue != null ? (
          <span style={{ color: colorForMean(r.reAvgGameValue) }}>{fmtMean(r.reAvgGameValue)}</span>
        ) : (
          <span style={{ color: 'var(--app-text-muted)' }}>—</span>
        ),
    },
  ];

  return (
    <div className="sts-page">
      <PageHeader title={t.statsTitle} backTo={-1 as never} />

      <div className="sts-scroll">
        <StatsHero gameModes={gameModes} />

        {/* Tabs row */}
        <div className="sts-tabs-row">
          <div className="sts-tabs">
            {TABS.map((tab) => (
              <button
                key={tab.id}
                className={`sts-tab${activeTab === tab.id ? ' sts-tab-active' : ''}`}
                onClick={() => setActiveTab(tab.id)}
              >
                {tab.label}
              </button>
            ))}
          </div>
        </div>

        {/* Tab content */}
        <div className="sts-tab-body">
          {activeTab === 'gm' && (
            <SortableTable
              columns={gameModeColumns}
              rows={gameModes}
              initialSort="totalRounds"
              rowKey={(r) => r.gameModeId}
            />
          )}
          {activeTab === 'sc' && <CollapsibleSpecialCardTable rows={specialCards} />}
          {activeTab === 'ep' && <CollapsibleExtraPointTable rows={extraPoints} />}
        </div>

        <div style={{ height: 24 }} />
      </div>
    </div>
  );
}
