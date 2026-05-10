import { useState } from 'react';
import type { PlayerListItem } from '@/types/analog';
import { t } from '@/utils/translations';
import { ToggleSwitch } from '@/components/ToggleSwitch/ToggleSwitch';
import { FormError } from '@/components/FormError/FormError';

interface PlayerRowProps {
  player: PlayerListItem;
  onNavigate: () => void;
  onToggle: () => void;
  onRename: (name: string) => Promise<void>;
}

export function PlayerRow({ player, onNavigate, onToggle, onRename }: PlayerRowProps) {
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
          <FormError message={renameError} />
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

      <ToggleSwitch
        on={leaving ? !player.isActive : player.isActive}
        onChange={handleToggle}
        stopPropagation
        ariaLabel={player.isActive ? t.analogActive : t.analogInactiveSection}
      />
    </div>
  );
}
