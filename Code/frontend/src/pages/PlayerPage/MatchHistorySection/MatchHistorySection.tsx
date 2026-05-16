import { useState } from 'react';
import { usePlayerRounds } from '@/hooks/usePlayerRounds';
import { StatusState } from '@/components/StatusState/StatusState';
import { t } from '@/utils/translations';
import { MatchHistoryCompactRow } from './MatchHistoryCompactRow/MatchHistoryCompactRow';
import './MatchHistorySection.css';

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

export function MatchHistorySection({ playerId }: { playerId: number }) {
  const { rounds, total, page, totalPages, loading, error, goToPage } = usePlayerRounds(playerId);
  const [expandedIds, setExpandedIds] = useState<Set<number>>(new Set());

  const toggle = (id: number) =>
    setExpandedIds((prev) => { const n = new Set(prev); n.has(id) ? n.delete(id) : n.add(id); return n; });

  return (
    <div className="ps-mh-section">
      <div className="ps-mh-header">
        <span className="ps-section-label">{t.playerMatchHistoryLabel}</span>
        {total > 0 && <span className="ps-mh-count">{total} {t.playerMatchHistoryGames}</span>}
      </div>

      {loading && <StatusState type="loading" />}
      {error && <StatusState type="error" message={error} />}
      {!loading && !error && rounds.length === 0 && (
        <div className="ps-mh-empty">{t.playerMatchHistoryEmpty}</div>
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
