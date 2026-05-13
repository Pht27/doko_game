import { useState, useEffect, useMemo, useRef } from 'react';
import ReactECharts from 'echarts-for-react';
import { t } from '@/utils/translations';
import type { PlayerListItem, PlayerDetail } from '@/types/analog';
import { useOrientationLock } from '@/hooks/useOrientationLock';
import { PlayerPickerModal } from '../PlayerPickerModal/PlayerPickerModal';
import './LeaderboardGraphOverlay.css';

function cssVar(name: string): string {
  return getComputedStyle(document.documentElement).getPropertyValue(name).trim();
}

const DEFAULT_ROUNDS = 24;
const MIN_ROUNDS = 10;
const DEFAULT_WEEKS = 8;
const MIN_WEEKS = 1;
const MS_PER_WEEK = 7 * 24 * 60 * 60 * 1000;

type XAxisMode = 'games' | 'date';
type DetailState = PlayerDetail | 'loading' | 'error';

interface LeaderboardGraphOverlayProps {
  players: PlayerListItem[];
  details: Record<number, DetailState>;
  colorMap: Map<number, string>;
  initialVisibleIds: Set<number>;
  onClose: () => void;
  onFetchDetail: (id: number) => void;
}

export function LeaderboardGraphOverlay({
  players,
  details,
  colorMap,
  initialVisibleIds,
  onClose,
  onFetchDetail,
}: LeaderboardGraphOverlayProps) {
  const [visibleIds, setVisibleIds] = useState(() => new Set(initialVisibleIds));
  const [pickerOpen, setPickerOpen] = useState(false);
  const [maxRounds, setMaxRounds] = useState(DEFAULT_ROUNDS);
  const [weeksBack, setWeeksBack] = useState(DEFAULT_WEEKS);
  const [xMode, setXMode] = useState<XAxisMode>('date');
  const chartRef = useRef<ReactECharts>(null);
  const chartWrapRef = useRef<HTMLDivElement>(null);
  useOrientationLock();

  useEffect(() => {
    const el = chartWrapRef.current;
    if (!el) return;
    const ro = new ResizeObserver(() => {
      chartRef.current?.getEchartsInstance()?.resize();
    });
    ro.observe(el);
    return () => ro.disconnect();
  }, []);

  useEffect(() => {
    visibleIds.forEach((id) => onFetchDetail(id));
  }, [visibleIds, onFetchDetail]);

  const maxAvailableRounds = useMemo(() => {
    let max = MIN_ROUNDS;
    players.forEach((p) => {
      if (!visibleIds.has(p.id)) return;
      const d = details[p.id];
      if (!d || d === 'loading' || d === 'error') return;
      max = Math.max(max, d.recentRounds.length);
    });
    return max;
  }, [players, details, visibleIds]);

  const maxAvailableWeeks = useMemo(() => {
    const now = Date.now();
    let oldest = now;
    let hasData = false;
    players.forEach((p) => {
      if (!visibleIds.has(p.id)) return;
      const d = details[p.id];
      if (!d || d === 'loading' || d === 'error') return;
      d.recentRounds.forEach((r) => {
        const ms = new Date(r.playedAt).getTime();
        if (ms < oldest) {
          oldest = ms;
          hasData = true;
        }
      });
    });
    if (!hasData) return MIN_WEEKS;
    return Math.max(MIN_WEEKS, Math.ceil((now - oldest) / MS_PER_WEEK));
  }, [players, details, visibleIds]);

  const effectiveMaxRounds = Math.min(maxRounds, maxAvailableRounds);
  const effectiveWeeksBack = Math.min(weeksBack, maxAvailableWeeks);

  const series = useMemo(() => {
    return players
      .filter((p) => visibleIds.has(p.id))
      .flatMap((p) => {
        const d = details[p.id];
        if (!d || d === 'loading' || d === 'error') return [];

        let rounds: PlayerDetail['recentRounds'];
        let data: [number, number][];

        if (xMode === 'date') {
          const cutoff = Date.now() - effectiveWeeksBack * MS_PER_WEEK;
          rounds = d.recentRounds.filter((r) => new Date(r.playedAt).getTime() >= cutoff);
          if (rounds.length < 2) return [];
          data = rounds.map((r) => [new Date(r.playedAt).getTime(), r.cumulativePoints]);
        } else {
          rounds = d.recentRounds.slice(-effectiveMaxRounds);
          if (rounds.length < 2) return [];
          const offset = effectiveMaxRounds - rounds.length;
          data = rounds.map((r, i) => [offset + i, r.cumulativePoints]);
        }

        return [
          {
            name: p.name,
            type: 'line',
            data,
            color: colorMap.get(p.id) ?? cssVar('--app-text-muted'),
            smooth: false,
            symbol: 'circle',
            symbolSize: 5,
            lineStyle: { width: 2.5 },
            emphasis: { focus: 'series' },
          },
        ];
      });
  }, [players, details, visibleIds, colorMap, effectiveMaxRounds, effectiveWeeksBack, xMode]);

  const xAxis = useMemo(() => {
    if (xMode === 'date') {
      return {
        type: 'time',
        axisLabel: {
          color: cssVar('--app-text-muted'),
          fontSize: 10,
          formatter: (val: number) => {
            const d = new Date(val);
            return `${d.getDate()}.${d.getMonth() + 1}.`;
          },
        },
        splitLine: { show: false },
        axisLine: { lineStyle: { color: cssVar('--app-border') } },
        axisTick: { show: false },
      };
    }
    return {
      type: 'value',
      min: 0,
      max: effectiveMaxRounds - 1,
      minInterval: effectiveMaxRounds - 1,
      axisLabel: {
        color: cssVar('--app-text-muted'),
        fontSize: 10,
        formatter: (val: number) => (val === 0 ? `vor ${effectiveMaxRounds}` : 'heute'),
      },
      splitLine: { show: false },
      axisLine: { lineStyle: { color: cssVar('--app-border') } },
      axisTick: { show: false },
    };
  }, [xMode, effectiveMaxRounds]);

  const option = useMemo(
    () => ({
      backgroundColor: 'transparent',
      animation: true,
      animationDuration: 250,
      grid: { top: 16, right: 20, bottom: 56, left: 50 },
      xAxis,
      yAxis: {
        type: 'value',
        axisLabel: {
          color: cssVar('--app-text-muted'),
          fontSize: 10,
          formatter: (val: number) => Math.round(val).toString(),
        },
        splitLine: { lineStyle: { color: cssVar('--app-border') } },
        axisLine: { show: false },
        axisTick: { show: false },
      },
      legend: {
        bottom: 6,
        left: 'center',
        textStyle: { color: cssVar('--app-text-sub'), fontSize: 11 },
        icon: 'circle',
        itemWidth: 9,
        itemHeight: 9,
        itemGap: 14,
      },
      dataZoom: [
        { type: 'inside', xAxisIndex: [0], filterMode: 'weakFilter' },
        { type: 'inside', yAxisIndex: [0], filterMode: 'none' },
      ],
      tooltip: {
        trigger: 'axis',
        backgroundColor: cssVar('--app-surface-2'),
        borderColor: cssVar('--app-border-md'),
        borderWidth: 1,
        textStyle: { color: cssVar('--app-text'), fontSize: 12 },
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        formatter: (params: any[]) => {
          if (!params?.length) return '';
          const dateLabelColor = cssVar('--app-text-muted');
          const header =
            xMode === 'date'
              ? `<div style="color:${dateLabelColor};font-size:11px;margin-bottom:4px">${new Date(params[0].value[0]).toLocaleDateString('de-DE', { day: '2-digit', month: '2-digit', year: '2-digit' })}</div>`
              : '';
          const rows = params
            .map(
              (p) =>
                `<div style="display:flex;align-items:center;gap:8px;padding:1px 0">` +
                `<span style="width:8px;height:8px;border-radius:50%;background:${p.color};display:inline-block;flex-shrink:0"></span>` +
                `<span style="flex:1">${p.seriesName}</span>` +
                `<span style="font-weight:700;font-family:monospace">${Math.round(p.value[1])}</span>` +
                `</div>`,
            )
            .join('');
          return header + rows;
        },
      },
      series,
    }),
    [series, xAxis, xMode],
  );

  const sliderMin = xMode === 'games' ? MIN_ROUNDS : MIN_WEEKS;
  const sliderMax = xMode === 'games' ? maxAvailableRounds : maxAvailableWeeks;
  const sliderValue = xMode === 'games' ? effectiveMaxRounds : effectiveWeeksBack;
  const sliderLabel =
    xMode === 'games'
      ? `${effectiveMaxRounds} ${t.analogLeaderboardGraphModeGames}`
      : `${effectiveWeeksBack} ${t.analogLeaderboardGraphWeeks}`;

  return (
    <div className="lgo-overlay">
      <div className="lgo-header">
        <span className="lgo-title">{t.analogLeaderboardGraphTitle}</span>
        <div className="lgo-header-actions">
          <button className="lgo-players-btn" onClick={() => setPickerOpen(true)}>
            {t.analogLeaderboardGraphPlayers}
          </button>
          <button className="lgo-close-btn" onClick={onClose} aria-label="Schließen">
            ✕
          </button>
        </div>
      </div>

      <div className="lgo-controls">
        <div className="lgo-mode-tabs">
          <button
            className={`lgo-mode-tab${xMode === 'games' ? ' lgo-mode-tab--active' : ''}`}
            onClick={() => setXMode('games')}
          >
            {t.analogLeaderboardGraphModeGames}
          </button>
          <button
            className={`lgo-mode-tab${xMode === 'date' ? ' lgo-mode-tab--active' : ''}`}
            onClick={() => setXMode('date')}
          >
            {t.analogLeaderboardGraphModeDate}
          </button>
        </div>

        <input
          type="range"
          className="lgo-slider"
          min={sliderMin}
          max={sliderMax}
          step={1}
          value={sliderValue}
          onChange={(e) => {
            const v = Number(e.target.value);
            if (xMode === 'games') setMaxRounds(v);
            else setWeeksBack(v);
          }}
        />

        <span className="lgo-slider-label">{sliderLabel}</span>
      </div>

      <div className="lgo-chart-wrap" ref={chartWrapRef}>
        {series.length === 0 ? (
          <div className="lgo-empty">Keine Daten</div>
        ) : (
          <ReactECharts
            key={xMode}
            ref={chartRef}
            option={option}
            notMerge
            style={{ width: '100%', height: '100%' }}
            opts={{ renderer: 'canvas', devicePixelRatio: window.devicePixelRatio }}
          />
        )}
      </div>

      {pickerOpen && (
        <PlayerPickerModal
          players={players}
          visibleIds={visibleIds}
          colorMap={colorMap}
          onConfirm={(ids) => setVisibleIds(ids)}
          onClose={() => setPickerOpen(false)}
        />
      )}
    </div>
  );
}
