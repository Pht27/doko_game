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
      label: 'Re Ø',
      sortValue: (r) => r.reAvgGameValue,
      render: (r) =>
        r.reAvgGameValue != null ? (
          <span style={{ color: colorForMean(r.reAvgGameValue) }}>{fmtMean(r.reAvgGameValue)}</span>
        ) : (
          <span style={{ color: 'var(--app-text-muted)' }}>—</span>
        ),
    },
  ];

  const specialCardColumns: Column<SpecialCardStat>[] = [
    { key: 'name', label: 'Karte' },
    {
      key: 'occurrences',
      label: 'Anzahl',
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
      sortValue: (r) => r.avgGameValue,
      render: (r) => (
        <span style={{ color: colorForMean(r.avgGameValue) }}>{fmtMean(r.avgGameValue)}</span>
      ),
    },
  ];

  const extraPointColumns: Column<ExtraPointStat>[] = [
    { key: 'name', label: 'Extrapunkt' },
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
      sortValue: (r) => r.avgGameValue,
      render: (r) => (
        <span style={{ color: colorForMean(r.avgGameValue) }}>{fmtMean(r.avgGameValue)}</span>
      ),
    },
  ];

  return (
    <div className="sts-page">
      <PageHeader title={t.statsTitle} backTo={-1 as never} />

      <div className="sts-scroll">
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
        </div>

        <div style={{ height: 24 }} />
      </div>
    </div>
  );
}
