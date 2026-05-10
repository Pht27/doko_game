import { useParams } from 'react-router-dom';
import { useAnalogPlayer } from '@/hooks/useAnalogPlayer';
import { t } from '@/utils/translations';
import { BackButton } from '@/components/BackButton/BackButton';
import { StatusState } from '@/components/StatusState/StatusState';
import './AnalogPlayerPage.css';

export function AnalogPlayerPage() {
  const { id } = useParams<{ id: string }>();
  const { player, loading, error } = useAnalogPlayer(Number(id));

  if (loading) {
    return (
      <div className="apd-page">
        <StatusState type="loading" />
      </div>
    );
  }

  if (error || !player) {
    return (
      <div className="apd-page">
        <StatusState type="error" message={error ?? t.analogPlayerNotFound} />
      </div>
    );
  }

  return (
    <div className="apd-page">
      <div className="apd-header">
        <BackButton to={-1} />
        <h1 className="apd-name">{player.name}</h1>
        {!player.isActive && <span className="apd-inactive">{t.analogInactive}</span>}
      </div>

      <div className="apd-stats-grid">
        <div className="apd-stat">
          <div className="apd-stat-value">{player.totalPoints}</div>
          <div className="apd-stat-label">{t.analogTotalPoints}</div>
        </div>
        <div className="apd-stat">
          <div className="apd-stat-value">{player.gamesPlayed}</div>
          <div className="apd-stat-label">{t.analogGamesPlayed}</div>
        </div>
        <div className="apd-stat">
          <div className="apd-stat-value apd-wins">{player.wins}</div>
          <div className="apd-stat-label">{t.analogWins}</div>
        </div>
        <div className="apd-stat">
          <div className="apd-stat-value apd-losses">{player.losses}</div>
          <div className="apd-stat-label">{t.analogLosses}</div>
        </div>
      </div>

      <div className="apd-rounds-section">
        <h2 className="apd-section-title">{t.analogRecentRounds}</h2>

        {player.recentRounds.length === 0 ? (
          <StatusState type="empty" message={t.analogNoRounds} />
        ) : (
          <table className="apd-table">
            <thead>
              <tr>
                <th>{t.analogDate}</th>
                <th>{t.analogPoints}</th>
                <th>{t.analogCumulative}</th>
              </tr>
            </thead>
            <tbody>
              {player.recentRounds.map((round) => (
                <tr key={round.roundId}>
                  <td>{new Date(round.playedAt).toLocaleDateString('de-DE')}</td>
                  <td className={round.won ? 'apd-td-won' : 'apd-td-lost'}>
                    {round.points > 0 ? '+' : ''}{round.points}
                  </td>
                  <td>{round.cumulativePoints}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
