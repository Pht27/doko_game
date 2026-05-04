import { useNavigate } from 'react-router-dom';
import { useAnalogRounds } from '@/hooks/useAnalogRounds';
import { t } from '@/utils/translations';
import type { PlayerRef } from '@/types/analog';
import './AnalogHistoryPage.css';

function PlayerList({
  players,
  won,
  align,
}: {
  players: PlayerRef[];
  won: boolean;
  align: 'left' | 'right';
}) {
  const navigate = useNavigate();
  return (
    <div className={`ahr-team ahr-team-${align}`}>
      {players.map((p) => (
        <button
          key={p.id}
          className={`ahr-player ${won ? 'ahr-player-won' : ''}`}
          onClick={(e) => {
            e.stopPropagation();
            navigate(`/analog/players/${p.id}`);
          }}
        >
          {p.name}
        </button>
      ))}
    </div>
  );
}

export function AnalogHistoryPage() {
  const navigate = useNavigate();
  const { rounds, loading, loadingMore, error, hasMore, loadMore, deleteRound } =
    useAnalogRounds();

  const handleDelete = async (id: number) => {
    if (!window.confirm(t.analogHistoryDeleteConfirm)) return;
    await deleteRound(id);
  };

  if (loading) {
    return (
      <div className="ahr-page">
        <div className="ahr-empty">{t.loading}</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="ahr-page">
        <div className="ahr-empty ahr-error">{error}</div>
      </div>
    );
  }

  return (
    <div className="ahr-page">
      <div className="ahr-header">
        <button className="ahr-back" onClick={() => navigate('/')}>
          {t.back}
        </button>
        <h1 className="ahr-title">{t.analogHistoryTitle}</h1>
      </div>

      <div className="ahr-list">
        {rounds.length === 0 ? (
          <div className="ahr-empty">{t.analogHistoryNoRounds}</div>
        ) : (
          rounds.map((round) => {
            const reWon = round.winningParty === 'Re';
            const isNormalGame = round.gameMode === 'Normalspiel';
            const date = new Date(round.playedAt);

            return (
              <div key={round.id} className="ahr-card">
                <div
                  className="ahr-card-main"
                  onClick={() => navigate(`/analog/edit/${round.id}`)}
                >
                  <div className="ahr-card-meta">
                    <span className="ahr-date">
                      {date.toLocaleDateString('de-DE', {
                        day: '2-digit',
                        month: '2-digit',
                        year: 'numeric',
                      })}
                    </span>
                    <span className="ahr-time">
                      {date.toLocaleTimeString('de-DE', { hour: '2-digit', minute: '2-digit' })}
                    </span>
                    {!isNormalGame && (
                      <span className="ahr-gamemode">{round.gameMode}</span>
                    )}
                  </div>

                  <div className="ahr-teams">
                    <PlayerList players={round.rePlayers} won={reWon} align="left" />

                    <div className="ahr-center">
                      <div className={`ahr-points ${reWon ? 'ahr-re-wins' : 'ahr-kontra-wins'}`}>
                        {round.points}
                      </div>
                      <div className="ahr-winner-label">
                        {reWon ? t.analogHistoryRe : t.analogHistoryKontra}
                      </div>
                    </div>

                    <PlayerList players={round.kontraPlayers} won={!reWon} align="right" />
                  </div>

                  {round.comment && (
                    <div className="ahr-comment">„{round.comment}"</div>
                  )}
                </div>

                <button
                  className="ahr-delete-btn"
                  onClick={() => handleDelete(round.id)}
                  aria-label={t.analogHistoryDelete}
                >
                  ×
                </button>
              </div>
            );
          })
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
