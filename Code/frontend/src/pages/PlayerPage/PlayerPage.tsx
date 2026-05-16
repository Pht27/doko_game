import { useState, useRef, useEffect, useCallback } from 'react';
import { useParams } from 'react-router-dom';
import { usePlayerStats } from '@/hooks/usePlayerStats';
import { t } from '@/utils/translations';
import { BackButton } from '@/components/BackButton/BackButton';
import { StatusState } from '@/components/StatusState/StatusState';
import { SortableTable } from '@/components/SortableTable/SortableTable';
import type { Column } from '@/components/SortableTable/SortableTable';
import type { PlayerPartnerStat } from '@/types/analog';
import { colorForRate, colorForMean, fmtRate, fmtMean, fmtInt } from '@/utils/statsUtils';
import { patchPlayer } from '@/api/analog';
import { HeroCardDisplay, RankBadge } from './HeroSection/HeroSection';
import { EditProfileModal } from './EditProfileModal/EditProfileModal';
import { BestWorstCards } from './BestWorstCards/BestWorstCards';
import { TimeSeriesChart } from './TimeSeriesChart/TimeSeriesChart';
import { PointTypeToggle } from './PointTypeToggle/PointTypeToggle';
import type { PointType } from './PointTypeToggle/PointTypeToggle';
import { AloneStats } from './AloneStats/AloneStats';
import { CollapsibleGameModeTable } from './CollapsibleGameModeTable/CollapsibleGameModeTable';
import { CollapsibleSpecialCardTable } from './CollapsibleSpecialCardTable/CollapsibleSpecialCardTable';
import { CollapsibleExtraPointTable } from './CollapsibleExtraPointTable/CollapsibleExtraPointTable';
import { MatchHistorySection } from './MatchHistorySection/MatchHistorySection';
import './PlayerPage.css';

type TabId = 'gm' | 'sc' | 'ep' | 'pt' | 'al';

const TABS: { id: TabId; label: string }[] = [
  { id: 'gm', label: t.playerTabGameModes },
  { id: 'sc', label: t.playerTabSpecialCards },
  { id: 'ep', label: t.playerTabExtraPoints },
  { id: 'pt', label: t.playerTabTeamStats },
  { id: 'al', label: t.playerTabAlone },
];

