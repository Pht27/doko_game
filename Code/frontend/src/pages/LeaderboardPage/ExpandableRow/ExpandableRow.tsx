import { Link } from 'react-router-dom';
import { t } from '@/utils/translations';
import type { PlayerListItem, PlayerDetail } from '@/types/analog';
import { StatusState } from '@/components/StatusState/StatusState';
import { MultiLineChart, MAX_ROUNDS } from '../MultiLineChart/MultiLineChart';
import type { ChartSeries } from '../MultiLineChart/MultiLineChart';

export type DetailState = PlayerDetail | 'loading' | 'error';

export function ExpandableRow({
  player, rank, color, detail, expanded, onToggle, onExpand,
}: {
  player: PlayerListItem;
  rank: number;
  color: string;
  detail: DetailState | undefined;
  expanded: boolean;
  onToggle: () => void;
  onExpand: () => void;
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

  const chartSeries: ChartSeries[] = rounds && cumulativePts.length >= 2
    ? [{ name: player.name, color, points: cumulativePts.slice(-MAX_ROUNDS) }]
    : [];

  const handleHeadClick = (e: React.MouseEvent) => {
    if ((e.target as HTMLElement).closest('a')) return;
    onToggle();
  };

  return (
    <div style={{ borderBottom: '1px solid var(--app-border)' }}>
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
            to={`/players/${player.id}`}
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
            {detail === 'error' && <StatusState type="error" message={t.analogLeaderboardDetailError} className="alb-row-detail-loading" />}
            {rounds && (
              <>
                {chartSeries.length >= 1 && (
                  <div className="alb-chart-row" onClick={(e) => { e.stopPropagation(); onExpand(); }}>
                    <MultiLineChart series={chartSeries} />
                  </div>
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
                    <span className="alb-mini-lbl">{t.analogLeaderboardBalance}</span>
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
