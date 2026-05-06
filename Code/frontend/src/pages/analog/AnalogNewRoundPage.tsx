import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useStaticData } from '@/hooks/useStaticData';
import { useAnalogPlayers } from '@/hooks/useAnalogPlayers';
import { useRoundForm } from '@/hooks/useRoundForm';
import { createRound } from '@/api/analog';
import { RoundForm } from '@/features/analog/RoundForm/RoundForm';
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
  } = useRoundForm();
  const [saving, setSaving] = useState(false);

  // Default game mode: Normal
  useEffect(() => {
    if (staticData && form.gameModeId === null) {
      const normal = staticData.gameModes.find((gm) => gm.name === 'Normal');
      if (normal) setGameMode(normal.id);
    }
  }, [staticData, form.gameModeId, setGameMode]);

  if (staticLoading || playersLoading) {
    return (
      <div className="arf-page">
        <div style={{ padding: 32, textAlign: 'center', color: '#aaaacc' }}>{t.loading}</div>
      </div>
    );
  }

  if (staticError || playersError || !staticData) {
    return (
      <div className="arf-page">
        <div style={{ padding: 32, textAlign: 'center', color: '#f87171' }}>
          {staticError ?? playersError ?? 'Fehler'}
        </div>
      </div>
    );
  }

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
    />
  );
}
