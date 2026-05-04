import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAnalogPlayers } from '@/hooks/useAnalogPlayers';
import { t } from '@/utils/translations';
import './AnalogPlayersPage.css';

export function AnalogPlayersPage() {
  const navigate = useNavigate();
  const { players, loading, error, createPlayer } = useAnalogPlayers();

  const [showDialog, setShowDialog] = useState(false);
  const [name, setName] = useState('');
  const [dialogError, setDialogError] = useState<string | null>(null);
  const [creating, setCreating] = useState(false);

  const openDialog = () => {
    setName('');
    setDialogError(null);
    setShowDialog(true);
  };

  const handleCreate = async () => {
    if (!name.trim() || creating) return;
    setCreating(true);
    setDialogError(null);
    try {
      await createPlayer(name.trim());
      setShowDialog(false);
    } catch (err) {
      setDialogError(
        err instanceof Error && err.message === 'name_taken'
          ? t.analogNameTaken
          : 'Fehler beim Anlegen',
      );
    } finally {
      setCreating(false);
    }
  };

  return (
    <div className="ap-page">
      <div className="ap-header">
        <div className="ap-header-left">
          <button className="ap-back" onClick={() => navigate('/')} aria-label={t.back}>
            ←
          </button>
          <h1 className="ap-title">{t.analogPlayersTitle}</h1>
        </div>
        <button className="ap-add-btn" onClick={openDialog} aria-label={t.analogNewPlayerTitle}>
          +
        </button>
      </div>

      <div className="ap-list">
        {loading && <div className="ap-empty">{t.loading}</div>}
        {error && <div className="ap-empty ap-error">{error}</div>}
        {!loading && !error && players.length === 0 && (
          <div className="ap-empty">{t.analogNoPlayers}</div>
        )}
        {players.map((player) => (
          <button
            key={player.id}
            className="ap-player-card"
            onClick={() => navigate(`/analog/players/${player.id}`)}
          >
            <div className="ap-player-left">
              <span className="ap-player-name">{player.name}</span>
              {!player.isActive && (
                <span className="ap-inactive-badge">{t.analogInactive}</span>
              )}
            </div>
            <div className="ap-player-right">
              <span className="ap-player-points">{player.totalPoints}</span>
              <div className="ap-player-stats">
                <span>{player.gamesPlayed} {t.analogGamesPlayed}</span>
                <span>{player.wins} {t.analogWins}</span>
                <span>{player.losses} {t.analogLosses}</span>
              </div>
            </div>
          </button>
        ))}
      </div>

      {showDialog && (
        <div className="ap-overlay" onClick={() => setShowDialog(false)}>
          <div className="ap-dialog" onClick={(e) => e.stopPropagation()}>
            <h2 className="ap-dialog-title">{t.analogNewPlayerTitle}</h2>

            <label className="ap-label">{t.analogNameLabel}</label>
            <input
              className="ap-input"
              type="text"
              value={name}
              maxLength={50}
              autoFocus
              onChange={(e) => setName(e.target.value)}
              onKeyDown={(e) => { if (e.key === 'Enter') handleCreate(); }}
            />

            {dialogError && <div className="ap-dialog-error">{dialogError}</div>}

            <div className="ap-dialog-actions">
              <button className="ap-btn-cancel" onClick={() => setShowDialog(false)}>
                {t.abbrechen}
              </button>
              <button
                className="ap-btn-create"
                onClick={handleCreate}
                disabled={!name.trim() || creating}
              >
                {t.analogCreateButton}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
