import { useState, useEffect } from 'react';
import { t } from '@/utils/translations';
import type { RoundFormState, Party } from '@/hooks/useRoundForm';
import type { PlayerListItem, StaticData } from '@/types/analog';
import { PageHeader } from '@/components/PageHeader/PageHeader';
import { Button } from '@/components/Button/Button';
import { FormError } from '@/components/FormError/FormError';
import { TeamBlock } from './TeamBlock';
import { TeamEditorModal } from './TeamEditorModal';
import { GameModePickerModal } from './GameModePickerModal';
import './RoundForm.css';

interface Props {
  title: string;
  form: RoundFormState;
  staticData: StaticData;
  players: PlayerListItem[];
  saving: boolean;
  lastSwitchedBlock: number | null;
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
  lastSwitchedBlock,
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
  const [showGameModePicker, setShowGameModePicker] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  // Local state to allow intermediate '-' during typing
  const [pointsDisplay, setPointsDisplay] = useState<string>(() =>
    form.points === '' ? '' : String(form.points),
  );

  // Sync when form.points changes externally (e.g. edit prefill)
  useEffect(() => {
    if (form.points !== '') setPointsDisplay(String(form.points));
    else if (pointsDisplay !== '' && pointsDisplay !== '-') setPointsDisplay('');
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [form.points]);

  const selectedMode = staticData.gameModes.find((gm) => gm.id === form.gameModeId);
  const isSolo = selectedMode?.isSolo ?? false;

  const handlePartyToggle = (party: Party) => {
    onSetWinningParty(form.winningParty === party ? null : party);
  };

  const handleSubmit = () => {
    const err = validate(form);
    if (err) { setSubmitError(err); return; }
    setSubmitError(null);
    onSubmit();
  };

  const otherBlockIds = (blockIndex: number) =>
    form.blocks.flatMap((b, i) => (i === blockIndex ? [] : b.playerIds));

  const otherBlockSpecialCardIds = (blockIndex: number) =>
    form.blocks.flatMap((b, i) => (i === blockIndex ? [] : b.specialCardIds));

  const indexed = form.blocks.map((block, index) => ({ block, index }));
  const reBlocks = indexed
    .filter(({ block }) => block.party === 'Re')
    .sort((a, b) => {
      if (a.index === lastSwitchedBlock) return 1;
      if (b.index === lastSwitchedBlock) return -1;
      return a.index - b.index;
    });
  const kontraBlocks = indexed
    .filter(({ block }) => block.party === 'Kontra')
    .sort((a, b) => {
      if (a.index === lastSwitchedBlock) return 1;
      if (b.index === lastSwitchedBlock) return -1;
      return a.index - b.index;
    });


  return (
    <div className="arf-page">
      <PageHeader title={title} onBack={onBack} />

      <div className="arf-body">
        {/* ── Spielmodus trigger ── */}
        <button
          className={`arf-gamemode-btn${isSolo ? ' arf-solo' : ''}${selectedMode ? ' arf-gamemode-selected' : ''}`}
          onClick={() => setShowGameModePicker(true)}
        >
          <span className="arf-gamemode-label">
            <span className="arf-gamemode-caption">
              {isSolo ? 'Solo' : t.analogGameModeCaption}
            </span>
            <span className="arf-gamemode-value">
              {selectedMode ? selectedMode.name : t.analogGameModeEmpty}
            </span>
          </span>
          <span className="arf-gamemode-chevron">›</span>
        </button>

        {/* ── Meta: Re | Punkte-Label | Kontra ── */}
        <div className="arf-meta">
          <button
            className={`arf-party-btn${form.winningParty === 'Re' ? ' arf-re-selected' : ''}`}
            onClick={() => handlePartyToggle('Re')}
          >
            {t.reLabel}
          </button>

          <div className="arf-points-label">{t.analogPointsLabel}</div>

          <button
            className={`arf-party-btn${form.winningParty === 'Kontra' ? ' arf-kontra-selected' : ''}`}
            onClick={() => handlePartyToggle('Kontra')}
          >
            {t.kontraLabel}
          </button>
        </div>

        {/* ── Team columns + Punkte input in center ── */}
        <div className="arf-teams">
          <div className="arf-party-col arf-re-col">
            {reBlocks.map(({ block, index }, vi) => (
              <TeamBlock
                key={`${index}-${block.party}`}
                block={block}
                winningParty={form.winningParty}
                allPlayers={players}
                specialCards={staticData.specialCards}
                extraPoints={staticData.extraPoints}
                animateOnLoad={vi === 0}
                justSwitched={lastSwitchedBlock === index}
                onSwitch={() => onSwitchParty(index)}
                onEdit={() => setEditingBlock(index)}
              />
            ))}
          </div>

          <div className="arf-teams-mid">
            <input
              className="arf-points-input"
              type="text"
              inputMode="numeric"
              value={pointsDisplay}
              onChange={(e) => {
                const raw = e.target.value.replace(/[^0-9-]/g, '');
                const cleaned = raw.startsWith('-')
                  ? '-' + raw.slice(1).replace(/-/g, '')
                  : raw.replace(/-/g, '');
                setPointsDisplay(cleaned);
                onSetPoints(cleaned === '' || cleaned === '-' ? '' : Number(cleaned));
              }}
              placeholder=""
              aria-label="Punkte"
            />
          </div>

          <div className="arf-party-col arf-kontra-col">
            {kontraBlocks.map(({ block, index }) => (
              <TeamBlock
                key={`${index}-${block.party}`}
                block={block}
                winningParty={form.winningParty}
                allPlayers={players}
                specialCards={staticData.specialCards}
                extraPoints={staticData.extraPoints}
                justSwitched={lastSwitchedBlock === index}
                onSwitch={() => onSwitchParty(index)}
                onEdit={() => setEditingBlock(index)}
              />
            ))}
          </div>
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

        <FormError message={submitError} />

        <div className="arf-save-spacer" />
      </div>

      <div className="arf-save-bar">
        <Button className="w-full pointer-events-auto" disabled={saving} onClick={handleSubmit}>
          {saving ? t.analogSaving : t.analogSave}
        </Button>
      </div>

      {showGameModePicker && (
        <GameModePickerModal
          gameModes={staticData.gameModes}
          selectedId={form.gameModeId}
          onSelect={onSetGameMode}
          onClose={() => setShowGameModePicker(false)}
        />
      )}

      {editingBlock !== null && (
        <TeamEditorModal
          block={form.blocks[editingBlock]}
          blockIndex={editingBlock}
          allPlayers={players}
          specialCards={staticData.specialCards}
          extraPoints={staticData.extraPoints}
          assignedPlayerIds={otherBlockIds(editingBlock)}
          assignedSpecialCardIds={otherBlockSpecialCardIds(editingBlock)}
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
