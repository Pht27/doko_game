import { useState } from 'react';
import { BottomSheet } from '@/components/BottomSheet/BottomSheet';
import { cardSvgPathById, ALL_CARD_IDS } from '@/api/cards';
import { t } from '@/utils/translations';
import './EditProfileModal.css';

function validateName(name: string): string | null {
  const trimmed = name.trim();
  if (!trimmed) return t.playerEditNameEmpty;
  if (trimmed.length > 50) return t.playerEditNameTooLong;
  return null;
}

interface EditProfileModalProps {
  initialName: string;
  currentHeroCard: string | null;
  onSave: (name: string, heroCard: string | null) => Promise<void>;
  onClose: () => void;
}

export function EditProfileModal({ initialName, currentHeroCard, onSave, onClose }: EditProfileModalProps) {
  const [name, setName] = useState(initialName);
  const [heroCard, setHeroCard] = useState<string | null>(currentHeroCard);
  const [nameError, setNameError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [saveError, setSaveError] = useState<string | null>(null);

  async function handleSave() {
    const err = validateName(name);
    if (err) { setNameError(err); return; }
    setSaving(true);
    setSaveError(null);
    try {
      await onSave(name.trim(), heroCard);
      onClose();
    } catch {
      setSaveError(t.playerEditSaveError);
    } finally {
      setSaving(false);
    }
  }

  return (
    <BottomSheet title={t.playerEditProfileTitle} onClose={onClose} maxHeight="92vh">
      <div className="ps-edit-profile">
        <div className="ps-edit-field">
          <label className="ps-edit-label" htmlFor="ps-name-input">{t.playerEditNameLabel}</label>
          <input
            id="ps-name-input"
            className={`ps-edit-name-input${nameError ? ' ps-edit-name-error' : ''}`}
            type="text"
            value={name}
            maxLength={50}
            onChange={(e) => { setName(e.target.value); setNameError(null); }}
            autoComplete="off"
            spellCheck={false}
          />
          {nameError && <span className="ps-edit-name-errmsg">{nameError}</span>}
        </div>

        <div className="ps-edit-field">
          <span className="ps-edit-label">{t.playerEditHeroCardLabel}</span>
          <div className="ps-card-picker-grid">
            {ALL_CARD_IDS.map((id) => {
              const url = cardSvgPathById(id);
              const isSelected = id === heroCard;
              return (
                <button
                  key={id}
                  className={`ps-card-picker-item${isSelected ? ' ps-card-picker-selected' : ''}`}
                  onClick={() => setHeroCard(id)}
                >
                  <img src={url} alt={id} width={52} height={75} style={{ display: 'block', borderRadius: 5 }} />
                </button>
              );
            })}
          </div>
        </div>

        {saveError && <div className="ps-edit-save-error">{saveError}</div>}

        <div className="ps-edit-actions">
          <button className="ps-edit-btn-cancel" onClick={onClose} disabled={saving}>{t.abbrechen}</button>
          <button className="ps-edit-btn-save" onClick={handleSave} disabled={saving}>
            {saving ? t.analogSaving : t.analogSave}
          </button>
        </div>
      </div>
    </BottomSheet>
  );
}
