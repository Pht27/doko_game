import { useState, useEffect } from 'react';
import { getPlayers } from '@/api/analog';
import { usePlayerPreference } from '@/context/PlayerPreferenceContext';
import { t } from '@/utils/translations';
import type { PlayerListItem } from '@/types/analog';
import './PlayerPreferencePicker.css';

interface PlayerPreferencePickerProps {
  onSelect?: () => void;
}

export function PlayerPreferencePicker({ onSelect }: PlayerPreferencePickerProps) {
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

  return (
    <div className="ppp-list">
      <button
        className={`ppp-card${selectedPlayer === null ? ' ppp-card--active' : ''}`}
        onClick={() => { clearSelectedPlayer(); onSelect?.(); }}
      >
        <div className="ppp-avatar ppp-avatar--none">–</div>
        <span className="ppp-name">{t.settingsNoPlayer}</span>
        {selectedPlayer === null && <span className="ppp-check">✓</span>}
      </button>

      {loading && <div className="ppp-loading">{t.loading}</div>}

      {!loading && players.map((p) => (
        <button
          key={p.id}
          className={`ppp-card${selectedPlayer?.id === p.id ? ' ppp-card--active' : ''}`}
          onClick={() => { setSelectedPlayer(p.id, p.name); onSelect?.(); }}
        >
          <div className="ppp-avatar">{p.name[0].toUpperCase()}</div>
          <span className="ppp-name">{p.name}</span>
          {selectedPlayer?.id === p.id && <span className="ppp-check">✓</span>}
        </button>
      ))}
    </div>
  );
}
