import { useAnalogRounds } from '@/hooks/useAnalogRounds';
import { t } from '@/utils/translations';
import { PageHeader } from '@/components/PageHeader/PageHeader';
import { StatusState } from '@/components/StatusState/StatusState';
import { RoundCard } from './RoundCard/RoundCard';
import './HistoryPage.css';

export function HistoryPage() {
  const { rounds, loading, loadingMore, error, hasMore, loadMore, deleteRound } =
    useAnalogRounds();

  const handleDelete = async (id: number) => {
    if (!window.confirm(t.analogHistoryDeleteConfirm)) return;
    await deleteRound(id);
  };

  if (loading) {
    return (
      <div className="ahr-page">
        <StatusState type="loading" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="ahr-page">
        <StatusState type="error" message={error} />
      </div>
    );
  }

  return (
    <div className="ahr-page">
      <PageHeader title={t.analogHistoryTitle} backTo="/" />

      <div className="ahr-list">
        {rounds.length === 0 ? (
          <StatusState type="empty" message={t.analogHistoryNoRounds} />
        ) : (
          rounds.map((round) => (
            <RoundCard key={round.id} round={round} onDelete={handleDelete} />
          ))
        )}

        {hasMore && (
          <button className="ahr-load-more" onClick={loadMore} disabled={loadingMore}>
            {loadingMore ? t.loading : t.analogHistoryLoadMore}
          </button>
        )}
      </div>
    </div>
  );
}
