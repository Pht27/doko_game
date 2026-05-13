import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useStaticData } from '@/hooks/useStaticData';
import { useAnalogPlayers } from '@/hooks/useAnalogPlayers';
import { useRoundForm } from '@/hooks/useRoundForm';
import { getRound, updateRound } from '@/api/analog';
import { RoundForm } from '@/features/RoundForm/RoundForm';
import { t } from '@/utils/translations';

export function AnalogEditRoundPage() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const roundId = Number(id);

  const { data: staticData, loading: staticLoading, error: staticError } = useStaticData();
  const { players, loading: playersLoading, error: playersError } = useAnalogPlayers();
  const [roundLoading, setRoundLoading] = useState(true);
  const [roundError, setRoundError] = useState<string | null>(null);

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
    loadFromDetail,
  } = useRoundForm();
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!roundId) return;
    let cancelled = false;
    getRound(roundId)
      .then((detail) => {
        if (cancelled) return;
        loadFromDetail(detail);
        setRoundLoading(false);
      })
      .catch((err: unknown) => {
        if (cancelled) return;
        setRoundError(err instanceof Error ? err.message : 'Fehler');
        setRoundLoading(false);
      });
    return () => { cancelled = true; };
  }, [roundId, loadFromDetail]);

  const isLoading = staticLoading || playersLoading || roundLoading;
  const error = staticError ?? playersError ?? roundError;

  if (isLoading) {
    return (
      <div className="arf-page">
        <div style={{ padding: 32, textAlign: 'center', color: 'var(--app-text-sub)' }}>{t.loading}</div>
      </div>
    );
  }

  if (error || !staticData) {
    return (
      <div className="arf-page">
        <div style={{ padding: 32, textAlign: 'center', color: 'var(--app-loss)' }}>
          {error ?? 'Fehler'}
        </div>
      </div>
    );
  }

  const handleSubmit = async () => {
    setSaving(true);
    try {
      await updateRound(roundId, toApiRequest());
      navigate('/history');
    } finally {
      setSaving(false);
    }
  };

  return (
    <RoundForm
      title={t.analogEditRoundTitle}
      form={form}
      staticData={staticData}
      players={players}
      saving={saving}
      lastSwitchedBlock={lastSwitchedBlock}
      onBack={() => navigate('/history')}
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
