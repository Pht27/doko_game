import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAnalogPlayers } from '@/hooks/useAnalogPlayers';
import { t } from '@/utils/translations';
import type { PlayerListItem } from '@/types/analog';
import './AnalogPlayersPage.css';

function Switch({ on, onChange }: { on: boolean; onChange: () => void }) {
  const [pressing, setPressing] = useState(false);

  return (
    <button
      className={`ap-switch${on ? ' ap-switch--on' : ''}${pressing ? ' ap-switch--pressing' : ''}`}
      onPointerDown={(e) => { e.stopPropagation(); setPressing(true); }}
      onPointerUp={(e) => { e.stopPropagation(); setPressing(false); onChange(); }}
      onPointerLeave={() => setPressing(false)}
      onPointerCancel={() => setPressing(false)}
      onClick={(e) => e.stopPropagation()}
      aria-pressed={on}
      aria-label={on ? t.analogActive : t.analogInactiveSection}
    >
      <span className="ap-switch-thumb" />
    </button>
  );
}

function PlayerRow({
  player,
  onNavigate,
  onToggle,
  onRename,
}: {
  player: PlayerListItem;
  onNavigate: () => void;
  onToggle: () => void;
  onRename: (name: string) => Promise<void>;
}) {
  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState('');
  const [renameError, setRenameError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [leaving, setLeaving] = useState(false);

  const handleToggle = () => {
    setLeaving(true);
    setTimeout(() => { onToggle(); }, 240);
  };

  const startEdit = (e: React.MouseEvent) => {
    e.stopPropagation();
    setDraft(player.name);
    setRenameError(null);
    setEditing(true);
  };

  const cancel = () => { setEditing(false); setRenameError(null); };

  const commit = async () => {
    const name = draft.trim();
    if (!name || name === player.name) { cancel(); return; }
    setSaving(true);
    setRenameError(null);
    try {
      await onRename(name);
      setEditing(false);
    } catch (err) {
      setRenameError(
        err instanceof Error && err.message === 'name_taken'
          ? t.analogNameTaken
          : 'Fehler',
      );
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className={`ap-row${player.isActive ? '' : ' ap-row--inactive'}${leaving ? ' ap-row--leaving' : ''}`}>
      {editing ? (
        <div className="ap-edit-area" onClick={(e) => e.stopPropagation()}>
          <input
            className={`ap-edit-input${renameError ? ' ap-edit-input--error' : ''}`}
            autoFocus
            value={draft}
            maxLength={50}
            onChange={(e) => setDraft(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === 'Enter') void commit();
              if (e.key === 'Escape') cancel();
            }}
          />
          {renameError && <span className="ap-edit-error">{renameError}</span>}
          <button className="ap-icon-btn ap-icon-btn--cancel" onClick={cancel} aria-label="Abbrechen">
            <svg width={14} height={14} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2.5} strokeLinecap="round" strokeLinejoin="round">
              <line x1="18" y1="6" x2="6" y2="18" /><line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </button>
          <button
            className="ap-icon-btn ap-icon-btn--confirm"
            onClick={() => void commit()}
            disabled={saving || !draft.trim()}
            aria-label="Speichern"
          >
            <svg width={14} height={14} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2.5} strokeLinecap="round" strokeLinejoin="round">
              <polyline points="20 6 9 17 4 12" />
            </svg>
          </button>
        </div>
      ) : (
        <button className="ap-name-btn" onClick={onNavigate}>
          <span className="ap-name">{player.name}</span>
        </button>
      )}

      {!editing && (
        <button className="ap-icon-btn ap-icon-btn--edit" onClick={startEdit} aria-label={t.analogRenamePlayer}>
          <svg width={14} height={14} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round">
            <path d="M12 20h9" /><path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4Z" />
          </svg>
        </button>
      )}

      <Switch on={leaving ? !player.isActive : player.isActive} onChange={handleToggle} />
    </div>
  );
}

function SectionHeader({ label, count }: { label: string; count: number }) {
  return (
    <div className="ap-section-header">
      <span className="ap-section-label">{label}</span>
      <span className="ap-section-count">{count}</span>
    </div>
  );
}

function EmptySection({ text }: { text: string }) {
  return <div className="ap-section-empty">{text}</div>;
}

export function AnalogPlayersPage() {
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
    onNavigate: () => navigate(`/analog/players/${player.id}`),
    onToggle: () => void toggleActive(player),
    onRename: (newName: string) => renamePlayer(player, newName),
  });

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

        {!loading && !error && (
          <>
            <SectionHeader label={t.analogActive} count={active.length} />
            <div className="ap-section">
              {active.length === 0
                ? <EmptySection text={t.analogNoActivePlayers} />
                : active.map((p) => (
                  <PlayerRow key={p.id} player={p} {...makeHandlers(p)} />
                ))}
            </div>

            <SectionHeader label={t.analogInactiveSection} count={inactive.length} />
            <div className="ap-section">
              {inactive.length === 0
                ? <EmptySection text={t.analogNoInactivePlayers} />
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

            {dialogError && <div className="ap-dialog-error">{dialogError}</div>}

            <div className="ap-dialog-actions">
              <button className="ap-btn-cancel" onClick={() => setShowDialog(false)}>
                {t.abbrechen}
              </button>
              <button
                className="ap-btn-create"
                onClick={() => void handleCreate()}
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