export function PlayerPage() {
  const { id } = useParams<{ id: string }>();
  const playerId = Number(id);
  const data = usePlayerStats(playerId);
  const [activeTab, setActiveTab] = useState<TabId>('gm');
  const [pointType, setPointType] = useState<PointType>('earned');
  const [localHeroCard, setLocalHeroCard] = useState<string | null | undefined>(undefined);
  const [localName, setLocalName] = useState<string | undefined>(undefined);
  const [showEditProfile, setShowEditProfile] = useState(false);
  const tabsRef = useRef<HTMLDivElement>(null);
  const [tabScroll, setTabScroll] = useState({ left: false, right: false });

  const updateTabScroll = useCallback(() => {
    const el = tabsRef.current;
    if (!el) return;
    setTabScroll({
      left: el.scrollLeft > 4,
      right: el.scrollLeft + el.clientWidth < el.scrollWidth - 4,
    });
  }, []);

  useEffect(() => {
    const el = tabsRef.current;
    if (!el) return;
    updateTabScroll();
    el.addEventListener('scroll', updateTabScroll, { passive: true });
    const ro = new ResizeObserver(updateTabScroll);
    ro.observe(el);
    return () => { el.removeEventListener('scroll', updateTabScroll); ro.disconnect(); };
  }, [updateTabScroll]);

  useEffect(() => {
    const el = tabsRef.current;
    if (!el) return;
    const activeBtn = el.querySelector('.ps-tab-active') as HTMLElement | null;
    activeBtn?.scrollIntoView({ inline: 'nearest', block: 'nearest', behavior: 'smooth' });
  }, [activeTab]);

  if (data.loading) return <div className="ps-page"><StatusState type="loading" /></div>;
  if (data.error || !data.stats) {
    return <div className="ps-page"><StatusState type="error" message={data.error ?? t.analogPlayerNotFound} /></div>;
  }

  const { stats, detail, gameModes, specialCards, extraPoints, partners, bestRound, worstRound, rank } = data;

  const effectiveHeroCard = localHeroCard !== undefined ? localHeroCard : (detail?.heroCard ?? null);
  const effectiveName = localName !== undefined ? localName : stats.name;

  async function handleProfileSave(newName: string, newHeroCard: string | null) {
    await patchPlayer(playerId, { isActive: stats.isActive, name: newName, heroCard: newHeroCard ?? undefined });
    setLocalName(newName);
    setLocalHeroCard(newHeroCard);
  }

  const partnerColumns: Column<PlayerPartnerStat>[] = [
    { key: 'partnerName', label: t.playerColPartner },
    { key: 'gamesTogether', label: t.statsColGamesShort, sortValue: (r) => r.gamesTogether, render: (r) => fmtInt(r.gamesTogether) },
    { key: 'winRateTogether', label: t.statsColWinRate, sortValue: (r) => r.winRateTogether,
      render: (r) => <span style={{ color: colorForRate(r.winRateTogether) }}>{fmtRate(r.winRateTogether)}</span> },
    { key: 'avgPointsWonLost', label: t.statsColAvgShort, sortValue: (r) => r.avgPointsWonLost,
      render: (r) => <span style={{ color: colorForMean(r.avgPointsWonLost) }}>{fmtMean(r.avgPointsWonLost)}</span> },
  ];

  const totalPts = detail?.totalPoints ?? 0;
  const avgVal =
    pointType === 'earned' ? stats.totalAvgPointsEarned
    : pointType === 'wonlost' ? stats.totalAvgPointsWonLost
    : stats.totalAvgGameValue;

  const toggleDisabledOptions: PointType[] =
    activeTab === 'ep' || activeTab === 'sc' ? ['wonlost', 'earned']
    : activeTab === 'pt' ? ['value', 'earned']
    : activeTab === 'al' ? ['value', 'wonlost']
    : [];
  const effectivePointType: PointType =
    activeTab === 'ep' || activeTab === 'sc' ? 'value'
    : activeTab === 'pt' ? 'wonlost'
    : activeTab === 'al' ? 'earned'
    : pointType;

  return (
    <div className="ps-page">
      <div className="ps-topbar">
        <BackButton to={-1 as never} />
        <div className="ps-topbar-text">
          <span className="ps-topbar-title">{t.playerPageTitle}</span>
          <span className="ps-topbar-sub">{effectiveName}</span>
        </div>
        {!stats.isActive && <span className="ps-inactive-badge">{t.analogInactive}</span>}
      </div>

      {showEditProfile && (
        <EditProfileModal
          initialName={effectiveName}
          currentHeroCard={effectiveHeroCard}
          onSave={handleProfileSave}
          onClose={() => setShowEditProfile(false)}
        />
      )}

      <div className="ps-scroll">
        <div className="ps-hero">
          <div className="ps-hero-mount">
            <HeroCardDisplay heroCard={effectiveHeroCard} />
          </div>
          <div className="ps-hero-right">
            <div className="ps-hero-name-row">
              <span className="ps-hero-name">{effectiveName}</span>
              <button className="ps-hero-edit-btn" onClick={() => setShowEditProfile(true)} title={t.playerEditProfileTitle}>✎</button>
              <RankBadge rank={rank} />
            </div>
            <div className="ps-hero-total" style={{ color: colorForMean(totalPts) }}>
              {totalPts >= 0 ? '+' : ''}{totalPts}
            </div>
            <div className="ps-hero-inline-stats">
              <div className="ps-hero-stat">
                <span className="ps-hero-stat-val">{fmtInt(stats.totalGames)}</span>
                <span className="ps-hero-stat-label">{t.playerHeroStatGames}</span>
              </div>
              <div className="ps-hero-stat-sep" />
              <div className="ps-hero-stat">
                <span className="ps-hero-stat-val" style={{ color: colorForRate(stats.totalWinRate) }}>
                  {fmtRate(stats.totalWinRate)}
                </span>
                <span className="ps-hero-stat-label">{t.playerHeroStatWinRate}</span>
              </div>
              <div className="ps-hero-stat-sep" />
              <div className="ps-hero-stat">
                <span className="ps-hero-stat-val" style={{ color: colorForMean(avgVal) }}>
                  {fmtMean(avgVal)}
                </span>
                <span className="ps-hero-stat-label">
                  {effectivePointType === 'earned' ? t.playerHeroAvgNetLabel : effectivePointType === 'wonlost' ? t.playerHeroAvgGrossLabel : t.playerHeroAvgValueLabel}
                </span>
              </div>
            </div>
          </div>
        </div>

        <BestWorstCards best={bestRound} worst={worstRound} />

        {detail && detail.recentRounds.length >= 2 && (
          <div className="ps-timeseries">
            <div className="ps-timeseries-header">
              <span className="ps-section-label">{t.playerTimeSeriesLabel}</span>
              <span className="ps-timeseries-count">{detail.recentRounds.length} {t.playerTimeSeriesRounds}</span>
            </div>
            <TimeSeriesChart rounds={detail.recentRounds} />
          </div>
        )}

        <div className="ps-tabs-row">
          {tabScroll.left && <div className="ps-tabs-fade ps-tabs-fade-left" aria-hidden />}
          <button
            className="ps-tab-arrow"
            disabled={TABS.findIndex((tab) => tab.id === activeTab) === 0}
            onClick={() => {
              const idx = TABS.findIndex((tab) => tab.id === activeTab);
              if (idx > 0) setActiveTab(TABS[idx - 1].id);
            }}
            aria-label="Vorheriger Tab"
          >‹</button>
          <div className="ps-tabs" ref={tabsRef}>
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
          <button
            className="ps-tab-arrow"
            disabled={TABS.findIndex((tab) => tab.id === activeTab) === TABS.length - 1}
            onClick={() => {
              const idx = TABS.findIndex((tab) => tab.id === activeTab);
              if (idx < TABS.length - 1) setActiveTab(TABS[idx + 1].id);
            }}
            aria-label="Nächster Tab"
          >›</button>
          {tabScroll.right && <div className="ps-tabs-fade ps-tabs-fade-right" aria-hidden />}
        </div>

        <PointTypeToggle
          value={effectivePointType}
          onChange={setPointType}
          disabledOptions={toggleDisabledOptions}
        />

        <div className="ps-tab-body">
          {activeTab === 'gm' && <CollapsibleGameModeTable rows={gameModes} pointType={effectivePointType} />}
          {activeTab === 'sc' && <CollapsibleSpecialCardTable rows={specialCards} />}
          {activeTab === 'ep' && <CollapsibleExtraPointTable rows={extraPoints} />}
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

        <MatchHistorySection playerId={playerId} />

        <div style={{ height: 24 }} />
      </div>
    </div>
  );
}
