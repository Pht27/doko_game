import { useState } from 'react';
import { t } from '@/utils/translations';
import type { RoundFormState, Party } from '@/hooks/useRoundForm';
import type { PlayerListItem, StaticData } from '@/types/analog';
import { TeamBlock } from './TeamBlock';
import { TeamEditorModal } from './TeamEditorModal';
import './RoundForm.css';

interface Props {
  title: string;
  form: RoundFormState;
  staticData: StaticData;
  players: PlayerListItem[];
  saving: boolean;
  onBack: () => void;
  onSetGameMode: (id: number | null) => void;
  onSetPoints: (v: number | '') => void;
  onSetWinningParty: (party: Party | null) => void;
  onSetComment: (comment: string) => void;
  onSwitchParty: (i: number) => void;
  onSetBlockPlayers: (i: number, ids: number[]) => void;
  onAddSpecialCard: (i: number, id: number) => void;
  onRemoveSpecialCard: (i: number, id: number) => void;
  onAddExtraPoint: (i: number, id: number) => void;
  onUpdateExtraPointCount: (i: number, epId: number, delta: number) => void;
  onRemoveExtraPoint: (i: number, epId: number) => void;
  onSubmit: () => void;
}

function validate(form: RoundFormState): string | null {
  if (form.gameModeId === null) return t.analogValidationGameMode;
  if (form.points === '') return t.analogValidationPoints;
  if (form.winningParty === null) return t.analogValidationWinningParty;
  if (form.blocks.some((b) => b.playerIds.length === 0)) return t.analogValidationPlayersRequired;
  if (form.blocks.some((b) => b.playerIds.length > 2)) return t.analogValidationMaxPlayers;
  const allIds = form.blocks.flatMap((b) => b.playerIds);
  if (new Set(allIds).size !== allIds.length) return t.analogValidationPlayerDuplicate;
  return null;
}

