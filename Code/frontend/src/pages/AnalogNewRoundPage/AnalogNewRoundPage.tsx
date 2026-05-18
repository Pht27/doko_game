import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useStaticData } from '@/hooks/useStaticData';
import { useAnalogPlayers } from '@/hooks/useAnalogPlayers';
import { useRoundForm } from '@/hooks/useRoundForm';
import { createRound, getRounds, getRound } from '@/api/analog';
import { RoundForm } from '@/features/RoundForm/RoundForm';
import { usePlayerPreference } from '@/context/PlayerPreferenceContext';
import { t } from '@/utils/translations';

export function AnalogNewRoundPage() {
  const navigate = useNavigate();
  const { data: staticData, loading: staticLoading, error: staticError } = useStaticData();
  const { players, loading: playersLoading, error: playersError } = useAnalogPlayers();
  const {
    form,
    lastSwitchedBlock,
    setGameMode,
    setPoints,
    setWinningParty,
    setComment,
    switchParty,
    setBlockPlayers,
    addSpecialCard,
    removeSpecialCard,
    addExtraPoint,
    updateExtraPointCount,
    removeExtraPoint,
    toApiRequest,
    resetForNew,
    importTeams,
  } = useRoundForm();
  const { selectedPlayer } = usePlayerPreference();
  const [saving, setSaving] = useState(false);

  // Default game mode: Normal
  useEffect(() => {
    if (staticData && form.gameModeId === null) {
      const normal = staticData.gameModes.find((gm) => gm.name === 'Normal');
      if (normal) setGameMode(normal.id);
    }
  }, [staticData, form.gameModeId, setGameMode]);

  // Pre-fill Re block with selected player if the block is still empty
  useEffect(() => {
    if (!selectedPlayer || players.length === 0) return;
    const isActive = players.some((p) => p.isActive && p.id === selectedPlayer.id);
    if (!isActive) return;
    if (form.blocks[0].playerIds.length === 0) {
      setBlockPlayers(0, [selectedPlayer.id]);
    }
  // Only run once after players load
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [players]);

  if (staticLoading || playersLoading) {
    return (
      <div className="arf-page">
        <div style={{ padding: 32, textAlign: 'center', color: 'var(--app-text-sub)' }}>{t.loading}</div>
      </div>
    );
  }

  if (staticError || playersError || !staticData) {
    return (
      <div className="arf-page">
        <div style={{ padding: 32, textAlign: 'center', color: 'var(--app-loss)' }}>
          {staticError ?? playersError ?? 'Fehler'}
        </div>
      </div>
    );
  }

  const handleImportLastTeams = async () => {
    const list = await getRounds(1, 1);
    if (list.items.length === 0) return;
    const detail = await getRound(list.items[0].id);
    const activeIds = new Set(players.filter((p) => p.isActive).map((p) => p.id));
    const filtered = {
      ...detail,
      teams: detail.teams.map((t) => ({
        ...t,
        players: t.players.filter((p) => activeIds.has(p.id)),
      })),
    };
    importTeams(filtered);
  };

  const handleSubmit = async () => {
    setSaving(true);
    try {
      await createRound(toApiRequest());
      resetForNew();
    } finally {
      setSaving(false);
    }
  };

  return (
    <RoundForm
      title={t.analogNewRoundTitle}
      form={form}
      staticData={staticData}
      players={players}
      saving={saving}
      lastSwitchedBlock={lastSwitchedBlock}
      onBack={() => navigate('/')}
      onSetGameMode={setGameMode}
      onSetPoints={setPoints}
      onSetWinningParty={setWinningParty}
      onSetComment={setComment}
      onSwitchParty={switchParty}
      onSetBlockPlayers={setBlockPlayers}
      onAddSpecialCard={addSpecialCard}
      onRemoveSpecialCard={removeSpecialCard}
      onAddExtraPoint={addExtraPoint}
      onUpdateExtraPointCount={updateExtraPointCount}
      onRemoveExtraPoint={removeExtraPoint}
      onSubmit={handleSubmit}
      onImportLastTeams={handleImportLastTeams}
    />
  );
}
