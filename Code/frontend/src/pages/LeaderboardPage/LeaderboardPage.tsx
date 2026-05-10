import { useState, useEffect, useCallback, useRef } from 'react';
import { getPlayers, getPlayer } from '@/api/analog';
import { t } from '@/utils/translations';
import type { PlayerListItem } from '@/types/analog';
import { PageHeader } from '@/components/PageHeader/PageHeader';
import { ToggleSwitch } from '@/components/ToggleSwitch/ToggleSwitch';
import { StatusState } from '@/components/StatusState/StatusState';
import { LeaderboardGraphOverlay } from './LeaderboardGraphOverlay/LeaderboardGraphOverlay';
import { MultiLineChart, MAX_ROUNDS } from './MultiLineChart/MultiLineChart';
import type { ChartSeries } from './MultiLineChart/MultiLineChart';
import { ExpandableRow } from './ExpandableRow/ExpandableRow';
import type { DetailState } from './ExpandableRow/ExpandableRow';
import './LeaderboardPage.css';

const PLAYER_COLORS = [
  '#818cf8', '#f472b6', '#34d399', '#fbbf24',
  '#60a5fa', '#fb923c', '#a78bfa', '#4ade80',
];

type SortKey = 'totalPoints' | 'winRate' | 'gamesPlayed';

// ── Main page ───────────────────────────────────────────────────────────────

export function LeaderboardPage() {
  const [players, setPlayers] = useState<PlayerListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [sortBy, setSortBy] = useState<SortKey>('totalPoints');
  const [showInactive, setShowInactive] = useState(false);
  const [expandedId, setExpandedId] = useState<number | null>(null);

  const [graphOpen, setGraphOpen] = useState(false);
  const [graphInitialIds, setGraphInitialIds] = useState<Set<number>>(new Set());

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

  // Pre-fetch all active players so the hero chart is complete
  useEffect(() => {
    if (players.length === 0) return;
    [...players]
      .filter((p) => p.isActive)
      .sort((a, b) => b.totalPoints - a.totalPoints)
      .forEach((p) => fetchDetail(p.id));
  }, [players, fetchDetail]);

  const sortedActive = [...players]
    .filter((p) => p.isActive)
    .sort((a, b) => b.totalPoints - a.totalPoints);
  const sortedInactive = [...players]
    .filter((p) => !p.isActive)
    .sort((a, b) => b.totalPoints - a.totalPoints);

  const allColorMap = new Map<number, string>([
    ...sortedActive.map((p, i) => [p.id, PLAYER_COLORS[i % PLAYER_COLORS.length]] as [number, string]),
    ...sortedInactive.map((p, i) => [p.id, PLAYER_COLORS[(sortedActive.length + i) % PLAYER_COLORS.length]] as [number, string]),
  ]);

  const sorted = [...players]
    .filter((p) => showInactive || p.isActive)
    .sort((a, b) => {
      if (sortBy === 'totalPoints') return b.totalPoints - a.totalPoints;
      if (sortBy === 'winRate') return b.winRate - a.winRate;
      return b.gamesPlayed - a.gamesPlayed;
    });

  const heroSeries: ChartSeries[] = sortedActive.flatMap((p) => {
    const d = details[p.id];
    if (!d || d === 'loading' || d === 'error') return [];
    const points = d.recentRounds.slice(-MAX_ROUNDS).map((r) => r.cumulativePoints);
    if (points.length < 2) return [];
    return [{ name: p.name, color: allColorMap.get(p.id) ?? PLAYER_COLORS[0], points }];
  });

  const openGraphForAll = () => {
    setGraphInitialIds(new Set(sortedActive.map((p) => p.id)));
    setGraphOpen(true);
  };

  const openGraphForPlayer = (playerId: number) => {
    setGraphInitialIds(new Set([playerId]));
    setGraphOpen(true);
  };

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
          {loading ? (
            <StatusState type="loading" />
          ) : heroSeries.length === 0 ? (
            <div className="alb-chart-empty">{t.analogLeaderboardNotEnoughData}</div>
          ) : (
            <button className="alb-hero-chart-btn" onClick={openGraphForAll} aria-label={t.analogLeaderboardExpandLabel}>
              <MultiLineChart series={heroSeries} />
            </button>
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
              color={allColorMap.get(player.id) ?? PLAYER_COLORS[idx % PLAYER_COLORS.length]}
              detail={details[player.id]}
              expanded={expandedId === player.id}
              onToggle={() => handleToggle(player.id)}
              onExpand={() => openGraphForPlayer(player.id)}
            />
          ))}
        </div>
      </div>

      {graphOpen && (
        <LeaderboardGraphOverlay
          players={players}
          details={details}
          colorMap={allColorMap}
          initialVisibleIds={graphInitialIds}
          onClose={() => setGraphOpen(false)}
          onFetchDetail={fetchDetail}
        />
      )}
    </div>
  );
}