export function RoundForm({
  title,
  form,
  staticData,
  players,
  saving,
  onBack,
  onSetGameMode,
  onSetPoints,
  onSetWinningParty,
  onSetComment,
  onSwitchParty,
  onSetBlockPlayers,
  onAddSpecialCard,
  onRemoveSpecialCard,
  onAddExtraPoint,
  onUpdateExtraPointCount,
  onRemoveExtraPoint,
  onSubmit,
}: Props) {
  const [editingBlock, setEditingBlock] = useState<number | null>(null);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const selectedMode = staticData.gameModes.find((gm) => gm.id === form.gameModeId);
  const isSolo = selectedMode?.isSolo ?? false;

  const handlePartyToggle = (party: Party) => {
    onSetWinningParty(form.winningParty === party ? null : party);
  };

  const handleSubmit = () => {
    const err = validate(form);
    if (err) {
      setSubmitError(err);
      return;
    }
    setSubmitError(null);
    onSubmit();
  };

  // All player IDs assigned to other blocks (for modal filtering)
  const otherBlockIds = (blockIndex: number) =>
    form.blocks.flatMap((b, i) => (i === blockIndex ? [] : b.playerIds));

  return (
    <div className="arf-page">
      <div className="arf-header">
        <button className="arf-back" onClick={onBack} aria-label={t.back}>
          ←
        </button>
        <h1 className="arf-title">{title}</h1>
      </div>

      <div className="arf-body">
        {/* ── Spielmodus ── */}
        <select
          className={`arf-gamemode-select${isSolo ? ' arf-solo' : ''}`}
          value={form.gameModeId ?? ''}
          onChange={(e) => onSetGameMode(e.target.value ? Number(e.target.value) : null)}
        >
          <option value="">{t.analogGameModeLabel}</option>
          {staticData.gameModes.map((gm) => (
            <option key={gm.id} value={gm.id}>
              {gm.name}
            </option>
          ))}
        </select>

        {/* ── Meta: Re | Punkte | Kontra ── */}
        <div className="arf-meta">
          <button
            className={`arf-party-btn${form.winningParty === 'Re' ? ' arf-party-selected' : ''}`}
            onClick={() => handlePartyToggle('Re')}
          >
            <span className="arf-party-check">✓</span>
            {t.reLabel}
          </button>

          <div className="arf-points-wrap">
            <input
              className="arf-points-input"
              type="text"
              inputMode="numeric"
              pattern="-?[0-9]*"
              value={form.points}
              onChange={(e) => {
                const raw = e.target.value.replace(/[^0-9-]/g, '').replace(/(?!^)-/g, '');
                onSetPoints(raw === '' || raw === '-' ? '' : Number(raw));
              }}
              placeholder="·"
              aria-label="Punkte"
            />
          </div>

          <button
            className={`arf-party-btn${form.winningParty === 'Kontra' ? ' arf-party-selected' : ''}`}
            onClick={() => handlePartyToggle('Kontra')}
          >
            {t.kontraLabel}
            <span className="arf-party-check">✓</span>
          </button>
        </div>

        {/* ── Team-Blocks ── */}
        <div className="arf-teams">
          {/* Block 0 (Re col) */}
          <TeamBlock
            block={form.blocks[0]}
            winningParty={form.winningParty}
            allPlayers={players}
            specialCards={staticData.specialCards}
            extraPoints={staticData.extraPoints}
            align="left"
            onSwitch={() => onSwitchParty(0)}
            onEdit={() => setEditingBlock(0)}
          />

          {/* Middle spacer spanning both rows */}
          <div className="arf-teams-mid">
            <span className="arf-swipe-hint">{'←\n→'}</span>
          </div>

          {/* Block 2 (Kontra col) */}
          <TeamBlock
            block={form.blocks[2]}
            winningParty={form.winningParty}
            allPlayers={players}
            specialCards={staticData.specialCards}
            extraPoints={staticData.extraPoints}
            align="right"
            onSwitch={() => onSwitchParty(2)}
            onEdit={() => setEditingBlock(2)}
          />

          {/* Block 1 (Re col, row 2) */}
          <TeamBlock
            block={form.blocks[1]}
            winningParty={form.winningParty}
            allPlayers={players}
            specialCards={staticData.specialCards}
            extraPoints={staticData.extraPoints}
            align="left"
            onSwitch={() => onSwitchParty(1)}
            onEdit={() => setEditingBlock(1)}
          />

          {/* Block 3 (Kontra col, row 2) */}
          <TeamBlock
            block={form.blocks[3]}
            winningParty={form.winningParty}
            allPlayers={players}
            specialCards={staticData.specialCards}
            extraPoints={staticData.extraPoints}
            align="right"
            onSwitch={() => onSwitchParty(3)}
            onEdit={() => setEditingBlock(3)}
          />
        </div>

        {/* ── Kommentar ── */}
        <input
          className="arf-comment-input"
          type="text"
          placeholder={t.analogCommentPlaceholder}
          value={form.comment}
          onChange={(e) => onSetComment(e.target.value)}
          maxLength={500}
        />

        {submitError && <div className="arf-error">{submitError}</div>}

        <button className="arf-save-btn" disabled={saving} onClick={handleSubmit}>
          {saving ? t.analogSaving : t.analogSave}
        </button>
      </div>

      {/* ── Team-Editor Modal ── */}
      {editingBlock !== null && (
        <TeamEditorModal
          block={form.blocks[editingBlock]}
          blockIndex={editingBlock}
          allPlayers={players}
          specialCards={staticData.specialCards}
          extraPoints={staticData.extraPoints}
          assignedPlayerIds={otherBlockIds(editingBlock)}
          onClose={() => setEditingBlock(null)}
          onSetPlayers={(ids) => onSetBlockPlayers(editingBlock, ids)}
          onAddSpecialCard={(id) => onAddSpecialCard(editingBlock, id)}
          onRemoveSpecialCard={(id) => onRemoveSpecialCard(editingBlock, id)}
          onAddExtraPoint={(id) => onAddExtraPoint(editingBlock, id)}
          onUpdateExtraPointCount={(id, delta) => onUpdateExtraPointCount(editingBlock, id, delta)}
          onRemoveExtraPoint={(id) => onRemoveExtraPoint(editingBlock, id)}
        />
      )}
    </div>
  );
}
