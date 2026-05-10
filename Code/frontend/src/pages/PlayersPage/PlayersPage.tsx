import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAnalogPlayers } from '@/hooks/useAnalogPlayers';
import { t } from '@/utils/translations';
import type { PlayerListItem } from '@/types/analog';
import { PageHeader } from '@/components/PageHeader/PageHeader';
import { Button } from '@/components/Button/Button';
import { StatusState } from '@/components/StatusState/StatusState';
import { FormError } from '@/components/FormError/FormError';
import { PlayerRow } from './PlayerRow/PlayerRow';
import { SectionHeader } from './SectionHeader/SectionHeader';
import './PlayersPage.css';

export function PlayersPage() {
  const navigate = useNavigate();
  const { players, loading, error, createPlayer, toggleActive, renamePlayer } = useAnalogPlayers();

  const [showDialog, setShowDialog] = useState(false);
  const [name, setName] = useState('');
  const [dialogError, setDialogError] = useState<string | null>(null);
  const [creating, setCreating] = useState(false);

  const openDialog = () => { setName(''); setDialogError(null); setShowDialog(true); };

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

  const active = players.filter((p) => p.isActive);
  const inactive = players.filter((p) => !p.isActive);

  const makeHandlers = (player: PlayerListItem) => ({
    onNavigate: () => navigate(`/players/${player.id}`),
    onToggle: () => void toggleActive(player),
    onRename: (newName: string) => renamePlayer(player, newName),
  });

  return (
    <div className="ap-page">
      <PageHeader
        title={t.analogPlayersTitle}
        backTo="/"
        right={
          <button className="ap-add-btn" onClick={openDialog} aria-label={t.analogNewPlayerTitle}>
            +
          </button>
        }
      />

      <div className="ap-list">
        {loading && <StatusState type="loading" message={t.loading} />}
        {error && <StatusState type="error" message={error} />}

        {!loading && !error && (
          <>
            <SectionHeader label={t.analogActive} count={active.length} />
            <div className="ap-section">
              {active.length === 0
                ? <StatusState type="empty" message={t.analogNoActivePlayers} dashed />
                : active.map((p) => (
                  <PlayerRow key={p.id} player={p} {...makeHandlers(p)} />
                ))}
            </div>

            <SectionHeader label={t.analogInactiveSection} count={inactive.length} />
            <div className="ap-section">
              {inactive.length === 0
                ? <StatusState type="empty" message={t.analogNoInactivePlayers} dashed />
                : inactive.map((p) => (
                  <PlayerRow key={p.id} player={p} {...makeHandlers(p)} />
                ))}
            </div>
          </>
        )}
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
              onKeyDown={(e) => { if (e.key === 'Enter') void handleCreate(); }}
            />

            <FormError message={dialogError} />

            <div className="ap-dialog-actions">
              <Button variant="secondary" className="flex-1" onClick={() => setShowDialog(false)}>
                {t.abbrechen}
              </Button>
              <Button
                className="flex-2"
                onClick={() => void handleCreate()}
                disabled={!name.trim() || creating}
              >
                {t.analogCreateButton}
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
