import { useState, useEffect, useCallback } from 'react';
import { getPlayers } from '@/api/analog';
import { usePlayerPreference } from '@/context/PlayerPreferenceContext';
import { useTheme, PRESETS } from '@/hooks/useTheme';
import { Toast } from '@/components/Toast/Toast';
import { t } from '@/utils/translations';
import type { PlayerListItem } from '@/types/analog';
import './PlayerPreferencePicker.css';

const ROSE_PRESET = PRESETS.find(p => p.id === 'rose')!;

export function PlayerPreferencePicker() {
  const { selectedPlayer, setSelectedPlayer, clearSelectedPlayer } = usePlayerPreference();
  const { setPreset } = useTheme();
  const [players, setPlayers] = useState<PlayerListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [greeting, setGreeting] = useState<string | null>(null);
  const [confettiColors, setConfettiColors] = useState<string[] | undefined>(undefined);

  useEffect(() => {
    getPlayers()
      .then((data) => {
        const active = data.filter((p) => p.isActive).sort((a, b) => a.name.localeCompare(b.name, 'de'));
        setPlayers(active);
        setLoading(false);
      })
      .catch(() => setLoading(false));
  }, []);

  const clearGreeting = useCallback(() => setGreeting(null), []);

  function handleChange(e: React.ChangeEvent<HTMLSelectElement>) {
    const val = e.target.value;
    if (val === '') {
      clearSelectedPlayer();
    } else {
      const id = Number(val);
      const player = players.find(p => p.id === id);
      if (player) {
        setSelectedPlayer(player.id, player.name);
        setGreeting(t.settingsGreeting(player.name));
        if (player.id === 5) {
          setPreset(ROSE_PRESET);
          setConfettiColors(['#f43f8e', '#fb7185', '#fda4af', '#e11d6a', '#fecdd3', '#9f1239', '#ff6b9d']);
        } else {
          setConfettiColors(undefined);
        }
      }
    }
  }

  return (
    <>
      {greeting && <Toast message={greeting} confettiColors={confettiColors} onDone={clearGreeting} />}
      <div className="ppp-wrap">
        <select
          className="ppp-select"
          value={selectedPlayer?.id ?? ''}
          onChange={handleChange}
          disabled={loading}
        >
          <option value="">{t.settingsNoPlayer}</option>
          {players.map((p) => (
            <option key={p.id} value={p.id}>{p.name}</option>
          ))}
        </select>
      </div>
    </>
  );
}
