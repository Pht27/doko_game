import { useState, useEffect } from 'react';
import { getPlayers } from '@/api/analog';
import { usePlayerPreference } from '@/context/PlayerPreferenceContext';
import { t } from '@/utils/translations';
import type { PlayerListItem } from '@/types/analog';
import './PlayerPreferencePicker.css';

export function PlayerPreferencePicker() {
  const { selectedPlayer, setSelectedPlayer, clearSelectedPlayer } = usePlayerPreference();
  const [players, setPlayers] = useState<PlayerListItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getPlayers()
      .then((data) => {
        const active = data.filter((p) => p.isActive).sort((a, b) => a.name.localeCompare(b.name, 'de'));
        setPlayers(active);
        setLoading(false);
      })
      .catch(() => setLoading(false));
  }, []);

  function handleChange(e: React.ChangeEvent<HTMLSelectElement>) {
    const val = e.target.value;
    if (val === '') {
      clearSelectedPlayer();
    } else {
      const id = Number(val);
      const player = players.find(p => p.id === id);
      if (player) setSelectedPlayer(player.id, player.name);
    }
  }

  return (
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
  );
}
