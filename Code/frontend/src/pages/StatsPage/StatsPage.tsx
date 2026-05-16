import { useState } from 'react';
import { useOverallStats } from '@/hooks/useOverallStats';
import { t } from '@/utils/translations';
import { PageHeader } from '@/components/PageHeader/PageHeader';
import { StatusState } from '@/components/StatusState/StatusState';
import { SortableTable } from '@/components/SortableTable/SortableTable';
import type { Column } from '@/components/SortableTable/SortableTable';
import type { GameModeStat } from '@/types/analog';
import { colorForRate, colorForMean, fmtRate, fmtMean, fmtInt } from '@/utils/statsUtils';
import { StatsHero } from './StatsHero/StatsHero';
import { CollapsibleSpecialCardTable } from './CollapsibleSpecialCardTable/CollapsibleSpecialCardTable';
import { CollapsibleExtraPointTable } from './CollapsibleExtraPointTable/CollapsibleExtraPointTable';
import './StatsPage.css';

type TabId = 'gm' | 'sc' | 'ep';

const TABS: { id: TabId; label: string }[] = [
  { id: 'gm', label: t.statsTabGameModes },
  { id: 'sc', label: t.statsTabSpecialCards },
  { id: 'ep', label: t.statsTabExtraPoints },
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
    { key: 'gameModeName', label: t.statsColGameModeName },
    { key: 'totalRounds', label: t.statsColRounds, sortValue: (r) => r.totalRounds, render: (r) => fmtInt(r.totalRounds) },
    {
      key: 'reWinRate',
      label: t.statsColReWinRate,
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
      label: t.statsColReAvgValue,
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
